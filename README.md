# Syncify

Automatically keeps a user's availability accurate across multiple calendar platforms by detecting events and blocking time on target calendars. For example, a student with a university lecture schedule and a work calendar no longer needs to manually copy lectures as busy blocks — Syncify handles it based on configurable rules.

## Theme

| | |
|---|---|
| **Theme** | Cross-platform calendar availability sync |
| **Core Item** | `SyncRule` — defines how events flow from a source calendar to a target calendar |
| **Core Action** | `CreateSyncRule` — configure a one-way sync with filters and visibility settings |
| **Workflow Action** | `ActivateSyncRule` — multi-step validation across modules before enabling sync |

## Domain rules

### Connections module (`CalendarAccount` aggregate)

| # | Rule |
|---|------|
| C1 | Token expiry drives status — expired token transitions connection to `Expired` |
| C2 | `Revoked` is terminal — cannot transition back to `Active` or `Expired` |
| C3 | Credential refresh requires `Active` or `Expired` status |
| C4 | Calendar list refresh requires `Active` status |
| C5 | Max connections per user per provider (enforced in application layer) |

Status transitions: `Active ⇄ Expired`, `Active/Expired → Revoked` (terminal)

### Sync module (`SyncRule` aggregate)

| #  | Rule |
|----|------|
| S1 | Source and target must reference different calendars |
| S2 | `FreeBusyOnly` source access → keyword filters forbidden |
| S3 | `FreeBusyOnly` source access → `copyTitle` must be false |
| S4 | TimeWindow: `startHour < endHour`, weekdays non-empty when set |
| S5 | Creation requires `CalendarAccess` for both source and target — rule starts `Active` |
| S6 | Target must have `ReadWrite` access |
| S7 | Archiving clears `syncCursor` — next resume triggers full re-sync |

Status transitions: `Active ⇄ Archived` (terminal: none — archiving is reversible via resume)

## Initial Architecture

Modular monolith — two bounded contexts in a single deployable binary, each with Clean Architecture layering (domain → application → infrastructure). Modules communicate through an explicit facade interface, never by accessing each other's internals.

See `docs/adr/` for detailed decisions: bounded context split (ADR-001), provider abstraction (ADR-002), auth & OAuth strategy (ADR-003), aggregate design & domain rules (ADR-004).

### Why modular monolith first?

We intentionally start with a modular monolith rather than microservices because we understand the domain the least at the beginning. A monolith with clear module boundaries lets us: delay irreversible infrastructure decisions (message brokers, service mesh, distributed tracing), keep business logic understandable in one codebase, and validate the bounded context split before paying the operational cost of distributed deployment. The module boundaries are designed so that in Practice 5, the Connections module can be extracted into its own service by replacing the in-process facade call with an HTTP client — no domain or application layer changes required.

## Prerequisites

- .NET 10.0 SDK
- Docker
- A Google Cloud project with Calendar API enabled and OAuth 2.0 credentials (installed application type)
- PostgreSQL 16+

## Helpful Commands

### Configuration
Generate a valid 32-byte Base64 encryption key (required for `Encryption:Key`):
```bash
openssl rand -base64 32
```

### Docker Management
```bash
# View running containers
docker ps

# View application logs
docker logs -f syncify-app

# Stop and remove all project containers
docker stop syncify-app syncify-db
docker rm syncify-app syncify-db
```

### Database (PostgreSQL)
```bash
# Connect to the database via psql (inside container)
docker exec -it syncify-db psql -U syncify -d syncify

# List all tables
# \dt

# View migration history
# SELECT * FROM "__EFMigrationsHistory";
```

## Run locally

