# StudyAI Current Development State

Last updated: 2026-10-21

Current Branch: `main`

Build & Test Status: **Passing — 102/102 tests, 0 failed, 0 skipped in three consecutive full-suite runs**

Navigation: [`docs/START_HERE.md`](../START_HERE.md) | [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`docs/INDEX.md`](../INDEX.md)

---

## 1. Active Work

There is no active implementation change or feature branch. Phase 2 Iteration 4 and its security remediation are complete and archived. The next UI stage must begin later from `main` with a new branch and a newly approved OpenSpec change.

| Task | Status | Evidence / Next Step |
|---|---|---|
| Authentication, Authorization & Personal Data Space | Archived | `openspec/changes/archive/2026-10-21-authentication-personal-data-space/` |
| Authentication & Database Security Remediation | Archived | `openspec/changes/archive/2026-10-21-auth-database-security-remediation/` |
| Academic Workspace / UI implementation | Not started | Agree scope and Product Owner decisions, then create a new OpenSpec change and branch from `main`. |

## 2. Verified Backend Readiness

- Cookie mutations use native ASP.NET Core antiforgery validation. Register is limited to 5 requests/hour/IP and login to 10 requests/minute/IP, with no queue, RFC 7807 responses, and `Retry-After` when available.
- Server-backed sessions store only a hash of the opaque session identifier. Idle expiry is 8 hours; ordinary absolute expiry is 8 hours; Remember Me absolute expiry is 30 days.
- Ordinary logout revokes only the active device session. Password, security-stamp, suspension, and soft-delete changes revoke every active session for the account.
- Registration has authoritative normalized-email uniqueness and stable HTTP 409 behavior, including concurrent mixed-case races.
- Explicit Data Space membership is the only access source. Infrastructure owns one transaction spanning actor context, membership/role authorization, Data Space context, and the endpoint handler.
- API endpoints and filters do not depend on Infrastructure, EF Core, `DbContext`, or `IApplicationDbContext`; content-space reads use the Application query contract.
- Forward migration `20261020114606_AuthDatabaseSecurityRemediation` adds sessions, normalized-email uniqueness, membership-role validation, and idempotently reasserts actor-scoped membership RLS without embedding credentials.

## 3. Verification Evidence

- `dotnet build StudyAI.slnx --no-restore`: 0 errors, 0 warnings.
- `dotnet test --solution StudyAI.slnx --no-restore`: three consecutive runs, each 102 passed, 0 failed, 0 skipped.
- Clean database bootstrap: unique disposable PostgreSQL container, ephemeral credentials, tmpfs storage, no inherited StudyAI roles/schemas, and restricted PUBLIC ACL.
- Populated Iteration 4 upgrade: seed data preserved; `identity.user_sessions` created; actor membership RLS policy present.
- OpenSpec 1.14.3 strict validation passed before both changes were synced and archived.
- Documentation index/links, PowerShell parser checks, secret scan, and `git diff --check` passed after the consolidation commit.

## 4. Known Deferred P2 Work

- In-memory authentication rate limiting is accepted only for the current single API instance. Multi-replica deployment requires distributed rate limiting.
- Session validation currently reads PostgreSQL on authenticated requests. A distributed session cache may be evaluated when measured load justifies it; PostgreSQL remains authoritative.
- UI visual direction, workspace lifecycle behavior, and remaining open Product Owner decisions must be resolved in the next OpenSpec proposal. No frontend scaffold or UI dependency has been added.
