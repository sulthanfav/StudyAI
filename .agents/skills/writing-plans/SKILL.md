---
name: writing-plans
description: Produce structured, testable implementation plans with dependency-ordered tasks and explicit verification criteria. USE FOR: writing implementation plans, scoping changes, breaking down complex tasks.
---

# Writing Plans Skill

Use this skill to convert requirements and architectural designs into concrete, phased implementation plans.

---

## When to Use
- Preparing a nontrivial change before writing code.
- User triggers `/write-plan` or requests an implementation plan.

---

## Workflow Selection: OpenSpec vs docs/plans

Before creating any planning artifacts, determine the workflow type:

### A. Feature & Implementation Changes (OpenSpec - PRIMARY)
For all new features, bug fixes, refactoring, and code changes in `apps/`, `src/`, `tests/`, or `infra/`:
- **OpenSpec is canonical**: Use `openspec/changes/<change-id>/`.
- Artifacts: `proposal.md`, `design.md`, `specs/`, and `tasks.md`.
- **Do NOT create duplicate `SPEC.md`, `PLAN.md`, or `task.md` in `docs/plans/`**.
- Track live execution directly in `openspec/changes/<change-id>/tasks.md`.
- Record session handoff in `HANDOFF.md` linking to the OpenSpec change.

### B. Non-Implementation Tasks (docs/plans - EXCEPTION ONLY)
Reserved strictly for work that genuinely does not produce code changes tracked by OpenSpec (e.g., meta-governance repairs, pure exploratory spikes, or documentation reorganization):
- Create `docs/plans/<TASK-ID>/` containing:
  - `SPEC.md`: Objectives and acceptance criteria.
  - `PLAN.md`: Implementation breakdown and dependencies.
  - `task.md`: Table-only execution tracker.
  - `HANDOFF.md`: Resumable environment state.

---

## Implementation Plan Breakdown Steps

1. **Mandatory Map Check & Impact Analysis**:
   - Perform Pre-Edit Analysis according to `.agents/rules/mandatory-map-check.md`.
   - Identify entry points, callers, callees, data flows, and tests.
2. **Dependency-Ordered Task Breakdown**:
   - Step 1: Interface / Contract definition.
   - Step 2: Core domain / logic implementation (test-driven).
   - Step 3: Integration and composition.
   - Step 4: Targeted testing (unit, integration, architecture).
   - Step 5: Documentation update & catalog sync.
   - Step 6: Verification & handoff preparation.
3. **Single-Flow Execution Rule**:
   - Only one active implementation task may be in progress per working tree.
