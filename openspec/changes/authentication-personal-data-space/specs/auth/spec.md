# Spec Delta: Authentication, Authorization & Personal Data Space

## Purpose
Defines the functional and security requirements for user registration, ASP.NET Core Identity authentication, session cookie management, server-side data space authorization, and atomic personal workspace provisioning.

## ADDED Requirements

### Requirement: User registration with atomic provisioning
The application SHALL provide a `POST /api/v1/auth/register` endpoint that validates input, verifies email uniqueness, and atomically provisions the user identity, default personal data space, owner membership, and root personal library in a single database transaction.

#### Scenario: Successful registration and provisioning
- **WHEN** a client sends a valid `RegisterRequest` with unique email, strong password, and display name
- **THEN** HTTP 201 Created is returned with the provisioned user ID, personal data space ID, and library content space ID
- **AND** records exist in `identity.users`, `ownership.data_spaces`, `ownership.data_space_memberships`, and `workspace.content_spaces`

#### Scenario: Duplicate registration rejection
- **WHEN** a client sends a `RegisterRequest` with an email that is already registered (case-insensitive)
- **THEN** HTTP 409 Conflict is returned with Problem Details and no duplicate records are created

#### Scenario: Atomic rollback on provisioning failure
- **WHEN** an unexpected failure occurs during personal space or library provisioning
- **THEN** the entire transaction is rolled back and no orphan records remain in `identity.users`

### Requirement: ASP.NET Core Identity authentication and session cookies
The application SHALL provide `POST /api/v1/auth/login` and `POST /api/v1/auth/logout` endpoints using standard ASP.NET Core Identity components and encrypted HttpOnly cookies.

#### Scenario: Successful login
- **WHEN** a client submits valid credentials to `POST /api/v1/auth/login`
- **THEN** HTTP 200 OK is returned with a `Set-Cookie` header containing an HttpOnly, SameSite=Lax session cookie
- **AND** the response body contains user profile information without exposing credentials or security stamps

#### Scenario: Invalid credentials rejection
- **WHEN** a client submits an incorrect password or unregistered email to `POST /api/v1/auth/login`
- **THEN** HTTP 401 Unauthorized is returned with a generic error message preventing account enumeration

#### Scenario: Account lockout
- **WHEN** 5 consecutive failed login attempts occur for a user account
- **THEN** the account is locked for 15 minutes and subsequent login attempts return HTTP 423 Locked or 401 with lockout notice

#### Scenario: Logout and session invalidation
- **WHEN** an authenticated user calls `POST /api/v1/auth/logout`
- **THEN** HTTP 204 No Content is returned, the session cookie is cleared, and subsequent requests with the old cookie are rejected with HTTP 401

### Requirement: Server-side Data Space authorization
Access to any Data Space resource SHALL require server-side verification that the authenticated actor possesses valid membership in `ownership.data_space_memberships`. Client-supplied Data Space IDs SHALL NOT be trusted without verification.

#### Scenario: Access to owned Data Space
- **WHEN** an authenticated user requests access to their own Personal Data Space
- **THEN** access is granted and `app.current_data_space_id` is set to the authorized Data Space ID for RLS enforcement

#### Scenario: Cross-tenant access denial
- **WHEN** User A attempts to access User B's Data Space
- **THEN** HTTP 403 Forbidden is returned and no cross-tenant data is read or modified

#### Scenario: Unauthenticated request
- **WHEN** a request without a valid session cookie attempts to access a protected Data Space
- **THEN** HTTP 401 Unauthorized is returned
