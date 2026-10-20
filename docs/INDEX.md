# StudyAI Documentation Index

Last reviewed: 2026-10-20.

## How authority works

Use this order when sources disagree:

1. Latest explicit Product Owner decision about scope or behavior.
2. [Final Product Requirements Document v1.0](product/StudyAI_Campus_MVP_Final_PRD_v1.0.md) for product scope, behavior, nine-module V1 target, acceptance, and launch gates.
3. [Approved Core Architecture v1.2](architecture/StudyAI_Core_Architecture_v1.2.md) for logical architecture and LOCKED/MUST invariants.
4. [Database Implementation Review v1.1](database/StudyAI_Database_Implementation_Review_v1.1.md) for the latest physical-database findings and implementation handoff.
5. [Reviewed SQL Schema v1.1](database/StudyAI_Physical_Schema_Reviewed_v1.1.sql) as the current clean-slate review candidate, not an approved production migration.
6. [Physical ERD and SQL Handbook v1.0](database/StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md) as the data dictionary and physical-design reference, subject to v1.1 review corrections.
7. Earlier decision, architecture, feature-list, and reference SQL documents for historical context only.

If a lower-ranked document conflicts with a higher-ranked source, stop, record the conflict, and request an ADR or Product Owner decision. Do not silently select a convenient interpretation.

## Product documents

| Document | Purpose | Status |
|---|---|---|
| [StudyAI_Campus_MVP_Final_PRD_v1.0.md](product/StudyAI_Campus_MVP_Final_PRD_v1.0.md) | Defines the Campus V1 product, all nine required modules, golden paths, requirements, NFRs, commercial hypotheses, acceptance scenarios, and launch gates. | FINAL and authoritative for product requirements. Commercial figures, partners, and named implementation choices may remain open. |
| [StudyAI_Campus_MVP_Decisions_v1.md](product/StudyAI_Campus_MVP_Decisions_v1.md) | Records the planning-stage product/technical decisions that preceded the final PRD and architecture. | Historical context. Later PRD and Architecture v1.2 override conflicts. |
| [StudyAI_Master_Feature_List.md](product/StudyAI_Master_Feature_List.md) | Captures the broad StudyAI B2C/B2B vision, feature inventory, early priorities, monetization hypotheses, and expansion roadmap. | Historical/vision reference. It is not V1 scope and does not prove implementation. |

## Architecture documents

| Document | Purpose | Status |
|---|---|---|
| [StudyAI_Core_Architecture_v1.2.md](architecture/StudyAI_Core_Architecture_v1.2.md) | Defines the approved logical architecture, modular boundaries, ownership model, placements, lifecycle, provenance, AI jobs, quota invariants, security, and ADR register. | APPROVED logical baseline. Physical design, vendors, policy numbers, and performance evidence remain separate. |
| [BACKEND_FOUNDATION.md](architecture/BACKEND_FOUNDATION.md) | Describes the Iteration 1 ASP.NET Core project boundaries, safe host configuration/logging, health contract, local commands, and explicit readiness exclusions. | Implemented foundation documentation; not a tenant-security, persistence, or production-readiness claim. |
| [PERSISTENCE_AND_SECURITY_ARCHITECTURE.md](architecture/PERSISTENCE_AND_SECURITY_ARCHITECTURE.md) | Defines EF Core 10 persistence, Npgsql, pgvector, least-privilege PostgreSQL roles, transaction-local RLS, connection-pooling safety, and optimistic concurrency. | Approved & implemented foundation (ADR-001, ADR-003, ADR-016). |
| [StudyAI_Core_Database_Architecture_v1.md](architecture/StudyAI_Core_Database_Architecture_v1.md) | Earlier conceptual database/knowledge architecture and initial open decisions. | Historical. Useful for rationale only; it cannot override v1.2. |

## Database documents and SQL

