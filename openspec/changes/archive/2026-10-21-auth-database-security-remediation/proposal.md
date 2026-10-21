# Proposal

## Why

The Iteration 4 authentication and tenant-isolation implementation does not yet satisfy the accepted security contracts in ADR-001, ADR-003, and ADR-016: several controls are documented as complete but are absent, non-reproducible, or insufficiently tested. This change closes the confirmed pre-frontend security blockers and establishes a reproducible backend baseline before a Next.js client is allowed to consume the authentication APIs.

## What Changes

- Enforce native antiforgery validation on cookie-authenticated state-changing endpoints and apply bounded rate limiting to sensitive authentication operations.
- Implement server-verifiable session revocation, account-state enforcement, security-stamp validation, and an absolute session lifetime; prove that a cookie captured before logout or account revocation cannot be replayed.
- Make registration uniqueness case-insensitive and race-safe in PostgreSQL, with a stable HTTP 409 contract for all duplicate-email paths.
- Define a same-origin Next.js proxy contract as the preferred browser integration; keep any direct cross-origin option explicit, origin-restricted, credential-aware, and disabled by default.
- Remove database passwords and role creation from EF migrations and source defaults. Document rotation of previously exposed local credentials without logging replacement values.
- Add reproducible, secret-free database bootstrap prerequisites; replace broad runtime grants with explicit privileges and deny runtime mutation of EF migration metadata.
- Repair the authorization-to-transaction boundary so tenant context is established only inside an active transaction and remains fail-closed across pooled connection reuse, commit, and rollback.
- Add the membership-role constraint and Identity store concurrency handling needed by the security model.
- Replace misleading completion claims with adversarial PostgreSQL integration tests and update security, architecture, status, and developer-runbook documentation with verified evidence.
- Do not rewrite applied migrations or Git history. Existing environments receive forward remediation; destructive database changes remain outside this change.

## Capabilities

### New Capabilities

- `identity/authentication-security`: Cookie authentication, CSRF, abuse throttling, account-state enforcement, session revocation, registration uniqueness, and stable authentication error contracts.
- `platform/browser-authentication`: The secure browser-to-API routing, cookie, HTTPS, CSRF exchange, method/header, and error-response contract for the Next.js client.

### Modified Capabilities

- `database/persistence`: Add reproducible clean-room migration/bootstrap behavior and race-safe normalized identity uniqueness while preserving the single-DbContext and incremental forward-fix strategy.
- `database/tenant-isolation`: Strengthen least-privilege runtime grants, migration-history protection, and the explicit authorization-to-transaction tenant-context boundary.

## Impact

- Governing sources: Final PRD v1.0, Core Architecture v1.2, ADR-001, ADR-003, ADR-016, and the existing `authentication-personal-data-space` change.
- Affected code: API authentication composition and endpoints, Application authentication contracts, Infrastructure Identity/UserStore and tenant transaction handling, EF mappings and a new forward migration, database bootstrap tooling, PostgreSQL integration fixtures, architecture/security tests, and developer documentation.
- Security and ownership: closes CSRF, cookie replay, suspended/deleted-account access, cross-tenant context lifetime, over-privileged database-role, and email-collision gaps without weakening server-side Data Space authorization or RLS.
- Data integrity: introduces collision detection before normalized-email enforcement and keeps existing databases on a forward-only remediation path.
- Provenance, quota, and lifecycle: no behavior change. Existing invariants remain mandatory and out of scope for functional expansion.
- Open decision: deployment topology remains open beyond the web contract. This change recommends same-origin proxying for V1 but does not approve a production hosting vendor or topology.
- Testing: requires real PostgreSQL clean-room bootstrap plus repeated full-suite runs; a single green run is not sufficient evidence.
