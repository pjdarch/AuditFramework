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
    opts.UseNpgsql(builder.Configuration.GetConnectionString("auditdb")));

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
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDb.Database.MigrateAsync();

    var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await auditDb.Database.MigrateAsync();
    await DataSeeder.SeedAsync(auditDb);
}

// Identity endpoints (login, register, manage)
app.MapIdentityApi<ApplicationUser>();
app.MapProfileEndpoints();

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
    ITemporalClient client) =>
{
    if (!ctx.Request.Headers.TryGetValue("X-User-Id", out var actorIdHeader) ||
        !Guid.TryParse(actorIdHeader, out var actorId))
        return Results.Problem("X-User-Id header is required", statusCode: 401);

    var actorRole = ctx.Request.Headers["X-User-Role"].ToString();

    if (actorRole != "admin" && actorId != userId)
        return Results.Problem("Forbidden: users can only update their own profile", statusCode: 403);

    var workflowId = $"user-{userId}";

    try
    {
        await client.StartWorkflowAsync(
            (IUserWorkflow wf) => wf.RunAsync(userId, null),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = TemporalConstants.TaskQueue
            });
    }
    catch (WorkflowAlreadyStartedException) { }

    var handle = client.GetWorkflowHandle(workflowId);
    var updatedState = await handle.ExecuteUpdateAsync<UserProfileState>(
        "UpdateProfileAsync",
        [new UpdateUserProfileRequest(actorId, actorRole, body.Name, body.Bio)]);

    return Results.Ok(updatedState);
});

app.MapDefaultEndpoints();
app.Run();

record UpdateProfileBody(string? Name, string? Bio);
