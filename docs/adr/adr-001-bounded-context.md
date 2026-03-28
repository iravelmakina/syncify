## Bounded Context Decomposition

**Status:** Proposed

**Deciders:** Team (Practice 4)

**[Initiative](Practice 4 — Modular Monolith Baseline)**

## Problem Statement

CalendarSync automatically keeps a user's availability accurate across multiple calendar platforms by detecting events and blocking time on target calendars. The system must be built as a modular monolith with at least two bounded contexts that can evolve into independently deployable microservices in Practices 5–8. We need to decide where to draw the module boundary.

## Decision Drivers

- Each module must own its data — no cross-module table access (prerequisite for database-per-service in Practice 5)
- The boundary must reflect a real domain seam, not an artificial technical split
- Both modules must carry enough domain logic for 3+ business rules each
- The split must produce a natural extraction candidate for Practice 5, event boundary for Practice 6, and Saga workflow for Practice 7
- Practice 4 is a 1-week pair exercise — keep it to two modules, no more

## Considered Options

### **Option A — Calendar Connections + Sync Rules**

Split by what they integrate with. **Connections** owns OAuth tokens, provider credentials, connected accounts, and the calendar catalog with access roles. **Sync Rules** owns rule configuration — filters, visibility modes, status transitions — and references calendars via a lightweight `CalendarRef` (connectionID + calendarID).

**Pros:**
- Clean security boundary — tokens and provider secrets never leak into business rule logic
- Connections is the natural Practice 5 extraction target (external API deps, different scaling profile)
- Practice 6 events are obvious: `ConnectionRevoked`, `AccessRoleChanged` → Sync re-validates affected rules
- Practice 7 Saga: `ActivateSyncRule` → verify source accessible → verify target writable → activate
- In Go, Google API client dependency stays isolated in one module

**Cons:**
- Connections domain layer is thinner than Sync Rules — mostly validation (token expiry, connection status) rather than rich business logic
- `AccessRole` in Sync's `CalendarRef` originates from Connections; stale data requires event-based re-validation

### **Option B — Sync Rules + Sync Execution**

Split by lifecycle phase. **Sync Rules** owns design-time configuration (CRUD, filters, status). **Sync Execution** owns runtime — fetching events, applying filters, creating blocks, tracking what was synced. Its aggregate is `SyncJob`.

**Pros:**
- Clean separation of configuration vs. runtime with different change rates
- `SyncJob` has real invariants: idempotency, conflict detection, failure tracking
- Each sync execution is already a multi-step workflow — natural Practice 7 Saga

**Cons:**
- Sync Execution has nearly zero code in Practice 4 — no async runtime exists until Practice 6+
- Both modules need calendar provider access — OAuth tokens have no clear owner, violating data ownership or requiring a third module
- Execution's domain logic is procedural (fetch → filter → write), making 3+ domain rules a stretch

### **Option C — Connections + Sync Rules + Sync Execution**

Full three-way decomposition combining A and B.

**Pros:**
- Cleanest separation — each module has a single reason to change
- Maps perfectly to scaling profiles: Connections (API rate limits), Rules (user traffic), Execution (event volume)

**Cons:**
- Three modules in a 1-week practice is overengineering — the course warns "start simple"
- Execution is an empty skeleton in Practice 4
- Nine packages minimum (3 × domain/application/infrastructure) — excessive boilerplate for the learning value

### **Option D — Identity + Sync**

Split by actor. **Identity** owns user accounts, authentication, OAuth flows, linked accounts. **Sync** owns everything calendar-specific — rules, filters, connections, execution.

**Pros:**
- Universally understood split — identity is almost always its own bounded context
- Identity has clear domain rules: email uniqueness, token expiry, account linking limits

**Cons:**
- Identity is a generic concern, not specific to calendar sync — teaches less about DDD in this domain
- Calendar-specific concepts (access roles, provider differences, calendar catalog) get jammed into a bloated Sync module that is hard to extract cleanly in Practice 5
- Weak Practice 6 event boundary — `UserCreated` is trivial; the interesting events (`AccessRoleChanged`, `ConnectionRevoked`) require calendar-awareness that lives in Sync, not Identity

## Decision

We decided for **Option A — Connections + Sync Rules** because it is the only option that satisfies all decision drivers without overengineering:

- Two modules with genuine data ownership (tokens/credentials vs. business policies)
- A real domain seam — different change rates, security profiles, and scaling needs
- Both modules carry enough domain logic for 3+ rules each
- Strongest Practice 5–7 evolution path (extraction, events, Saga)
- Fits the 1-week scope

### Expected Benefits

- OAuth tokens isolated from business rule logic — clean security boundary from day one
- Provider-agnostic design enforced structurally: Sync depends on Connections facade, never on Google types
- Inter-module facade becomes the service API in Practice 5 with minimal refactoring
- Each module is independently testable — Sync Rules works against a mock Connections facade

### Accepted Downsides

- Connections domain layer is thinner than Sync Rules. Mitigated by ensuring meaningful domain rules (token expiry, connection status machine, calendar access verification) rather than pure pass-through.
- `AccessRole` staleness in Sync's `CalendarRef`. Mitigated by re-checking via facade at activation time (monolith); via `AccessRoleChanged` events (microservices).

---

**Previous version:** n/a — initial architecture decision
