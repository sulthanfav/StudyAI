# ADR-016 Recommendation: Persistence Framework, Single DbContext, and Migration Parity Strategy

Status: **PROPOSED / PENDING APPROVAL**  
Date: 2026-10-20  
Governing references: Core Architecture v1.2 LOCKED-01, LOCKED-04, LOCKED-05, LOCKED-08; Database Review v1.1; ADR-015

---

## 1. Executive Summary

This document proposes the persistence framework selection, DbContext architecture, dependency pinning, and migration strategy for StudyAI Campus V1. It defines how the application transitions safely from the clean-slate SQL candidate (`StudyAI_Physical_Schema_Reviewed_v1.1.sql`) into maintainable, version-controlled EF Core incremental migrations without losing database invariants.

---

## 2. Core Decisions & Recommendations

### 1. Single Primary DbContext Principle
- **Single DbContext**: In alignment with Core Architecture v1.2, StudyAI will use **one primary `DbContext` (`StudyAIDbContext`)** located in `StudyAI.Infrastructure.Persistence`.
- **Modular Entity Configurations**: Mappings are decoupled across subdirectories matching PostgreSQL schemas:
  - `Persistence/Configurations/Ai/`
  - `Persistence/Configurations/Billing/`
  - `Persistence/Configurations/Documents/`
  - `Persistence/Configurations/Identity/`
  - `Persistence/Configurations/Knowledge/`
  - `Persistence/Configurations/Learning/`
  - `Persistence/Configurations/Ops/`
  - `Persistence/Configurations/Ownership/`
  - `Persistence/Configurations/Planner/`
- **Application Interface**: `StudyAI.Application` defines `IApplicationDbContext` exposing only `IQueryable` / `DbSet` sets needed for query orchestration, preserving Clean Architecture boundaries.

### 2. Dependency Pinning & Compatibility
Based on verification against the repository's `.NET 10` (`net10.0`, SDK `10.0.300`) toolchain:
- `Microsoft.EntityFrameworkCore`: Pin to `10.0.13` (matches ASP.NET Core `10.0.13` already in `Directory.Packages.props`).
- `Microsoft.EntityFrameworkCore.Relational`: Pin to `10.0.13`.
- `Microsoft.EntityFrameworkCore.Design`: Pin to `10.0.13`.
- `Npgsql.EntityFrameworkCore.PostgreSQL`: Pin to `10.0.3` (official .NET 10 driver release).
- `Pgvector.EntityFrameworkCore`: Pin to `0.3.0` (supports `vector` column mappings).

### 3. Migration Strategy: Hybrid EF Core + Raw SQL
PostgreSQL 16 features like `pgvector` indexing (`hnsw` / `ivfflat`), trigger functions (`prevent_orphaned_library_item`), `FORCE ROW LEVEL SECURITY`, and custom check constraints are not fully expressible in fluent C# API alone.
- **Relational Tables & Columns**: Handled via standard EF Core migrations.
- **PostgreSQL-Specific Extensions & Policies**: Handled via explicit `migrationBuilder.Sql(...)` blocks inside versioned migrations or accompanying `.sql` migration scripts.
- **No Monolithic Clean-Slate Embedding**: Migrations must be modular, progressive, and traceable rather than pasting the 68-table script into a single opaque migration.

### 4. Schema Parity Verification
- Before any EF Core migration is accepted into `main`, an automated parity test must compare the EF Core schema metadata against `StudyAI_Physical_Schema_Reviewed_v1.1.sql`.
- The test verifies:
  1. All 14 schemas exist.
  2. All 68 tables exist with identical column names and data types.
  3. Primary keys, foreign keys, unique constraints, and check constraints match.
  4. Vector dimensions (`vector(1536)`) and text-search indexes match.

### 5. Recovery & Rollback Runbook
- **Destructive Migrations (e.g. column drops, table purges)**: Cannot be reversed safely in production.
- **Rollback Policy**:
  - Development / Staging: Reversible migrations (`Down()`) are rehearsed.
  - Production: Forward-fix only. Every release must be preceded by a point-in-time PostgreSQL snapshot / physical backup. If a migration fails mid-deployment, the database is restored from backup rather than executing untested `Down()` methods on corrupted state.
