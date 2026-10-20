---
name: documentation-maintenance
description: Keep architecture maps, module indexes, and structured file catalogs synchronized with actual code changes. USE FOR: updating file catalogs, synchronizing indexes, running documentation validation.
---

# Documentation Maintenance Skill

Use this skill to update repository documentation and verify catalog integrity as part of task completion.

---

## When to Use
- After adding, modifying, renaming, or deleting any source or configuration file.
- Before completing an implementation task or preparing an agent handoff.
- Running automated documentation health checks.

---

## Maintenance Obligations

| Change Made | Documentation Action Required |
|---|---|
| **Added new file** | Add record to `docs/codebase/catalog/<module>.md`; run `sync_codebase_index.py`. |
| **Modified file responsibility** | Update description in `docs/codebase/catalog/<module>.md`. |
| **Deleted / renamed file** | Remove or rename record in catalog; verify no stale links. |
| **New feature implemented** | Update `docs/codebase/FEATURE_MAP.md` status to VERIFIED with tests. |
| **Architecture boundary changed** | Update `SYSTEM_MAP.md` and `docs/codebase/MODULE_INDEX.md`. |
| **Task completed** | Update `docs/development/CURRENT_STATE.md` and task `HANDOFF.md`. |

---

## Tooling Commands

### 1. Check Codebase Index Synchronization (Safe Check)
```powershell
python scripts/docs/sync_codebase_index.py --check
```
Exits nonzero if uncataloged source/.csproj files exist, stale entries exist, or `FILE_INDEX.md` is out of date.

### 2. Synchronize Codebase Index (Update Mode)
```powershell
python scripts/docs/sync_codebase_index.py
```
Discovers tracked source and project files, checks for missing catalog entries, and regenerates `docs/codebase/FILE_INDEX.md`. Fails safely without modifying the index if uncataloged files are detected (use `--repair` only if forcing update).

### 3. Validate Documentation Integrity
```powershell
python scripts/docs/validate_docs.py
```
This verifies that:
- Required files exist (`AGENTS.md`, `SYSTEM_MAP.md`, `docs/START_HERE.md`, `CURRENT_STATE.md`).
- All catalog entries correspond to existing filesystem paths.
- Skill frontmatter is valid YAML with `name` and `description`.
- Internal markdown links resolve properly.
- Returns exit code 0 on success, non-zero on failure.
