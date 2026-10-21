# File Catalog: Test Suites (`tests/`)

Owning Module: `tests/`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## 1. Unit Tests (`tests/StudyAI.UnitTests`)

### `tests/StudyAI.Api.IntegrationTests/Support/AntiforgeryHttpClientExtensions.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test Support
- **Responsibility**: Submits state-changing API requests with matching antiforgery cookies and headers.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.UnitTests/StudyAI.UnitTests.csproj`
- **Module**: `tests/StudyAI.UnitTests`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Unit test project targeting `net10.0` with xUnit v3 and Microsoft Testing Platform.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.UnitTests/packages.lock.json`
- **Module**: `tests/StudyAI.UnitTests`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution for unit tests.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.UnitTests/Configuration/FoundationOptionsValidatorTests.cs`
- **Module**: `tests/StudyAI.UnitTests`
- **Category**: Test
- **Responsibility**: Unit tests for `FoundationOptionsValidator` verifying valid options pass and missing or malformed options fail while protecting secret values from being exposed in errors.
- **Associated Code**: `apps/api/StudyAI.Api/Configuration/FoundationOptionsValidator.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.UnitTests/Diagnostics/CorrelationIdPolicyTests.cs`
- **Module**: `tests/StudyAI.UnitTests`
- **Category**: Test
- **Responsibility**: Unit tests verifying correlation ID length bounds, ASCII-only character filtering, and valid GUID fallback generation.
- **Associated Code**: `apps/api/StudyAI.Api/Diagnostics/CorrelationIdPolicy.cs`.
- **Verification Status**: `VERIFIED`

---

## 2. Architecture Tests (`tests/StudyAI.ArchitectureTests`)

### `tests/StudyAI.ArchitectureTests/StudyAI.ArchitectureTests.csproj`
- **Module**: `tests/StudyAI.ArchitectureTests`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Architecture test project targeting `net10.0` with xUnit v3 and NetArchTest.Rules.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.ArchitectureTests/packages.lock.json`
- **Module**: `tests/StudyAI.ArchitectureTests`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution for architecture tests.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.ArchitectureTests/EndpointArchitectureTests.cs`
- **Module**: `tests/StudyAI.ArchitectureTests`
- **Category**: Test
- **Responsibility**: Enforces API architectural isolation: ensures HTTP endpoints and controllers do not inject or depend directly on DbContext types, requiring Application abstractions instead.
- **Associated Code**: `apps/api/StudyAI.Api/Endpoints/`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.ArchitectureTests/ProjectReferenceRulesTests.cs`
- **Module**: `tests/StudyAI.ArchitectureTests`
- **Category**: Test
- **Responsibility**: Enforces compile-time Clean Architecture boundary rules: Domain has no outward references; Application does not reference Infrastructure or Api; Contracts has zero dependencies.
- **Associated Code**: All projects in `src/` and `apps/`.
- **Verification Status**: `VERIFIED`

---

## 3. Integration Tests (`tests/StudyAI.Api.IntegrationTests`)