| Document | Purpose | Status |
|---|---|---|
| [StudyAI_Database_Implementation_Review_v1.1.md](database/StudyAI_Database_Implementation_Review_v1.1.md) | Latest audit of the v1.0 physical proposal; identifies P0/P1/P2 findings, v1.1 SQL changes, transaction recipes, EF Core handoff, test gates, RLS caveats, and open ADRs. | REVIEWED DRAFT and latest database guidance. No runtime execution was proven in the document. |
| [StudyAI_Physical_Schema_Reviewed_v1.1.sql](database/StudyAI_Physical_Schema_Reviewed_v1.1.sql) | Clean-slate PostgreSQL schema candidate incorporating the implementation review corrections. | Runtime-validated on PostgreSQL 16.15 with vector 0.8.7, pg_trgm 1.6, and btree_gist 1.7. Still NOT production-approved and not an incremental migration. |
| [StudyAI_DB_Smoke_Tests_v1.1.sql](database/StudyAI_DB_Smoke_Tests_v1.1.sql) | Transactional smoke tests for key ownership, subtype, immutability, processing, subscription, stale-note, and quota constraints. | Executed on 2026-10-19: 10 expected PASS notices, zero FAIL notices, clean rollback. This is not a concurrent-session or security test suite. |
| [DATABASE_RUNTIME_VALIDATION_REPORT.md](database/DATABASE_RUNTIME_VALIDATION_REPORT.md) | Records the reproducible Docker/PostgreSQL runtime, object inventory, smoke scenarios, dump/restore evidence, limitations, and EF Core readiness assessment. | Phase 1 runtime evidence. Clean-slate candidate passed; production, RLS, concurrency, migration, load, and security readiness remain unproven. |
| [StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md](database/StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md) | Full 68-table physical proposal, data dictionary, query cookbook, constraints, lifecycle, EF mapping guidance, QA plan, and reference SQL. | Reference proposal. Section 7 SQL and Q12–Q14 are superseded where v1.1 says so. |
| [StudyAI_Physical_Schema_Reference_v1.0.sql](database/StudyAI_Physical_Schema_Reference_v1.0.sql) | Standalone copy of the handbook v1.0 clean-slate reference schema. | Historical SQL reference; superseded by reviewed v1.1 for current evaluation. |

## Planning and ADR documents

| Document | Purpose | Status |
|---|---|---|
| [IMPLEMENTATION_ROADMAP.md](plans/IMPLEMENTATION_ROADMAP.md) | Dependency-ordered Phase 0–7 implementation plan with objectives, prerequisites, deliverables, tasks, acceptance, tests, and DoD. | Active planning baseline; does not approve open technical choices. |
| [AGENT_SKILLS_ASSESSMENT.md](plans/AGENT_SKILLS_ASSESSMENT.md) | Evaluates candidate Codex/agent skills and plugins, overlaps, risks, and a staged adoption sequence. | Active assessment. Ponytail approval/installation is recorded; other entries remain recommendations until separately approved and verified. |
| [CODEX_SKILLS_SETUP.md](plans/CODEX_SKILLS_SETUP.md) | Records approved Codex tooling, installed revisions, scope, permissions, lifecycle-hook security, verification, maintenance, and StudyAI workflow integration. | Active setup record. Project-level OpenSpec, a narrow official .NET subset, and three Superpowers skills are installed; Ponytail hooks are untrusted/inactive; Context7 and Vercel remain explicitly deferred. |
| [OPEN_DECISIONS.md](adr/OPEN_DECISIONS.md) | Consolidates unresolved technical/product decisions, their deadlines, constraints, options, recommendations, and required evidence. | Active decision queue. Recommendations are not approvals. |
| [ADR-001_AUTHENTICATION_AND_SESSION_ARCHITECTURE.md](adr/ADR-001_AUTHENTICATION_AND_SESSION_ARCHITECTURE.md) | Accepts ASP.NET Core Identity with secure HttpOnly cookies for web and PKCE for future mobile; establishes strict separation of authentication from Data Space authorization. | ACCEPTED. Implementation deferred to Phase 2 Iteration 4. |
| [ADR-003_POSTGRESQL_ROLES_AND_ROW_LEVEL_SECURITY.md](adr/ADR-003_POSTGRESQL_ROLES_AND_ROW_LEVEL_SECURITY.md) | Accepts least-privilege PostgreSQL runtime roles (`studyai_app`), transaction-local session context, and defense-in-depth RLS. | ACCEPTED. |
| [ADR-015_BACKEND_FOUNDATION_TOOLCHAIN.md](adr/ADR-015_BACKEND_FOUNDATION_TOOLCHAIN.md) | Accepts the .NET 10 SDK feature band, native Microsoft Testing Platform, xUnit v3, central package pins, and NuGet lock policy for Backend Foundation Iteration 1 only. | ACCEPTED. |
| [ADR-016_PERSISTENCE_AND_MIGRATION_STRATEGY.md](adr/ADR-016_PERSISTENCE_AND_MIGRATION_STRATEGY.md) | Accepts single primary DbContext, EF Core 10 + Npgsql + pgvector, CPM pins, hybrid EF/raw SQL incremental migrations, and parity verification. | ACCEPTED. |
| [AI_ARCHITECTURE.md](plans/AI_ARCHITECTURE.md) | Multi-Agent Engineering & Codebase Intelligence System v2.0 blueprint defining universal operating contracts, living system map, file catalogs, and cross-agent handoff. | IMPLEMENTED baseline for developer experience and agent coordination. |

