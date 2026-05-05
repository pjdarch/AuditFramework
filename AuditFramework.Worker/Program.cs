using AuditFramework.Worker.Activities;
using AuditFramework.Worker.Workflows;
using Temporalio.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var temporalAddress = builder.Configuration["Temporal:Address"] ?? "localhost:7233";

builder.Services
    .AddHostedTemporalWorker(temporalAddress, "default", "audit-task-queue")
    .AddScopedActivities<AuditActivities>()
    .AddWorkflow<AuditWorkflow>();

var host = builder.Build();
await host.RunAsync();
