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

## Architecture (Practice 6 — Event-Driven Microservices)

The system uses four independent services with an API Gateway and asynchronous event-driven communication via RabbitMQ:

```
Client
  │
  ▼
┌──────────────┐
│   Gateway    │  YARP reverse proxy
│  (port 5000) │  /connections/* → connections-service
│              │  /sync-rules/*  → sync-service
│              │  /health        → self
└──────┬───────┘
       │
  ┌────┴─────────────────┐
  ▼                      ▼
┌────────────┐  ┌─────────────────┐
│Connections │  │  Sync Service   │
│  Service   │  │     :8082       │
│   :8081    │  │                 │
└─────┬──────┘  └──┬──────┬───────┘
      │            │      │
      ▼            │      │ Event: SyncRuleCreated
┌──────────┐       │      │ (Transactional Outbox)
│connections│       │      ▼
│   -db    │       │  ┌──────────┐
│(Postgres)│       │  │ RabbitMQ │
└──────────┘       │  │  :5672   │
                   │  │  :15672  │ (Management UI)
                   │  └────┬─────┘
                   │       │
                   ▼       ▼
              ┌──────────┐ ┌─────────────────────┐
              │  sync-db │ │ Notifications Service│
              │(Postgres)│ │       :8083         │
              └──────────┘ └──────────┬──────────┘
                                      │
                                      ▼
                              ┌───────────────┐
                              │notifications-db│
                              │   (Postgres)   │
                              └───────────────┘

HTTP: Sync → Connections
  GET /internal/calendars/{id}/access
  GET /internal/calendars/{id}/token

Events: Sync → RabbitMQ → Notifications
  SyncRuleCreatedEvent (via Outbox pattern)
```

### Service Responsibilities

| Component | Responsibility | Port | Database |
|---|---|---|---|
| **Gateway** | Routes `/connections/*` and `/sync-rules/*`, adds `X-Correlation-Id`, forwards `X-User-ID` | 5000 | None |
| **Connections Service** | OAuth, calendar accounts, calendar listing, internal endpoints for Sync | 8081 | connections-db (port 5433) |
| **Sync Service** | Sync rules, polling, execution — calls Connections via HTTP, publishes events via Outbox | 8082 | sync-db (port 5434) |
| **RabbitMQ** | Message broker for asynchronous event-driven communication | 5672, 15672 | None |
| **Notifications Service** | Consumes `SyncRuleCreated` events, stores notifications with idempotency | 8083 | notifications-db (port 5435) |

### Why event-driven architecture?

Event-driven communication decouples services in time and space:
- **Temporal decoupling**: Sync Service doesn't wait for Notifications — publishes and continues
- **Service independence**: Notifications can be down without affecting sync rule creation
- **Extensibility**: New consumers can subscribe to `SyncRuleCreated` without changing Sync Service
- **Reliability**: Transactional Outbox ensures no events are lost even if RabbitMQ is down

The system evolved from modular monolith (Practice 4) → microservices (Practice 5) → event-driven microservices (Practice 6). Each phase validated architectural decisions before adding complexity.

See `docs/adr/` for detailed decisions: bounded context split (ADR-001), provider abstraction (ADR-002), auth & OAuth strategy (ADR-003), aggregate design & domain rules (ADR-004).

## Service Degradation Behavior

### When Connections Service is Down

| Operation | Behavior |
|---|---|
| Create sync rule | ❌ Returns 503 Service Unavailable (cannot validate calendar access) |
| Resume archived rule | ❌ Returns 503 Service Unavailable (cannot validate calendar access) |
| Execute sync | ❌ Returns 503 Service Unavailable (cannot fetch fresh OAuth token) |
| List sync rules | ✅ Still works (no cross-service call needed) |
| Get sync rule | ✅ Still works (no cross-service call needed) |
| Archive sync rule | ✅ Still works (no cross-service call needed) |

### When Sync Service is Down

| Operation | Behavior |
|---|---|
| All Connections endpoints | ✅ Still work (Connections is independent) |
| All Sync endpoints | ❌ Return 502 Bad Gateway or timeout |

### When Gateway is Down

All endpoints become unreachable from external clients. Services can still communicate internally (Sync → Connections).

## Event-Driven Architecture (Practice 6)

Syncify uses **RabbitMQ** for asynchronous communication with the **Transactional Outbox pattern** for guaranteed event delivery.

### Architecture

- **Sync Service**: Publishes events via Outbox when sync rules are created
- **RabbitMQ**: Message broker (ports 5672, 15672) — decouples services
- **Notifications Service**: Consumes events and stores notifications