## Design documents

| Document | Purpose | Status |
|---|---|---|
| [UI_REFERENCE_RESEARCH.md](design/UI_REFERENCE_RESEARCH.md) | Reviews verified product references and extracts applicable navigation, hierarchy, reading, knowledge-organization, learning, responsive, and accessibility lessons. | DRAFT; inspiration only, pending Product Owner approval. |
| [DESIGN_DIRECTIONS.md](design/DESIGN_DIRECTIONS.md) | Proposes three visual directions—Ruang Studi, Campus Grid, and Evidence Atelier—and recommends Ruang Studi for Iteration 5. | DRAFT; no direction is approved yet. |
| [DESIGN_SYSTEM.md](design/DESIGN_SYSTEM.md) | Defines candidate semantic tokens, accessible component foundation, responsive rules, states, and cross-platform boundaries. | DRAFT; no UI library or token set is approved or installed. |
| [UI_UX_GUIDELINES.md](design/UI_UX_GUIDELINES.md) | Defines proposed information architecture, initial Workspace flows, lifecycle language, contextual AI behavior, and accessibility floor. | DRAFT; unresolved product policies remain explicitly blocked. |
| [FRONTEND_SKILLS_RESEARCH.md](design/FRONTEND_SKILLS_RESEARCH.md) | Reviews four upstream UI agent-skill options, their scripts/hooks/network behavior, overlap, compatibility, risks, and staged recommendation. | DRAFT research; no skill or hook was installed. |

## Multi-Agent & Codebase Intelligence documents

| Document | Purpose | Status |
|---|---|---|
| [START_HERE.md](START_HERE.md) | Fast single entry point for developers and AI coding agents (Codex, OpenCode, Antigravity). | Active navigation guide. |
| [CURRENT_STATE.md](development/CURRENT_STATE.md) | Live index of active tasks, current branches, build health, and immediate next steps. | Active project status. |
| [MODULE_INDEX.md](codebase/MODULE_INDEX.md) | Directory of applications, core modules, test suites, and boundary responsibilities. | Active codebase directory. |
| [FEATURE_MAP.md](codebase/FEATURE_MAP.md) | Map tracing user-facing and foundation features to concrete implementation and test files. | Active feature map. |
| [FILE_INDEX.md](codebase/FILE_INDEX.md) | Master aggregate index of source and infrastructure files with documentation coverage. | Automatically synchronized. |

## What is approved

