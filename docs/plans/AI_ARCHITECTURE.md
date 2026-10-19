# StudyAI Campus — Multi-Agent Engineering & Codebase Intelligence System v2.0

## ROLE & MISSION

You are the **Principal Software Architect, AI Agent Infrastructure Engineer, and Senior Developer Experience Engineer** responsible for establishing the development foundation of StudyAI Campus.

Your mission is to implement a production-quality, documentation-driven, multi-agent development infrastructure inside the existing StudyAI monorepo.

This system must allow **OpenAI Codex, OpenCode, and Google Antigravity** to work interchangeably on the project with consistent architectural understanding, efficient codebase navigation, reliable progress tracking, and minimal context loss.

This task is a repository infrastructure setup, **not a business-feature implementation task**.

Do not stop after proposing a design. Inspect the existing repository and implement the requested system.

Preserve existing work, avoid destructive operations, and report the actual results.

---

# 1. ARCHITECTURAL FOUNDATION

The new system must combine these seven pillars:

1. **Agent Execution Engine** — structured workflows for brainstorming, planning, implementation, debugging, review, and verification.
2. **Governance & Safety Rules** — mandatory impact analysis and regression prevention.
3. **Living System Map** — concise, continuously maintained architecture and repository navigation.
4. **Modular Agent Skills** — reusable domain-specific and engineering procedures.
5. **Live Task Tracking** — explicit and verifiable execution progress.
6. **Codebase Intelligence** — hierarchical module documentation, file catalogs, and automated consistency checking.
7. **Cross-Agent Context & Handoff** — persistent state allowing different AI agents to resume development safely.

The system must be:

- Model and provider agnostic.
- Git-native and repository-based.
- Token-efficient.
- Compatible with a growing monorepo.
- Maintainable without a complex external service.
- Explicit about unknown or unverified information.
- Safe for security-sensitive production code.

Do not introduce a database, vector search service, or MCP memory server solely for project documentation.

Use ordinary version-controlled Markdown, structured metadata, Git, and lightweight scripts first.

---

# 2. STUDYAI ARCHITECTURE BASELINE

StudyAI Campus is an AI-powered academic SaaS platform.

The approved architectural baseline is **StudyAI Core Architecture v1.2**.

Technology stack:

- Monorepo with independently deployable applications.
- Next.js and TypeScript for the web application.
- ASP.NET Core with a modular monolith backend.
- PostgreSQL with pgvector.
- EF Core for .NET persistence.
- S3-compatible private file storage.
- Independent .NET and Python background workers as needed.
- Future React Native / Expo applications for iOS and Android.
- Docker-based portable deployment infrastructure.
- API-first and mobile-ready architecture.

Important established principles:

- Personal Data Space ownership boundaries.
- Content Spaces supporting Library, Course, and Project.
- Unified Knowledge Items and reusable placements without duplicating underlying files.
- Scoped ownership isolation enforced across application and persistence boundaries.
- Authorized, scope-aware AI retrieval.
- Source version provenance.
- Idempotent AI quota reservation, commit, and release.
- Versioned object storage and explicit file lifecycle.
- Independent background processing.
- Replaceable AI and external service integrations.

Existing approved architecture specifications, ERDs, ADRs, and implementation documents take precedence over this summary.

Discover and reference them when available.

If these source documents are missing, create clearly labeled provisional references and report what is missing. Do not invent decisions or claim to have reviewed unavailable documents.

Do not redesign the approved architecture.

---

# 3. FIRST ACTION: SAFE REPOSITORY DISCOVERY

Before implementation:

1. Inspect repository root and Git status.
2. Identify existing applications, packages, workers, tests, and infrastructure.
3. Locate current AGENTS.md files, system maps, skills, rules, workflows, and architecture documentation.
4. Detect existing documentation conventions.
5. Identify existing scripts and CI configuration.
6. Check for conflicting or duplicated agent instructions.
7. Preserve useful existing infrastructure instead of replacing it unnecessarily.

The initial setup may require a broader repository inventory.

However, distinguish between:

- File discovery.
- Metadata extraction.
- Targeted source inspection.
- Full code analysis.

Do not dump the entire repository into the model context.

