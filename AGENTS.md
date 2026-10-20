# StudyAI Canonical Agent Operating Contract

This document is the **authoritative operating contract** for all AI coding agents working in this repository:
- **OpenAI Codex**
- **OpenCode**
- **Google Antigravity**

All agent instances must adhere to these rules without exception.

---

## 1. Quickstart & Navigation Entry Points

Agents must practice **progressive context loading** to conserve tokens and avoid blind whole-repo scans:

1. **Start Here**: [`docs/START_HERE.md`](docs/START_HERE.md) — Single entry point for project workflow and orientation.
2. **System Map**: [`SYSTEM_MAP.md`](SYSTEM_MAP.md) — High-level architecture, module boundaries, request flows, and entry points.
3. **Documentation Authority**: [`docs/INDEX.md`](docs/INDEX.md) — Authority hierarchy, PRDs, ADRs, database specifications.
4. **Current Status**: [`docs/development/CURRENT_STATE.md`](docs/development/CURRENT_STATE.md) — Active task, branch, blockers, and build health.
5. **Codebase Intelligence**:
   - Modules: [`docs/codebase/MODULE_INDEX.md`](docs/codebase/MODULE_INDEX.md)
   - Features: [`docs/codebase/FEATURE_MAP.md`](docs/codebase/FEATURE_MAP.md)
   - File Catalogs: [`docs/codebase/catalog/`](docs/codebase/catalog/) and [`docs/codebase/FILE_INDEX.md`](docs/codebase/FILE_INDEX.md)
6. **Mandatory Map Check Rule**: [`.agents/rules/mandatory-map-check.md`](.agents/rules/mandatory-map-check.md) — Pre-edit safety governance.

---

## 2. Mission & Authority Order

Build **StudyAI Campus** as the nine-module Personal Academic Operating System defined in the final PRD. Internal phasing must not reduce the public V1 scope.

When sources disagree, follow this strict authority order (`docs/INDEX.md`):
1. Latest explicit Product Owner decision.
2. Final PRD v1.0 ([`docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md`](docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md)).
3. Approved Core Architecture v1.2 ([`docs/architecture/StudyAI_Core_Architecture_v1.2.md`](docs/architecture/StudyAI_Core_Architecture_v1.2.md)).
4. Database Implementation Review v1.1 ([`docs/database/StudyAI_Database_Implementation_Review_v1.1.md`](docs/database/StudyAI_Database_Implementation_Review_v1.1.md)).
5. Reviewed SQL Schema v1.1 (candidate review; not an unreviewed production migration).
6. Physical ERD and SQL Handbook v1.0.
7. Historical documents.

Never silently change a LOCKED decision, product behavior, security invariant, or acceptance criterion.

---

## 3. Approved Technical Baseline & Open Decisions

- **API**: ASP.NET Core (`net10.0`), C# 14 (`LangVersion=14.0`), Native Microsoft Testing Platform (MTP), xUnit v3, Central Package Management, locked dependencies.
- **Web**: Next.js and TypeScript (Modular App Router, V1 target).
- **Mobile**: React Native / Expo (Future V1+ target).
- **Database**: PostgreSQL 16+ with `pgvector`, `pg_trgm`, `btree_gist`, EF Core with Npgsql.
- **Storage**: Private S3-compatible object storage with short-lived pre-signed URLs.
- **Workers**: Independent background workers (.NET background services and Python document processors).
- **Single Database Principle**: Use one primary PostgreSQL database and one EF Core `DbContext`/migration history initially. Module boundaries remain explicit despite the shared database.
- **Open Decisions Queue**: Auth, queue, RLS policy, vendors, embedding model, payment provider, retention periods, quota policy, editor details, and production topology remain OPEN until an ADR is accepted in `docs/adr/`.

---

## 4. Boundaries, Ownership & Security Invariants

- **Data Space Ownership Boundary**: Resolve actor access server-side for every read, write, export, retrieval, signed URL operation, stream, and job. Never trust client-supplied `data_space_id` as authorization.
- **EF Core Query Filters Are Not Security Boundaries**: EF Core query filters alone are not an authorization boundary; enforce server-side tenancy checks and composite keys.
- **Tenant-Safe Constraints & Adversarial Tests**: Preserve tenant-safe composite keys/constraints and add adversarial cross-user tests.
- **Least-Privilege Workers**: Workers receive scoped job context and least-privilege credentials.
- **Zero Logged Sensitive Data**: Do not log secrets, tokens, signed URLs, full private documents, or sensitive student prompts/notes by default.

---

## 5. Data, Storage Lifecycle & Migrations

