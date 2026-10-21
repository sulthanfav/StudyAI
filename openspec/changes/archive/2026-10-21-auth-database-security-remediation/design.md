# Design

## Context

See `proposal.md` for motivation. Iteration 4 already has ASP.NET Core Identity Core, a custom EF-backed user store, cookie endpoints, one primary `StudyAIDbContext`, transaction-local RLS support, and real PostgreSQL tests. The remediation must preserve accepted ADR-001, ADR-003, and ADR-016, avoid the dirty frontend checkout, and correct applied environments through forward changes.

The original authentication OpenSpec change remains unarchived and overstates several controls. This change adds the missing observable contracts rather than treating its checked task list as evidence.

## Goals / Non-Goals

**Goals:**

- Make browser authentication safe and deterministic before Next.js integration.
- Revoke one logged-out session without unnecessarily revoking other devices, while retaining immediate all-device revocation for password or account-state changes.
- Make supported database bootstrap reproducible without known passwords or ambient default ACLs.
- Keep authorization, actor context, Data Space context, and RLS within an explicit transaction lifetime.
- Produce adversarial evidence on fresh PostgreSQL and repeated full-suite runs.

**Non-Goals:**

- Implement the Next.js application or choose a hosting vendor.
- Introduce OAuth/OIDC or mobile tokens.
- Rewrite Git history, silently replace applied migrations, or mutate non-isolated databases.
- Expand RLS to unrelated tables or implement unrelated Phase 3 modules.

## Decisions

### 1. Same-origin proxy is the default web contract

Next.js exposes a same-origin `/api` route that proxies an allowlisted path set to ASP.NET Core. Development uses the Next.js origin with an internal API base URL; production assumes one public HTTPS origin with path routing. Browser code never chooses the upstream host.

This avoids credentialed CORS as the default and preserves `SameSite=Lax`. Direct CORS remains an explicit configuration mode with exact origins, credentials, methods, and headers; no wildcard is allowed. A BFF that owns separate application sessions was rejected because it duplicates the existing ASP.NET Core session boundary.

### 2. Antiforgery is mandatory for every cookie mutation

`GET /api/v1/auth/csrf` issues the readable antiforgery cookie and request token. Register, login, logout, and future protected mutations validate `X-XSRF-TOKEN`; missing or invalid pairs fail before the handler. Integration tests use a helper that obtains and submits a real pair rather than disabling antiforgery in the test host.

### 3. Rate limiting and account lockout remain separate controls

ASP.NET Core rate limiting partitions sensitive endpoints by normalized direct peer address plus endpoint policy, with bounded queueing disabled and a stable 429 Problem Details response. Forwarded headers influence the peer address only when trusted proxies/networks are explicitly configured. Account lockout continues to protect a known account; rate limiting protects the service and unknown-account paths.

### 4. Sessions have a server-side registry

A new identity session table stores a random session identifier hash, user ID, security-stamp snapshot, issued time, absolute expiry, revoked time, and last-seen metadata that is safe to retain. The cookie contains the opaque session identifier claim; it never contains the database hash.

Cookie principal validation checks session existence, revocation, absolute expiry, current user status/deletion, and security stamp. Current-session logout revokes only that session row before clearing the browser cookie. Password changes and account suspension/deletion update the security stamp and revoke all active session rows for the user. Per-request validation is selected to satisfy immediate revocation; later caching requires a separate reviewed change because it weakens that guarantee.

Pure cookie deletion and updating the security stamp on every logout were rejected: the first permits replay, while the second logs out unrelated devices.

### 5. Registration produces one normalized identity and one public error

Email normalization is stored in `normalized_email` and protected by a PostgreSQL unique index. A migration preflight detects collisions and aborts without selecting a winner. Both Identity validation failures for duplicate email and PostgreSQL unique violations become one application result mapped to HTTP 409 with a stable public code.

The application pre-check remains a user-friendly fast path, not the correctness boundary. The database constraint is authoritative. Custom UserStore update/delete methods map optimistic-concurrency exceptions to Identity concurrency failures instead of throwing raw EF exceptions.