Prefer Git file listings, directory metadata, project manifests, language-server symbols, and targeted source inspection.

Use broader scanning only when necessary.

If this is a new or mostly empty monorepo, create infrastructure and templates without fabricating source modules.

Do not modify application business logic.

---

# 4. CANONICAL AGENT OPERATING CONTRACT

Create or update the root:

`AGENTS.md`

This file is the canonical operating contract for Codex, OpenCode, and Antigravity.

It must remain concise and contain:

- Project identity and confirmed technology stack.
- Repository navigation entry points.
- Mandatory architectural invariants.
- Coding and testing conventions.
- Git safety rules.
- Progressive context-loading rules.
- Documentation maintenance obligations.
- Task execution and handoff procedures.
- References to specialized rules and skills.

Avoid embedding extensive architecture documentation directly inside AGENTS.md.

Use references to detailed documents that agents load only when needed.

Support scoped AGENTS.md files for applications or modules when their conventions differ significantly.

Avoid duplicated or contradictory instructions.

## Cross-Agent Compatibility

Verify the actual supported instruction, skill, and workflow discovery mechanisms for the installed versions of:

- Codex.
- OpenCode.
- Google Antigravity.

Use root AGENTS.md as the shared canonical contract.

Preserve `.agent/` as an optional Antigravity-specific compatibility layer where appropriate.

Preserve `.agents/skills/` for portable reusable skills where supported.

Create small tool-specific adapters only when required and supported.

Never assume a tool automatically reads a particular file without verification.

Do not hardcode unsupported tool names into shared instructions.

Tool-specific instructions must remain isolated from portable engineering rules.

---

# 5. MANDATORY MAP CHECK & SAFETY GOVERNANCE

Preserve and modernize the previous architecture's Mandatory Map Check mechanism.

Create or adapt:

`.agents/rules/mandatory-map-check.md`

The rule must be referenced by root AGENTS.md and applied proportionally to task risk.

## Understand Before Modify

Before modifying existing behavior, determine:

- Relevant entry points.
- Callers and consumers.
- Callees and dependencies.
- Data flow.
- Public interfaces.
- Persistence implications.
- Tests covering affected behavior.

Start with the documented map, then verify necessary details against the actual source code.

Do not blindly trust stale documentation.

## Trace-by-Flow

For applicable request-processing paths, trace:

`Trigger → Entry Point → Handler → Service → Domain → Repository → Database`

For UI work, trace:

`Route → Component → Hook/State → API Client → Backend Contract`

For background jobs, trace:

`Trigger → Queue/Event → Worker → Service → Persistence/External System`

Adapt the flow to the actual codebase.

## Pre-Edit Analysis

For nontrivial changes, identify:

- Target files.
- Affected execution flow.
- Root cause or implementation reason.
- Expected behavior.
- Compatibility considerations.
- Relevant tests.

Keep this analysis brief and focused.

Do not require a full formal report for typo fixes, formatting, or other demonstrably low-risk changes.

## Root Cause Over Symptom

For bugs, identify evidence of the root cause before adding defensive patches.

Avoid arbitrary timeouts, null checks, retries, or other workarounds that only hide the underlying problem.

## Smallest Safe Change

Prefer the smallest coherent implementation that meets acceptance criteria.

Avoid unrelated refactoring.

## Risk-Based Verification

Use stronger impact analysis for:

- Authentication and authorization.
- Scoped ownership and tenant isolation.
- Payments and subscriptions.
- AI quota accounting.
- Database migrations.
- File deletion and storage lifecycle.
- Retrieval security.
- Concurrency and background workers.

Changes to these areas require relevant negative, boundary, and regression tests.

---

# 6. LIVING SYSTEM MAP

Create or update:

`SYSTEM_MAP.md`

This is the primary high-level navigation map for the repository.

It must contain:

1. System overview.
2. Confirmed technology stack.
3. Application and module boundaries.
4. Important entry points.
5. Main request and data flows.
6. Database and persistence overview.
7. Background workers and integration points.
8. Security-critical boundaries.
9. Links to detailed module documentation.
10. Links to the codebase indexes.
11. Documentation status and unresolved areas.

Use Mermaid diagrams where they materially improve understanding.

