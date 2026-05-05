using AuditFramework.ApiService.Data;
using AuditFramework.ApiService.Endpoints;
using AuditFramework.ApiService.Temporal;
using AuditFramework.ApiService.Temporal.Activities;
using AuditFramework.ApiService.Temporal.Workflows;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Temporalio.Client;
using Temporalio.Exceptions;
using Temporalio.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Identity DbContext (ASP.NET Core Identity users)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("auditdb")));

// Audit DbContext (Temporal entity tracking + audit events)
builder.Services.AddDbContext<AuditDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("auditdb"))
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Identity auth (Bearer tokens)
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

// Temporal client (injected into API endpoints)
var temporalAddress = builder.Configuration["Temporal__Address"] ?? "localhost:7233";
builder.Services.AddTemporalClient(temporalAddress);

// Temporal worker
builder.Services.AddHostedTemporalWorker(TemporalConstants.TaskQueue)
    .AddScopedActivities<SaveUserStateActivity>()
    .AddScopedActivities<WriteAuditEventActivity>()
    .AddWorkflow<UserWorkflow>();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Migrate and seed on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    // Identity schema — EnsureCreated (no EF migrations needed for demo)
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDb.Database.EnsureCreatedAsync();

    // Seed demo Identity users
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedIdentityUsersAsync(userManager);

    // Audit schema — apply managed migrations + seed domain users
    var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await auditDb.Database.MigrateAsync();
    await DataSeeder.SeedAsync(auditDb);
}

static async Task SeedIdentityUsersAsync(UserManager<ApplicationUser> userManager)
{
    var demos = DataSeeder.AllEmails.Select(e => (Email: e, Password: "Demo@1234")).ToArray();
    foreach (var (email, password) in demos)
    {
        if (await userManager.FindByEmailAsync(email) is null)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            await userManager.CreateAsync(user, password);
        }
    }
}

// Identity endpoints (login, register, manage)
app.MapIdentityApi<ApplicationUser>();
app.MapProfileEndpoints();

// GET /users — list all users (admin use)
app.MapGet("/users", async (AuditDbContext db) =>
    Results.Ok(await db.Users.OrderBy(u => u.Name).ToListAsync()));

// GET /audit/events?resourceId=optional
app.MapGet("/audit/events", async (Guid? resourceId, AuditDbContext db) =>
{
    var query = db.Events.AsQueryable();
    if (resourceId.HasValue)
        query = query.Where(e => e.ResourceId == resourceId.Value);

    var events = await query
        .OrderByDescending(e => e.OccurredAt)
        .Take(200)
        .ToListAsync();

    return Results.Ok(events.Select(e => new
    {
        e.Id,
        e.ActorId,
        e.Action,
        e.ResourceType,
        e.ResourceId,
        OldResource = e.OldResource == null ? (object?)null : e.OldResource.RootElement,
        NewResource = (object)e.NewResource.RootElement,
        Metadata    = e.Metadata == null ? (object?)null : e.Metadata.RootElement,
        e.OccurredAt,
    }));
});

// GET /users/{userId}/profile
app.MapGet("/users/{userId:guid}/profile", async (Guid userId, AuditDbContext db) =>
{
    var user = await db.Users.FindAsync(userId);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// PATCH /users/{userId}/profile — triggers the UserWorkflow update handler
app.MapPatch("/users/{userId:guid}/profile", async (
    Guid userId,
    UpdateProfileBody body,
    HttpContext ctx,
    ITemporalClient client,
    AuditDbContext db) =>
{
    if (!ctx.Request.Headers.TryGetValue("X-User-Id", out var actorIdHeader) ||
        !Guid.TryParse(actorIdHeader, out var actorId))
        return Results.Problem("X-User-Id header is required", statusCode: 401);

    var actorRole = ctx.Request.Headers["X-User-Role"].ToString();

    if (actorRole != "admin" && actorId != userId)
        return Results.Problem("Forbidden: users can only update their own profile", statusCode: 403);

    var workflowId = $"user-{userId}";

    // Seed the workflow with current DB state so the first update has correct email/role
    var existing = await db.Users.FindAsync(userId);
    var initialState = existing is null ? null : new UserProfileState
    {
        UserId = existing.Id,
        Email = existing.Email,
        Name = existing.Name,
        Bio = existing.Bio,
        Role = existing.Role,
    };

    try
    {
        await client.StartWorkflowAsync(
            (IUserWorkflow wf) => wf.RunAsync(userId, initialState),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = TemporalConstants.TaskQueue
            });
    }
    catch (WorkflowAlreadyStartedException) { }

    var handle = client.GetWorkflowHandle(workflowId);
    // Note: Temporal .NET SDK strips "Async" from method names → "UpdateProfileAsync" → "UpdateProfile"
    var updatedState = await handle.ExecuteUpdateAsync<UserProfileState>(
        "UpdateProfile",
        [new UpdateUserProfileRequest(actorId, actorRole, body.Name, body.Bio, body.Email,
            // Only admins can change roles
            actorRole == "admin" ? body.Role : null)]);

    return Results.Ok(updatedState);
});

app.MapDefaultEndpoints();
app.Run();

record UpdateProfileBody(string? Name, string? Bio, string? Email, string? Role);