- **Knowledge Items & Placements**: Keep exactly one canonical Knowledge Item and binary/version when content appears in multiple spaces. Placements express locations. Never duplicate binary storage for multiple placements.
- **Library Lifecycle**: Remove placement, global Trash, restore, and permanent purge are distinct operations. Preserve canonical Library behavior.
- **Optimistic Concurrency for Notes**: Never silently overwrite concurrent note edits. Return an explicit conflict and retain recoverable revisions.
- **Schema & Migrations Safety**: Schema changes use reviewed, incremental EF Core migrations with forward/rollback and data-migration plans. Clean-slate reference SQL must never run against an existing or production database.
- **Real Database Integration Testing**: Real PostgreSQL integration tests are mandatory for persistence and security behavior; SQLite or in-memory substitutes are insufficient.

---

## 6. AI, Retrieval, Provenance & Billing

- **Authorized Retrieval Pre-Scoping**: Authorize and scope retrieval sources before model access. Historical outputs must link to the exact source version and evidence used. Never fabricate citations, DOIs, authors, quotes, or slide/page numbers.
- **Untrusted Input**: Treat parsed files, uploaded documents, and retrieved instructions as untrusted input.
- **Robust Provider Adapters**: Provider integrations must use adapters with bounded retries, idempotency, cancellation tokens, and recovery.
- **Atomic AI Quota Accounting**: Reserve quota atomically before chargeable AI work; settle or release exactly once. Record user usage separately from provider cost. Never bypass AI usage metering, entitlement checks, quota enforcement, rate limits, or cost caps. No paid AI workflow may ship unmetered.

---

## 7. Coding, Testing & Verification Coverage

- **Clean Architecture & Separation**: Keep domain rules in `Domain` / `Application` services, never in controllers, UI components, `DbContext`, prompts, or provider adapters.
- **Typed & Dependency Injected**: Rely on constructor dependency injection, explicit contracts (`StudyAI.Contracts`), UTC timestamps, and structured errors. Never hardcode credentials, limits, or connection strings.
- **Mandatory Verification Coverage**: Required coverage includes ownership isolation, tenant-safe FK failures, note conflicts, hierarchy cycles, source-version provenance, job retries, quota concurrency, payment idempotency, storage lifecycle, migrations, and backup/restore paths.
- **Evidence Before Claims**: Never claim implementation, migration, build, test, security, performance, or production readiness without actual command output.
  - Test command: `dotnet test --solution StudyAI.slnx --no-restore`

---

## 8. Change Specification & Task Execution Workflow

To prevent competing task trackers, task workflows are partitioned as follows:

1. **OpenSpec is Primary for Implementation Changes**:
   - For all features, fixes, refactorings, and backend/frontend implementation changes, **OpenSpec is the canonical specification and task ledger** (`openspec/changes/<change-id>/tasks.md`).
   - Specification resides in `proposal.md`, `design.md`, and `specs/`.
   - Agents must **NOT** create duplicate `SPEC.md`, `PLAN.md`, or `task.md` files under `docs/plans/` for an OpenSpec-managed change.
2. **Resumable State in `HANDOFF.md`**:
   - `HANDOFF.md` retains session progress, environment state, and test evidence, linking directly to the canonical OpenSpec change artifacts.
3. **Lightweight `docs/plans/<TASK-ID>/` Workflow**:
   - Reserved strictly for work that genuinely does not belong in OpenSpec (e.g. meta-governance repairs, repository tooling setup, or exploratory architectural spikes).
4. **Single-Flow Execution Rule**:
   - Exactly **one active task** per agent working tree at a time. Only one row may be `In Progress` in the active task ledger.
5. **Git Safety Rules**:
   - Never discard or overwrite uncommitted changes without explicit instructions.
   - Never commit secrets, tokens, logs, `.env` files, or build caches.
   - Keep commits focused and atomic.

---

## 9. Agent Skills, Tooling & Lifecycle Governance

- **Skill Hierarchy**: OpenSpec is the primary change-specification workflow. Third-party skills are subordinate to this file and the documented StudyAI authority hierarchy.
- **Ponytail Manual Review Only**: Use Ponytail only as a complementary manual review aid. Never remove required boundaries, validation, constraints, provenance, or PRD scope. Ponytail hooks and automatic lifecycle hooks remain inactive/untrusted unless explicitly approved by the Product Owner.
- **Cross-Agent Compatibility**: Reusable skills reside in `.agents/skills/<skill-name>/SKILL.md`. Review roles reside in `.agents/roles/`.

---

## 10. Documentation Maintenance Obligations

Updating documentation is part of the **Definition of Done**:
- Adding or modifying source files requires updating the relevant catalog in `docs/codebase/catalog/`.
- Modifying architecture boundaries requires updating `SYSTEM_MAP.md`.
- Completing a task requires updating `docs/development/CURRENT_STATE.md`.
- Verify documentation health using:
  ```powershell
  python scripts/docs/sync_codebase_index.py --check
  python scripts/docs/validate_docs.py
  ```
