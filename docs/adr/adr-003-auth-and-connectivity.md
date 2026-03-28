## Authentication Strategy and Calendar OAuth Connectivity

**Status:** Proposed

**Deciders:** Team (Practice 4)

**[Initiative](Practice 4 — Modular Monolith Baseline)**

## Problem Statement

CalendarSync needs two distinct identity concepts: (1) knowing *who the user is* (authentication) and (2) *accessing their calendars* on external providers (calendar OAuth). These are often conflated — "log in with Google" and "connect your Google Calendar" look similar but have different lifecycles. A user might revoke calendar access without logging out, or connect multiple Google accounts under one login. We need to decide how to handle both concerns across practices 4–8 without overbuilding now.

## Decision Drivers

- Practice 4 has no UI — browser-based OAuth redirects don't apply
- Both modules need a `UserID` to scope data ownership, but neither needs user profile data (name, email, avatar)
- Calendar OAuth tokens are sensitive secrets with rotation and expiry logic — they belong in the Connections module
- Authentication will evolve: fake in Practice 4, API Gateway in Practice 5, possibly JWT in later practices
- The solution must not couple calendar connectivity to user authentication — they have different lifecycles
- Infrastructure is not tied to GCP — Google Cloud project is only needed for Calendar API credentials, deployment can be anywhere (AWS, local, etc.)

## Considered Options

### **Option A — User entity + Identity module**

Create a third module (`Identity`) with a `User` aggregate. Handle authentication and user management as a first-class bounded context. Both Connections and Sync reference `UserID` from the Identity module.

**Pros:**
- Standard approach for production systems
- Clear ownership of auth concerns

**Cons:**
- `User` entity would have near-zero domain logic in Practice 4 — no registration, no profile, no password, no sessions
- Third module in a 1-week practice violates "start simple" guidance
- ADR-001 already decided on two modules; adding a third contradicts that decision

### **Option B — Shared UserID value object, no user entity**

Define `UserID` as a typed UUID in the `shared/` package. Both modules store it as an owner field on their aggregates. No user entity, no identity module. Authentication is handled externally to the domain:

- Practice 4: `X-User-ID` request header (API layer extracts it, no validation)
- Practice 5: API Gateway validates tokens, injects trusted `X-User-ID` header
- Later: JWT middleware, session management — all in API/gateway layer, never in domain

**Pros:**
- Zero overhead — no entity, no module, no migration for something with no domain logic
- Both modules are testable with any UserID — no auth dependency in tests
- Authentication strategy can evolve without touching domain code
- Matches the reality: neither module cares *who* the user is, only that data is scoped by owner

**Cons:**
- No user-level invariants (e.g., "free tier users limited to 3 sync rules") — these would need to live in the Sync module or be added later
- `X-User-ID` header in Practice 4 is trivially fakeable — acceptable for a non-production exercise

### **Option C — Embed user management in Connections module**

Since the Connections module already handles OAuth with Google, extend it to also own user identity — "connecting Google" creates both the user session and the calendar connection.

**Pros:**
- Single OAuth flow for both auth and calendar access

**Cons:**
- Conflates two lifecycles: revoking calendar access would log the user out
- Connecting a second Google account becomes an identity crisis — which one is "the user"?
- Makes Connections module responsible for too many concerns — violates single responsibility

## Decision

We decided for **Option B — Shared UserID, no user entity** because neither module has domain logic that depends on user attributes. Authentication is a cross-cutting infrastructure concern that belongs in the API/gateway layer, not in the domain.

### Authentication evolution path

```mermaid
flowchart LR
    subgraph "Practice 4"
        C4["Client"] -->|"X-User-ID header"| API4["API handlers"]
        API4 -->|"UserID"| App4["Use cases"]
    end

    subgraph "Practice 5"
        C5["Client"] -->|"Bearer token"| GW["API Gateway"]
        GW -->|"validates token,\ninjects X-User-ID"| API5["Service APIs"]
        API5 -->|"UserID"| App5["Use cases"]
    end
```

### Calendar OAuth flow (Practice 4, no UI)

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant API
    participant ConnApp as Connections application
    participant Google as Google OAuth

    Dev->>API: POST /connections/google/auth-url
    API->>ConnApp: GenerateAuthURL()
    ConnApp-->>API: consent URL
    API-->>Dev: {"url": "https://accounts.google.com/..."}

    Dev->>Dev: open URL in browser, consent
    Dev->>API: POST /connections/google/callback {"code": "..."}
    API->>ConnApp: CompleteOAuth(userID, authCode)
    ConnApp->>Google: exchange code for tokens
    Google-->>ConnApp: access_token + refresh_token
    ConnApp->>Google: list calendars
    Google-->>ConnApp: calendar list with access roles
    ConnApp->>ConnApp: map access roles → SourceCapability
    ConnApp->>ConnApp: create CalendarConnection aggregate
    ConnApp-->>API: ConnectionID
    API-->>Dev: 201 Created
```

This is Google's "installed application" OAuth flow — the developer manually opens the consent URL, pastes the code back. No browser redirect callback needed. The Go binary doesn't need to serve a callback endpoint for the redirect; the code is submitted via a regular POST.

**GCP vs deployment infrastructure:** The Google Cloud project is only needed to register OAuth client credentials (client ID + secret). It's a free registration step. The actual CalendarSync binary, database, and Docker containers run on any infrastructure — AWS, local machine, university lab. The Go code makes outbound HTTPS calls to `googleapis.com` like any external REST API.

### Expected Benefits

- Zero boilerplate for a concept with no domain logic
- Authentication strategy evolves in the API layer only — domain code is stable across practices
- Calendar OAuth is cleanly owned by Connections module, decoupled from user identity
- CLI-style OAuth flow works without a UI, with real Google tokens and real API calls

### Accepted Downsides

- No user-level business rules (plan limits, usage quotas) until a `User` entity is introduced. Acceptable — no such rules exist in Practice 4 scope.
- `X-User-ID` header is unsecured in Practice 4. Acceptable — this is a development exercise, not production. API Gateway in Practice 5 adds real validation.

---

**Previous version:** n/a — initial architecture decision
