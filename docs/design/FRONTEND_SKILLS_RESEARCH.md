# Frontend Agent Skills Research

Status: **DRAFT — no skill installed and no hook activated**

Reviewed: 2026-10-20

Scope: upstream repositories and instructions available on the review date

## Recommendation

Use the smallest staged stack:

1. **Anthropic `frontend-design`**, project-level, for design direction and self-critique.
2. **A reviewed, commit-pinned snapshot of Vercel `web-design-guidelines`**, project-level, only when UI code exists and after removing or pinning its live mutable fetch.

Defer **Impeccable** and **UI/UX Pro Max**. They are capable, but both overlap substantially with the repository's own design documents and the proposed two-skill stack. Reconsider Impeccable later as an on-demand polish/audit tool with hooks off. Reconsider UI/UX Pro Max only if the team needs its searchable offline dataset.

No global installation is recommended. StudyAI has stronger product/security constraints than generic projects, and checked-in project scope makes review, pinning, cross-agent parity, and rollback practical.

## Comparison

| Candidate | Primary value | Executable surface | Network behavior | Codex fit | Recommendation |
|---|---|---|---|---|---|
| Anthropic Frontend Design | Distinctive direction, typography, layout, restraint, critique | None in the `frontend-design` skill directory observed | None required by its SKILL.md | Native Agent Skills format | Adopt project-level after approval |
| Vercel Web Design Guidelines | Focused code audit for accessibility and web-interface conventions | SKILL.md delegates review to fetched Markdown | Fetches mutable `main/command.md` on every review | Supported through Vercel Skills CLI | Vendor/pin before use; do not install current live-fetch form |
| Impeccable | Broad design workflow, context, critique, audit, polish, detector | Launcher plus self-contained platform binary; optional hooks | Launcher can download a pinned release binary and checksum on first run | Explicit Codex hook support | Defer; later manual use with hooks off |
| UI/UX Pro Max | Searchable style/product/UX datasets and generated design-system suggestions | npm CLI plus Python standard-library scripts | Installer/package retrieval needs network; bundled search scripts claim no runtime network | Explicit `--ai codex` support | Defer due overlap and footprint |

## Candidate details

### Anthropic Frontend Design

Upstream:

