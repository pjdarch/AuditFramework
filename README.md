# AuditFramework

AuditFramework is a .NET 10 / Aspire proof-of-concept that demonstrates how to build a tamper-proof audit trail for entity mutations using Temporal's entity workflow pattern. Every change to a user profile flows through a dedicated, long-running Temporal workflow that persists the updated state to PostgreSQL and appends an immutable before/after snapshot to a separate audit schema — giving you a complete, queryable history of every change without touching the production table.

## What It Demonstrates

- **Temporal entity pattern**: one long-running workflow per user, identified by a stable workflow ID (`user-{uuid}`), acting as a reliable coordinator for all mutations to that entity.
- **One-workflow-per-entity**: each user's workflow is started on first profile update and remains open indefinitely, accumulating a verifiable event history.
- **Update handlers**: profile changes arrive as synchronous Temporal updates, so the HTTP caller receives a confirmed result only after both database writes have committed.
- **ContinueAsNew after 100 events**: the workflow automatically resets its history every 100 events to keep the Temporal event log bounded while preserving all audit data in PostgreSQL.
- **Immutable audit log in PostgreSQL**: the `audit.events` table is append-only; each row stores the `resource_type`, `changed_by`, timestamp, and full before/after JSON snapshots of the resource.
- **Aspire orchestration**: a single `dotnet run` command starts Postgres 17, the Temporal server, Temporal UI, the API worker, and the Blazor frontend — no manual service wiring required.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (must be running before you start)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

No other installs are needed. PostgreSQL and Temporal start automatically as Docker containers managed by Aspire.

## Running the Project

### 1. Clone the repository

```bash
git clone <repo-url>
cd AuditFramework
```

### 2. Start everything

```bash
dotnet run --project AuditFramework.AppHost
```

The first run downloads Docker images for Postgres and Temporal, which takes roughly one minute. After that, Aspire starts all services automatically.

### 3. Open the Aspire dashboard

The terminal prints a dashboard URL such as `https://localhost:17xxx`. Open it to see every service, its status, and live log streams.

### 4. Open the web app

Find the `webfrontend` endpoint in the Aspire dashboard and open it in your browser, or look for it printed directly in the console output.

## Demo Walkthrough

1. Open the web app and click **Sign in**.
2. Log in as `admin@example.com` with password `Demo@1234`.
3. Navigate to **Profile** in the sidebar.
4. Change the Name or Bio field and click **Save Changes**.
5. Open **Temporal UI** at [http://localhost:8233](http://localhost:8233).
6. Locate the workflow named `user-00000000-0000-0000-0000-000000000001`.
7. Click into it and inspect the event history: you will see `WorkflowExecutionStarted`, followed by `ActivityTaskScheduled: SaveUserState` and `ActivityTaskScheduled: WriteAuditEvent` for each profile change.
8. Sign out, then log in as `user@example.com` / `Demo@1234`. Edit that user's profile and return to Temporal UI — a second workflow, `user-00000000-0000-0000-0000-000000000002`, will have been created for the regular user.

### Seeded demo credentials

| Email | Password | Role |
|---|---|---|
| admin@example.com | Demo@1234 | admin — can edit any profile |
| user@example.com | Demo@1234 | user — can only edit own profile |

## Audit Events in PostgreSQL (optional)

Audit events are also queryable directly. Open the **pgweb** link from the Aspire dashboard and run:

```sql
SELECT * FROM audit.events ORDER BY occurred_at DESC;
```

Each row contains the resource type, the user who made the change, the timestamp, and JSONB columns holding the full before and after snapshots of the resource.

## Project Structure

```
AuditFramework.AppHost/          — Aspire orchestrator: declares all services and their wiring
AuditFramework.ApiService/
  Data/                          — EF Core models, migrations, seeder
  Temporal/
    Workflows/UserWorkflow.cs    — Entity pattern: one workflow per user
    Activities/                  — SaveUserState + WriteAuditEvent activities
  Program.cs                     — Minimal API endpoints + Temporal worker registration
AuditFramework.Web/
  Components/Pages/Profile.razor — Profile edit page (calls PATCH /users/{id}/profile)
  Services/                      — Typed HTTP clients for the API
AuditFramework.ServiceDefaults/  — Shared Aspire telemetry and health-check defaults
```

## Architecture

```
Browser → Blazor Web → PATCH /users/{id}/profile
                              ↓
                        Temporal Worker
                              ↓
                    UserWorkflow (entity, per-user)
                      ├─ SaveUserStateActivity → public.users (upsert)
                      └─ WriteAuditEventActivity → audit.events (append)
                              ↓
                        Temporal UI (audit history visible at localhost:8233)
```

## Key Concepts

**Entity Pattern**
One long-running workflow is created per entity and identified by a stable, deterministic workflow ID (e.g. `user-{uuid}`). The workflow acts as the authoritative coordinator for all mutations to that entity, ensuring changes are processed serially and durably regardless of API restarts or transient failures.

**Update Handlers**
Temporal Update is a synchronous request/response mechanism: the caller sends a named update to a running workflow and blocks until the workflow acknowledges it. In this project, every profile-change request is delivered as an update, so the HTTP response is only sent after both the `SaveUserState` and `WriteAuditEvent` activities have completed successfully.

**ContinueAsNew**
Temporal workflows accumulate an event history that is replayed on recovery. To prevent this history from growing without bound, the `UserWorkflow` calls `ContinueAsNew` after processing 100 updates, creating a fresh workflow execution that carries forward only the current state. All audit history remains intact in PostgreSQL, unaffected by the reset.

**Immutable Audit Log**
The `audit.events` table is never updated or deleted — only appended to. Each row captures the resource type, the actor, the wall-clock timestamp, and full JSONB snapshots of the resource before and after the change. This makes the audit trail queryable with standard SQL and independent of the Temporal event history.
