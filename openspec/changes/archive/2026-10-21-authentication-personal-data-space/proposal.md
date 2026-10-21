# OpenSpec Change Proposal: Authentication, Authorization & Personal Data Space

> **Current status (2026-10-20):** Implemented baseline, not security-complete. The active `auth-database-security-remediation` change is the authoritative readiness ledger. This proposal's intended controls must not be treated as verified where that remediation still lists bootstrap/privilege or tenant transaction-boundary work as incomplete.

## Change Identifier
`authentication-personal-data-space`

## Problem Statement
StudyAI Campus V1 requires an authoritative, production-grade authentication and user authorization foundation. Following Phase 2 Iteration 3's successful completion of the 11-table EF Core persistence subset and transaction-local PostgreSQL RLS validation, the application currently lacks:
1. Production authentication endpoints for registration, login, logout, and authenticated actor resolution.
2. An atomic account provisioning workflow that binds a newly registered identity user to their default Personal Data Space, owner membership, and root Personal Library container.
3. Secure HttpOnly cookie session management with CSRF protection, sliding expiration, lockout policy, and security stamp revocation.
4. Server-side Data Space authorization separating authentication identity (`actor_id`) from data space access (`data_space_id`), preventing client-side forgery and tenant leakage.

## Proposed Changes
1. **Schema Expansion & Parity**:
   - Audit canonical `identity.users` and extend `StudyAI.Domain.Identity.User` with ASP.NET Core Identity credential columns (`password_hash`, `security_stamp`, `concurrency_stamp`, `normalized_email`, `normalized_user_name`, `lockout_end`, `lockout_enabled`, `access_failed_count`, `email_confirmed`) while preserving all canonical columns, defaults, and the unique normalized email index.
   - Add `ownership.data_space_memberships` (`DataSpaceMembership`) to represent explicit user-to-data-space membership and roles (`owner`, `member`, `viewer`), adhering to ADR-001.
   - Generate an incremental EF Core migration (`AddIdentityAndMembership`) and apply it with `studyai_migrator`, granting least-privilege DML to `studyai_app`.
2. **ASP.NET Core Identity Core & Cookie Sessions**:
   - Integrate ASP.NET Core Identity (`UserManager<User>`, `SignInManager<User>`) using an EF Core-backed `IUserStore<User>` in `StudyAI.Infrastructure.Identity`.
   - Configure HttpOnly, Secure, `SameSite=Lax` session cookies with Antiforgery / CSRF protection (`X-XSRF-TOKEN`), an eight-hour idle/absolute cap, a thirty-day Remember Me cap, immediate security-stamp revocation, and balanced per-IP rate limits.
3. **Atomic Provisioning Pipeline**:
   - Implement `IAuthService` executing an atomic unit of work: User creation -> Personal Data Space creation -> Owner Membership creation -> Personal Library creation.
   - Ensure complete rollback on any failure, preventing orphan users or duplicate spaces.
4. **Authentication Endpoints**:
   - `POST /api/v1/auth/register` (201 Created)
   - `POST /api/v1/auth/login` (200 OK + cookie)
   - `POST /api/v1/auth/logout` (204 No Content + clear cookie)
   - `GET /api/v1/auth/me` (200 OK + user & personal space info)
5. **Server-Side Data Space Authorization**:
   - Implement `IDataSpaceAuthorizationService` and endpoint authorization filter validating explicit membership before populating scoped actor/data-space contexts and setting PostgreSQL session variables `app.current_actor_id` and `app.current_data_space_id`.
   - Enforce fail-closed semantics for unauthenticated, unauthorized, or forged Data Space IDs.
6. **Automated Integration Testing**:
   - Add comprehensive tests covering registration, login, logout, lockout, CSRF, cross-tenant isolation, session invalidation, and connection pooling safety against real PostgreSQL.

## Non-Goals
- Building frontend UI or Razor views (API only).
- Creating custom password hashers or homegrown JWT issuing systems (strictly prohibited by ADR-001).
- Creating a new OAuth/OIDC authorization server (future mobile PKCE extension point only).
- Migrating remaining unused database tables (strict minimal expansion).
