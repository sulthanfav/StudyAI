# Tasks

## 1. CSRF and Authentication Abuse Controls

- [x] 1.1 Add failing integration tests for missing, invalid, and valid antiforgery tokens on register, login, logout, and a protected mutation; verify the new negative tests fail against the Iteration 4 baseline
- [x] 1.2 Enforce antiforgery validation on every cookie-based mutation and add a reusable real-token test client; verify all scenarios in `identity/authentication-security` pass without disabling antiforgery in the test host
- [x] 1.3 Add failing tests for per-endpoint auth request limits, stable 429 Problem Details, and untrusted forwarded-address handling; verify the configured rejection is observed
- [x] 1.4 Implement bounded register/login rate-limit policies and trusted-proxy-aware client resolution; verify rate-limit tests pass while normal lockout tests remain green
- [x] 1.5 Document the CSRF exchange, rate limits, and safe operational configuration in the auth/security runbook; verify every documented request example matches the integration contract

## 2. Server-Verifiable Sessions and Account State

- [x] 2.1 Add failing tests that capture and replay the exact pre-logout cookie, exercise absolute expiry, and prove ordinary logout does not revoke a separate device session
- [x] 2.2 Add the identity session entity, mapping, catalog entry, and migration model changes; verify model/migration tests cover required indexes, revocation fields, and no plaintext session identifier storage
- [x] 2.3 Implement session issuance and per-request principal validation for revocation, absolute expiry, security stamp, active status, and soft deletion; verify old-cookie replay and suspended/deleted-session tests pass
- [x] 2.4 Implement current-session logout plus all-device revocation for password and account-state changes; verify captured cookies from the correct scope are rejected and unaffected device sessions remain valid for ordinary logout
- [x] 2.5 Update ADR-001 clarification and session operations documentation with the verified current-session versus all-device semantics; verify unsupported security-complete claims are removed

## 3. Race-Safe Registration and Identity Store Semantics

- [x] 3.1 Add failing mixed-case and same-case concurrent registration tests that require exactly one 201 and deterministic 409 responses; run the focused race repeatedly to demonstrate the baseline failure
- [x] 3.2 Add a normalized-email collision preflight and authoritative unique database constraint without silently changing existing rows; verify both collision-free and collision-present migration paths on isolated PostgreSQL
- [x] 3.3 Replace exception/message-based duplicate handling with an explicit application result mapped to one public 409 error contract; verify pre-check, Identity validation, and unique-constraint paths are identical
- [x] 3.4 Map custom UserStore optimistic-concurrency exceptions to controlled Identity failures; verify concurrent failed-login/update tests do not emit unhandled 500 responses
- [x] 3.5 Update the authentication OpenSpec status and API contract documentation to distinguish verified behavior from prior intent; verify the original misleading completion claims are corrected

## 4. Secret-Free Bootstrap and Least-Privilege Roles

- [x] 4.1 Add a disposable clean-room PostgreSQL bootstrap test that starts a uniquely named container with ephemeral credentials and no inherited volume/default ACL; verify cleanup targets only resources created by the test
- [x] 4.2 Move supported role/bootstrap provisioning to an administrator-run script that reads secrets from approved configuration and pre-creates roles before migrations; verify repository and logs contain no active credential value
- [x] 4.3 Add explicit runtime grants and revocations, including denial of EF migration-history mutation, DDL, ownership, grant, and unapproved tables; verify allowed application DML and denied administrative probes as `studyai_app`
- [x] 4.4 Add the forward security migration for identity sessions, normalized-email uniqueness, and membership role constraint without modifying historical migration IDs; verify it upgrades a populated isolated database and preserves its data
- [x] 4.5 Write the credential rotation and clean-room bootstrap runbooks, flag direct historical migration execution as unsupported, and verify commands succeed against the disposable environment without printing secrets
- [x] 4.6 Update ADR-003/ADR-016 clarifications and database documentation with the proven privilege/bootstrap boundary; verify documentation does not claim ambient default ACLs are repository-managed

## 5. Tenant Authorization Transaction Boundary

- [x] 5.1 Add failing integration tests for missing actor/Data Space context, cross-tenant reads and writes, role-insufficient mutations, commit, rollback, and pooled physical-connection reuse
- [x] 5.2 Move actor authorization and tenant context into one request-owned explicit transaction and remove context-setting outside a transaction; verify RLS-dependent endpoints cannot execute outside the authorized unit of work
- [x] 5.3 Enforce the membership role CHECK constraint and operation-specific role authorization; verify invalid roles fail at PostgreSQL and insufficient valid roles return HTTP 403
- [x] 5.4 Remove endpoint-owned duplicate transactions and keep use-case rules behind Application contracts; verify architecture tests detect direct API-to-DbContext use in protected feature endpoints
- [x] 5.5 Update tenant-isolation diagrams and developer guidance with actor-context, membership lookup, Data Space context, and transaction lifetime; verify SYSTEM_MAP links and examples remain valid

## 6. Next.js Browser Contract Without Frontend Implementation

- [x] 6.1 Add host-level proxy-contract tests or a minimal test fixture proving cookie, antiforgery header, status, and Problem Details preservation through an allowlisted same-origin route
- [x] 6.2 Add explicit browser-auth configuration validation for internal API origin, HTTPS production assumptions, allowed proxy routes/methods/headers, and optional exact-origin CORS mode; verify wildcard credentialed CORS is rejected
- [x] 6.3 Publish the development and production routing contract without scaffolding Next.js, including CSRF acquisition, cookie behavior, HTTPS, and error codes; verify it is referenced from the developer start guide

## 7. Integrated Verification and Readiness Evidence

- [x] 7.1 Run focused authentication, migration, permission, tenant-isolation, architecture, and documentation tests against the disposable clean database; record literal pass/fail counts and commands in the change handoff
- [x] 7.2 Run `dotnet test --solution StudyAI.slnx --no-restore` at least three consecutive times after focused tests; require every run to pass and record any intermittent failure instead of retrying it away
- [x] 7.3 Run documentation unit tests, codebase index synchronization check, documentation validation, and OpenSpec strict validation; fix only issues within this change and record outputs
- [x] 7.4 Perform a manual Ponytail security/code review of the connected auth, persistence, bootstrap, and test paths; resolve correctness findings or record remaining P2 debt without enabling hooks
- [x] 7.5 Update `SYSTEM_MAP.md`, `FEATURE_MAP.md`, `MODULE_INDEX.md`, `CURRENT_STATE.md`, relevant catalogs, and the remediation handoff with verified evidence and remaining P2 items; verify the original frontend worktree remains unchanged

## Workflow follow-up

- Preserve credential-free historical migrations; role creation and credential rotation remain administrator bootstrap responsibilities.
- Review focused commit boundaries before any commit is created.
- Archive and sync the OpenSpec change only after Product Owner review and all acceptance evidence is complete.
