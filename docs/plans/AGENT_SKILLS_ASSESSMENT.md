# Agent Skills and Plugin Assessment

Assessment date: 2026-10-19. Ponytail 5.3.2 is installed user-level with hooks untrusted/inactive. OpenSpec, four official .NET skills, and three isolated Superpowers skills are installed project-level under `.agents/skills`; Context7 and Vercel remain deferred. Exact revisions and evidence are recorded in `CODEX_SKILLS_SETUP.md`.

## Assessment criteria

- Must preserve the PRD and Core Architecture hierarchy and never reduce the nine-module V1 scope.
- Prefer official, narrowly scoped, inspectable skills.
- Treat every skill as privileged instructions/code: review its SKILL.md, scripts, hooks, network access, permissions, update model, and license before approval.
- Install project-scoped rather than globally where practical; pin a reviewed release/commit; record ownership and an uninstall/update path.
- Avoid overlapping always-on instructions that compete with root AGENTS.md.

OpenAI's official skill documentation describes a skill as a SKILL.md manifest with optional references, scripts, and assets, and warns that skills can influence planning, tool use, and command execution. That supports a review-first, least-privilege adoption policy: https://developers.openai.com/api/docs/guides/tools-skills

## Candidate evaluation

| Candidate | Fit for StudyAI | Overlap and risk | Recommendation |
|---|---|---|---|
| OpenSpec | High for feature/change proposals, acceptance criteria, and traceable implementation plans across a large PRD. Official project: https://github.com/Fission-AI/OpenSpec | Overlaps docs/plans, docs/adr, PRD acceptance criteria, and Codex planning. A second requirements tree can drift or accidentally appear more authoritative than the PRD. Adds CLI/artifact conventions. | **Initialized project-level with six Codex skills.** Its config explicitly preserves the StudyAI authority hierarchy; pilot it on one narrow prerequisite change before wider adoption. |
| Official .NET Agent Skills | High once Phase 2 starts. The .NET team's collection includes ASP.NET Core and testing skills: https://github.com/dotnet/skills | Overlaps Context7/current docs retrieval and generic C# knowledge. Installing the whole catalog would add unnecessary context and update surface. Some custom-agent formats are host-specific, while portable skills are suitable for Codex. | **Four individual project skills installed:** `csharp-expert`, `dotnet-webapi`, `optimizing-ef-core-queries`, and `run-tests`. No full plugin or host-specific agent was installed. |
| Superpowers subset | High for disciplined diagnosis, test-first behavioral changes, and evidence-backed completion: https://github.com/obra/superpowers | The full plugin competes with OpenSpec through planning, execution, worktree, subagent, and session-start workflows. Its TDD rule is intentionally strict and needs human-approved exceptions for generated/config-only work. | **Only** `systematic-debugging`, `test-driven-development`, and `verification-before-completion` are installed as project skills. The plugin and its hooks are excluded; OpenSpec remains the specification workflow. |
| Vercel React Skills | High for Next.js/React performance and review after apps/web exists. Official collection: https://github.com/vercel-labs/agent-skills | React performance rules overlap framework docs and linting; premature optimization can distract from behavior, accessibility, and security. Vercel deployment-specific advice must not silently choose the production host. | Add react-best-practices after the frontend baseline and tests exist. Treat Vercel hosting/optimization skills as optional unless Vercel is explicitly selected. |
| Playwright CLI and skills | High for interactive browser diagnosis, selector inspection, screenshots, and agent-assisted E2E; official coding-agent guide: https://playwright.dev/docs/getting-started-cli | Overlaps Playwright Test and Codex computer-use/browser tools. The CLI is not a replacement for checked-in deterministic Playwright tests. Browser downloads add size and supply-chain surface. | Add the project test dependency and deterministic E2E suite when Phase 3 UI begins. Approve Playwright CLI/skills separately if interactive agent workflows materially help. Pin versions and browsers in CI. |
| Context7 | Medium to high for current Next.js, EF/Npgsql, pgvector, and library documentation. The official Codex integration was audited but not installed. Docs: https://context7.com/docs/api-guide | Overlaps official OpenAI Docs, Microsoft Learn/docs, framework docs, and technology-specific skills. It requires OAuth or an API key/external account, and retrieved snippets are external context rather than project authority. | **Deferred pending explicit external-account/API-key approval.** Prefer official vendor docs for architectural/security decisions and verify critical behavior with tests. |
| Ponytail | Medium. It emphasizes minimal complete solutions and correctness/security-aware review: https://github.com/DietrichGebert/ponytail | Strong overlap with root AGENTS.md, Codex behavior, review skills, and OpenSpec. Its broad “any coding task” trigger and always-on hooks could mistake mandatory abstractions or staged nine-module delivery for removable complexity. | **Approved and installed for initial setup.** Prefer explicit review/simplification invocation. Keep lifecycle hooks untrusted until separately approved; root StudyAI rules narrow and override its generic trigger. See `CODEX_SKILLS_SETUP.md`. |
| OpenAI security-best-practices skill | Useful for secure-by-default TypeScript/Python guidance and targeted reviews: https://github.com/openai/skills/tree/main/skills/.curated/security-best-practices | Its published trigger/coverage is focused on Python, JavaScript/TypeScript, and Go, so it does not replace ASP.NET Core/.NET guidance, database tests, SAST, or threat modeling. | Consider a reviewed project-scoped copy when frontend/Python source exists. Pair with .NET security guidance and automated scanners. |
| Codex Security | Potentially high after meaningful source exists. The official plugin contains repository/diff scan workflows: https://github.com/openai/plugins/tree/main/plugins/codex-security | Overlaps manual security review and generic security skills but provides a deeper dedicated workflow. Access may be gated; scans can produce sensitive findings and do not prove absence of vulnerabilities. | Reassess at the end of Phase 2 and before Phase 7. Keep results private, validate findings, and retain ordinary threat modeling, CodeQL/SAST, dependency, secret, and adversarial integration tests. |
| GitHub plugin | Useful later for repository/PR/issue/CI workflows; discovery showed it available but not installed. | It is a repository integration, not a security scanner or replacement for local Git. It can write to a remote when authorized, which is outside this bootstrap scope. | Do not connect until a remote and team workflow are intentionally created. Apply least-privilege permissions and write approvals. |

