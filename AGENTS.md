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

## 3. Approved Technical Baseline

- **API**: ASP.NET Core (`net10.0`), C# 13, Native Microsoft Testing Platform (MTP), xUnit v3, Central Package Management, locked dependencies.
- **Web**: Next.js and TypeScript (Modular App Router, V1 target).
- **Mobile**: React Native / Expo (Future V1+ target).
- **Database**: PostgreSQL 16+ with `pgvector`, `pg_trgm`, `btree_gist`, EF Core + Npgsql.
- **Storage**: Private S3-compatible storage with short-lived pre-signed URLs.
- **Workers**: Independent background workers (.NET background services and Python document processors).

---

## 4. Mandatory Architectural Invariants

- **Data Space Ownership Boundary**: Resolve actor access server-side for every read, write, export, retrieval, signed URL operation, stream, and job. Never trust client-supplied `data_space_id`.
- **Knowledge Items & Placements**: Keep exactly one canonical Knowledge Item and binary/version when content appears in multiple spaces. Placements express locations. Never duplicate binary storage for multiple placements.
- **Library Lifecycle**: Remove placement, global Trash, restore, and permanent purge are distinct operations.
- **Optimistic Concurrency for Notes**: Never silently overwrite concurrent note edits. Return an explicit conflict and preserve recoverable revisions.
- **AI Quota Reservation**: Reserve quota atomically before chargeable AI work; settle or release exactly once. Record user usage separately from provider cost. No paid AI workflow may run unmetered.
- **Source-Version Provenance**: Every retrieved AI response must link to the exact source version and evidence used. Never fabricate citations, DOIs, authors, quotes, or slide/page numbers.
- **Schema & Migrations**: Use reviewed, incremental EF Core migrations with forward/rollback plans. Never run clean-slate SQL against existing databases.

---

## 5. Coding & Testing Conventions

- **Clean Architecture & Separation**: Keep domain rules in `Domain` / `Application` services, never in controllers, UI components, `DbContext`, prompts, or provider adapters.
- **Typed & Dependency Injected**: Rely on constructor dependency injection, explicit contracts (`StudyAI.Contracts`), UTC timestamps, and structured errors.
- **Zero Hardcoded Secrets**: Validate configuration at startup using options validators. Never hardcode credentials, limits, or connection strings.
- **Test-Driven & Proportionate Verification**:
  - Test command: `dotnet test --solution StudyAI.slnx --no-restore`
  - Unit tests: Isolated domain, configuration, and policy tests.
  - Integration tests: Host health, middleware, correlation, and redaction tests.
  - Architecture tests: Verify dependency rules (e.g., Domain must not depend on Application/Infrastructure/Api).
  - Every change requires proportionate automated test evidence.

---

## 6. Single-Flow Task Execution & Git Safety

- **One Active Task Per Working Tree**: An agent must only execute one active implementation task in a working tree at a time.
- **Task Structure**: Track active tasks under `docs/plans/<TASK-ID>/`:
  - `SPEC.md` — Requirements and acceptance criteria.
  - `PLAN.md` — Implementation steps and dependencies.
  - `task.md` — Live table tracker (`| Task ID | Description | Status | Evidence |`). Only one row may be `In Progress`.
  - `HANDOFF.md` — Resumable progress and environment state.
- **Git Safety Rules**:
  - Never discard or overwrite uncommitted changes without explicit instructions.
  - Never commit secrets, tokens, logs, `.env` files, or build caches.
  - Keep commits focused and atomic.

---

## 7. Cross-Agent Compatibility & Handoff

The repository is model- and agent-agnostic:
- **Canonical Instructions**: Root `AGENTS.md` is the primary instruction file for Codex, OpenCode, and Antigravity.
- **Portable Skills**: Reusable procedures reside in `.agents/skills/<skill-name>/SKILL.md` with standard YAML frontmatter.
- **Specialized Roles**: Review roles reside in `.agents/roles/`.
- **Handoff Protocol**: Before ending an agent session or switching agents (Codex ↔ OpenCode ↔ Antigravity):
  1. Check `git status` and test results.
  2. Record completed items, incomplete items, affected files, and blockers in `docs/plans/<TASK-ID>/HANDOFF.md`.
  3. Update `docs/development/CURRENT_STATE.md`.
  4. Ensure documentation catalogs are synced (`python scripts/docs/sync_codebase_index.py`).

---

## 8. Documentation Maintenance Obligations

Updating documentation is part of the **Definition of Done**:
- Adding or modifying source files requires updating the relevant catalog in `docs/codebase/catalog/`.
- Modifying architecture boundaries requires updating `SYSTEM_MAP.md`.
- Completing a task requires updating `docs/development/CURRENT_STATE.md`.
- Verify documentation health using:
  ```powershell
  python scripts/docs/validate_docs.py
  ```
