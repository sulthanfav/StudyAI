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
  - Request pipeline middleware (Correlation ID, structured JSON logging, redaction).
- **Public Boundaries**: HTTP REST endpoints on configured ports (`https://localhost:7001`, `http://localhost:5001`).
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
- **Purpose**: Pure domain layer containing enterprise business rules, entities, value objects, and domain events.
- **Responsibilities**:
  - Data Space boundaries and invariants.
  - Knowledge item domain models.
  - Note concurrency and revision logic.
  - Quota accounting rules.
- **Dependencies**: None (enforced by architecture tests).
- **File Catalog**: [`catalog/domain.md`](catalog/domain.md).

### `src/StudyAI.Application`
- **Purpose**: Application orchestration layer executing user use cases.
- **Responsibilities**:
  - Command and query handlers.
  - Orchestration of domain entities and repository interfaces.
  - Cross-cutting authorization and quota reservation pipelines.
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Contracts`.
- **File Catalog**: [`catalog/application.md`](catalog/application.md).

### `src/StudyAI.Infrastructure`
- **Purpose**: Adapters for external dependencies, persistence, and third-party systems.
- **Responsibilities**:
  - PostgreSQL persistence via EF Core and Npgsql.
  - S3-compatible private object storage client.
  - AI provider adapters (LLM gateways, embedding clients).
- **Dependencies**: `StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Contracts`.
- **File Catalog**: [`catalog/infrastructure.md`](catalog/infrastructure.md).

### `src/StudyAI.Contracts`
- **Purpose**: Shared public contracts, DTOs, and event schemas.
- **Responsibilities**:
  - Request and response models for public endpoints.
  - Serialization contracts.
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
- **Key Tests**: `ProjectReferenceRulesTests.cs`.
- **File Catalog**: [`catalog/tests.md`](catalog/tests.md).

### `tests/StudyAI.Api.IntegrationTests`
- **Purpose**: WebApplicationFactory-based host integration tests.
- **Key Tests**: `HealthEndpointTests.cs`, `CorrelationAndLoggingTests.cs`, `ConfigurationStartupTests.cs`, `ApiSurfaceTests.cs`.
- **File Catalog**: [`catalog/tests.md`](catalog/tests.md).

---

## 5. Infrastructure & Tooling (`infra/`, `scripts/`)

- `infra/compose/compose.database.yml`: Docker Compose definition for local PostgreSQL 16 with pgvector and pg_trgm.
- `infra/postgres/verify-runtime.sql`: PostgreSQL schema and smoke test verification script.
- `scripts/Validate-Database.ps1`: Automated PowerShell database validation runner.
- `scripts/docs/`: Cross-platform documentation indexing, sync, and validation tools.
- **File Catalog**: [`catalog/infra.md`](catalog/infra.md).