- Campus V1 remains Web-first and includes all nine modules in the final PRD.
- The logical architecture and invariants in Core Architecture v1.2 are approved.
- Next.js/TypeScript, ASP.NET Core/C#, PostgreSQL, modular monolith, private S3-compatible storage, API-first behavior, independent workers, and portable container deployment are approved baselines at the status specified in v1.2.
- Data Space ownership, shared Knowledge Items with placements, source-version provenance, explicit note conflicts, tenant-safe relationships, and quota reserve/settle/release behavior are mandatory concepts.

## What remains unverified or open

- The v1.1 SQL has passed three clean-slate executions on PostgreSQL 16.15 with vector, pg_trgm, and btree_gist; it has not been tested as an incremental upgrade on populated data.
- No EF Core model/migration has been generated, compiled, or compared with the reviewed SQL.
- The supplied transactional smoke test and an empty-fixture dump/restore passed. Integration, true multi-session concurrency, RLS/pooling, migration, populated restore, performance, load, and security suites have not run.
- Auth, queue, production RLS, storage vendor/retention, AI providers, embeddings/retrieval parameters, payment, quota policy, notes editor details, and production deployment are unresolved.
- Prices, quota numbers, SLOs, capacity, partner-gated similarity, OCR/media coverage, and legal/compliance sign-off are not final.

## Known conflicts and supersessions

- The Master Feature List recommends an early narrow paid workflow; the later final PRD locks a feature-rich nine-module public V1. Treat the earlier roadmap as historical, while still implementing internally in phases.
- The final PRD line describing Architecture v1.2 as proposed predates Architecture v1.2's explicit approval statement. The architecture document is authoritative for its current approved status.
- Core Database Architecture v1 says the physical schema is still to be designed. That work was later proposed in the v1.0 handbook, reviewed in v1.1, and clean-slate runtime-validated on 2026-10-19, but remains unverified for production use.
- Database Review v1.1 supersedes v1.0 handbook retrieval queries Q12–Q14 and addresses multiple physical gaps. Use v1.1 for current evaluation while retaining the handbook as reference.
- Database Review v1.1 says .NET was unavailable in its original review environment; the 2026-10-19 repository inspection found .NET SDK 10.0.300 locally. This changes environment readiness, not the historical review evidence.

## Navigation workflow for an AI coding agent

1. Read root AGENTS.md and this index.
2. Read the PRD sections and acceptance IDs for the requested feature.
3. Read the corresponding Architecture v1.2 boundaries/invariants.
4. Check OPEN_DECISIONS.md. Stop before implementation if a required decision is still unresolved.
5. For data work, read Database Review v1.1 first, then only the relevant handbook dictionary/query sections, then the reviewed SQL.
6. Implement the smallest milestone-approved slice without reducing V1 scope.
7. Add ownership, failure, concurrency, provenance, usage/cost, and lifecycle tests required by the source documents.
8. Update documentation and evidence. Never promote a candidate or test status without actual command output.

## Reorganization record

All original files were moved without renaming or editing:

| Original path | Current path |
|---|---|
| StudyAI_Campus_MVP_Final_PRD_v1.0.md | docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md |
| StudyAI_Campus_MVP_Decisions_v1.md | docs/product/StudyAI_Campus_MVP_Decisions_v1.md |
| StudyAI_Master_Feature_List.md | docs/product/StudyAI_Master_Feature_List.md |
| StudyAI_Core_Architecture_v1.2.md | docs/architecture/StudyAI_Core_Architecture_v1.2.md |
| StudyAI_Core_Database_Architecture_v1.md | docs/architecture/StudyAI_Core_Database_Architecture_v1.md |
| StudyAI_Database_Implementation_Review_v1.1.md | docs/database/StudyAI_Database_Implementation_Review_v1.1.md |
| StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md | docs/database/StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md |
| StudyAI_Physical_Schema_Reviewed_v1.1.sql | docs/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql |
| StudyAI_DB_Smoke_Tests_v1.1.sql | docs/database/StudyAI_DB_Smoke_Tests_v1.1.sql |
| StudyAI_Physical_Schema_Reference_v1.0.sql | docs/database/StudyAI_Physical_Schema_Reference_v1.0.sql |
