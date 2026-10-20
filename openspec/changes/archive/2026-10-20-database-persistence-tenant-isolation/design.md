# Design: Database Persistence & Tenant Isolation

## 1. Technical Toolchain & Dependency Matrix

StudyAI API runs on .NET 10 (`net10.0`, SDK `10.0.300`, C# 14). To establish persistence while preserving the locked toolchain:

### Package Candidates & Compatibility
- `Npgsql.EntityFrameworkCore.PostgreSQL` & `Npgsql`:
  - Npgsql releases major versions aligned with .NET. For .NET 10, preview versions (e.g. `10.0.0-preview.*`) or the latest stable .NET 9 release (`9.0.3` or `9.0.4`) targeting `net9.0` with .NET 10 runtime compatibility.
  - In ASP.NET Core 10, EF Core 10 preview (`10.0.0-preview.*`) or EF Core 9 (`9.0.2`+) can run seamlessly.
  - Pin exact versions in `Directory.Packages.props` with locked mode (`--locked-mode`).
- `Pgvector.EntityFrameworkCore`:
  - Pgvector EF Core extension integrates `pgvector` with Npgsql `Vector` type for `vector(1536)` mapping.

### Central Package Management Pinning
```xml
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0-preview.1.25081.1" /> <!-- or 9.0.4 -->
<PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-preview.1.25081.1" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0-preview.1" /> <!-- or 9.0.4 -->
<PackageVersion Include="Pgvector.EntityFrameworkCore" Version="0.3.0" />
```

## 2. Database Context & Architecture Boundaries

Following the **Single Database Principle** mandated in root `AGENTS.md` Section 3:
- A single primary `DbContext` (`StudyAIDbContext`) lives in `apps/api/StudyAI.Infrastructure/Persistence/`.
- It registers entity configurations segregated by schema folder:
  - `Persistence/Configurations/Ai/`
  - `Persistence/Configurations/Billing/`
  - `Persistence/Configurations/Documents/`
  - `Persistence/Configurations/Identity/`
  - `Persistence/Configurations/Knowledge/`
  - `Persistence/Configurations/Learning/`
  - `Persistence/Configurations/Ops/`
  - `Persistence/Configurations/Ownership/`
  - `Persistence/Configurations/Planner/`
- `StudyAI.Domain` and `StudyAI.Application` define entity types and interface contracts (`IApplicationDbContext`), with zero direct reference to Npgsql or PostgreSQL-specific libraries.
- `StudyAI.Infrastructure` implements the database persistence interfaces and configures the PostgreSQL dialect, connection retry policies, and session interceptors.

## 3. PostgreSQL Runtime Role & Least-Privilege Model (ADR-003)

In production and non-local environments, the application must NEVER connect as a superuser or the schema owner:

```
[ PostgreSQL Superuser / postgres ] (Cluster administration only)
       │
       ▼
[ Schema Migrator Role: studyai_migrator ]
  - Owns schemas, tables, sequences, types, indexes, and RLS policies.
  - Full DDL rights (CREATE, ALTER, DROP).
  - Used strictly during CI/CD migration runs.
       │
       ▼
[ Application Runtime Role: studyai_app ]
  - DML rights only: SELECT, INSERT, UPDATE, DELETE on application tables.
  - USAGE on sequences and schemas.
  - NO DDL rights (cannot CREATE, ALTER, DROP tables or policies).
  - Subject to FORCE ROW LEVEL SECURITY (NO BYPASSRLS).
       │
       ▼
[ Background Worker Role: studyai_worker ]
  - Scoped DML rights on worker tables (`ops.*`, `documents.*`, `ai.*`).
  - Read-only on identity/learning tables.
  - Subject to RLS or explicit worker-lease scopes.
       │
       ▼
[ Read-Only Replica Role: studyai_ro ]
  - SELECT only for analytical queries, export, and read-replica workloads.
```

## 4. Tenant Isolation & Connection Pooling Architecture

### Client Trust Model
- The application server resolves the authenticated user's identity to authorized `data_space_id`(s).
- Any client-supplied `data_space_id` in a request header or parameter is treated solely as a requested workspace selector, validated server-side against the actor's membership in `ownership.data_space_membership`.

### Transaction-Local Session Context vs Pooling
In pooled connection environments (such as Npgsql's default connection pool), setting ambient session variables with `SET app.current_data_space_id = '...'` risks **cross-tenant data leakage** if a connection is returned to the pool without resetting or if a connection fails.

To guarantee zero leakage:
1. **Transaction-Local Parameters**:
   All tenant context must be set using transaction-local configuration:
   ```sql
   SELECT set_config('app.current_data_space_id', $1, true);
   ```
   The third parameter `true` scopes the variable strictly to the current transaction (`LOCAL`). When the transaction commits or rolls back, PostgreSQL automatically resets the setting to empty/null.
2. **Fail-Closed Policy**:
   If `app.current_data_space_id` is not set or empty, the RLS policy must evaluate to FALSE:
   ```sql
   CREATE POLICY tenant_isolation_policy ON knowledge.knowledge_item
       FOR ALL
       TO studyai_app
       USING (
           data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid
       );
   ```
   `current_setting(..., true)` returns NULL if the parameter has not been set in the transaction, causing `NULLIF(NULL, '')` to be NULL and the comparison to fail-closed.
3. **Table Enforcement**:
   ```sql
   ALTER TABLE knowledge.knowledge_item ENABLE ROW LEVEL SECURITY;
   ALTER TABLE knowledge.knowledge_item FORCE ROW LEVEL SECURITY;
   ```
   `FORCE ROW LEVEL SECURITY` ensures that even if a query is executed by the table owner or app role, RLS is unconditionally applied.

## 5. Multi-Session Concurrency Control Invariants

### 1. Note Concurrency (PRD / Architecture LOCKED-05)
- Notes use optimistic concurrency control via a `version` or `revision_number` column and historical append to `learning.note_revision`.
- Concurrent edits to the same note detect stale revision; the second transaction fails with an explicit 409 Conflict rather than silently overwriting.

### 2. Atomic Quota Allocation (Architecture LOCKED-08)
- AI quota allocation in `billing.ai_quota_allocation` reserves quota before chargeable work:
  ```sql
  UPDATE billing.ai_quota_allocation
  SET reserved_units = reserved_units + :requested_units,
      updated_at = NOW()
  WHERE data_space_id = :data_space_id
    AND period_start <= NOW() AND period_end > NOW()
    AND (allocated_units - consumed_units - reserved_units) >= :requested_units
  RETURNING id;
  ```
- If rows returned == 0, the reservation fails atomically without race conditions.
- On job completion, quota is settled (`consumed_units += actual; reserved_units -= requested`) or released (`reserved_units -= requested`).

### 3. Duplicate Document-Processing Prevention
- A document processing run in `ops.document_processing_run` ensures that concurrent workers cannot initiate duplicate processing for the same document version.
- Enforced by unique active lease / status constraint:
  `UNIQUE (document_id, document_version_id) WHERE status IN ('queued', 'processing')`.

## 6. Migration and Parity Strategy

- Transition from the reviewed clean-slate SQL to EF Core requires:
  1. EF Core Model Configuration matching all 68 tables, schemas, columns, constraints, and indexes in `StudyAI_Physical_Schema_Reviewed_v1.1.sql`.
  2. Incremental migration baseline generation.
  3. Parity verification tool that asserts 100% catalog match between clean-slate SQL execution and EF migration output.
  4. Non-reversible migration runbooks specifying backup before migration and forward-fix protocols.
