# StudyAI Master Codebase File Index

Last synchronized: 2026-10-19  
Generator: `scripts/docs/sync_codebase_index.py`  
Navigation: [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`MODULE_INDEX.md`](MODULE_INDEX.md) | [`FEATURE_MAP.md`](FEATURE_MAP.md)

---

## 1. Summary of Documentation Coverage

| Module | Catalog File | Cataloged Files | Verified | Needs Review | Coverage |
|---|---|---|---|---|---|
| **API Host** | [`catalog/api-host.md`](catalog/api-host.md) | 14 | 14 | 0 | 100% |
| **Application** | [`catalog/application.md`](catalog/application.md) | 2 | 2 | 0 | 100% |
| **Contracts** | [`catalog/contracts.md`](catalog/contracts.md) | 2 | 2 | 0 | 100% |
| **Domain** | [`catalog/domain.md`](catalog/domain.md) | 2 | 2 | 0 | 100% |
| **Infra & Tooling** | [`catalog/infra.md`](catalog/infra.md) | 12 | 12 | 0 | 100% |
| **Infrastructure** | [`catalog/infrastructure.md`](catalog/infrastructure.md) | 2 | 2 | 0 | 100% |
| **Test Suites** | [`catalog/tests.md`](catalog/tests.md) | 16 | 16 | 0 | 100% |
| **Total Baseline** | — | **50** | **50** | **0** | **100%** |

---

## 2. Master File Catalog

| File Path | Module | Category | Status | Catalog Reference |
|---|---|---|---|---|
| `.editorconfig` | `root` | Configuration (Code Style) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `.env.example` | `root` | Configuration (Environment Template) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `.gitignore` | `root` | Configuration (Git Ignore) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `Directory.Build.props` | `root` | Configuration (Build Defaults) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `Directory.Packages.props` | `root` | Configuration (Central Package Management) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `StudyAI.slnx` | `root` | Configuration (Solution) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `apps/api/StudyAI.Api/Composition/FoundationServiceCollectionExtensions.cs` | `apps/api/StudyAI.Api` | Source (Service Registration) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Configuration/FoundationOptions.cs` | `apps/api/StudyAI.Api` | Source (Options Model) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Configuration/FoundationOptionsValidator.cs` | `apps/api/StudyAI.Api` | Source (Validation) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Diagnostics/CorrelationIdMiddleware.cs` | `apps/api/StudyAI.Api` | Source (HTTP Middleware) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Diagnostics/CorrelationIdPolicy.cs` | `apps/api/StudyAI.Api` | Source (Policy Utility) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Health/FoundationHealthEndpoints.cs` | `apps/api/StudyAI.Api` | Source (Routing Endpoints) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Health/SafeHealthResponseWriter.cs` | `apps/api/StudyAI.Api` | Source (HTTP Response Formatter) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Program.cs` | `apps/api/StudyAI.Api` | Source (Host Entry Point) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/Properties/launchSettings.json` | `apps/api/StudyAI.Api` | Configuration (Debug Profiles) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/StudyAI.Api.csproj` | `apps/api/StudyAI.Api` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/StudyAI.Api.http` | `apps/api/StudyAI.Api` | Tooling (HTTP Scratchpad) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/appsettings.Development.json` | `apps/api/StudyAI.Api` | Configuration | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/appsettings.json` | `apps/api/StudyAI.Api` | Configuration | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `apps/api/StudyAI.Api/packages.lock.json` | `apps/api/StudyAI.Api` | Configuration (Lockfile) | `VERIFIED` | [`catalog/api-host.md`](catalog/api-host.md) |
| `global.json` | `root` | Configuration (SDK Pinning) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `infra/compose/compose.database.yml` | `infra/` | Infrastructure (Docker Compose) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `infra/postgres/verify-runtime.sql` | `infra/` | Script (SQL Verification) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `scripts/Validate-Database.ps1` | `scripts/` | Script (Automation Runner) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `scripts/docs/sync_codebase_index.py` | `scripts/docs/` | Script (Documentation Automation) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `scripts/docs/validate_docs.py` | `scripts/docs/` | Script (Validation & CI) | `VERIFIED` | [`catalog/infra.md`](catalog/infra.md) |
| `src/StudyAI.Application/StudyAI.Application.csproj` | `src/StudyAI.Application` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/application.md`](catalog/application.md) |
| `src/StudyAI.Application/packages.lock.json` | `src/StudyAI.Application` | Configuration (Lockfile) | `VERIFIED` | [`catalog/application.md`](catalog/application.md) |
| `src/StudyAI.Contracts/StudyAI.Contracts.csproj` | `src/StudyAI.Contracts` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/contracts.md`](catalog/contracts.md) |
| `src/StudyAI.Contracts/packages.lock.json` | `src/StudyAI.Contracts` | Configuration (Lockfile) | `VERIFIED` | [`catalog/contracts.md`](catalog/contracts.md) |
| `src/StudyAI.Domain/StudyAI.Domain.csproj` | `src/StudyAI.Domain` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/domain.md`](catalog/domain.md) |
| `src/StudyAI.Domain/packages.lock.json` | `src/StudyAI.Domain` | Configuration (Lockfile) | `VERIFIED` | [`catalog/domain.md`](catalog/domain.md) |
| `src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj` | `src/StudyAI.Infrastructure` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/infrastructure.md`](catalog/infrastructure.md) |
| `src/StudyAI.Infrastructure/packages.lock.json` | `src/StudyAI.Infrastructure` | Configuration (Lockfile) | `VERIFIED` | [`catalog/infrastructure.md`](catalog/infrastructure.md) |
| `tests/StudyAI.Api.IntegrationTests/ApiSurfaceTests.cs` | `tests/StudyAI.Api.IntegrationTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/ConfigurationStartupTests.cs` | `tests/StudyAI.Api.IntegrationTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/CorrelationAndLoggingTests.cs` | `tests/StudyAI.Api.IntegrationTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/HealthEndpointTests.cs` | `tests/StudyAI.Api.IntegrationTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/StudyAI.Api.IntegrationTests.csproj` | `tests/StudyAI.Api.IntegrationTests` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/Support/CapturingLoggerProvider.cs` | `tests/StudyAI.Api.IntegrationTests` | Test Support | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/Support/StudyAIApiFactory.cs` | `tests/StudyAI.Api.IntegrationTests` | Test Support | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.Api.IntegrationTests/packages.lock.json` | `tests/StudyAI.Api.IntegrationTests` | Configuration (Lockfile) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.ArchitectureTests/ProjectReferenceRulesTests.cs` | `tests/StudyAI.ArchitectureTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.ArchitectureTests/StudyAI.ArchitectureTests.csproj` | `tests/StudyAI.ArchitectureTests` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.ArchitectureTests/packages.lock.json` | `tests/StudyAI.ArchitectureTests` | Configuration (Lockfile) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.UnitTests/Configuration/FoundationOptionsValidatorTests.cs` | `tests/StudyAI.UnitTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.UnitTests/Diagnostics/CorrelationIdPolicyTests.cs` | `tests/StudyAI.UnitTests` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.UnitTests/StudyAI.UnitTests.csproj` | `tests/StudyAI.UnitTests` | Configuration (Project Manifest) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/StudyAI.UnitTests/packages.lock.json` | `tests/StudyAI.UnitTests` | Configuration (Lockfile) | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
| `tests/scripts/test_docs_tools.py` | `tests/scripts` | Test | `VERIFIED` | [`catalog/tests.md`](catalog/tests.md) |