Keep the SYSTEM_MAP concise and navigational.

**Do not turn it into a massive encyclopedia or file-by-file listing.**

Separate the following concepts:

- Approved architecture: what the system is intended to be.
- Living system map: the current known organization of the repository.
- Source code: the observed implementation.
- Tests and runtime evidence: verified behavior.

When inconsistencies exist, report them explicitly.

Update SYSTEM_MAP when module boundaries, major flows, or important entry points change.

Do not rewrite it unnecessarily for internal implementation details.

---

# 7. HIERARCHICAL CODEBASE INTELLIGENCE

Create the following documentation hierarchy:

- `docs/START_HERE.md`
- `docs/codebase/MODULE_INDEX.md`
- `docs/codebase/FEATURE_MAP.md`
- `docs/codebase/FILE_INDEX.md`
- `docs/codebase/catalog/`

The navigation model must be:

**Project → Application → Module → Feature → Relevant Files**

## MODULE_INDEX

Maintain a concise inventory of:

- Applications.
- Modules.
- Module responsibilities.
- Public boundaries.
- Entry points.
- Related documentation.
- Associated tests.

Each significant module should have a small README or equivalent documentation.

Module documentation should describe:

- Purpose.
- Responsibilities.
- Important interfaces.
- Dependencies.
- Data flows.
- Security constraints.
- Relevant tests.
- Links to the module file catalog.

## FEATURE_MAP

Map user-facing and infrastructure features to their implementations.

For each feature, document:

- Feature name.
- Owning application/module.
- Primary entry points.
- Related services/components.
- Associated tests.
- Relevant specifications.
- Current implementation status, if known.

Do not create mappings for features that have not been implemented.

Planned features may be listed separately and explicitly marked PLANNED.

## FILE CATALOG

Implement structured file catalogs grouped by module.

Each relevant handwritten source file should have a discoverable record containing:

- Exact repository-relative path.
- Module ownership.
- File category.
- Concise responsibility.
- Important public interfaces or exports.
- Significant direct dependencies.
- Known callers or consumers where useful.
- Associated tests, if known.
- Security or architectural notes when relevant.
- Documentation verification status.

Use:

- `VERIFIED`
- `NEEDS_REVIEW`

A file listing or generated metadata is not proof that its semantic description is correct.

Only mark descriptions VERIFIED after appropriate source inspection.

Never invent callers, dependencies, responsibilities, or test coverage.

## Per-File Documentation Policy

Do not generate separate Markdown files for every source file.

Use compact centralized catalogs per module.

For complex files, allow additional focused documentation.

Preserve standard language-specific documentation conventions.

For critical source files, document:

- Non-obvious responsibilities.
- External side effects.
- Important assumptions.
- Security-sensitive invariants.
- Contracts that consumers depend upon.

Do not add repetitive boilerplate headers to obvious components, DTOs, generated code, or trivial files.

Avoid duplicate information between headers, module docs, and catalogs.

---

# 8. AUTOMATED FILE INDEXING

Implement lightweight tooling under:

`scripts/docs/`

Provide at least:

- A synchronization command.
- A validation command.
- Deterministic aggregate index generation.

Prefer Python standard library or an existing repository runtime.

The tooling must:

1. Discover relevant tracked source files.
2. Respect appropriate ignore rules.
3. Identify newly added source files.
4. Identify missing catalog entries.
5. Identify deleted files and stale catalog paths.
6. Detect probable renames when practical.
7. Refresh generated file indexes.
8. Report documentation coverage.
9. Support CI-friendly validation and nonzero failure exit codes.

The system may auto-generate placeholder catalog entries.

However:

**Never auto-generate unverified semantic descriptions and label them VERIFIED.**

Exclude generated files, binaries, caches, and build artifacts from per-file documentation coverage.

Treat generated artifacts at the module level instead.

Add lightweight tests for the indexing scripts.

Keep output stable to avoid unnecessary Git diffs.

A successful structural validation does not prove the semantic accuracy of descriptions. Document that limitation.

---

# 9. AGENT WORKFLOW ENGINE

Preserve the strongest elements from the previous Antigravity Superpowers architecture.

Implement portable workflows for:

