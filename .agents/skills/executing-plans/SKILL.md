---
name: executing-plans
description: Execute structured implementation plans step-by-step with single-flow execution and verifiable evidence. USE FOR: executing implementation plans, managing task progress, updating live task trackers.
---

# Executing Plans Skill

Use this skill when executing an approved plan from OpenSpec (`openspec/changes/<change-id>/tasks.md`) or a standalone task (`docs/plans/<TASK-ID>/task.md`).

---

## When to Use
- Working through implementation tasks in an active OpenSpec change or standalone task.
- User triggers `/execute-plan` or instructs to proceed with the plan.

---

## Single-Flow Execution Rules

1. **One Task at a Time**: Only ONE task may be `In Progress` per agent working tree. Never execute multiple tasks in parallel within the same working tree.
2. **Canonical Ledger**:
   - For implementation changes: Execute against **`openspec/changes/<change-id>/tasks.md`**. Do not create a duplicate task table in `docs/plans/`.
   - For non-OpenSpec tasks only: Execute against `docs/plans/<TASK-ID>/task.md`.
3. **Evidence Before Complete**: Never mark a task complete without recording command output, test results, or verifiable evidence.
4. **Immediate State Transition**: Check off tasks as completed in the canonical task ledger as soon as verified.

---

## Execution Loop

For each task in the canonical task ledger:

### 1. Set Task In Progress
Mark the specific task as currently active in the working tree.

### 2. Implement the Scoped Increment
- Apply the smallest safe code change that satisfies the task objective.
- Follow test-driven development conventions ([`.agents/skills/test-driven-development/`](../test-driven-development/SKILL.md)).
- Do not refactor unrelated files or modify business rules without authorization.

### 3. Run Targeted Verification
- Run the specific test command for the modified subsystem:
  ```powershell
  dotnet test --project tests/...
  ```
- If tests fail, use [`.agents/skills/systematic-debugging/`](../systematic-debugging/SKILL.md) to identify the root cause before patching.

### 4. Record Evidence & Complete Task
- Mark the task item complete in the canonical task list (e.g. `- [x]` in `tasks.md`).

### 5. Transition or Handoff
- If more tasks remain, proceed to the next item.
- If pausing or switching agents, invoke [`.agents/skills/agent-handoff/`](../agent-handoff/SKILL.md).
- If all tasks are finished, invoke [`.agents/skills/verification-before-completion/`](../verification-before-completion/SKILL.md).