### Published Events

| Event | When | Consumer |
|---|---|---|
| `SyncRuleCreated` | After sync rule created | Notifications Service |

### Outbox Pattern

Events are written to the `OutboxMessage` table in the same transaction as sync rules. A background worker publishes to RabbitMQ every 10 seconds, ensuring guaranteed delivery even if RabbitMQ is temporarily down.

**Why Outbox?**
- ✅ Atomic: Event and sync rule creation in single database transaction
- ✅ Reliable: No lost events even if message broker is down
- ✅ At-least-once delivery: MassTransit handles retries

### Notification Model

The Notifications service uses an extensible model that supports multiple event types without schema changes:

- **`EventType`**: Discriminator for filtering by event type (e.g., "SyncRuleCreated")
- **`Payload`**: Full event serialized as JSONB for debugging and auditing
- **`IsRead`**: Future notification UI support
- **Idempotency**: `EventId` as primary key prevents duplicate notifications

### Failure Scenarios

| Scenario | Behavior |
|---|---|
| RabbitMQ down | Events stay in Outbox, published when RabbitMQ recovers |
| Notifications Service down | Events queue in RabbitMQ, consumed when service recovers |
| Notifications DB down | Consumer retries with exponential backoff, eventually DLQ |
| Duplicate event | Ignored via unique constraint on `EventId` (PostgreSQL error 23505) |

## Prerequisites

- .NET 10.0 SDK
- Docker & Docker Compose
- A Google Cloud project with Calendar API enabled and OAuth 2.0 credentials (installed application type)

## Quick Start (Docker Compose)

This is the recommended way to run the full system with all services:

```bash
# 1. Copy the environment template
cp .dev/.env.example .dev/.env

# 2. Edit .dev/.env with your Google OAuth credentials
# GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
# GOOGLE_CLIENT_SECRET=your-client-secret
# ENCRYPTION_KEY=dev-key-replace-in-production-32b

# 3. Start all services
cd .dev
docker compose up --build

# The gateway will be available at http://localhost:5000
```

### Service URLs

| Service | URL | OpenAPI |
|---|---|---|
| **Gateway** (main entry point) | http://localhost:5000 | N/A |
| Connections Service | http://localhost:8081 | http://localhost:8081/scalar/v1 |
| Sync Service | http://localhost:8082 | http://localhost:8082/scalar/v1 |
| Notifications Service | http://localhost:8083/health | N/A |
| RabbitMQ Management UI | http://localhost:15672 | guest/guest |
| Connections DB | localhost:5433 | psql -h localhost -p 5433 -U connections -d connections |
| Sync DB | localhost:5434 | psql -h localhost -p 5434 -U sync -d sync |
| Notifications DB | localhost:5435 | psql -h localhost -p 5435 -U notifications -d notifications |

> All endpoints expect an `X-User-ID` header (UUID) for user identification. For testing, you can use:
> ```
> 00000000-0000-0000-0000-000000000001
> ```

## Interactive API Documentation

Each service provides interactive API documentation via Scalar UI:

- **Connections Service**: http://localhost:8081/scalar/v1
  - OAuth endpoints (`/connections/google/*`)
  - Calendar account management (`/connections`)
  - Calendar listing (`/connections/{accountId}/calendars`)
  - Internal endpoints (`/internal/calendars/*`) — for service-to-service communication

- **Sync Service**: http://localhost:8082/scalar/v1
  - Sync rule CRUD (`/sync-rules`)
  - Sync execution (`/sync-rules/{id}/execute`)
  - Archive/Resume (`/sync-rules/{id}/archive`, `/sync-rules/{id}/resume`)

- **Gateway**: Routes client traffic to the above services
  - `/connections/*` → Connections Service
  - `/sync-rules/*` → Sync Service
  - `/health` → Gateway self-health

> **Note**: When testing via Scalar UI, you can use the default test user ID:
> ```
> 00000000-0000-0000-0000-000000000001
> ```
> This should be provided in the `X-User-ID` header for all endpoints.

## Filter Policy JSON

`filterPolicy.criteria` accepts all implementations of `IFilterCriterion` from the domain value objects.
- `type` must be one of `excludes`, `keywords`, or `timeWindow`
- `excludes` must contain at least one value
- `keywords` must contain between 1 and 20 values
- `startHour` and `endHour` must be between `0` and `23`, and `startHour` must be less than `endHour`
- `weekdays` must contain at least one `DayOfWeek` value serialized as integers (`1` = Monday, ..., `5` = Friday)

