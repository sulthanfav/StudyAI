# Tasks: Authentication, Authorization & Personal Data Space

## 1. Schema Expansion & Compatibility

- [x] 1.1 Extend `StudyAI.Domain.Identity.User` with standard Identity fields (`PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `NormalizedUserName`, `NormalizedEmail`, `UserName`, `EmailConfirmed`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`) preserving all canonical columns and defaults.
- [x] 1.2 Create `StudyAI.Domain.Ownership.DataSpaceMembership` domain entity representing user-to-data-space membership with role validation.
- [x] 1.3 Configure EF Core mappings (`UserConfiguration`, `DataSpaceMembershipConfiguration`) and register `DataSpaceMemberships` in `StudyAIDbContext` and `IApplicationDbContext`.
- [x] 1.4 Generate incremental EF Core migration `AddIdentityAndMembership` and apply it to disposable test database `studyai_persistence_test`.
- [x] 1.5 Update database roles (`studyai_migrator`, `studyai_app`) and RLS policies to cover `ownership.data_space_memberships`.
- [x] 1.6 Verify schema parity against clean-slate reference definitions.

## 2. ASP.NET Core Identity & Atomic Provisioning Service

- [x] 2.1 Implement custom `StudyAIUserStore` in `StudyAI.Infrastructure.Identity` implementing `IUserStore<User>`, `IUserPasswordStore<User>`, `IUserEmailStore<User>`, `IUserSecurityStampStore<User>`, `IUserLockoutStore<User>`.
- [x] 2.2 Configure ASP.NET Core Identity services and cookie authentication in `StudyAI.Api`.
- [x] 2.3 Implement atomic registration and provisioning service (`IAuthService` / `AuthService`) in `StudyAI.Application` ensuring single-transaction rollback on failure.

## 3. Auth Endpoints & Session Management

- [x] 3.1 Define DTO contracts in `StudyAI.Contracts.Auth` (`RegisterRequest`, `LoginRequest`, `AuthUserResponse`, `DataSpaceSummaryResponse`) as sealed records.
- [x] 3.2 Implement `POST /api/v1/auth/register` returning 201 Created with provisioned metadata.
- [x] 3.3 Implement `POST /api/v1/auth/login` returning 200 OK with HttpOnly cookie, lockout handling, and generic error messages.
- [x] 3.4 Implement `POST /api/v1/auth/logout` returning 204 No Content and clearing cookie.
- [x] 3.5 Implement `GET /api/v1/auth/me` returning authenticated user profile and active personal data space.
- [x] 3.6 Configure Antiforgery / CSRF protection with `X-XSRF-TOKEN` and endpoint rate limiting.

## 4. Server-Side Data Space Authorization

- [x] 4.1 Implement `IDataSpaceAuthorizationService` querying `ownership.data_space_memberships` by actor ID and data space ID.
- [x] 4.2 Implement endpoint filter / middleware `DataSpaceAuthorizationFilter` verifying membership, populating `ICurrentDataSpaceAccessor`, and executing transaction-local RLS `SET LOCAL app.current_data_space_id`.
- [x] 4.3 Add protected data space endpoint `GET /api/v1/data-spaces/{id}/content-spaces` to verify authorization boundary.

## 5. Security & Integration Testing

- [x] 5.1 Implement automated tests for registration (success, duplicate rejection, atomic rollback, concurrent uniqueness).
- [x] 5.2 Implement automated tests for authentication (login success, invalid credentials, lockout, logout invalidation, session revocation).
- [x] 5.3 Implement automated tests for authorization and tenancy (owned space access, cross-tenant denial, forged ID rejection, pooled connection context clearance).
- [x] 5.4 Implement automated tests for CSRF rejection on state-changing requests.
- [x] 5.5 Verify that all existing 40 persistence, concurrency, and architecture tests remain passing.

## 6. Documentation & Verification

- [x] 6.1 Update catalogs in `docs/codebase/catalog/` and run `scripts/docs/sync_codebase_index.py`.
- [x] 6.2 Validate documentation integrity with `scripts/docs/validate_docs.py`.
- [x] 6.3 Verify solution build and full test suite with `dotnet test StudyAI.slnx --no-restore`.
- [x] 6.4 Compile final 14-point completion report.