```bash
# 1. Start PostgreSQL (if not running)
docker run -d --name syncify-db \
  -e POSTGRES_USER=syncify \
  -e POSTGRES_PASSWORD=syncify \
  -e POSTGRES_DB=syncify \
  -p 5432:5432 \
  postgres:16

# 2. Configure secrets in appsettings.Development.json
#    - Google:ClientId / Google:ClientSecret
#    - Encryption:Key (base64-encoded 32-byte key)

# 3. Run
# Migrations run automatically in Development environment.
# To run ONLY migrations, use:
# dotnet run --project src/Syncify.Api -- --migrate

dotnet run --project src/Syncify.Api
```

The API starts at `http://localhost:5030` by default.

## Run with Docker

```bash
# Build the image
docker build -t syncify .

# Run (make sure to fill in .env)
docker run -p 8080:8080 \
  --env-file .env \
  -e ASPNETCORE_ENVIRONMENT=Development \
  syncify
```

The containerized API listens on port 8080.

## Tests

```bash
# Run all tests
dotnet test syncify.sln

# Run specific test project
dotnet test tests/Syncify.Connections.Domain.Tests
dotnet test tests/Syncify.Sync.Domain.Tests
dotnet test tests/Syncify.Connections.Application.Tests
dotnet test tests/Syncify.Sync.Application.Tests
dotnet test tests/Syncify.Api.Tests          # requires Docker (Testcontainers)
```

## API Examples

All endpoints expect an `X-User-ID` header (UUID) for user identification.

```bash
BASE=http://localhost:5030
USER_ID="00000000-0000-0000-0000-000000000001"

# --- Connections ---

# Generate Google OAuth URL
curl -s -X POST $BASE/connections/google/auth-url

# Complete OAuth callback
curl -s -X POST $BASE/connections/google/callback \
  -H "Content-Type: application/json" \
  -H "X-User-ID: $USER_ID" \
  -d '{"code": "auth-code-from-google"}'

# List connections
curl -s $BASE/connections -H "X-User-ID: $USER_ID"

# List calendars for a connection
curl -s $BASE/connections/{accountId}/calendars

# Revoke a connection
curl -s -X DELETE $BASE/connections/{accountId}

# --- Sync Rules ---

# Create a sync rule
curl -s -X POST $BASE/sync-rules \
  -H "Content-Type: application/json" \
  -H "X-User-ID: $USER_ID" \
  -d '{
    "sourceCalendarId": "...",
    "targetCalendarId": "...",
    "copyTitle": false,
    "customTitle": "Busy",
    "filterPolicy": { "criteria": [] }
  }'

# Get a sync rule
curl -s $BASE/sync-rules/{id}

# List sync rules
curl -s $BASE/sync-rules -H "X-User-ID: $USER_ID"

# Archive a sync rule
curl -s -X POST $BASE/sync-rules/{id}/archive

# Resume a sync rule
curl -s -X POST $BASE/sync-rules/{id}/resume

# Update filter policy
curl -s -X PATCH $BASE/sync-rules/{id}/filter \
  -H "Content-Type: application/json" \
  -d '{"filterPolicy": {"criteria": []}}'

# Update title settings
curl -s -X PATCH $BASE/sync-rules/{id}/title \
  -H "Content-Type: application/json" \
  -d '{"copyTitle": true, "customTitle": ""}'

# Trigger manual sync execution
curl -s -X POST $BASE/sync-rules/{id}/execute

# --- Health ---

# Health check
curl -s $BASE/health
```

## Team workflow

1. Each team member works on a separate feature branch
2. Open a PR against `main` when ready for review
3. Domain and application tests must pass before merge
4. Integration tests (`Syncify.Api.Tests`) run against a real PostgreSQL via Testcontainers

PRs: https://github.com/iravelmakina/syncify/pulls?q=is%3Apr+is%3Aclosed

## Programming sessions

| Date | Focus | What was done |
|---|---|---|
| 28 March | Architecture brainstorming and design | Bounded context split, aggregate design, ADRs |
| 5 April | Practice 4 | Infrastructure, API endpoints, domain/application tests |
| 7 April | Practice 4 refinement and testing | Code review fixes, migrations, Docker, README |
