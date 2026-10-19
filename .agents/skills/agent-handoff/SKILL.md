---
name: agent-handoff
description: Structured procedure for safe context persistence and handoff between Codex, OpenCode, and Antigravity. USE FOR: preparing handoff before leaving, resuming work in a new agent session, recovering after session interruption.
---

# Agent Handoff Skill

Use this skill when concluding a work session, pausing an active task, or switching between AI coding agents (OpenAI Codex, OpenCode, Google Antigravity).

---

## When to Use
- Concluding or pausing an implementation session.
- Agent token quota or context limit approaching.
- Handing over progress from one agent model/tool to another.
- Resuming work after an interrupted session.

---

## 1. Outgoing Agent Procedure (Before Leaving)

Before disconnecting or ending a turn:

1. **Inspect Git Status**:
   ```powershell
   git status -s
   ```
2. **Execute Targeted Tests**:
   ```powershell
   dotnet test --solution StudyAI.slnx --no-restore
   ```
3. **Record Evidence in `docs/plans/<TASK-ID>/HANDOFF.md`**:
   - Outgoing agent identity and timestamp.
   - Current branch and last commit SHA.
   - Completed tasks with verified evidence.
   - Incomplete tasks and immediate next step.
   - List of modified or created files.
   - Actual test run summary (passed / failed / duration).
   - Known blockers, caveats, or unverified items.
4. **Update `docs/development/CURRENT_STATE.md`**:
   - Update active task pointer, branch status, and blocker summary.
5. **Ensure Catalogs Synced**:
   ```powershell
   python scripts/docs/sync_codebase_index.py
   ```

---

## 2. Incoming Agent Procedure (When Resuming)

When entering the repository as a new or resumed agent:

1. **Read Canonical Operating Contract**:
   Read root `AGENTS.md`.
2. **Check Current State**:
   Read `docs/development/CURRENT_STATE.md` to identify the active task and branch.
3. **Read Active Task Context**:
   Read `docs/plans/<TASK-ID>/SPEC.md` and `docs/plans/<TASK-ID>/HANDOFF.md`.
4. **Verify Working Tree Against Handoff**:
   Run `git status -s` to confirm uncommitted changes match the recorded handoff.
5. **Identify Next Actionable Step**:
   Read the "Next Recommended Step" in `HANDOFF.md`.
6. **Execute Next Step**:
   Do not repeat full-repository analysis; proceed directly to the identified step.

---

## 3. Emergency Interruption Recovery

If the prior agent crashed, ran out of quota, or exited without writing `HANDOFF.md`:
1. Check `git status` and `git diff` to see what was modified.
2. Check `docs/plans/` for the latest modified task directory.
3. Inspect `task.md` to see which row was marked `In Progress`.
4. Run tests (`dotnet test --solution StudyAI.slnx`) to discover whether the current state compiles and passes.
5. Record recovered findings in `HANDOFF.md` and continue.