- Brainstorming.
- Writing implementation plans.
- Executing plans.
- Single-flow task execution.
- Test-driven development.
- Systematic debugging.
- Verification before completion.
- Code review.
- Receiving and applying review feedback.
- Git worktree usage.
- Completing development branches.
- Documentation maintenance.
- Agent handoff.

Store canonical reusable procedures as skills or referenced repository documentation.

Provide tool-specific workflow shortcuts only when supported.

Use three primary conceptual entry points:

`brainstorm`

`write-plan`

`execute-plan`

Do not assume identical slash-command support across all coding agents.

If supported, provide adapters for commands such as:

`/brainstorm`

`/write-plan`

`/execute-plan`

Each workflow must remain concise and reference deeper instructions only when necessary.

Use brainstorming and formal planning for nontrivial or ambiguous work.

Do not require extensive planning for every simple correction.

---

# 10. SINGLE-FLOW TASK EXECUTION

Retain the Single-Flow Execution Model from the previous architecture.

The primary rule is:

**One actively edited implementation task per agent working tree at a time.**

Do not dispatch multiple code-writing agents into the same working tree simultaneously.

For independent parallel development, use separate Git worktrees or branches, with clear scope ownership.

Do not implement unnecessary multi-agent orchestration infrastructure during initial setup.

## Task Tracking

Create a task-specific structure:

`docs/plans/TASK-ID/`

Containing:

- `SPEC.md`
- `PLAN.md`
- `task.md`
- `HANDOFF.md`

`SPEC.md` describes requirements and acceptance criteria.

`PLAN.md` contains implementation steps and dependencies.

`task.md` is the live execution tracker.

`HANDOFF.md` describes resumable progress.

Preserve the previous table-only task tracking convention.

Recommended task tracker:

| Task ID | Description | Status | Evidence |
|---|---|---|---|
| 1 | Inspect relevant code | Pending | |
| 2 | Implement scoped change | Pending | |
| 3 | Execute targeted tests | Pending | |
| 4 | Update documentation | Pending | |
| 5 | Verify and hand off | Pending | |

Only one row may be `In Progress` per active agent working tree.

After a step is completed and verified, update its status and evidence.

Do not mark work complete before verification.

Use `docs/development/CURRENT_STATE.md` as a concise index of active tasks, branches, blockers, and project status.

Do not store detailed implementation history inside CURRENT_STATE.md.

---

# 11. RELIABLE CROSS-AGENT HANDOFF

Implement the handoff process as a first-class engineering workflow.

The system must support:

Codex → OpenCode

OpenCode → Antigravity

Antigravity → Codex

And other combinations.

## Before Leaving an Agent

The outgoing agent must:

1. Inspect Git status.
2. Inspect current diffs.
3. Identify the relevant branch and commit.
4. Record completed work.
5. Record incomplete work.
6. Record files affected.
7. Record actual test and build results.
8. Record blockers and known problems.
9. Record next recommended step.
10. Update the task HANDOFF.md.

Never claim that tests passed without running them successfully.

Never discard or overwrite unrelated changes.

Do not automatically create commits without authorization.

## When Starting a New Agent

The incoming agent must:

1. Read root AGENTS.md.
2. Read CURRENT_STATE.md.
3. Identify the active task.
4. Read its SPEC.md and HANDOFF.md.
5. Consult SYSTEM_MAP and relevant indexes.
6. Inspect Git status and relevant diffs.
7. Verify documented progress against actual source code.
8. Read only the necessary module and file-level documentation.
9. Identify the next actionable step.
10. Resume safely within the existing architecture.

Do not repeat full repository analysis by default.

## Unexpected Agent Interruption

If the previous agent reaches a quota limit, crashes, or stops without producing a handoff:

- Recover status from Git.
- Read the active task specification and tracker.
- Inspect relevant uncommitted changes.
- Reconstruct the minimum necessary context.
- Record uncertainty.
- Continue without discarding unfinished work.

The repository must remain the durable context source, not proprietary AI session history.

---

# 12. MODULAR SKILLS ECOSYSTEM

Preserve the modular skills concept from the previous architecture.

However, avoid installing a large collection of third-party skills immediately.

Start with the smallest useful set:

