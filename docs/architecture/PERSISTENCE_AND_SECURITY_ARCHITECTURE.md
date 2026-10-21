# StudyAI Persistence & Security Architecture — Iteration 3

Status: Approved foundation with completed security remediation (ADR-001, ADR-003, ADR-016)

Target: StudyAI Campus V1  
Frameworks: ASP.NET Core (`net10.0`), EF Core 10, Npgsql 10, pgvector, PostgreSQL 16+

---

## 1. Overview & Architectural Boundaries

In accordance with Clean Architecture principles:
- **`StudyAI.Domain`**: Pure POCO domain models (`User`, `DataSpace`, `ContentSpace`, `KnowledgeItem`, `EmbeddingModel`, `StorageObject`, `Document`, `DocumentVersion`, `ProcessingRun`, `DocumentChunk`, `ChunkEmbedding`). No dependencies on EF Core, third-party libraries, or database annotations.
- **`StudyAI.Application`**: Exposes persistence abstraction `IApplicationDbContext` (providing `IQueryable<T>`), alongside tenant context interfaces `ICurrentDataSpaceAccessor` and `ICurrentDataSpaceContext`.
- **`StudyAI.Infrastructure`**: Implements persistence via `StudyAIDbContext`, EF Core fluent configurations, `CurrentDataSpaceAccessor` (`AsyncLocal<Guid?>`), and `TenantSessionInterceptor`.
- **`StudyAI.Api`**: Composition root registering persistence via `AddInfrastructurePersistence(configuration)`. Design-time scaffolding supported via `StudyAIDbContextFactory`.

---

## 2. Multi-Tenancy & Row-Level Security (RLS)

As approved in **ADR-003**:
1. **Server-Side Authorization**: Application authorization is mandatory. Client-supplied tenant IDs are never trusted as authorization.
2. **PostgreSQL Roles**:
   - An administrator-controlled bootstrap identity creates/rotates `studyai_migrator`, `studyai_app`, `studyai_worker`, and `studyai_ro` before migrations.
   - `studyai_migrator`: administrative migration identity; never used by the running API.
   - `studyai_app`: least-privilege runtime role granted DML (`SELECT, INSERT, UPDATE, DELETE`) only on the reviewed table allowlist. It has `NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS`, no object ownership, no migration-history access, no grant option, and no schema `CREATE`.
   - The repository applies explicit target-database grants; it does not claim control of provider/cluster-wide default ACLs. Deployment verifies catalogs after applying `infra/postgres/apply-runtime-privileges.sql`.
3. **Defense-in-Depth RLS Policies**:
   - The currently migrated high-risk tenant-scoped subset has `ROW LEVEL SECURITY` enabled and forced (`FORCE ROW LEVEL SECURITY`); future module tables require their own reviewed migration and tests.
   - Policy: `tenant_isolation_policy` applied `FOR ALL TO studyai_app`:
     ```sql
     USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
     WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);
     ```
   - **Fail-Closed Guarantee**: Missing, empty, or invalid tenant contexts yield `NULL`, denying all rows on reads and rejecting all inserts/updates with `42501`.

---

## 3. Transaction-Local Scope & Npgsql Connection Pooling

1. **Transaction-Local Setting**:
   `TenantSessionInterceptor` intercepts database transaction startup (`TransactionStarted` / `TransactionStartedAsync`) and executes:
   ```sql
   SELECT set_config('app.current_data_space_id', @space_id, true);
   ```
   Passing `is_local = true` binds the configuration strictly to the active transaction.
2. **Pooling Safety**:
   - Upon transaction `COMMIT` or `ROLLBACK`, PostgreSQL automatically clears the session variable.
   - Connections returned to the Npgsql connection pool retain zero tenant context.
   - Subsequent transactions without tenant context explicitly execute `set_config('app.current_data_space_id', '', true)`, guaranteeing fail-closed isolation across reused connections.

---

## 4. Optimistic Concurrency Control

In alignment with PRD invariants ("Never silently overwrite concurrent edits"):
- Native PostgreSQL `xmin` system column is mapped as an optimistic concurrency token on mutable entities (`User`, `KnowledgeItem`) via EF Core shadow property:
  ```csharp
  builder.Property<uint>("xmin")
      .HasColumnName("xmin")
      .IsRowVersion();
  ```
- Any concurrent modification from a stale snapshot causes EF Core to throw `DbUpdateConcurrencyException`, preventing silent data overwrites.

---

## 5. Incremental Migration & Schema Parity

1. **Migration Baseline**:
   - Stored in schema `ops` under table `__ef_migrations_history`.
   - Initial migration `20261020045112_InitialPersistenceBaseline` creates the approved 11-table coherent subset, schemas, extensions, triggers, RLS policies, and role grants.
   - Supported deployment pre-creates roles with `scripts/Initialize-DatabaseRoles.ps1`, supplies `STUDYAI_MIGRATION_CONNECTION_STRING`, applies forward migrations, then runs `scripts/Set-DatabaseRuntimePrivileges.ps1`.
   - Running the historical migration chain directly on an unbootstrapped cluster is unsupported; see `docs/development/DATABASE_SECURITY_BOOTSTRAP_RUNBOOK.md`.
2. **Coverage Accounting**:
   - **Covered (11 tables)**:
     - `identity.users`
     - `ownership.data_spaces`
     - `workspace.content_spaces`
     - `knowledge.knowledge_items`
     - `ai.embedding_models`
     - `documents.storage_objects`
     - `documents.documents`
     - `documents.document_versions`
     - `documents.processing_runs`
     - `documents.document_chunks`
     - `documents.chunk_embeddings`
   - **Remaining (57 tables)**: Academic courses, notes, note revisions, research, learning, writing, presentation, integrity, planner, billing, and ops audit tables will be incrementally introduced by subsequent domain modules.
