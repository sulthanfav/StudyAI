# File Catalog: Infrastructure, Tooling & Solution Root (`infra/`, `scripts/`, root configs)

Owning Module: `infra/`  
Module Documentation: [`../MODULE_INDEX.md`](../MODULE_INDEX.md)  
Status: **VERIFIED**

---

## 1. Solution & MSBuild Configuration

### `StudyAI.slnx`
- **Module**: `root`
- **Category**: Configuration (Solution)
- **Responsibility**: XML-based Visual Studio / .NET 10 Solution file grouping applications, modules, and test projects.
- **Verification Status**: `VERIFIED`

### `Directory.Build.props`
- **Module**: `root`
- **Category**: Configuration (Build Defaults)
- **Responsibility**: Repository-wide MSBuild properties (Nullable enabled, TreatWarningsAsErrors, deterministic builds, C# 14 language version / LangVersion=14.0).
- **Verification Status**: `VERIFIED`

### `Directory.Packages.props`
- **Module**: `root`
- **Category**: Configuration (Central Package Management)
- **Responsibility**: Central Package Management (CPM) pinfile defining exact versions for all NuGet packages across all projects.
- **Verification Status**: `VERIFIED`

### `global.json`
- **Module**: `root`
- **Category**: Configuration (SDK Pinning)
- **Responsibility**: Pins the .NET SDK feature band to `10.0.300` with `rollForward: latestFeature`.
- **Verification Status**: `VERIFIED`

### `.editorconfig`
- **Module**: `root`
- **Category**: Configuration (Code Style)
- **Responsibility**: Enforces formatting standards (indentation, line endings, C# code quality rules).
- **Verification Status**: `VERIFIED`

### `.env.example`
- **Module**: `root`
- **Category**: Configuration (Environment Template)
- **Responsibility**: Documents required environment variables with safe placeholder values; zero secrets.
- **Verification Status**: `VERIFIED`

### `.gitignore`
- **Module**: `root`
- **Category**: Configuration (Git Ignore)
- **Responsibility**: Prevents committing build outputs, binaries, secrets, `.env` files, temporary artifacts, and IDE caches.
- **Verification Status**: `VERIFIED`

---

## 2. Infrastructure & Docker (`infra/`)

### `infra/compose/compose.database.yml`
- **Module**: `infra/`
- **Category**: Infrastructure (Docker Compose)
- **Responsibility**: Defines local PostgreSQL 16 container with `pgvector`, `pg_trgm`, `btree_gist`, healthcheck, and persistent volume.
- **Verification Status**: `VERIFIED`

### `infra/postgres/verify-runtime.sql`
- **Module**: `infra/`
- **Category**: Script (SQL Verification)
- **Responsibility**: Runtime SQL script that verifies PostgreSQL extensions, tables, indices, and smoke test constraints.
- **Verification Status**: `VERIFIED`

### `infra/postgres/tenant-poc-setup.sql`
- **Module**: `infra/`
- **Category**: Script (SQL PoC Setup)
- **Responsibility**: Scaffolds isolated roles (`studyai_migrator`, `studyai_app`, `studyai_worker`, `studyai_ro`), grants, RLS policies, and seed data for tenant isolation testing.
- **Verification Status**: `VERIFIED`

### `infra/postgres/concurrency-poc-setup.sql`
- **Module**: `infra/`
- **Category**: Script (SQL Concurrency Setup)
- **Responsibility**: Seeds notes, quota balances, document versions, plans, and subscriptions for real multi-session concurrency testing.
- **Verification Status**: `VERIFIED`

---

## 3. Automation Scripts (`scripts/`)

### `scripts/Validate-Database.ps1`
- **Module**: `scripts/`
- **Category**: Script (Automation Runner)
- **Responsibility**: Cross-platform PowerShell automation script to spin up the local PostgreSQL container, apply schema, run smoke tests, verify dump/restore, and tear down.
- **Verification Status**: `VERIFIED`

### `scripts/Test-TenantIsolationPoC.ps1`
- **Module**: `scripts/`
- **Category**: Script (Test Runner)
- **Responsibility**: Executes 10 tenant isolation, role permission boundary, pooled connection reuse, and fail-closed security scenarios against PostgreSQL.
- **Verification Status**: `VERIFIED`

### `scripts/Test-CleanRoomPostgresBootstrap.ps1`
- **Module**: `scripts`
- **Category**: Database Security Acceptance Test
- **Responsibility**: Starts a uniquely named PostgreSQL container with a cryptographic ephemeral credential and tmpfs data directory, proves no inherited StudyAI roles/schemas or permissive PUBLIC schema ACL, and removes only resources created by that run.
- **Security / Architectural Notes**: Uses no retained volume, never prints the generated credential, and records container ownership before cleanup.
- **Verification Status**: `VERIFIED`

### `scripts/Initialize-DatabaseRoles.ps1`
- **Module**: `scripts`
- **Category**: Administrative Database Bootstrap
- **Responsibility**: Validates approved environment configuration and invokes the role bootstrap through `psql` without placing passwords in command arguments or logs.
- **Associated Code**: `infra/postgres/bootstrap-roles.sql`.
- **Security / Architectural Notes**: Intended to run before EF migrations; restores pre-existing libpq process variables after execution.
- **Verification Status**: `VERIFIED`

### `infra/postgres/bootstrap-roles.sql`
- **Module**: `infra/postgres`
- **Category**: Administrative SQL Bootstrap
- **Responsibility**: Idempotently pre-creates or rotates migrator, application, worker, and read-only roles from environment-backed psql variables.
- **Security / Architectural Notes**: Contains no credential literal and gives every service role non-admin flags with `NOBYPASSRLS`.
- **Verification Status**: `VERIFIED`

### `scripts/Set-DatabaseRuntimePrivileges.ps1`
- **Module**: `scripts`
- **Category**: Administrative Database Privilege Application
- **Responsibility**: Applies the reviewed runtime allowlist through `psql` using administrator configuration without emitting credentials.
- **Associated Code**: `infra/postgres/apply-runtime-privileges.sql`.
- **Verification Status**: `VERIFIED`

### `infra/postgres/apply-runtime-privileges.sql`
- **Module**: `infra/postgres`
- **Category**: Database Security Policy
- **Responsibility**: Replaces ambient runtime grants with explicit CONNECT, schema USAGE, and allowlisted table DML while denying migration history, DDL, TEMP, ownership, role membership, grant options, and unapproved tables.
- **Verification Status**: `VERIFIED`

### `scripts/Test-ConcurrencyInvariants.ps1`
- **Module**: `scripts/`
- **Category**: Script (Test Runner)
- **Responsibility**: Executes real multi-session concurrent operations for note edits, atomic quota allocations, duplicate processing runs, and subscription exclusion.
- **Verification Status**: `VERIFIED`

### `scripts/Verify-SchemaParity.ps1`
- **Module**: `scripts/`
- **Category**: Script (Verification Runner)
- **Responsibility**: Asserts catalog parity across all 14 schemas, 68 tables, primary keys, GiST constraints, triggers, and pgvector types against canonical SQL v1.1.
- **Verification Status**: `VERIFIED`

### `scripts/Verify-PersistenceSubsetParity.ps1`
- **Module**: `scripts/`
- **Category**: Script (Verification Runner)
- **Responsibility**: Verifies 8 schema parity checks on the 11-table persistence subset, RLS policies, immutability triggers, and pgvector types.
- **Verification Status**: `VERIFIED`

### `.config/dotnet-tools.json`
- **Module**: `root`
- **Category**: Configuration (Local .NET Tools)
- **Responsibility**: Local dotnet tool manifest pinning `dotnet-ef` CLI tooling for migrations.
- **Verification Status**: `VERIFIED`

### `scripts/docs/sync_codebase_index.py`
- **Module**: `scripts/docs/`
- **Category**: Script (Documentation Automation)
- **Responsibility**: Discovers tracked repository source files, detects uncataloged files, and generates the aggregate `FILE_INDEX.md`.
- **Verification Status**: `VERIFIED`

### `scripts/docs/validate_docs.py`
- **Module**: `scripts/docs/`
- **Category**: Script (Validation & CI)
- **Responsibility**: Validates documentation links, catalog path integrity, skill frontmatter, and reports documentation coverage with non-zero exit on failure.
- **Verification Status**: `VERIFIED`

