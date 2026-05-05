var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", "postgres", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var auditDb = postgres.AddDatabase("auditdb");
var temporalDb = postgres.AddDatabase("temporaldb");

var temporal = builder
    .AddContainer("temporal", "temporalio/auto-setup", "1.25.2")
    .WithEnvironment("DB", "postgres12")
    .WithEnvironment("DB_PORT", "5432")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PWD", "postgres")
    .WithEnvironment("POSTGRES_DB", "temporal")
    .WithEnvironment("POSTGRES_SEEDS", "postgres")
    .WithEndpoint(port: 7233, targetPort: 7233, name: "grpc")
    .WaitFor(postgres);

var temporalUi = builder
    .AddContainer("temporal-ui", "temporalio/ui", "2.31.2")
    .WithEnvironment("TEMPORAL_ADDRESS", "temporal:7233")
    .WithHttpEndpoint(port: 8233, targetPort: 8080, name: "http")
    .WithExternalHttpEndpoints()
    .WaitFor(temporal);

var apiService = builder
    .AddProject<Projects.AuditFramework_ApiService>("apiservice")
    .WithReference(auditDb)
    .WaitFor(auditDb)
    .WithEnvironment("Temporal__Address", "temporal:7233")
    .WithHttpHealthCheck("/health");

builder
    .AddProject<Projects.AuditFramework_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WaitFor(temporalUi);

builder.Build().Run();
