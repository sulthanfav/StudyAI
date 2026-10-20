# StudyAI Database Migration and Schema Parity Strategy

Status: **APPROVED & IMPLEMENTED (ADR-016)**  
Date: 2026-10-20  
Governing References: `StudyAI_Physical_Schema_Reviewed_v1.1.sql`; `DATABASE_RUNTIME_VALIDATION_REPORT.md`; `ADR-016`; `ADR-003`

---

## 1. Executive Summary

This document defines the transition strategy from the 68-table clean-slate reference schema (`StudyAI_Physical_Schema_Reviewed_v1.1.sql`) to production-grade Entity Framework Core incremental migrations. It addresses database bootstrap, advanced PostgreSQL extension support, schema parity verification, and disaster recovery runbooks.

---

## 2. Transition Architecture: Hybrid EF Core + Scripted Migrations

PostgreSQL 16 and `pgvector` introduce database-level invariants that standard C# EF Core fluent APIs cannot natively generate without custom SQL:
- Extension loading (`vector`, `pg_trgm`, `btree_gist`).
- Immutable snapshot triggers (`ops.reject_snapshot_update()`).
- GiST Exclusion constraints with `tstzrange` (`ex_subscription_one_active_per_space`).
- `FORCE ROW LEVEL SECURITY` and multi-tenant policies.
- Generated stored columns with `tsvector`.

### Multi-Tier Migration Lifecycle

```
[ Phase 0: Database Cluster Bootstrap ] (Run once by superuser / DBA)
  • CREATE EXTENSION vector, pg_trgm, btree_gist;
  • CREATE ROLE studyai_migrator, studyai_app, studyai_worker, studyai_ro;
       │
       ▼
[ Phase 1: EF Core Incremental Schema Migrations ] (Executed by studyai_migrator)
  • Schemas (14 application schemas)
  • Tables (68 canonical tables)
  • Standard Primary Keys, Foreign Keys, Unique Indexes, and Default Values
  • Model Snapshot maintenance in StudyAI.Infrastructure
       │
       ▼
[ Phase 2: PostgreSQL Feature SQL Migrations ] (Executed via migrationBuilder.Sql)
  • Exclusion constraints (EXCLUDE USING gist)
  • Row-Level Security policies & FORCE RLS statements
  • Triggers and PL/pgSQL guard functions
  • Vector indexing (HNSW / IVFFlat parameters)
```

---

## 3. Schema Parity Verification Pipeline

Before any EF Core migration is merged to `main`, it must undergo automated catalog comparison against `StudyAI_Physical_Schema_Reviewed_v1.1.sql`:

1. **Clean-Slate Comparison**:
   - Apply EF Core migration to an empty validation database.
   - Run `scripts/Verify-SchemaParity.ps1`.
   - Assert:
     - All 14 schemas exist.
     - Exactly 68 base tables exist.
     - 68 primary keys match.
     - All foreign key relationships and cascade rules (`ON DELETE RESTRICT`) match.
     - All immutability triggers and GiST exclusion constraints exist.
     - `pgvector` column types and dimensions match.

2. **Populated Fixture Rehearsal**:
   - Seed test fixtures representing real student workflows.
   - Execute forward migration.
   - Assert zero data truncation, constraint violations, or index invalidation.

---

## 4. Rollback Limitations and Disaster Recovery Runbook

Not all database migrations are safely reversible in production. Adding columns or indexes is reversible; dropping columns, restructuring tables, or migrating data types creates permanent data loss if rolled back.

### Production Forward-Fix Policy
1. **Never Run Down() in Production**:
   - If an EF Core migration fails mid-flight or causes runtime regression in production, do NOT execute automatic `Down()` rollback scripts against partially migrated live data.
2. **Mandatory Pre-Deployment Snapshot**:
   - Every production deployment must trigger a physical storage volume snapshot or `pg_dump -Fc` prior to applying migrations.
3. **Recovery Runbook**:
   - **Scenario A (Migration Failed During Deployment)**:
     1. Abort deployment pipeline.
     2. Restore database from pre-deployment snapshot.
     3. Restart API services pointing to the restored database.
     4. Triage failure in local/staging environment.
   - **Scenario B (Runtime Bug Detected Post-Migration)**:
     1. If data is intact, deploy a forward-fix migration (`NextMigration_Fix.cs`).
     2. If critical data corruption occurred, initiate point-in-time recovery (PITR) to the instant immediately preceding deployment.
