# Technical Design: Authentication, Authorization & Personal Data Space

## 1. Identity Schema Integration & Compatibility

### 1.1 Canonical `identity.users` Compatibility Audit
In `docs/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql`, `identity.users` contains:
- `id uuid PRIMARY KEY`
- `email text NOT NULL`
- `display_name text NOT NULL`
- `status text NOT NULL DEFAULT 'active'`
- `locale text NOT NULL DEFAULT 'id-ID'`
- `timezone text NOT NULL DEFAULT 'Asia/Jakarta'`
- `created_at timestamptz NOT NULL DEFAULT now()`
- `updated_at timestamptz NOT NULL DEFAULT now()`
- `deleted_at timestamptz`
- Unique index `ux_users_email_normalized ON identity.users (lower(email))`
- Canonical comment: `Auth credentials/sessions/refresh tokens belong in an auth-provider migration; do not create homegrown password storage here.`

To support ASP.NET Core Identity (`UserManager<User>` and `SignInManager<User>`) without breaking existing queries or violating canonical invariants, we expand `identity.users` with the standard framework fields:
- `password_hash text`
- `security_stamp text NOT NULL DEFAULT ''`
- `concurrency_stamp text NOT NULL DEFAULT ''`
- `user_name text NOT NULL DEFAULT ''`
- `normalized_user_name text NOT NULL DEFAULT ''`
- `normalized_email text NOT NULL DEFAULT ''`
- `email_confirmed boolean NOT NULL DEFAULT false`
- `lockout_end timestamptz`
- `lockout_enabled boolean NOT NULL DEFAULT true`
- `access_failed_count integer NOT NULL DEFAULT 0`

### 1.2 User-to-Data-Space Membership Table
As defined in ADR-001, resource authorization requires server-side validation against `ownership.data_space_memberships`:
```sql
CREATE TABLE ownership.data_space_memberships (
    id uuid PRIMARY KEY,
    data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
    user_id uuid NOT NULL REFERENCES identity.users(id) ON DELETE RESTRICT,
    role text NOT NULL CHECK(role IN ('owner','member','viewer')),
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE(data_space_id, user_id)
);
CREATE INDEX ix_data_space_memberships_user ON ownership.data_space_memberships(user_id);
```

## 2. Component Architecture & Separation

```
[ HTTP Request (Cookie + Antiforgery Header) ]
                     │
                     ▼
  [ Antiforgery & Cookie Authentication Middleware ]
                     │
                     ▼
  [ DataSpaceAuthorizationFilter ] ──▶ Validates (ActorId, DataSpaceId) in ownership.data_space_memberships
                     │
                     ├── Sets ICurrentDataSpaceAccessor.CurrentDataSpaceId
                     ├── Executes SET LOCAL app.current_data_space_id (via TenantSessionInterceptor)
                     │
                     ▼
             [ API Endpoint ]
                     │
                     ▼
        [ Application Services ] (IAuthService, RegistrationService)
                     │
                     ▼
         [ Infrastructure / EF Core ] (StudyAIDbContext + StudyAIUserStore)
                     │
                     ▼
      [ PostgreSQL 16 DB (RLS & Least Privilege Role studyai_app) ]
```

### 2.1 ASP.NET Core Identity Wiring
- `AddIdentityCore<User>(options => { ... })`:
  - Password: Min length 8, requires digit, upper, lower, non-alphanumeric.
  - Lockout: Max failed attempts 5, lockout duration 15 minutes.
  - User: Require unique email.
- `AddSignInManager()`:
  - Issues encrypted HttpOnly session cookies using `IdentityConstants.ApplicationScheme`.
- `StudyAIUserStore`:
  - Implements `IUserStore<User>`, `IUserPasswordStore<User>`, `IUserEmailStore<User>`, `IUserSecurityStampStore<User>`, `IUserLockoutStore<User>`.
  - Backed directly by `StudyAIDbContext` with async queries and cancellation tokens.
- Cookie configuration:
  - `HttpOnly = true`
  - `SecurePolicy = CookieSecurePolicy.Always` (or `SameAsRequest` in testing environment)
  - `SameSite = SameSiteMode.Lax`
  - Eight-hour idle timeout and absolute cap for normal sessions.
  - Thirty-day absolute cap for explicit Remember Me sessions.
  - `SlidingExpiration = true`
- Antiforgery and abuse controls:
  - Native ASP.NET Core Antiforgery middleware.
  - Cookie named `.StudyAI.Antiforgery`, header `X-XSRF-TOKEN`.
  - State-changing auth requests (register, login, logout) validate antiforgery or use explicit antiforgery validation.
  - Login is limited to 10 attempts/minute/IP and registration to 5 attempts/hour/IP, with no queue and a `Retry-After` response.

### 2.2 Atomic Registration Transaction
The registration flow runs in a single database transaction via `StudyAIDbContext.Database.BeginTransactionAsync`:
1. Validate incoming `RegisterRequest` (email syntax, password policy, display name).
2. Check if email already registered (fail with 409 Conflict if duplicate).
3. Create `User` entity via `UserManager.CreateAsync(user, request.Password)`.
4. Create `DataSpace` entity (`kind = 'personal'`, `name = $"{user.DisplayName}'s Personal Space"`, `owner_user_id = user.Id`).
5. Create `DataSpaceMembership` (`data_space_id = dataSpace.Id`, `user_id = user.Id`, `role = 'owner'`).
6. Create `ContentSpace` entity (`kind = 'library'`, `title = 'Personal Library'`, `data_space_id = dataSpace.Id`).
7. Commit transaction.
8. If any exception occurs, transaction rolls back automatically and an error problem details response is returned.

### 2.3 Server-Side Authorization Boundary
When an endpoint requires Data Space access (e.g. `/api/v1/data-spaces/{id}/...`):
1. Extract `actor_id` from `ClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)`. If missing -> 401 Unauthorized.
2. Set transaction-local `app.current_actor_id`, then query actor-scoped `ownership.data_space_memberships` for `(data_space_id, user_id)`.
3. If not found -> 403 Forbidden.
4. Set `ICurrentDataSpaceAccessor.CurrentDataSpaceId = dataSpaceId`.
5. When `StudyAIDbContext` executes database queries, `TenantSessionInterceptor` runs `SELECT set_config('app.current_data_space_id', dataSpaceId, true)` transaction-locally.
6. When connection is released or transaction ends, the ambient context is discarded.

### 2.4 Session Revocation
- When `UserManager.UpdateSecurityStampAsync(user)` is called, the security stamp changes.
- The cookie authentication handler periodically validates `SecurityStamp`.
- Logout rotates the user's security stamp before clearing the local cookie, invalidating replayed cookies on every device immediately.
- Security stamps are validated on every authenticated request.
