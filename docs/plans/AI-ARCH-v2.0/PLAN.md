# Implementation Plan: Multi-Agent Engineering & Codebase Intelligence v2.0

Task ID: `AI-ARCH-v2.0`  
Specification: [`SPEC.md`](SPEC.md)

---

## Phases & Deliverables

- **Phase A — Discovery & Baseline Safety**:
  - Inspect Git status, confirm test suite health (28 tests passed).
  - Preserve all existing code, specs, and configurations.
- **Phase B — Shared Agent Contract & Governance**:
  - Update `AGENTS.md` as canonical operating contract.
  - Create `docs/START_HERE.md` entry point.
  - Create `.agents/rules/mandatory-map-check.md` safety governance rule.
- **Phase C — Living System Map & Hierarchical Codebase Intelligence**:
  - Create `SYSTEM_MAP.md`.
  - Create `docs/codebase/MODULE_INDEX.md`, `FEATURE_MAP.md`, `FILE_INDEX.md`.
  - Create structured catalogs in `docs/codebase/catalog/` (`api-host.md`, `contracts.md`, `domain.md`, `application.md`, `infrastructure.md`, `tests.md`, `infra.md`).
- **Phase D — Agent Execution Workflows & Modular Skills**:
  - Implement portable skills in `.agents/skills/`: `brainstorming`, `writing-plans`, `executing-plans`, `codebase-navigation`, `documentation-maintenance`, `agent-handoff`.
  - Create specialized review roles in `.agents/roles/`.
- **Phase E — State Persistence & Task Tracking**:
  - Create `docs/development/CURRENT_STATE.md`.
  - Create `docs/plans/TASK-TEMPLATE/` (`SPEC.md`, `PLAN.md`, `task.md`, `HANDOFF.md`).
  - Initialize active task tracker `docs/plans/AI-ARCH-v2.0/`.
- **Phase F — Automation Tooling & Validation**:
  - Implement `scripts/docs/sync_codebase_index.py`.
  - Implement `scripts/docs/validate_docs.py`.
  - Add unit tests for scripts in `tests/scripts/test_docs_tools.py`.
- **Phase G — End-to-End Walkthrough & Delivery Report**:
  - Execute sync and validation commands.
  - Execute full .NET test suite.
  - Compile final delivery report per Section 19.
