# ADR-001 Recommendation: Authentication and Session Architecture

Status: **PROPOSED / PENDING APPROVAL**  
Date: 2026-10-20  
Governing references: PRD OD-01; Core Architecture v1.2 OPEN-05; Database Review v1.1 ADR-AUTH / DBR-16

---

## 1. Executive Summary

This document presents the technical evaluation and architectural recommendation for StudyAI Campus V1 authentication and session management. It addresses identity lifecycle, web session mechanics, future mobile token integration, and the strict separation of authentication identity from Data Space resource authorization.

**Implementation of authentication endpoints is NOT part of this phase and remains blocked until explicit approval.**

---

## 2. Recommended Architecture: ASP.NET Core Identity + OIDC-Ready Design

We recommend an **in-monolith ASP.NET Core Identity foundation designed for OIDC/OAuth 2.0 readiness**:

1. **Web Sessions (Campus V1 Web-First Target)**:
   - **Secure Cookie-Based Authentication**: Encrypted, HTTP-only, `SameSite=Lax` (or `Strict` for mutations) cookies.
   - **CSRF Protection**: Native ASP.NET Core Antiforgery tokens tied to the authenticated session context.
   - **Session Sliding & Expiration**: 24-hour absolute lifespan with sliding expiration during active use; sliding reset halts upon idle timeout.
   - **Revocation**: Backed by a `SecurityStamp` in `identity.users`. Changing password, logging out of all devices, or admin suspension immediately revokes all active session cookies.

2. **Mobile Architecture (V1+ Expo / React Native Target)**:
   - **Protocol**: OAuth 2.0 Authorization Code Flow with Proof Key for Code Exchange (PKCE) (RFC 7636).
   - **Tokens**: Short-lived, cryptographically signed asymmetric JWT access tokens (15-minute lifespan) paired with single-use, rotatable refresh tokens stored hashed in the database.
   - **No Shared Session Cookie**: Mobile clients use standard `Authorization: Bearer <token>` headers, keeping browser cookie constraints decoupled from mobile networking.

3. **Separation of Authentication Identity from Authorization Scope (CRITICAL INVARIANT)**:
   - **Authentication**: Establishes *who* the caller is (`actor_id` / `user_id`).
   - **Authorization**: The application server independently queries `ownership.data_space_membership` to determine *which* Data Spaces the actor has permission to access (Personal Space vs Academic Group Spaces).
   - **No Direct Trust**: The client CANNOT supply a `data_space_id` and have it accepted without server-side validation against the actor's authorized memberships.

4. **Account Provisioning Lifecycle**:
   - Registration executes as an atomic database transaction:
     1. Creates `identity.users` credential and profile.
     2. Provisions the user's default Personal Data Space in `ownership.data_space`.
     3. Adds owner membership in `ownership.data_space_membership` (`role = 'owner'`).
     4. Provisions the root Library container in `knowledge.data_space_library`.
   - If any step fails, the entire transaction rolls back.

5. **External Federation (Google / Apple)**:
   - Leverages standard Microsoft External Authentication middleware (`AddGoogle`, `AddApple`).
   - Foreign identities link to `identity.users` via a dedicated federation mapping table (`identity.external_logins`), avoiding external provider lock-in.

---

## 3. Comparison with Alternatives

| Option | Operational Overhead | Portability & Privacy | Multi-Platform (Web + Mobile) | Data Space Fit | Recommendation |
|---|---|---|---|---|---|
| **ASP.NET Core Identity (In-Engine)** | Low (managed within existing ASP.NET / PostgreSQL stack) | High (self-hosted in-region PostgreSQL, zero third-party data egress) | Excellent (cookie for web, standard endpoints for mobile PKCE) | Native (direct transaction with `ownership.data_space`) | **RECOMMENDED** |
| **Fully Managed SaaS (Auth0 / Clerk)** | Low initial, High long-term cost ($$$ per MAU) | Low (user metadata stored on vendor servers, US/EU egress) | Good | Requires webhook synchronization to provision Data Spaces | Rejected (vendor lock-in, recurring operational cost) |
| **Separate Keycloak / IdentityServer** | High (separate Java/.NET cluster, separate database) | High | Excellent | Requires cross-service coordination | Over-engineered for Campus V1 phase |

---

## 4. Security Invariants & Open Questions Requiring Approval

1. **Credential Storage**: PBKDF2 with SHA-512 (ASP.NET Core Identity v3 default) or Argon2id.
2. **Account Deactivation Policy**: Grace period (e.g. 30 days) before soft-deleted accounts and personal data spaces are purged.
3. **MFA Requirement**: Optional for students initially, mandatory for system administrative accounts.

---

## 5. Next Steps Pending Approval

Upon Product Owner approval of this ADR:
- Scaffold `identity.users` entity configurations and identity stores targeting PostgreSQL schema `identity`.
- Implement cookie authentication and registration handlers in Phase 2 Iteration 3.