## Overlap map

- Governance: root AGENTS.md sets mandatory repository behavior; OpenSpec can structure individual change proposals; Ponytail also changes agent behavior and therefore has the highest instruction-conflict risk.
- Current documentation: official .NET/Vercel skills provide domain workflows; Context7 retrieves current library snippets. Prefer a relevant official skill first and use Context7 only to fill a version-specific gap.
- Browser testing: Playwright Test is the committed test framework; Playwright CLI/skills accelerate exploration and debugging; generic browser automation is not test evidence.
- Security: secure-by-default skills guide implementation; SAST/dependency/secret tools detect classes of issues; Codex Security performs dedicated model-assisted review; manual threat models and ownership/concurrency tests remain mandatory.
- Repository integration: GitHub plugin manages remote workflow; it does not replace Git, CI configuration, code review, or security tooling.

## Recommended approval and installation sequence

1. Initial approved set: OpenSpec as workflow authority, a narrow official .NET skill subset, three isolated Superpowers workflow skills, and Ponytail as a complementary quality layer are installed and verified. Context7 remains deferred for credential/permission approval.
2. Keep Ponytail lifecycle hooks untrusted initially. Pilot explicit `ponytail:ponytail-review` on a non-critical change and reassess hook activation only with separate approval.
3. Pilot OpenSpec on one technical change. Reject it if it creates a duplicate authority hierarchy.
4. Connect Context7 read-only for version-specific documentation after permission/network review; continue to cite primary vendor sources for decisions.
5. Re-audit and deliberately update the pinned .NET and Superpowers skill copies when needed; do not install their full plugin catalogs by default.
6. Phase 3: add Vercel react-best-practices and the Playwright test toolchain/optional CLI only when frontend code and E2E targets exist.
7. End of Phase 2 onward: introduce language-appropriate secure-coding guidance, dependency/secret/SAST checks, and evaluate Codex Security for authorized repositories.
8. Connect GitHub only after the user creates/chooses a remote and authorizes the intended repository actions.

## Approval checklist for any candidate

- Exact source, release/commit, owner, license, and checksum recorded.
- SKILL.md and every referenced script/hook reviewed.
- Permissions, network destinations, credential needs, and write/destructive actions documented.
- Project authority and trigger rules do not conflict with AGENTS.md, PRD, or Architecture v1.2.
- Narrow installation scope and rollback/removal steps verified.
- Trial task and measurable benefit defined; redundant tooling removed rather than accumulated.
