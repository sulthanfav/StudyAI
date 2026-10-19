---
name: executing-plans
description: Execute structured implementation plans step-by-step with single-flow execution and verifiable evidence. USE FOR: executing implementation plans, managing task progress, updating live task trackers.
---

# Executing Plans Skill

Use this skill when executing an approved plan from `docs/plans/<TASK-ID>/PLAN.md`.

---

## When to Use
- Working through implementation tasks in `docs/plans/<TASK-ID>/`.
- User triggers `/execute-plan` or instructs to proceed with the plan.

---

## Single-Flow Execution Rules

1. **One Task at a Time**: Only ONE implementation task may be `In Progress` per agent working tree. Never multitask across unrelated features.
2. **Evidence Before Complete**: Never mark a task `Complete` without recording command output, test results, or verifiable evidence.
3. **Continuous Tracking**: Update `docs/plans/<TASK-ID>/task.md` immediately upon state transitions.

---

## Execution Loop

For each task in `docs/plans/<TASK-ID>/task.md`:

### 1. Set Task In Progress
Update `task.md` status from `Pending` to `In Progress`.

### 2. Implement the Scoped Increment
- Apply the smallest safe code change that satisfies the task objective.
- Follow test-driven development conventions (`.agents/skills/test-driven-development/`).
- Do not refactor unrelated files or modify business rules without authorization.

### 3. Run Targeted Verification
- Run the specific test command for the modified subsystem:
  ```powershell
  dotnet test --project tests/...
  ```
- If tests fail, use `.agents/skills/systematic-debugging/` to identify the root cause before patching.

### 4. Record Evidence & Complete Task
- Update the table row in `task.md` with status `Complete` and paste concise command output or test summary under `Evidence`.

### 5. Transition or Handoff
- If more tasks remain, proceed to the next row.
- If all tasks are finished, invoke `.agents/skills/verification-before-completion/` and `.agents/skills/agent-handoff/`.
