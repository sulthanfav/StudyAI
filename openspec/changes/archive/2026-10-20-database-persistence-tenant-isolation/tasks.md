# Tasks: Database Persistence & Tenant Isolation

## 1. Toolchain Pinning & ADR Recommendations

- [x] 1.1 Formulate and document ADR-001 recommendation (Authentication & Session Architecture: ASP.NET Core Identity with OIDC-ready adapters, cookie web sessions, PKCE mobile tokens, server-side data space authorization).
- [x] 1.2 Formulate and document ADR-003 recommendation (PostgreSQL Isolation: separate runtime roles `studyai_migrator`, `studyai_app`, `studyai_worker`, `studyai_ro`; defense-in-depth RLS; transaction-scoped pooling safety).
- [x] 1.3 Formulate and document Persistence strategy recommendation (Single `DbContext` boundary, EF Core incremental migrations, raw SQL parity verification, and package pinning).
- [x] 1.4 Evaluate EF Core 10 / Npgsql / pgvector package compatibility and document proposed Central Package Management pins in `Directory.Packages.props`.

## 2. Tenant Isolation Proof-of-Concept & Role Verification

- [x] 2.1 Scaffold non-production PostgreSQL test environment / database for tenant isolation PoC without altering retained validation databases.
- [x] 2.2 Create restricted PostgreSQL roles (`studyai_migrator`, `studyai_app`, `studyai_worker`, `studyai_ro`) with explicit grants and verify `studyai_app` lacks DDL and `BYPASSRLS`.
- [x] 2.3 Implement RLS policy on representative tables (`knowledge.knowledge_items`, `documents.documents`, `documents.document_chunks`, `documents.chunk_embeddings`) enforcing transaction-scoped context `app.current_data_space_id`.
- [x] 2.4 Test User A access to authorized Data Space A and verify expected read/write permissions.
- [x] 2.5 Test User A attempting unauthorized access to Data Space B and verify access is rejected (zero rows returned / insert forbidden).
- [x] 2.6 Test User B immediately following User A on a reused pooled connection and verify zero ambient context leakage.
- [x] 2.7 Test missing, empty, or malformed authorization context and verify fail-closed behavior.
- [x] 2.8 Test transaction rollback and abort handling to verify session context is unconditionally cleaned up.

## 3. Real Multi-Session Concurrency Testing

- [x] 3.1 Implement multi-session test for concurrent note updates (`knowledge.notes` & `knowledge.note_revisions`) verifying optimistic conflict detection and rejection of stale writes.
- [x] 3.2 Implement multi-session test for atomic quota reservations (`billing.quota_balances`) verifying race-free limit enforcement under concurrent requests.
- [x] 3.3 Implement multi-session test for duplicate document-processing runs (`documents.processing_runs`) verifying lease/status conflict prevention.
- [x] 3.4 Implement multi-session test for concurrent subscription lifecycle operations (`billing.subscription_periods`).
- [x] 3.5 Record setup, operations, expected result, actual output, and pass/fail status for all concurrency scenarios.

## 4. Schema Parity & Migration Strategy

- [x] 4.1 Evaluate transition path from clean-slate SQL (`StudyAI_Physical_Schema_Reviewed_v1.1.sql`) to EF Core-managed incremental migrations.
- [x] 4.2 Establish catalog comparison strategy asserting 100% parity across all 68 tables, columns, constraints, and indexes.
- [x] 4.3 Formulate disaster recovery, forward-fix, and rollback runbooks for destructive schema operations.

## 5. Verification & Completion Reporting

- [x] 5.1 Run full solution tests (`dotnet test --solution StudyAI.slnx --no-restore`) and confirm zero regressions.
- [x] 5.2 Validate documentation integrity and links (`python scripts/docs/sync_codebase_index.py --check` and `python scripts/docs/validate_docs.py`).
- [x] 5.3 Compile and present Step 10 Completion Report with literal command outputs, evidence, and open decision prompts.

## 6. Persistence Integration & Security Hardening Implementation

- [x] 6.1 Pin EF Core 10, Npgsql, and pgvector in `Directory.Packages.props` and reference in `StudyAI.Infrastructure.csproj`.
- [x] 6.2 Implement domain entity abstractions and `IApplicationDbContext` in `StudyAI.Domain` / `StudyAI.Application` for the approved foundational subset (`identity.users`, `ownership.data_spaces`, `workspace.content_spaces`, `knowledge.knowledge_items`, `documents.*`).
- [x] 6.3 Implement `StudyAIDbContext` in `StudyAI.Infrastructure.Persistence` with modular schema-based `IEntityTypeConfiguration` classes.
- [x] 6.4 Implement `TenantSessionInterceptor` and ambient context service for transaction-local `SET LOCAL app.current_data_space_id` with connection-pooling safety.
- [x] 6.5 Implement design-time factory `StudyAIDbContextFactory` and generate initial incremental EF Core migration.
- [x] 6.6 Execute migration on isolated disposable test database and verify schema parity against canonical SQL v1.1.
- [x] 6.7 Implement automated .NET integration tests covering all 12 persistence, security, pooling, and concurrency criteria.
- [x] 6.8 Run complete verification suite (`dotnet restore --locked-mode`, `dotnet build`, `dotnet test`, docs sync/validate).
