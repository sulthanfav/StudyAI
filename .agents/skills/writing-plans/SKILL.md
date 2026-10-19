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
- Creating a new task in `docs/plans/<TASK-ID>/`.

---

## Workflow

### 1. Establish Task Folder & Specification
Create `docs/plans/<TASK-ID>/` containing:
- `SPEC.md`: Requirements, acceptance criteria, boundaries, risks.
- `PLAN.md`: Implementation breakdown, phased tasks, verification steps.
- `task.md`: Table-only execution tracker.
- `HANDOFF.md`: Resumable state and environment record.

### 2. Mandatory Map Check & Impact Analysis
- Perform Pre-Edit Analysis according to `.agents/rules/mandatory-map-check.md`.
- Identify entry points, callers, callees, data flows, and tests.

### 3. Dependency-Ordered Task Breakdown
Break the work into small, verifiable tasks:
1. Interface / Contract definition.
2. Core domain / logic implementation (test-driven).
3. Integration and composition.
4. Targeted testing (unit, integration, architecture).
5. Documentation update & catalog sync.
6. Verification & handoff preparation.

### 4. Initialize Live Task Tracker (`task.md`)
Use the standardized table-only format:
```markdown
| Task ID | Description | Status | Evidence |
|---|---|---|---|
| 1 | Inspect affected code & map check | Pending | |
| 2 | Implement scoped changes | Pending | |
| 3 | Execute targeted test suite | Pending | |
| 4 | Update file catalogs & documentation | Pending | |
| 5 | Verify health & record handoff | Pending | |
```
Only one task may be `In Progress` at any given time.
