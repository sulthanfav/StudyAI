# Browser Routing & Proxy Contract

Status: **APPROVED BASELINE**

Governing References: `openspec/changes/auth-database-security-remediation/specs/platform/browser-authentication/spec.md`, PRD Section 7, ADR-001.

---

## 1. Overview & Architecture

StudyAI Web V1 adopts a **same-origin browser routing architecture**:
- The Next.js web application acts as the public frontend origin.
- Browser requests to `/api/v1/*` are proxied directly to the upstream ASP.NET Core API host.
- Browser code never accesses an internal API origin directly; CORS credentialed wildcards (`*`) are strictly prohibited and rejected by startup validation.

```
+-------------------------------------------------------------------+
| Browser (SPA)                                                     |
| Origin: https://app.studyai.local                                 |
+---------------------------------+---------------------------------+
                                  | Same-origin HTTPS requests
                                  v
+-------------------------------------------------------------------+
| Next.js Reverse Proxy / Gateway Route Handler                     |
| Preserves: Cookies, Set-Cookie, X-XSRF-TOKEN, Problem Details     |
+---------------------------------+---------------------------------+
                                  | Forwarded internal request
                                  v
+-------------------------------------------------------------------+
| ASP.NET Core API Host                                             |
| Validates Antiforgery, Session, Tenant Context, RLS Boundary      |
+-------------------------------------------------------------------+
```

---

## 2. Approved Proxy Route Allowlist

Only documented API routes may be proxied from the web frontend to the API host:

| Route | Method | Authentication | CSRF Required | Description |
|---|---|---|---|---|
| `/api/v1/auth/csrf` | `GET` | Anonymous / Session | No | Generates antiforgery token pair (`.StudyAI.Antiforgery` cookie + JSON token). |
| `/api/v1/auth/register` | `POST` | Anonymous | **Yes** | Provisions new user, default data space, and library. |
| `/api/v1/auth/login` | `POST` | Anonymous | **Yes** | Validates credentials, issues `.StudyAI.Session` cookie. |
| `/api/v1/auth/logout` | `POST` | Authenticated | **Yes** | Revokes current user session and clears cookie. |
| `/api/v1/auth/me` | `GET` | Authenticated | No | Returns authenticated user profile and active data space. |
| `/api/v1/data-spaces/{id}/content-spaces` | `GET` | Authenticated | No | Lists content spaces within authorized data space. |

Any request to routes outside this allowlist MUST be rejected at the proxy boundary.

---

## 3. Cookie & CSRF Lifecycle

### Cookie Attributes
- **`.StudyAI.Session`**:
  - `HttpOnly: true` (inaccessible to JavaScript)
  - `Secure: true` (mandatory over HTTPS in production)
  - `SameSite: Lax`
  - `Path: /`
- **`.StudyAI.Antiforgery`**:
  - `HttpOnly: true`
  - `Secure: true` (in production)
  - `SameSite: Strict`

### Antiforgery Protocol
1. Before every state-changing request (`POST`, `PUT`, `DELETE`, `PATCH`), the client sends `GET /api/v1/auth/csrf`.
2. The proxy returns the JSON `{ "token": "..." }` and sets the `.StudyAI.Antiforgery` cookie.
3. The mutation request includes:
   - Header `X-XSRF-TOKEN: <token>`
   - Cookie `.StudyAI.Antiforgery=<cookie>`
4. Upon authentication state transition (e.g., login or logout), the token MUST be re-acquired.

---

## 4. Status Codes & Problem Details Preservation

The proxy MUST NOT convert API errors into HTML error pages or generic 500 responses. All upstream HTTP status codes and `application/problem+json` response bodies must be preserved:

| Status Code | Code / Reason | Description |
|---|---|---|
| `400 Bad Request` | `antiforgery_token_invalid` | Missing or mismatched CSRF token header or cookie. |
| `401 Unauthorized` | `unauthorized` | Missing, expired, or revoked session cookie. |
| `403 Forbidden` | `forbidden` | Authenticated actor lacks required role or membership in target data space. |
| `409 Conflict` | `auth_email_already_registered` | Email address is already registered. |
| `423 Locked` | `auth_account_locked` | Account locked due to repeated failed login attempts. |
| `429 Too Many Requests` | `auth_rate_limit_exceeded` | Request rate limit exceeded on `/register` or `/login`. |

---

## 5. Development vs. Production Setup

### Local Development
- Next.js development server: `http://localhost:3000`
- API host: `http://localhost:5000`
- Next.js `rewrites` or Route Handler forwards `/api/:path*` to `http://localhost:5000/api/:path*`.

### Production Deployment
- Single public domain: `https://studyai.campus.edu`
- TLS termination at reverse proxy (Caddy / Nginx / Cloudflare).
- Proxied upstream headers: `X-Forwarded-For`, `X-Forwarded-Proto: https`, `X-Correlation-Id`.
