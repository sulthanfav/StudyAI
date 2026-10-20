# Proposal: Database Persistence & Tenant Isolation

## 1. Context and Problem Statement

Backend Foundation Iteration 1 established the modular monolith architecture (`StudyAI.Domain`, `StudyAI.Application`, `StudyAI.Infrastructure`, `StudyAI.Contracts`, `StudyAI.Api`), host diagnostics, correlation middleware, and health contracts. The clean-slate schema candidate (`StudyAI_Physical_Schema_Reviewed_v1.1.sql`) also passed initial runtime validation and transactional smoke tests in PostgreSQL 16.15.

However, before implementing authentication (Phase 2 Iteration 3) and business modules, StudyAI Campus V1 requires an authoritative persistence and tenant-isolation foundation. Currently:
- Entity Framework Core, Npgsql, and pgvector package dependencies and configurations are unpinned.
- Database access roles, least-privilege permissions, and RLS defense-in-depth policies have not been validated against real connection pooling.
- Cross-tenant data leakage risks (e.g. User A context leaking to User B across pooled connections) and malformed context handling are unproven.
- Critical database invariants (optimistic note revision conflicts, atomic quota reservation/settle, duplicate job prevention) lack multi-session concurrency verification.
- EF Core schema generation lacks parity verification against the canonical 68-table reviewed SQL schema.

## 2. Governing Documents and Authority

This change is governed strictly by the authority hierarchy defined in `AGENTS.md` and `docs/INDEX.md`:
1. **Final PRD v1.0** (`docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md`): OD-01 (Auth), OD-04 (RLS/Roles), Data Space boundaries, nine-module V1 scope.
2. **Approved Core Architecture v1.2** (`docs/architecture/StudyAI_Core_Architecture_v1.2.md`):
   - LOCKED-01: Explicit Data Space ownership boundary; server-side actor resolution.
   - LOCKED-04: Single canonical Knowledge Item binary with placements.
   - LOCKED-05: Optimistic concurrency for note editing (explicit conflict, no silent overwrite).
   - LOCKED-08: Atomic AI quota reservation, settlement, and release.
   - OPEN-09: PostgreSQL RLS and runtime role architecture.
3. **Database Implementation Review v1.1** (`docs/database/StudyAI_Database_Implementation_Review_v1.1.md`): P0/P1 audit findings, transaction recipes, and EF Core handoff criteria.
4. **Reviewed Physical Schema v1.1** (`docs/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql`): 68-table clean-slate reference.
5. **Database Runtime Validation Report** (`docs/database/DATABASE_RUNTIME_VALIDATION_REPORT.md`): Local Docker PostgreSQL 16.15 validation baseline.
6. **Open Decisions Register** (`docs/adr/OPEN_DECISIONS.md`): ADR-001 (Auth), ADR-003 (RLS/Roles), and Persistence strategy.

## 3. Proposed Scope and Capabilities

The scope of this iteration is strictly limited to the database persistence and tenant isolation foundation:
1. **EF Core / Npgsql / pgvector Compatibility & Pinning**:
   - Establish compatible package versions for .NET 10 (`net10.0`), Npgsql 10 preview / 9.x stable, and pgvector integration.
   - Define exact Central Package Management (`Directory.Packages.props`) additions.
2. **Database Context & Migration Ownership**:
   - Define the Single Database Principle and single primary `DbContext` strategy (`StudyAIDbContext`).
   - Scaffold modular entity configurations without introducing business controllers or domain services prematurely.
3. **PostgreSQL Role and Permission Model (ADR-003 recommendation)**:
   - Formulate least-privilege role hierarchy: `studyai_migrator` (DDL owner), `studyai_app` (DML only), `studyai_worker` (scoped batch jobs), and `studyai_ro` (reporting).
4. **Tenant Isolation & RLS Proof-of-Concept**:
   - Build an isolated test harness targeting high-risk vector/document tables (`knowledge.knowledge_item`, `documents.document_chunk`, `documents.chunk_embedding`).
   - Implement and test transaction-local session context (`SET LOCAL app.current_data_space_id = '...'`).
   - Test adversarial scenarios: User A vs User B, pooled connection reuse without leaks, missing/malformed context (fail-closed), non-superuser enforcement (no `BYPASSRLS`).
5. **Multi-Session Concurrency Verification**:
   - Execute real multi-session tests for:
     - Concurrent note edits (`learning.note_revision` optimistic lock).
     - Atomic quota allocation (`billing.ai_quota_allocation` reservation/settle).
     - Duplicate document-processing jobs (`ops.document_processing_run` unique lease / status constraint).
     - Subscription overlap prevention (`billing.subscription`).
6. **Schema Parity & Incremental Migration Strategy**:
   - Evaluate transition from reviewed clean-slate SQL to EF Core incremental migrations.
   - Design parity verification tooling comparing compiled EF metadata against `StudyAI_Physical_Schema_Reviewed_v1.1.sql`.
   - Provide forward-fix and recovery runbooks for non-reversible schema steps.

## 4. Invariants & Security Impact

- **Server-Side Authorization Invariant**: Client-supplied `data_space_id` is NEVER trusted. The application server resolves actor identity to authorized Data Spaces.
- **Defense-in-Depth Principle**: EF Core query filters are convenient application filters, NOT security boundaries. PostgreSQL RLS and restricted database credentials act as mandatory secondary defense.
- **Fail-Closed Context**: Queries executed with missing, blank, or invalid session context return zero rows.
- **Connection-Pooling Safety**: Application connections returned to the Npgsql pool must not leak ambient session state; connection initialization and cleanup must isolate every transaction.
- **No Schema Degradation**: The canonical 68-table schema is preserved without unapproved modifications.

## 5. Explicit Exclusions

This change does NOT:
- Implement authentication endpoints, password hashing, or token generation (deferred to ADR-001 acceptance).
- Implement business API controllers or domain workflow services.
- Deploy background queues, Hangfire, or external broker daemons.
- Execute external AI provider calls or live billing webhooks.
- Create Git commits or destroy retained validation databases.
