# ADR-001: Authentication and Session Architecture

Status: **ACCEPTED**  
Decision Date: 2026-10-20  
Decision Owner: Product Owner / Technical Owner  
Governing References: PRD OD-01; Core Architecture v1.2 OPEN-05; Database Review v1.1 ADR-AUTH / DBR-16

---

## 1. Context

StudyAI Campus V1 requires an authoritative authentication and session management architecture before implementing user account provisioning, Data Space access, and business modules. The architecture must satisfy:
1. Web-First Campus V1 target: secure, seamless browser sessions.
2. Future Mobile (V1+ Expo / React Native) readiness: standard token-based authentication.
3. Strict separation of authentication identity (`actor_id` / `user_id`) from Data Space authorization (`data_space_id`).
4. Zero homegrown cryptographic algorithms, custom password hashing, or insecure custom JWT issuing frameworks.
5. Atomic user registration creating identity credentials, default Personal Data Space, and root Library container.

---

## 2. Decision

1. **Identity Provider Framework**:
   - Adopt **ASP.NET Core Identity** within the application boundary as the identity store and authentication engine.
   - Credentials, passwords, and security stamps are managed via ASP.NET Core Identity's battle-tested password hasher (PBKDF2 with HMAC-SHA512).

2. **Web Session Architecture (Campus V1 Target)**:
   - Use encrypted, HTTP-only, `SameSite=Lax` cookie sessions.
   - Enforce native ASP.NET Core Antiforgery / CSRF tokens for all state-changing endpoints.
   - Implement sliding expiration with absolute session caps.
   - Back each cookie with a server-verifiable session row. Store only a hash of the opaque session identifier.
   - Ordinary logout revokes only the current session; password changes, suspension, and deletion revoke every active session for the account.
   - Validate revocation state, absolute expiry, the security-stamp snapshot, active account status, and soft deletion on every authenticated request.

3. **Mobile Token Strategy (Future V1+ Target)**:
   - Mobile authentication will strictly use a standards-compliant **OAuth 2.0 / OIDC Authorization Code Flow with PKCE (RFC 7636)**.
   - Do NOT implement a custom OAuth/OIDC authorization server or invent an ad-hoc insecure JWT issuing mechanism.

4. **Resource Authorization Boundary (CRITICAL INVARIANT)**:
   - Authentication verifies identity (`user_id`).
   - The application server independently queries `ownership.data_space_membership` to resolve authorized Data Spaces.
   - Client-supplied `data_space_id` values are NEVER trusted directly.

5. **Atomic Provisioning Transaction**:
   - User registration atomically creates:
     1. `identity.users` profile and credential.
     2. `ownership.data_spaces` default Personal Data Space (`kind = 'personal'`).
     3. `ownership.data_space_membership` (`role = 'owner'`).
     4. `workspace.content_spaces` root Library (`kind = 'library'`).
   - Any failure rolls back the entire registration.

6. **Implementation Milestone**:
   - Actual authentication controllers, cookie middleware, and registration endpoints are deferred to **Phase 2 Iteration 4**.

---

## 3. Consequences

- **Positive**: Low operational complexity, zero third-party data egress, in-monolith transactional integrity with `ownership.data_spaces`, standard security patching.
- **Negative / Constraints**: Requires careful cookie domain configuration in staging/production; mobile support requires an OIDC-compliant bridge rather than quick ad-hoc API tokens.
- **Security Invariants**: Client requests without valid cookies or authorized memberships fail closed (401/403).

---

## 4. Verified Session-Scope Clarification (2026-10-20)

The phrase “immediate session revocation across all active devices” applies only to security-sensitive account changes. It does not mean that normal logout is an all-device logout.

- **Current-session logout**: hashes the opaque identifier from the current encrypted cookie, sets `revoked_at` on that one `identity.user_sessions` row, and clears that browser cookie. Replaying the exact captured pre-logout cookie returns HTTP 401. A separately authenticated device remains active.
- **All-device revocation**: password/security-stamp changes and account eligibility changes mark every active session row for that user revoked. Previously issued cookies are rejected on their next request.
- **Absolute lifetime**: each row receives `issued_at` and `absolute_expires_at` at issuance. Cookie sliding renewal cannot move the database absolute expiry.
- **Defense in depth**: per-request validation also rejects missing session rows, revoked rows, expired rows, security-stamp mismatches, non-active users, and soft-deleted users.

Evidence is maintained in `ServerSessionLifecycleIntegrationTests`. This clarification does not claim the broader `auth-database-security-remediation` change is complete; registration races, database bootstrap/privileges, and tenant transaction-boundary work remain separately tracked.
