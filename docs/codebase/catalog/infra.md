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
- **Responsibility**: Repository-wide MSBuild properties (Nullable enabled, TreatWarningsAsErrors, deterministic builds, C# 13 language version).
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

---

## 3. Automation Scripts (`scripts/`)

### `scripts/Validate-Database.ps1`
- **Module**: `scripts/`
- **Category**: Script (Automation Runner)
- **Responsibility**: Cross-platform PowerShell automation script to spin up the local PostgreSQL container, apply schema, run smoke tests, verify dump/restore, and tear down.
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
