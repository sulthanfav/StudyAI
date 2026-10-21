# StudyAI Codebase Module Index

This index categorizes all applications, modules, libraries, and test suites in the StudyAI Campus monorepo.

Navigation: [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`docs/START_HERE.md`](../START_HERE.md) | [`FEATURE_MAP.md`](FEATURE_MAP.md) | [`FILE_INDEX.md`](FILE_INDEX.md)

---

## 1. Applications (`apps/`)

### `apps/api/StudyAI.Api`
- **Purpose**: ASP.NET Core foundation host and entry point for the modular monolith HTTP API.
- **Responsibilities**:
  - Host configuration, options validation, and service collection composition.
  - Health check endpoints (`/health/live`, `/health/ready`).
  - Authentication & DataSpace endpoints (`/api/v1/auth`, `/api/v1/data-spaces`).
  - Antiforgery CSRF endpoint filters and authentication rate limiting.
  - Request-owned tenant transaction and actor RLS boundary (`DataSpaceAuthorizationFilter`).
  - Request pipeline middleware (Correlation ID, structured JSON logging, redaction).
- **Public Boundaries**: HTTP REST endpoints on configured port (`http://localhost:5080` per `launchSettings.json`).
- **Entry Points**: [`apps/api/StudyAI.Api/Program.cs`](../../apps/api/StudyAI.Api/Program.cs).
- **Associated Tests**: [`tests/StudyAI.Api.IntegrationTests`](../../tests/StudyAI.Api.IntegrationTests/).
- **File Catalog**: [`catalog/api-host.md`](catalog/api-host.md).

### `apps/web` (Planned V1)
- **Purpose**: Web frontend for students and educators.
- **Planned Stack**: Next.js 15, TypeScript, React 19, Vanilla CSS.
- **Status**: Scaffolded directory for V1 frontend implementation.

### `apps/mobile` (Future Target)
- **Purpose**: iOS and Android mobile client.
- **Planned Stack**: React Native, Expo.
- **Status**: Reserved directory for post-V1 mobile app.

---

## 2. Core Modules (`src/`)

### `src/StudyAI.Domain`
- **Purpose**: Pure domain layer for enterprise business rules, entities, value objects, and domain events.
- **Current Status**: Foundation project scaffold (`StudyAI.Domain.csproj`, `packages.lock.json`).
- **Target Planned Responsibilities**: Data Space boundaries, Knowledge item models, Note concurrency, Quota accounting.
- **Dependencies**: None (enforced by architecture tests).
- **File Catalog**: [`catalog/domain.md`](catalog/domain.md).

### `src/StudyAI.Application`
- **Purpose**: Application orchestration layer executing user use cases.
- **Current Status**: Foundation project scaffold (`StudyAI.Application.csproj`, `packages.lock.json`).
- **Target Planned Responsibilities**: Command/query handlers, domain orchestration, cross-cutting quota/auth pipelines.
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Contracts`.
- **File Catalog**: [`catalog/application.md`](catalog/application.md).

### `src/StudyAI.Infrastructure`
- **Purpose**: Adapters for external dependencies, persistence, and third-party systems.
- **Current Status**: Foundation project scaffold (`StudyAI.Infrastructure.csproj`, `packages.lock.json`).
- **Target Planned Responsibilities**: PostgreSQL persistence via EF Core and Npgsql, S3 object storage client, AI provider adapters (planned for subsequent iterations; zero provider code in current Iteration 1).
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Contracts`.
- **File Catalog**: [`catalog/infrastructure.md`](catalog/infrastructure.md).

### `src/StudyAI.Contracts`
- **Purpose**: Shared public contracts, DTOs, and event schemas.
- **Current Status**: Foundation project scaffold (`StudyAI.Contracts.csproj`, `packages.lock.json`).
- **Target Planned Responsibilities**: Public endpoint request/response models and serialization contracts.
- **Dependencies**: None.
- **File Catalog**: [`catalog/contracts.md`](catalog/contracts.md).

---

## 3. Background Workers (`workers/`)

### `workers/document-processing` (Planned V1)
- **Purpose**: Independent Python/FastAPI/Celery or .NET worker for document extraction, PDF parsing, OCR, and embedding chunking.
- **Status**: Reserved directory; design documented in Core Architecture v1.2.

---

## 4. Test Suites (`tests/`)

### `tests/StudyAI.UnitTests`
- **Purpose**: Fast unit tests for isolated domain rules, options validation, and diagnostic policies.
- **Key Tests**: `FoundationOptionsValidatorTests.cs`, `CorrelationIdPolicyTests.cs`.
- **File Catalog**: [`catalog/tests.md`](catalog/tests.md).

### `tests/StudyAI.ArchitectureTests`
- **Purpose**: NetArchTest / ArchUnit-style tests enforcing Clean Architecture project dependency rules.
- **Key Tests**: `ProjectReferenceRulesTests.cs`, `EndpointArchitectureTests.cs`.
- **File Catalog**: [`catalog/tests.md`](catalog/tests.md).

### `tests/StudyAI.Api.IntegrationTests`
- **Purpose**: WebApplicationFactory-based host integration tests with real PostgreSQL persistence.
- **Key Tests**:
  - Foundation: `HealthEndpointTests.cs`, `CorrelationAndLoggingTests.cs`, `ConfigurationStartupTests.cs`, `ApiSurfaceTests.cs`.
  - Authentication & Sessions: `AuthenticationAndSessionIntegrationTests.cs`, `ServerSessionLifecycleIntegrationTests.cs`, `AntiforgeryIntegrationTests.cs`, `AuthenticationRateLimitIntegrationTests.cs`, `RegistrationRaceIntegrationTests.cs`.
  - Tenant & Persistence: `DatabaseSecurityAndIsolationTests.cs`, `DataSpaceAuthorizationIntegrationTests.cs`, `SessionModelMetadataTests.cs`.
  - Browser Contract: `BrowserAuthConfigurationTests.cs`, `BrowserProxyContractIntegrationTests.cs`.
- **File Catalog**: [`catalog/tests.md`](catalog/tests.md).

---

## 5. Infrastructure & Tooling (`infra/`, `scripts/`)

- `infra/compose/compose.database.yml`: Docker Compose definition for local PostgreSQL 16 with pgvector and pg_trgm.
- `infra/postgres/verify-runtime.sql`: PostgreSQL schema and smoke test verification script.
- `scripts/Validate-Database.ps1`: Automated PowerShell database validation runner.
- `scripts/docs/`: Cross-platform documentation indexing, sync, and validation tools.
- **File Catalog**: [`catalog/infra.md`](catalog/infra.md).