- `brainstorming`
- `writing-plans`
- `executing-plans`
- `systematic-debugging`
- `test-driven-development`
- `verification-before-completion`
- `codebase-navigation`
- `documentation-maintenance`
- `agent-handoff`

Add dedicated skills for ASP.NET Core security, Next.js, PostgreSQL, or other technologies only when the repository demonstrates a real need.

Use portable Agent Skills conventions when supported.

Every SKILL.md should have valid frontmatter, a clear purpose, and explicit triggers.

Avoid duplicated skill logic.

## External Skills Security

Do not execute arbitrary skill installation commands copied from the previous blueprint without verifying package identity, source, compatibility, and trustworthiness.

Do not automatically run external installers.

Prefer local skills with reviewable source content.

If third-party skills are approved and installed, preserve their installation metadata and lockfile when supported by the installation tooling.

Do not invent a skills-lock.json format or claim cryptographic verification that the chosen tool does not actually provide.

Review installed skills for unsafe or overly broad instructions.

---

# 13. SPECIALIZED REVIEW ROLES

Preserve the idea of specialized agent roles, but use them selectively.

Prepare lightweight definitions for:

- Software architecture reviewer.
- Security reviewer.
- Code quality reviewer.
- Frontend/UI reviewer.

The review process must check the implementation against both task requirements and StudyAI's architectural invariants.

For higher-risk code, distinguish:

- Specification compliance.
- Security and architectural correctness.
- Code quality and maintainability.
- Test coverage and verification evidence.

Do not claim independent review when the same agent merely rephrased its own output.

When independent agent execution is unavailable, provide a structured review checklist instead.

Avoid costly multi-agent review workflows for trivial changes.

---

# 14. DOCUMENTATION LIFECYCLE

Documentation maintenance is part of implementation completion.

When a code change:

- Creates or deletes files, update file catalogs.
- Renames files, update catalog paths and references.
- Changes responsibilities, update relevant descriptions.
- Adds modules, update module indexes.
- Changes feature ownership, update FEATURE_MAP.
- Changes architecture boundaries, update SYSTEM_MAP.
- Changes approved architectural decisions, obtain authorization and update ADRs.
- Changes APIs, update contracts and consumer documentation.
- Completes tasks, update the handoff and current state.

Do not rewrite unchanged documentation unnecessarily.

Avoid full regeneration of manually maintained semantic content.

Prefer targeted updates.

When documentation becomes stale, correct it using verified source evidence.

Never silently modify architecture documentation to rationalize an implementation that violates the approved design.

---

# 15. TOKEN-EFFICIENCY POLICY

This system must actively reduce unnecessary context usage.

Use progressive context loading.

Default session workflow:

1. Read compact repository instructions.
2. Identify the task.
3. Read active task context.
4. Locate relevant modules through SYSTEM_MAP or FEATURE_MAP.
5. Consult module documentation and file catalogs.
6. Open targeted source files.
7. Expand investigation only when evidence requires it.

For a simple known-file edit, skip unnecessary discovery and formal planning.

For an unfamiliar complex feature, begin with the navigation indexes rather than dumping source code.

Avoid:

- Repeated repository-wide scans.
- Loading all architecture documents.
- Reading every AGENTS.md or SKILL.md without relevance.
- Rewriting the entire SYSTEM_MAP for minor changes.
- Repeatedly running unrelated test suites.
- Verbose status updates without actionable information.

Optimize for **verified feature completion per unit of model usage**, not merely shorter responses.

Never trade away essential verification to save tokens.

---

# 16. AUTOMATED VALIDATION & CI

Create or integrate documentation validation into the existing development workflow.

Checks should include:

- Required documentation presence.
- Valid file catalog structure.
- Valid skill frontmatter.
- Missing or stale file catalog paths.
- Broken relative documentation links.
- Duplicate canonical instruction files where detectable.
- Correct separation of generated and manually maintained content.
- Valid task templates.
- Deterministic index generation.

Use cross-platform tools wherever practical.

A Python-based validation command should work on Windows, Linux, and macOS when Python is installed.

Do not assume Bash-only tooling is available in native Windows terminals.

Preserve existing tests and CI conventions.

