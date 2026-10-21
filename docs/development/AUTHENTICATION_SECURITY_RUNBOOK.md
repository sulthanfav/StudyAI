# Authentication Security Runbook

Status: operational contract for the implemented CSRF, authentication request-rate, and server-verifiable session controls. The broader OpenSpec change `auth-database-security-remediation` is still in progress and is not claimed complete here.

## Browser request contract

StudyAI uses an encrypted `HttpOnly` session cookie and a separate antiforgery cookie. Every cookie-based mutation must use this exchange:

1. Send `GET /api/v1/auth/csrf` with the browser cookie jar enabled.
2. Read the `token` property from the JSON response. Keep the `.StudyAI.Antiforgery` cookie returned with it.
3. Send the mutation with the same cookie jar and `X-XSRF-TOKEN: <token>`.
4. Acquire a new token before each mutation. This is required after login because the authenticated identity has changed.

Missing, invalid, or mismatched cookie/header pairs return HTTP 400 with Problem Details before the endpoint handler runs. The rule currently applies to:

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`

Future cookie-authenticated mutation endpoints must add the same `AntiforgeryValidationFilter`; `UseAntiforgery` alone does not opt an endpoint into validation.

### Verified PowerShell example

```powershell
$apiOrigin = 'http://localhost:5080'
$browserSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$csrf = Invoke-RestMethod `
  -Method Get `
  -Uri "$apiOrigin/api/v1/auth/csrf" `
  -WebSession $browserSession

$headers = @{ 'X-XSRF-TOKEN' = $csrf.token }
$registerBody = @{
  email = 'student@example.test'
  password = 'Password123!'
  displayName = 'Example Student'
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "$apiOrigin/api/v1/auth/register" `
  -WebSession $browserSession `
  -Headers $headers `
  -ContentType 'application/json' `
  -Body $registerBody
```

Use the same exchange for login. After login, fetch `/api/v1/auth/csrf` again with the authenticated session before calling logout or another protected mutation.

## Registration duplicate contract

Email uniqueness is enforced authoritatively by a PostgreSQL unique expression index over `upper(btrim(email))`. The application lookup is only a fast path. A duplicate detected by the pre-check, ASP.NET Core Identity validation, or a concurrent database insert returns the same HTTP 409 Problem Details shape:

```json
{
  "title": "Email Already Registered",
  "status": 409,
  "detail": "An account with this email already exists.",
  "code": "auth_email_already_registered"
}
```

Clients must branch on status 409 and `code`, never on the human-readable title or detail. Before the remediation migration creates the index, it performs a read-only collision preflight. Existing case/whitespace collisions abort migration with SQLSTATE `23505`; no account row is rewritten or deleted automatically.

## Session lifecycle operations

Each successful login creates an independent row in `identity.user_sessions`. The encrypted session cookie carries a random opaque identifier; PostgreSQL stores only its SHA-256 hash. Never log either value.

Idle and absolute lifetimes are set in seconds and must be positive. Ordinary sessions have an 8-hour idle and absolute cap; Remember Me keeps the same 8-hour idle window with a 30-day absolute cap:

```json
{
  "Authentication": {
    "Sessions": {
      "IdleLifetimeSeconds": 28800,
      "StandardAbsoluteLifetimeSeconds": 28800,
      "RememberMeAbsoluteLifetimeSeconds": 2592000
    }
  }
}
```

Session operations have two distinct scopes:

- `POST /api/v1/auth/logout` revokes only the row identified by the current cookie, then clears that cookie. It must not revoke other device sessions.
- Password changes, security-stamp changes, suspension, and soft deletion revoke all active session rows for that user. This is the all-device operation.

Every authenticated request checks the server row, idle and absolute expiry, revocation timestamp, security-stamp snapshot, account status, and soft-deletion timestamp, then advances `last_seen_at` for an eligible session. Replaying a cookie after current-session logout, all-device revocation, idle expiry, or absolute expiry returns HTTP 401.

For incident response, prefer the reviewed account-state/UserManager path so the user update and session-row revocations are saved together. Direct SQL changes can make cookies fail through per-request account checks, but bypass the explicit revocation audit state and are not the supported operating path.

## Authentication request-rate controls

Register and login use independent fixed-window policies. Requests are partitioned by the normalized direct peer address plus the endpoint policy. Queueing is disabled, so excess requests are rejected immediately and do not invoke authentication or registration logic.

Default configuration:

```json
{
  "Authentication": {
    "RateLimits": {
      "Register": {
        "PermitLimit": 5,
        "WindowSeconds": 3600
      },
      "Login": {
        "PermitLimit": 10,
        "WindowSeconds": 60
      }
    }
  }
}
```

Both values for each policy must be positive. Rate limiting is independent of per-account lockout: the limiter protects the service and unknown-account paths, while lockout protects a known account after repeated credential failures.

Rejected requests return HTTP 429, `application/problem+json`, an optional integer `Retry-After` header, and this stable public shape:

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "The authentication request limit was exceeded.",
  "code": "auth_rate_limit_exceeded"
}
```

Additional standard diagnostics such as `traceId` or `correlationId` may be present. Clients must branch on HTTP 429 and `code`, not on the human-readable detail.

## Trusted reverse proxies

`X-Forwarded-For` is ignored by default. Configure it only when the API is behind an operator-controlled reverse proxy and use exact proxy addresses or CIDR networks:

```json
{
  "Authentication": {
    "ForwardedHeaders": {
      "KnownProxies": ["10.0.0.10"],
      "KnownNetworks": ["10.20.0.0/16"]
    }
  }
}
```

The middleware accepts at most one forwarded hop. Never add public or catch-all networks. The edge proxy must replace inbound forwarding headers rather than append user-supplied values. When no trusted entry is configured, direct connection addresses remain authoritative.

## Operational checks

- Confirm production uses HTTPS before enabling browser authentication.
- Keep register and login limits in environment-specific configuration; do not patch source to tune them.
- Alert on sustained `auth_rate_limit_exceeded` responses, grouped by endpoint and direct peer without logging credentials, cookies, tokens, full request bodies, or forwarded-header chains.
- After proxy topology changes, rerun `AuthenticationRateLimitIntegrationTests` and verify spoofed `X-Forwarded-For` values cannot create new partitions.
- Verify legitimate login and account-lockout tests remain green whenever limits change.
- Use `ServerSessionLifecycleIntegrationTests` after changing cookie, expiry, password, or account-state behavior; it distinguishes current-session and all-device revocation.
