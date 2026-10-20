# StudyAI Current Development State

Last updated: 2026-10-20  
Current Branch: `main`  
Build & Test Status: **Passing (55/55 tests passed)**  
Navigation: [`docs/START_HERE.md`](../START_HERE.md) | [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`docs/INDEX.md`](../INDEX.md)

---

## 1. Active Work & Tasks

| Task Identifier | Title / Scope | Owning Agent | Status | Next Immediate Step |
|---|---|---|---|---|
| **ACADEMIC-WORKSPACE-01** | Academic Workspace & Library Management (Phase 2 Iteration 5) | Antigravity / Team | Ready to Start | Scope course, project, folder, and placement domain operations on top of Personal Data Space. |
| **AUTH-DATA-SPACE-01** | Authentication, Authorization & Personal Data Space (Phase 2 Iteration 4) | Antigravity | Completed (55/55 Tests Passing) | Prepared for review and archiving. |
| **DB-PERSISTENCE-02** | Persistence Integration & Security Hardening (Phase 2 Iteration 3) | Antigravity | Completed | Milestone archived to `openspec/changes/archive/2026-10-20-database-persistence-tenant-isolation/`. |
| **DB-PERSISTENCE-01** | Database Persistence & Tenant Isolation PoC (Phase 2 Iteration 2) | Antigravity | Completed | Baseline PoC archived. |
| **BACKEND-FOUNDATION-01** | Modular Monolith Backend Foundation Iteration 1 | Codex | Archived | Milestone archived to `openspec/changes/archive/2026-10-20-backend-foundation-iteration-1/`. |

---

## 2. Recent Milestones Completed

- **Authentication, Authorization & Personal Data Space (Phase 2 Iteration 4)**:
  - ASP.NET Core Identity integrated without duplicating tables or violating canonical schema.
  - Added `ownership.data_space_memberships` domain entity, configuration, and EF Core migration `20261020053351_AddIdentityAndMembership`.
  - Atomic provisioning service (`IAuthService` / `AuthService`) implementing User -> Personal Data Space -> Owner Membership -> Personal Library in a single atomic transaction.
  - Secure HttpOnly cookie sessions, lockout handling, antiforgery (`/api/v1/auth/csrf` and `X-XSRF-TOKEN`), and session revocation.
  - Server-side Data Space authorization filter (`DataSpaceAuthorizationFilter`) binding trusted actor identity to data space membership and setting transaction-local PostgreSQL RLS context.
  - 15 automated integration tests in `StudyAI.Api.IntegrationTests/Auth/` covering registration, duplicate handling, transaction rollback, concurrency, login, invalid credentials, lockout, session invalidation, CSRF rejection, and multi-tenant isolation.
  - 55/55 automated tests passing across the entire solution.
- **Persistence Integration & Security Hardening (Phase 2 Iteration 3)**:
  - Product Owner approved ADR-001 (Auth), ADR-003 (Roles & RLS), ADR-016 (Persistence Strategy). Recorded in `docs/adr/`.
  - Pinned EF Core 10 (`10.0.13`), Npgsql (`10.0.3`), Pgvector (`0.3.0`) under Central Package Management and verified locked-mode restore.
  - Implemented pure POCO domain entities (`User`, `DataSpace`, `ContentSpace`, `KnowledgeItem`, `EmbeddingModel`, `StorageObject`, `Document`, `DocumentVersion`, `ProcessingRun`, `DocumentChunk`, `ChunkEmbedding`).
  - Implemented `StudyAIDbContext` and `IApplicationDbContext` with modular configurations and PostgreSQL native `xmin` optimistic concurrency tokens (`IsRowVersion()`).
  - Implemented `TenantSessionInterceptor` and `CurrentDataSpaceAccessor` for transaction-local `set_config('app.current_data_space_id', ..., true)` with fail-closed default and Npgsql connection pooling safety.
  - Scaffolding and execution of real incremental EF Core migration `20261020045112_InitialPersistenceBaseline` against disposable `studyai_persistence_test` database.
  - Verified 8/8 subset schema parity checks (`scripts/Verify-PersistenceSubsetParity.ps1`).
  - Added 12 comprehensive automated .NET integration tests in `StudyAI.Api.IntegrationTests/Persistence/` using real PostgreSQL.
  - 40/40 solution tests passing with 0 warnings and 0 errors.
- **Database Persistence & Tenant Isolation (Phase 2 Iteration 2)**:
  - Technical ADR recommendations authored: ADR-001 (Auth), ADR-003 (Roles & RLS), ADR-016 (Persistence).
  - OpenSpec change `database-persistence-tenant-isolation` scaffolded with proposal, design, specs, and tasks.
  - Previous milestone `backend-foundation-iteration-1` archived and synced to `openspec/specs/platform/api-host/spec.md`.
  - Dedicated PostgreSQL runtime test database `studyai_tenant_poc_20261020` established.
  - PostgreSQL role separation (`studyai_migrator`, `studyai_app`, `studyai_worker`, `studyai_ro`) implemented with non-bypass RLS and restricted permissions.
  - 10/10 Tenant Isolation PoC scenarios verified (`scripts/Test-TenantIsolationPoC.ps1`) including cross-space leakage prevention, connection-pooling safety, and fail-closed null context.
  - 6/6 Multi-Session Concurrency tests verified (`scripts/Test-ConcurrencyInvariants.ps1`) including optimistic note conflicts, atomic quota limits, duplicate run prevention, and GiST subscription exclusion.
  - 7/7 Schema Parity tests verified (`scripts/Verify-SchemaParity.ps1`) against canonical SQL v1.1.
  - Zero solution test regressions (28/28 passing).
- **Backend Foundation Iteration 1**:
  - ASP.NET Core foundation host (`apps/api/StudyAI.Api`).
  - Core modular monolith projects (`StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Infrastructure`, `StudyAI.Contracts`).
  - Host health contract (`/health/live`, `/health/ready`).
  - Correlation ID and safe structured JSON logging.
  - Startup options validation.
  - 28 automated tests passing (Unit, Architecture, Integration).
- **Multi-Agent Infrastructure v2.0**:
  - Root `AGENTS.md` and `docs/START_HERE.md`.
  - Mandatory Map Check rule (`.agents/rules/mandatory-map-check.md`).
  - Living System Map (`SYSTEM_MAP.md`).
  - Hierarchical Codebase Intelligence (`MODULE_INDEX.md`, `FEATURE_MAP.md`, `FILE_INDEX.md`, `catalog/`).
  - Portable workflow skills (`brainstorming`, `writing-plans`, `executing-plans`, `codebase-navigation`, `documentation-maintenance`, `agent-handoff`).
  - Specialized review roles in `.agents/roles/`.

---

## 3. Known Blockers & Unverified Areas

- **ADR Approvals Required**: ADR-001 (Auth Architecture), ADR-003 (Roles & RLS), and ADR-016 (Persistence Strategy) require Product Owner sign-off before scaffolding production migrations.
- **External Providers**: Authentication, S3 storage client, and LLM gateway adapters are currently stubbed/unimplemented.
- **Frontend App**: `apps/web` is reserved and awaits Next.js initialization.

---

## 4. Environment & Health Summary

- **.NET SDK**: `10.0.300` (pinned via `global.json`), C# 14 (`LangVersion=14.0`).
- **Test Framework**: Native Microsoft Testing Platform (MTP) with xUnit v3 (`dotnet test --solution StudyAI.slnx --no-restore`).
- **Python Runtime**: `3.8.10` (used for documentation tooling in `scripts/docs/`).
- **Docker Compose**: `infra/compose/compose.database.yml` ready for PostgreSQL 16.
