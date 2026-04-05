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

- .NET 9.0 SDK
- Docker & Docker Compose
- A Google Cloud project with Calendar API enabled and OAuth 2.0 credentials (installed application type)

## Run locally
TBA

## Run with Docker
TBA

## Tests
TBA

## API Examples
TBA

## Team workflow
TBA
