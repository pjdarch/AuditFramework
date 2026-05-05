# AuditFramework

AuditFramework is a .NET Aspire solution that wires together a Blazor web frontend, an ASP.NET Core API service, PostgreSQL, and Temporal for local development.

## Solution layout

- `AuditFramework.AppHost` orchestrates the local distributed application.
- `AuditFramework.Web` is the Blazor frontend served as `webfrontend`.
- `AuditFramework.ApiService` exposes backend HTTP endpoints as `apiservice`.
- `AuditFramework.ServiceDefaults` centralizes health checks, service discovery, resilience, and OpenTelemetry defaults.
- `AuditFramework.Tests` contains integration tests that boot the Aspire app host and verify the running services.

## Prerequisites

- .NET 10 SDK compatible with the `net10.0` target in this repo
- Docker Desktop or another local Docker runtime

The AppHost starts PostgreSQL, pgWeb, Temporal, and Temporal UI as containers, so Docker needs to be running before you launch the application or run the integration tests.

## Run the application

Restore dependencies:

```bash
dotnet restore AuditFramework.slnx
```

Start the Aspire app host:

```bash
dotnet run --project AuditFramework.AppHost
```

From the Aspire dashboard you can open:

- `webfrontend` for the Blazor UI
- `apiservice` for the backend service
- `temporal-ui` for Temporal workflows
- `postgres` / `pgweb` for database inspection

## Run the tests

Run the full test project:

```bash
dotnet test AuditFramework.Tests/AuditFramework.Tests.csproj
```

The current test suite verifies:

- the web frontend root page responds successfully
- the API `GET /weatherforecast` endpoint returns five forecast items
- the `webfrontend` and `apiservice` health endpoints report healthy

Because the tests start the distributed app, expect the first run to take longer while containers initialize.

## Current application behavior

- The API exposes a sample `GET /weatherforecast` endpoint.
- The frontend includes a weather page that reads from the API through service discovery.
- Health endpoints are available in development at `/health` and `/alive`.

## Notes for ongoing work

Identity and authentication work appears to be in progress in `AuditFramework.ApiService`. The tests and documentation in this branch intentionally avoid that surface area so they can coexist cleanly with auth changes.