Do not fail all development solely because an old file has not yet received a verified semantic description. Report baseline coverage separately and enforce at least structural consistency, with a documented path to stronger coverage rules.

---

# 17. IMPLEMENTATION PHASES

Execute in this order.

## Phase A — Discovery and Migration

Inspect the repository.

Identify existing structures and reusable parts of the previous agent architecture.

Preserve useful files.

Resolve duplicated or conflicting documentation.

Do not delete legacy content without permission.

## Phase B — Shared Agent Contract

Create or update root AGENTS.md, START_HERE.md, architectural references, and governance rules.

Verify the intended compatibility approach for Codex, OpenCode, and Antigravity.

## Phase C — Living System Map and Codebase Intelligence

Implement SYSTEM_MAP, module navigation, feature mapping, file catalogs, and generated indexes.

Use actual repository evidence.

Mark unknown information explicitly.

## Phase D — Agent Execution Workflows

Implement portable skills, planning templates, single-flow execution, verification rules, and review procedures.

Create platform-specific adapters only where supported.

## Phase E — Handoff and State Persistence

Implement CURRENT_STATE.md, task trackers, handoff templates, and recovery procedures.

## Phase F — Automation and Testing

Implement indexing, synchronization, validation scripts, and relevant CI checks.

Test the indexing process with temporary safe fixtures.

Test documentation link resolution and skill validity.

## Phase G — End-to-End Demonstration

Simulate the following:

1. A new agent enters the repository.
2. It reads the canonical instructions.
3. It identifies a task through CURRENT_STATE.
4. It navigates to a module using the system map.
5. It identifies relevant files through catalogs.
6. It determines an appropriate validation strategy.
7. It prepares a handoff for another agent.

Use an existing module when available.

If no source modules exist, demonstrate the process using templates and mark the scenario as hypothetical.

---

# 18. ACCEPTANCE CRITERIA

The setup is complete when:

1. A canonical root AGENTS.md exists.
2. The repository has a usable SYSTEM_MAP.md.
3. Architectural invariants are documented or linked to approved sources.
4. Relevant source files have structured catalog coverage or clearly reported documentation gaps.
5. Agents can locate modules without defaulting to full-repository source analysis.
6. A working documentation sync/validation mechanism exists.
7. Task specifications and live trackers are available.
8. Cross-agent handoff procedures are documented and testable.
9. Existing source and Git history remain safe.
10. Actual verification results are reported.

Do not claim the system is fully compatible with an agent unless the relevant integration has been tested.

Do not report semantic documentation coverage as complete when files remain NEEDS_REVIEW.

---

# 19. FINAL DELIVERY REPORT

After implementation, report:

### Infrastructure
- Files created and modified.
- Final directory structure.
- Existing files reused.
- Changes made to the previous architecture.

### Codebase Documentation
- Number of inventoried source files.
- Number of cataloged source files.
- Number of VERIFIED entries.
- Number of NEEDS_REVIEW entries.
- Modules documented.
- Any missing architecture sources.

### Agent Compatibility
- Codex compatibility status.
- OpenCode compatibility status.
- Antigravity compatibility status.
- Tested versus untested integrations.

### Verification
- Commands executed.
- Actual exit codes.
- Tests that passed.
- Tests that failed.
- Checks that were not run.

### Developer Instructions
Provide concise instructions explaining:

1. How to start a new AI coding session.
2. How to begin a new feature.
3. How to perform safe code modifications.
4. How to keep documentation synchronized.
5. How to switch between coding agents.
6. How to recover after an interrupted session.
7. How to run documentation validation.

### Remaining Work
List unresolved issues and optional future enhancements.

---

# FINAL OPERATING PRINCIPLES

**Documentation Before Blind Discovery.**

**Understand Before Modify.**

**Root Cause Before Workaround.**

**Smallest Safe Change.**

**Evidence Before Completion.**

**Git and Repository Context Before Conversation Memory.**

**Load Only Relevant Context.**

**One Active Implementation Task per Working Tree.**

The result should be a scalable AI engineering environment that allows StudyAI to grow into a large multi-application codebase without forcing every new coding agent to rediscover the entire system.

Implement the setup now using the repository's actual state. Preserve existing work, avoid unrelated implementation changes, and finish with verifiable results.