### `tests/StudyAI.Api.IntegrationTests/StudyAI.Api.IntegrationTests.csproj`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Configuration (Project Manifest)
- **Responsibility**: Host integration test project targeting `net10.0` with Microsoft.AspNetCore.Mvc.Testing.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/packages.lock.json`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Configuration (Lockfile)
- **Responsibility**: NuGet package lockfile ensuring deterministic dependency resolution for integration tests.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/HealthEndpointTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Tests HTTP `/health/live` and `/health/ready` endpoints, verifying status codes, JSON response formatting, and absence of leaked environment/exception info.
- **Associated Code**: `apps/api/StudyAI.Api/Health/FoundationHealthEndpoints.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/CorrelationAndLoggingTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Proves end-to-end correlation ID flow and verifies that sensitive canary tokens, authorization headers, and confidential notes are never leaked into structured log streams.
- **Associated Code**: `apps/api/StudyAI.Api/Diagnostics/CorrelationIdMiddleware.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/ConfigurationStartupTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Validates that host startup fails fast when required configuration sections are absent or invalid.
- **Associated Code**: `apps/api/StudyAI.Api/Configuration/FoundationOptions.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/ApiSurfaceTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies that no premature business or security endpoints are exposed in the baseline host before being approved.
- **Associated Code**: `apps/api/StudyAI.Api/Program.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Support/StudyAIApiFactory.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test Support
- **Responsibility**: Custom `WebApplicationFactory<Program>` test fixture providing in-memory test host with test log capturing.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Support/CapturingLoggerProvider.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test Support
- **Responsibility**: In-memory test logger provider capturing structured log entries for assertion.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Persistence/PersistenceFixture.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test Support
- **Responsibility**: Integration test fixture managing admin/app connections, seed data, and DbContext creation.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Persistence/DatabaseSecurityAndIsolationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: 12 automated integration tests verifying PostgreSQL runtime role DDL denial, RLS tenant isolation, cross-tenant blocking, connection pooling safety, transaction cleanup, fail-closed invalid context, EF mappings, migration data preservation, and optimistic concurrency.
- **Associated Code**: `src/StudyAI.Infrastructure/Persistence/StudyAIDbContext.cs`, `src/StudyAI.Infrastructure/Persistence/Interceptors/TenantSessionInterceptor.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/UserRegistrationIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Automated integration tests verifying atomic user registration, password policy, rollback on failure, and concurrency idempotency.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/AuthenticationAndSessionIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Automated integration tests verifying login, logout, cookie session lifecycle, account lockout, and antiforgery/CSRF validation.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/DataSpaceAuthorizationIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Automated integration tests verifying server-side Data Space ownership/membership authorization, cross-user denial, and multi-user isolation.
- **Verification Status**: `VERIFIED`

---

## 4. Script Tests (`tests/scripts/`)

### `tests/scripts/test_docs_tools.py`
- **Module**: `tests/scripts`
- **Category**: Test
- **Responsibility**: Unit tests for Python documentation indexing and validation tools (`sync_codebase_index.py`, `validate_docs.py`).
- **Associated Code**: `scripts/docs/sync_codebase_index.py`, `scripts/docs/validate_docs.py`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/AntiforgeryIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies missing, invalid, and valid antiforgery exchanges for register, login, and authenticated logout, including no-state-change assertions.
- **Associated Code**: `apps/api/StudyAI.Api/Security/AntiforgeryValidationFilter.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/AuthenticationRateLimitIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies independent rate-limiting policies for registration and login endpoints, stable 429 Problem Details, and trusted proxy header resolution.
- **Associated Code**: `apps/api/StudyAI.Api/Composition/AuthenticationRateLimiting.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/BrowserAuthConfigurationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies validation rules of BrowserAuthOptions: HTTPS requirements in production, explicit proxy origin allowlist, and rejection of wildcard CORS origins.
- **Associated Code**: `apps/api/StudyAI.Api/Composition/BrowserAuthOptions.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/BrowserProxyContractIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Validates that browser reverse-proxy semantics preserve SameSite/HttpOnly session cookies, antiforgery headers, HTTP status codes, and RFC 7807 Problem Details.
- **Associated Code**: `apps/api/StudyAI.Api/Endpoints/AuthEndpoints.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/RegistrationRaceIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Validates race safety of concurrent registration attempts for identical and case/whitespace-colliding emails, ensuring exactly one 201 Created and deterministic 409 Conflict.
- **Associated Code**: `src/StudyAI.Infrastructure/Identity/AuthService.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Auth/ServerSessionLifecycleIntegrationTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies server-verifiable session lifecycle: cookie replay rejection after logout, absolute session expiry, security stamp revocation, and multi-session isolation.
- **Associated Code**: `src/StudyAI.Infrastructure/Identity/SessionCookieValidator.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Persistence/SessionModelMetadataTests.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test
- **Responsibility**: Verifies database model constraints and metadata for user sessions: SHA-256 identifier hashing, absence of plaintext session secrets, and indexes.
- **Associated Code**: `src/StudyAI.Infrastructure/Persistence/Configurations/Identity/UserSessionConfiguration.cs`.
- **Verification Status**: `VERIFIED`

### `tests/StudyAI.Api.IntegrationTests/Support/AntiforgeryHttpClientExtensions.cs`
- **Module**: `tests/StudyAI.Api.IntegrationTests`
- **Category**: Test Support
- **Responsibility**: Extension methods for test HTTP clients to automate cookie jar handling, antiforgery token extraction from cookies, and header propagation.
- **Associated Tests**: Integration test suites in `tests/StudyAI.Api.IntegrationTests/Auth/`.
- **Verification Status**: `VERIFIED`
