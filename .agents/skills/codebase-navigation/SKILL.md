---
name: codebase-navigation
description: Token-efficient, hierarchical navigation of the StudyAI monorepo without expensive blind repository scans. USE FOR: locating modules, understanding file ownership, discovering entry points and architecture boundaries.
---

# Codebase Navigation Skill

Use this skill to navigate the StudyAI codebase efficiently using progressive context loading.

---

## When to Use
- Starting a new session or encountering an unfamiliar part of the codebase.
- Locating which project, module, or file owns a specific feature.
- Tracing execution flow from API host to domain and database.

---

## Navigation Protocol: Project → Application → Module → Feature → Target File

```
1. SYSTEM_MAP.md
   ↓ (Identify application tier & boundaries)
2. docs/codebase/MODULE_INDEX.md
   ↓ (Identify responsible module & contracts)
3. docs/codebase/FEATURE_MAP.md
   ↓ (Identify feature ID & entry points)
4. docs/codebase/catalog/<module>.md
   ↓ (Identify exact target files & verification status)
5. Target Source File
```

---

## Rules of Engagement

1. **Never Blind-Scan**: Do not run recursive directory dumps or read dozens of files into context.
2. **Consult Indexes First**: Look up files in `docs/codebase/FILE_INDEX.md` or the appropriate module catalog in `docs/codebase/catalog/`.
3. **Verify Status**: Note whether an entry is `VERIFIED` or `NEEDS_REVIEW`.
4. **Targeted Reading**: Use slice notation or targeted viewing to read only the lines or symbols necessary for the task.
