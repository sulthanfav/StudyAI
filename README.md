# StudyAI

StudyAI is an AI-powered Personal Academic Operating System for Indonesian university students. The Campus V1 product target retains all nine PRD modules: workspace and knowledge management, document intelligence, AI tutor, quiz/exam preparation, research, writing, presentation, academic integrity, and planner.

This repository has completed bootstrap and clean-slate PostgreSQL runtime validation and now contains the first ASP.NET Core host foundation. It still contains no application business logic, authentication, tenant-sensitive persistence, or production migrations.

## Start here

1. Read [docs/INDEX.md](docs/INDEX.md) for document authority and status.
2. Read [AGENTS.md](AGENTS.md) before making code or schema changes.
3. Review [docs/adr/OPEN_DECISIONS.md](docs/adr/OPEN_DECISIONS.md) before work that depends on an unresolved choice.
4. Follow [docs/plans/IMPLEMENTATION_ROADMAP.md](docs/plans/IMPLEMENTATION_ROADMAP.md).

## Repository layout

- apps/web — future Next.js and TypeScript client.
- apps/api/StudyAI.Api — ASP.NET Core composition root and host-operability endpoints.
- apps/mobile — reserved for the future React Native and Expo client; not part of the Web-first launch.
- src — Domain, Application, Infrastructure, and Contracts project boundaries for the .NET modular monolith.
- workers/document-processing — future Python extraction/processing worker.
- tests — current unit, architecture, and in-process API integration tests; later security, PostgreSQL, migration, end-to-end, and load suites remain gated.
- infra/compose and infra/postgres — local PostgreSQL validation runtime and catalog assertions.
- docs — product, architecture, database, ADR, and implementation planning sources.

Reserved empty directories do not imply that their application or provider has been implemented.

## Approved baseline

- Next.js with TypeScript; React Native with Expo later.
- C# with ASP.NET Core, API-first modular monolith, and independent workers.
- PostgreSQL 16+ target with pgvector and pg_trgm; EF Core with Npgsql.
- Private S3-compatible object storage.
- Docker/Compose for local development and portable deployment.

Specific auth, queue, RLS, AI/embedding, storage, payment, editor, quota, and production-hosting choices remain open ADRs.

## Current runtime

The 2026-10-19 validation used Docker Engine 29.8.2, Docker Compose 5.6.0, and the pinned `pgvector/pgvector:0.8.7-pg16-bookworm` image. PostgreSQL 16.15 with vector, pg_trgm, and btree_gist passed clean-slate schema execution, object assertions, all 10 supplied smoke scenarios, rollback verification, and an empty-fixture dump/restore check. See [the runtime validation report](docs/database/DATABASE_RUNTIME_VALIDATION_REPORT.md).

Runtime credentials and evidence are local ignored files. The Docker volume is persistent and is not removed by the validation script.

## Database warning

docs/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql is the current clean-slate runtime-tested candidate. It is not a production migration and has not passed RLS/security, true concurrent-session, incremental migration, populated-data restore, performance, or production-operability validation.

## Backend foundation

The repository pins .NET SDK `10.0.300` within the accepted `10.0.3xx` feature band and uses native Microsoft Testing Platform with xUnit v3. From the repository root:

```powershell
dotnet restore StudyAI.slnx --locked-mode
dotnet build StudyAI.slnx --no-restore
dotnet test --solution StudyAI.slnx --no-restore
dotnet run --project apps/api/StudyAI.Api/StudyAI.Api.csproj --no-restore
```

The Development launch profile listens on `http://localhost:5080`. Available host routes are:

- `GET /health/live` — process-only liveness; no external dependencies are probed.
- `GET /health/ready` — foundation readiness; no database, queue, storage, auth, or AI check exists yet.
- `GET /openapi/v1.json` — development-only OpenAPI document.

Configuration is validated at startup. Default structured logs contain bounded correlation and request metadata, not headers, query strings, bodies, credentials, signed URLs, prompts, notes, or document text. See [Backend Foundation — Iteration 1](docs/architecture/BACKEND_FOUNDATION.md) and [ADR-015](docs/adr/ADR-015_BACKEND_FOUNDATION_TOOLCHAIN.md).

This host is not tenant-secure, database-ready, security-certified, performance-tested, or production-ready. Health success proves only that the foundation process and registered foundation checks are healthy.

The foundation and persistence integration verification completed locked restore and a warning-free build, running 40 automated tests with 40 passed, 0 failed, and 0 skipped (including unit, architecture, API, PostgreSQL least-privilege, RLS tenant isolation, Npgsql connection-pooling, and optimistic concurrency tests). See [Persistence & Security Architecture — Iteration 3](docs/architecture/PERSISTENCE_AND_SECURITY_ARCHITECTURE.md).

To run persistence subset parity verification:
```powershell
powershell -File scripts/Verify-PersistenceSubsetParity.ps1
```

## Next milestone

Iteration 4: Authentication & Session Architecture (implementing approved ADR-001 with ASP.NET Core Identity, secure HttpOnly cookie web sessions, CSRF protections, explicit session revocation, and server-side Data Space authorization).