Example with all currently supported criteria:

```json
{
  "filterPolicy": {
    "criteria": [
      {
        "type": "excludes",
        "excludes": [
          "out of office",
          "holiday"
        ]
      },
      {
        "type": "keywords",
        "keywords": [
          "meeting",
          "call"
        ]
      },
      {
        "type": "timeWindow",
        "startHour": 9,
        "endHour": 17,
        "weekdays": [1, 2, 3, 4, 5]
      }
    ]
  }
}
```

Single-criterion example:

```json
{
  "filterPolicy": {
    "criteria": [
      {
        "type": "excludes",
        "keywords": ["busy"]
      }
    ]
  }
}
```

## Development (Local)

To run services individually for development:

### Connections Service

```bash
# Start PostgreSQL
docker run -d --name connections-db \
  -e POSTGRES_USER=connections \
  -e POSTGRES_PASSWORD=connections \
  -e POSTGRES_DB=connections \
  -p 5433:5432 \
  postgres:17

# Run service
cd src/Syncify.Connections.Api
dotnet run
# Available at http://localhost:8081
```

### Sync Service

```bash
# Start PostgreSQL
docker run -d --name sync-db \
  -e POSTGRES_USER=sync \
  -e POSTGRES_PASSWORD=sync \
  -e POSTGRES_DB=sync \
  -p 5434:5432 \
  postgres:17

# Run service (requires Connections Service running)
cd src/Syncify.Sync.Api
dotnet run
# Available at http://localhost:8082
```

### Gateway

```bash
# Requires both Connections and Sync services running
cd src/Syncify.Gateway
dotnet run
# Available at http://localhost:5000
```

## Tests

```bash
# Run all tests
dotnet test syncify.sln

# Run specific test projects
dotnet test tests/Syncify.Connections.Domain.Tests
dotnet test tests/Syncify.Sync.Domain.Tests
dotnet test tests/Syncify.Connections.Application.Tests
dotnet test tests/Syncify.Sync.Application.Tests
dotnet test tests/Syncify.Sync.Infrastructure.Tests  # includes HttpConnectionService tests
dotnet test tests/Syncify.Sync.Api.Tests             # requires Docker (Testcontainers)
```

Total test coverage: 16+ tests across domain, application, infrastructure, and integration layers.

## Helpful Commands

### Docker Compose Management

```bash
# View all running containers
docker ps

# View logs for a specific service
docker logs -f connections-service
docker logs -f sync-service
docker logs -f notifications-service
docker logs -f gateway
docker logs -f rabbitmq

# Stop all services
docker compose down

# Stop and remove volumes (fresh start)
docker compose down -v

# Rebuild a specific service
docker compose up --build connections-service
```

### Database Access

```bash
# Connect to Connections DB
docker exec -it connections-db psql -U connections -d connections

# Connect to Sync DB
docker exec -it sync-db psql -U sync -d sync

# Connect to Notifications DB
docker exec -it notifications-db psql -U notifications -d notifications

# View migration history
# SELECT * FROM "__EFMigrationsHistory";

# List all tables
# \dt

# Check notifications (for Practice 6 testing)
# SELECT event_id, event_type, user_id, summary, created_at
# FROM notifications ORDER BY created_at DESC LIMIT 5;
```

### Configuration

Generate a valid 32-byte Base64 encryption key (required for `Encryption:Key`):
```bash
openssl rand -base64 32
```

## Team workflow

1. Each team member works on a separate feature branch
2. Open a PR against `main` when ready for review
3. Domain and application tests must pass before merge
4. Integration tests (`Syncify.Sync.Api.Tests`) run against a real PostgreSQL via Testcontainers

PRs: https://github.com/iravelmakina/syncify/pulls?q=is%3Apr+is%3Aclosed

## Programming sessions

| Date | Focus | What was done |
|---|---|---|
| 28 March | Architecture brainstorming and design | Bounded context split, aggregate design, ADRs |
| 5 April | Practice 4 | Infrastructure, API endpoints, domain/application tests |
| 7 April | Practice 4 refinement and testing | Code review fixes, migrations, Docker, README |
| 12-14 April | Practice 5 — Microservices extraction | Gateway setup, Connections service extraction, HTTP facade implementation, Docker Compose |
| 17-18 April | Practice 6 — Event-Driven Architecture | Transactional Outbox pattern, RabbitMQ setup, Notifications service, event consumer with idempotency |
