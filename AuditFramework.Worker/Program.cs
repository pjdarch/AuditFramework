using AuditFramework.Worker.Activities;
using AuditFramework.Worker.Workflows;
using Temporalio.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Register the Temporal client separately so it can also be injected
// (e.g. to start workflows from within this process)
builder.Services.AddTemporalClient(options =>
{
    options.TargetHost = builder.Configuration["Temporal:Address"] ?? "localhost:7233";
    options.Namespace = "default";
});

// Worker references the already-registered client; only needs the task queue
builder.Services
    .AddHostedTemporalWorker("audit-task-queue")
    .AddScopedActivities<AuditActivities>()
    .AddWorkflow<AuditWorkflow>();

var host = builder.Build();
await host.RunAsync();
