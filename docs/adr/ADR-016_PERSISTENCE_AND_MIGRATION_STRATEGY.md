# ADR-016: Persistence Framework, Single DbContext, and Incremental Migration Strategy

Status: **ACCEPTED**  
Decision Date: 2026-10-20  
Decision Owner: Product Owner / Technical Owner  
Governing References: Core Architecture v1.2 LOCKED-01, LOCKED-04, LOCKED-05, LOCKED-08; Database Review v1.1; ADR-015

---

## 1. Context

StudyAI Campus V1 requires an authoritative, maintainable data access layer that bridges the reviewed 68-table physical database schema (`StudyAI_Physical_Schema_Reviewed_v1.1.sql`) with the ASP.NET Core modular monolith backend. The strategy must satisfy:
1. Compatibility with .NET 10 (`net10.0`, SDK `10.0.300`, C# 14).
2. The Single Database Principle (one primary DbContext).
3. Exact package version pinning under Central Package Management (CPM).
4. Support for PostgreSQL extensions (`vector`, `pg_trgm`, `btree_gist`).
5. Incremental, traceable migrations rather than a monolithic clean-slate script.
6. Schema parity verification and forward-fix disaster recovery runbooks.

---

## 2. Decision

1. **Persistence Stack & Package Versions**:
   - Framework: Entity Framework Core 10.
   - Provider: Npgsql.EntityFrameworkCore.PostgreSQL.
   - Vector Extension: Pgvector.EntityFrameworkCore.
   - Version Pinning in `Directory.Packages.props`:
     - `Microsoft.EntityFrameworkCore`: `10.0.13`
     - `Microsoft.EntityFrameworkCore.Relational`: `10.0.13`
     - `Microsoft.EntityFrameworkCore.Design`: `10.0.13`
     - `Npgsql.EntityFrameworkCore.PostgreSQL`: `10.0.3`
     - `Pgvector.EntityFrameworkCore`: `0.3.0`

2. **Single Primary DbContext**:
   - Maintain exactly one primary `DbContext` (`StudyAIDbContext`) in `StudyAI.Infrastructure.Persistence`.
   - Entity configurations are organized in subdirectories matching PostgreSQL schemas (`Identity/`, `Ownership/`, `Workspace/`, `Knowledge/`, `Documents/`, etc.).
   - `StudyAI.Domain` and `StudyAI.Application` remain completely free of EF Core and Npgsql dependencies.

3. **Incremental Migration Lifecycle**:
   - Begin with a minimal, coherent schema subset (`identity`, `ownership`, `workspace`, `knowledge`, `documents`).
   - Standard relational entities are generated via EF Core migrations.
   - Specialized PostgreSQL features (pgvector index parameters, immutability triggers, GiST exclusion constraints, RLS policies) are integrated via explicit `migrationBuilder.Sql(...)` calls.
   - Monolithic clean-slate embedding is strictly prohibited.

4. **Schema Parity Testing & Quality Gate**:
   - Automated catalog comparison asserts 100% parity between generated EF migrations and the reviewed physical SQL schema.
   - All migrations must be rehearsed against populated test fixtures.

5. **Disaster Recovery & Rollback Runbook**:
   - Development/Staging: Rehearsal of `Down()` rollback scripts.
   - Production: **Forward-fix only**. Destructive migrations are never reversed with `Down()` on live data; rollbacks are handled via pre-deployment database snapshots and point-in-time recovery (PITR).

---

## 3. Consequences

- **Positive**: Clean Architecture boundaries preserved, deterministic reproducible dependencies, official .NET 10 pgvector support, safety against catastrophic schema drift.
- **Negative / Constraints**: Generating EF Core migrations requires design-time factory setup and maintenance of custom SQL for advanced PostgreSQL triggers and exclusion constraints.

---

## 4. Verified Administrative Migration Clarification (2026-10-20)

- `StudyAIDbContextFactory` has no fallback credential. Migration tooling requires `STUDYAI_MIGRATION_CONNECTION_STRING` from approved administrator configuration.
- The supported order is: pre-create/rotate roles, apply forward EF migrations with an administrative/migrator connection, then replace runtime grants with the reviewed explicit allowlist. The complete procedure is in `docs/development/DATABASE_SECURITY_BOOTSTRAP_RUNBOOK.md`.
- Direct execution of the historical migration chain on a cluster without a pre-created `studyai_app` role is unsupported because migrations grant privileges to administrator-provisioned roles. Historical migrations remain credential-free; role secrets are supplied and rotated only through the administrator bootstrap workflow.
- Forward migration `20261020114606_AuthDatabaseSecurityRemediation` adds server-verifiable sessions, a read-only normalized-email collision preflight plus authoritative expression index, and a validated membership-role CHECK. A populated isolated upgrade preserved the fixture checksum; collision preflight failure preserved all rows and migration history.
- Repository migrations and scripts do not manage provider-wide/default ACLs. Privileges are asserted from PostgreSQL catalogs after deployment rather than inferred from role names or environment defaults.