- Repository: [anthropics/skills](https://github.com/anthropics/skills)
- Skill: [frontend-design/SKILL.md](https://github.com/anthropics/skills/blob/main/skills/frontend-design/SKILL.md)
- License: Apache-2.0 in the skill directory.

Capabilities observed in SKILL.md:

- grounds visual choices in the subject matter;
- asks for a compact color/type/layout/principle plan before coding;
- discourages template-like typography, indiscriminate cards, decoration, and motion;
- requires responsive, keyboard, reduced-motion, and visual review discipline;
- emphasizes precise interface copy and useful empty/error states.

Executable/scripts/hooks:

- The reviewed `frontend-design` skill contains guidance rather than a required executable workflow.
- No hook activation, shell command, external API, or live instruction fetch is declared in its SKILL.md.
- The wider Anthropic repository contains other skills with scripts; installing only this named skill is therefore preferable to copying or trusting the whole repository indiscriminately.

Overlap:

- Overlaps with this repository's brainstorming and future UI guidelines, but adds a compact visual-critique lens.
- Does not replace StudyAI's PRD, architecture, design tokens, accessibility verification, or implementation tests.

Compatibility and install approach:

```powershell
# Example only; do not run until approved and pin an audited commit/ref.
npx skills add anthropics/skills --skill frontend-design
```

Install project-level into the standard `.agents/skills/` path used by StudyAI. Record the audited commit, license, included files, and update procedure. Avoid `@latest`-style unpinned automation in CI.

Risk: low executable risk for this isolated skill; medium governance risk if future upstream prompt changes silently alter design behavior. Mitigate by pinning and reviewing updates.

### Vercel Agent Skills — Web Design Guidelines

Upstream:

- Repository: [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)
- Skill: [web-design-guidelines/SKILL.md](https://github.com/vercel-labs/agent-skills/blob/main/skills/web-design-guidelines/SKILL.md)
- Rule source: [web-interface-guidelines/command.md](https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md)
- Installer: [vercel-labs/skills](https://github.com/vercel-labs/skills)

Capabilities observed:

- reviews specified files against web interface guidelines;
- emits terse `file:line` findings;
- is best suited to audit after components exist, not product direction discovery.

Executable/scripts/hooks:

- The skill itself is a short prompt and has no local analyzer.
- Its required action is to fetch the current rule document from the `main` branch on every review.
- No automatic project hook is declared in the reviewed skill.

Network and security finding:

- The fetched Markdown is both remote content and operational instruction. Because the URL is mutable and commit-unpinned, a review can change without a repository diff. An open upstream issue also calls for pinning or vendoring this content.
- This is a prompt-supply-chain and reproducibility risk, even when hosted by a reputable organization.

Compatibility and install approach:

```powershell
# Upstream command; do not run in current mutable-fetch form.
npx skills add https://github.com/vercel-labs/agent-skills --skill web-design-guidelines
```

Recommended adaptation: vendor the reviewed SKILL.md and guidelines at an audited commit into a project-level skill, retain license/attribution, remove the live fetch, and update through an explicit review. Confirm repository license metadata before vendoring.

Risk: low local execution risk; high reproducibility risk in the current design; medium maintenance risk because snapshots need deliberate refresh.

### Impeccable

Upstream:

- Site: [impeccable.style](https://impeccable.style/)
- Repository: [pbakaus/impeccable](https://github.com/pbakaus/impeccable)
- Skill: [impeccable/SKILL.md](https://github.com/pbakaus/impeccable/blob/main/.agent/skills/impeccable/SKILL.md)
- Hook reference: [reference/hooks.md](https://github.com/pbakaus/impeccable/blob/main/skill/reference/hooks.md)
- License: Apache-2.0.

Capabilities observed:

- extensive commands for shape, critique, audit, polish, accessibility, responsive adaptation, copy, motion, and design-system extraction;
- separates Operate, Read, Persuade, and Experience modes;
- loads product/design context and offers a design detector.

Executable/scripts/hooks:

- SKILL.md requires `scripts/impeccable context` once per session.
- On Windows the launcher uses `impeccable.cmd`.
- The launcher can execute a bundled platform binary or download a version-pinned engine binary on first use. The reviewed launcher checks a SHA-256 sidecar and refuses an unverified or mismatched download.
- Optional Codex hooks are written to `.codex/hooks.json` and run the detector after design-relevant edits. Hook configuration also uses `.impeccable/config.json` and a local config file.
- Hooks are opt-in and require Codex approval; they must remain off unless the Product Owner explicitly approves them.

Overlap:

- Heavy overlap with Anthropic Frontend Design, Vercel guidelines, StudyAI UI/UX guidelines, and manual review.
- Its broader lifecycle is useful only after a real frontend and visual truth exist.

Compatibility and install approach:

```powershell
# Example only; do not run without a pinned version, file review, and approval.
npx skills add pbakaus/impeccable
```

If later adopted: project-level, pin the release/commit, inventory binary hashes, keep hooks off, run commands manually, and prohibit automatic changes to product copy or design documents.

Risk: medium–high execution surface due to downloaded native binary and optional post-edit hooks; medium prompt overlap; mitigations are available but add operational cost.

### UI/UX Pro Max

Upstream:

- Repository: [nextlevelbuilder/ui-ux-pro-max-skill](https://github.com/nextlevelbuilder/ui-ux-pro-max-skill)
- Instructions: [README](https://github.com/nextlevelbuilder/ui-ux-pro-max-skill/blob/main/README.md)
- Skill template: [skill-content.md](https://github.com/nextlevelbuilder/ui-ux-pro-max-skill/blob/main/src/ui-ux-pro-max/templates/base/skill-content.md)
- Search script: [search.py](https://github.com/nextlevelbuilder/ui-ux-pro-max-skill/blob/main/src/ui-ux-pro-max/scripts/search.py)
- License: MIT.

Capabilities observed:

- BM25 search across curated style, color, typography, product, UX, icon, chart, and framework datasets;
- design-system suggestions with density, variance, and motion controls;
- stack guidance for Next.js, React, shadcn/ui, React Native, and others;
- optional persistence of generated master/page design-system files.

Executable/scripts/hooks:

- Installation uses the `ui-ux-pro-max-cli` npm package and `uipro`/`npx` commands.
- Runtime searches use bundled Python 3 standard-library scripts; upstream says these scripts install nothing and make no network calls.
- No hook behavior was found in the reviewed README.
- Persist mode writes design-system files, so it must not run casually in a repository with an authoritative documentation model.

Compatibility and install approach:

```powershell
# Upstream examples only; do not run now.
npx ui-ux-pro-max-cli init --ai codex --dry-run
npx ui-ux-pro-max-cli init --ai codex
```

Project-level is preferable. A dry run and package/file inventory must precede installation. Pin the npm package version and review generated files before accepting them.

Overlap and risk:

- High overlap with this design foundation and other design skills.
- A large recommendation dataset can produce false authority or generic category matching that conflicts with StudyAI's approved product model.
- npm supply-chain exposure exists at installation/update time; Python search is a smaller runtime risk but still executes repository-provided code.
- Upstream has had packaging/symlink issues, which reinforces the need for a dry run and pinned artifact review.

Risk: medium execution and maintenance risk; high duplication/decision-noise risk for the current stage.

## Installation governance

Before any later installation:

1. Open a dedicated, approved repository-tooling task; do not mix it with UI implementation.
2. Pin an exact commit or package version and record it in the skills setup documentation.
3. Inspect the complete selected skill directory, executable scripts, manifests, hooks, licenses, and transitive dependencies.
4. Run installer dry-run/list modes where available and capture intended writes.
5. Prefer project scope; prohibit unreviewed global installation.
6. Keep lifecycle/post-edit hooks disabled by default.
7. Treat remote Markdown, generated recommendations, and downloaded binaries as untrusted inputs.
8. Re-run documentation and security validation after installation.
9. Updates require the same review; do not automatically follow upstream `main` or `latest`.

## Final adoption decision needed

- Approve or reject project-level Anthropic `frontend-design` at a pinned revision.
- Approve whether StudyAI may vendor a pinned Vercel guideline snapshot when UI audit begins.
- Keep Impeccable and UI/UX Pro Max deferred unless a concrete gap appears.
- Keep every automatic hook disabled until a separate Product Owner security approval.
