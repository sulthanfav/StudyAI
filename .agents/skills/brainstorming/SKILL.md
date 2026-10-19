---
name: brainstorming
description: Interactive exploration and requirements clarification before creating formal plans or code changes. USE FOR: clarifying underspecified requirements, exploring design options, identifying edge cases.
---

# Brainstorming Skill

Use this skill when exploring new features, architectural ideas, or resolving underspecified requirements before formal planning or implementation.

---

## When to Use
- Starting an unfamiliar or ambiguous feature.
- Evaluating architectural trade-offs across multiple options.
- User triggers `/brainstorm` or requests brainstorming.

---

## Workflow

### 1. Identify Context & Authority
- Review `docs/INDEX.md` and `docs/product/StudyAI_Campus_MVP_Final_PRD_v1.0.md`.
- Determine whether the topic touches an existing architectural decision or Open Decision (`docs/adr/OPEN_DECISIONS.md`).

### 2. Clarify Core Requirements & Constraints
- Frame the problem statement: What user goal or system invariant is being addressed?
- Identify mandatory non-negotiable constraints (e.g., Data Space ownership, quota metering, clean architecture boundaries).

### 3. Generate 2–3 Viable Options
- Compare trade-offs (complexity, token efficiency, maintainability, performance).
- Avoid premature optimization or introducing unneeded external services (vector DBs, external memory servers).

### 4. Present Recommendation & Seek User Alignment
- Recommend the simplest option that satisfies PRD acceptance criteria.
- Ask targeted clarifying questions if intent remains ambiguous.

### 5. Transition
- Once aligned, proceed to `writing-plans` or OpenSpec change proposal.
