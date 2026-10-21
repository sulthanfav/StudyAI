# Spec Delta

## Purpose

Defines the browser authentication security contract for CSRF enforcement, abuse controls, account eligibility, server-verifiable sessions, and race-safe identity registration.

## ADDED Requirements

### Requirement: Antiforgery validation for cookie-based mutations
The API SHALL reject every state-changing cookie-based request that lacks a valid antiforgery token bound to the current browser context. Registration, login, logout, and protected mutations MUST use the same documented token exchange.

#### Scenario: Missing or invalid token
- **WHEN** a client calls a state-changing authentication endpoint without a token or with an invalid token
- **THEN** the API returns the documented antiforgery failure status and performs no state change

#### Scenario: Valid token
- **WHEN** a client first acquires an antiforgery token and submits it with its matching antiforgery cookie
- **THEN** the state-changing request proceeds to its normal authentication or business validation

### Requirement: Bounded authentication request rate
Sensitive authentication endpoints SHALL apply configured request limits independent of account lockout. Client identity for limiting MUST come from the direct connection unless explicitly trusted proxy configuration is present.

#### Scenario: Request limit exceeded
- **WHEN** a client exceeds the configured policy for a sensitive authentication endpoint
- **THEN** the API returns HTTP 429 with a stable Problem Details response and does not invoke the authentication operation

### Requirement: Server-verifiable session lifecycle
Every issued browser session SHALL have a server-verifiable identifier, issued time, absolute expiry, and revocation state. Sliding renewal MUST NOT extend a session beyond its absolute expiry.

#### Scenario: Current session logout
- **WHEN** an authenticated user logs out
- **THEN** only the current session is revoked server-side, its browser cookie is cleared, and replay of the exact pre-logout cookie returns HTTP 401

#### Scenario: Absolute session expiry
- **WHEN** a session reaches its absolute lifetime despite prior sliding renewals
- **THEN** the next authenticated request is rejected with HTTP 401

#### Scenario: Security-sensitive all-device revocation
- **WHEN** a password is changed or an account becomes suspended or deleted
- **THEN** all previously issued sessions for that account are rejected on their next use

### Requirement: Account state is enforced at login and session validation
Only active, non-deleted accounts SHALL authenticate or continue using an existing session. Authentication errors MUST NOT disclose whether a user exists.

#### Scenario: Suspended or deleted account login
- **WHEN** valid credentials are submitted for a suspended or soft-deleted account
- **THEN** authentication is rejected and no session cookie is issued

#### Scenario: Existing session after account revocation
- **WHEN** an active account session is presented after that account becomes suspended or deleted
- **THEN** the request returns HTTP 401 and no protected operation executes

### Requirement: Case-insensitive race-safe email identity
The database SHALL enforce one account per normalized email regardless of casing. Concurrent attempts for the same normalized email MUST create exactly one account and no orphan personal workspace records.

#### Scenario: Mixed-case duplicate
- **WHEN** registrations for `User@example.test` and `user@example.test` race
- **THEN** exactly one returns HTTP 201 and every duplicate returns HTTP 409

#### Scenario: Existing normalized collision before migration
- **WHEN** deployment validation detects more than one existing row for the same normalized email
- **THEN** constraint application stops with a non-secret diagnostic and no conflicting row is silently changed or deleted

### Requirement: Stable authentication error contracts
Equivalent authentication outcomes SHALL map to one documented HTTP status and Problem Details shape regardless of timing or persistence path.

#### Scenario: Duplicate detected by validation or database constraint
- **WHEN** registration discovers a duplicate before insert or through a concurrent unique-constraint failure
- **THEN** both paths return HTTP 409 with public error code `auth_email_already_registered`, title `Email Already Registered`, and detail `An account with this email already exists.`

#### Scenario: Identity persistence concurrency conflict
- **WHEN** an Identity update loses an optimistic-concurrency race
- **THEN** the API returns a controlled authentication failure and does not expose an unhandled database exception
