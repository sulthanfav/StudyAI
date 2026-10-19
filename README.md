# StudyAI

StudyAI is an AI-powered Personal Academic Operating System for Indonesian university students. The Campus V1 product target retains all nine PRD modules: workspace and knowledge management, document intelligence, AI tutor, quiz/exam preparation, research, writing, presentation, academic integrity, and planner.

This repository has completed bootstrap and clean-slate PostgreSQL runtime validation. It contains approved product and logical architecture documentation plus a runtime-tested database candidate; it does not yet contain application business logic or production migrations.

## Start here

1. Read [docs/INDEX.md](docs/INDEX.md) for document authority and status.
2. Read [AGENTS.md](AGENTS.md) before making code or schema changes.
3. Review [docs/adr/OPEN_DECISIONS.md](docs/adr/OPEN_DECISIONS.md) before work that depends on an unresolved choice.
4. Follow [docs/plans/IMPLEMENTATION_ROADMAP.md](docs/plans/IMPLEMENTATION_ROADMAP.md).

## Repository layout

- apps/web — future Next.js and TypeScript client.
- apps/api — future ASP.NET Core API host.
- apps/mobile — reserved for the future React Native and Expo client; not part of the Web-first launch.
- src — future .NET modular-monolith projects and shared contracts.
- workers/document-processing — future Python extraction/processing worker.
- tests — future unit, integration, security, end-to-end, migration, and load tests.
- infra/compose and infra/postgres — local PostgreSQL validation runtime and catalog assertions.
- docs — product, architecture, database, ADR, and implementation planning sources.

Empty implementation directories are intentional reservations only; no framework has been scaffolded.

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

## Next milestone

Close the remaining Phase 1 gates—especially runtime roles/RLS, true concurrent-session tests, migration strategy, and the relevant ADRs—before beginning the gated Phase 2 ASP.NET Core/EF Core foundation. Do not treat the clean-slate SQL as a production migration.