### 6. Role bootstrap is outside EF migrations

The supported bootstrap creates database roles and supplies passwords from environment-specific secret input before EF migrations run. Because the historical baseline migration conditionally creates roles, pre-creation prevents its known literal from becoming active on a clean bootstrap. Existing environments must rotate the exposed runtime and migration credentials before application rollout.

The historical migration is not edited in this change. Directly running it as a privileged user without the bootstrap remains unsupported and is documented as unsafe. Removing the literal from that already-applied migration would alter historical source and requires explicit Product Owner approval plus rebuild instructions for every disposable environment.

### 7. Forward migration owns schema and privilege remediation

A new migration adds identity sessions, the normalized-email unique index, and the membership role CHECK constraint. Administrative bootstrap SQL owns role creation and secret rotation. Reviewed grant SQL explicitly grants required schema/table/sequence operations and revokes runtime access to migration history and unapproved objects; it does not rely on cluster default ACLs.

Production rollback remains forward-fix plus snapshot/PITR per ADR-016. Development clean-room tests may destroy only their uniquely named disposable container and volume.

### 8. Authorization and tenant work share one explicit transaction

The Data Space endpoint filter begins a transaction before authorization, sets transaction-local actor context, performs the membership lookup, sets transaction-local Data Space context only after authorization, invokes the endpoint, then commits on success or rolls back on failure. The current context-setting call outside a transaction is removed.

Membership authorization uses the actor-scoped membership row and evaluates the required role for the operation. Membership RLS can use transaction-local actor context without trusting a caller-supplied Data Space ID. The endpoint may not create another independent transaction; application use cases consume the existing unit of work.

### 9. Verification uses disposable PostgreSQL plus repeated runs

A repository script starts a uniquely named PostgreSQL/pgvector container without reusing the persistent development volume, supplies ephemeral credentials, runs bootstrap and migrations, executes permission probes and integration tests, and removes only its own resources. The normal full suite is run repeatedly after focused tests; all repetitions must pass before stability is claimed.

## Risks / Trade-offs

- **Per-request session lookup adds one database query** → Keep the query indexed and narrow; correctness takes precedence until a separately reviewed bounded cache exists.
- **Existing normalized-email collisions block deployment** → Provide a read-only preflight report and require Product Owner-directed data resolution; never merge accounts automatically.
- **Historical migration still contains an exposed literal** → Pre-create/rotate roles so it is unusable, mark direct migration execution unsupported, and request approval before any historical rewrite.
- **A proxy can accidentally broaden access** → Allowlist paths, methods, and forwarded headers; reject dynamic upstream URLs and test status/header/cookie preservation.
- **Transaction filter can conflict with nested transactions** → Make one request-scoped owner of the transaction and fail clearly if an endpoint attempts an incompatible nested transaction.
- **Role permissions can be too narrow** → Derive the allowlist from approved workflows and prove both allowed operations and denied administrative operations on a clean database.

## Migration Plan

1. Add failing focused tests for CSRF, rate limiting, cookie replay, account state, mixed-case races, permissions, and transaction context.
2. Add session and normalized-email model changes plus a reviewed forward migration with collision preflight and membership role constraint.
3. Add secret-free bootstrap and explicit grants; rotate credentials in each existing environment through an operator-only runbook.
4. Apply the forward migration only to the disposable clean-room database during automated verification. Do not modify shared or production databases in this task.
5. Implement authentication and transaction-boundary changes, then run focused tests and repeated full suites.
6. Update ADR clarification, OpenSpec status, architecture maps, security/runbook documentation, and current state with actual evidence.

Rollback of application code may use the previous binary only while the new schema remains backward-compatible. Production schema rollback is not performed with destructive `Down()`; use forward correction or approved recovery procedures.

## Open Questions

- The final production router and secret-management vendor remain open. They do not change the same-origin, exact-origin fallback, or secret-free source contracts.
- Historical migrations remain credential-free. Role creation and credential rotation stay in the administrator bootstrap workflow and must not be moved into migration source.
