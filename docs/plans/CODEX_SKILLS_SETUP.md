# StudyAI Codex Skills Setup

Status: active setup record, last verified 2026-10-19.  
Scope: user-level Ponytail plus project-level assets under `D:\WORK\StudyAI\.agents`; no application code, schema, PRD, or architecture change.

## 1. Authority and workflow

Installed skills are implementation aids, not project authorities. The precedence remains:

1. Latest explicit Product Owner decision.
2. StudyAI Final PRD v1.0.
3. Approved Core Architecture v1.2.
4. Root `AGENTS.md` and the remaining documentation hierarchy in `docs/INDEX.md`.
5. Change-specific OpenSpec artifacts when OpenSpec is installed and adopted.
6. Framework/documentation/review skills, including Ponytail.

The intended daily flow is:

`Requirements → OpenSpec change specification → plan → implement → test → Ponytail review/simplification → verification`

OpenSpec remains the primary specification and change-management workflow. Ponytail may simplify an implementation or review a diff, but it may not redefine requirements or architecture.

## 2. Initial setup plan and current state

| Capability | Intended role | Current state |
|---|---|---|
| OpenSpec | Primary change specification and traceability | **Initialized project-level with OpenSpec 1.14.3; six Codex skills discovered** |
| Context7 | Version-relevant library documentation | Audited but **deferred** because the supported Codex paths require OAuth or an API key/external account permission |
| Official .NET Agent Skills | Narrow C#, ASP.NET Core, EF Core, and testing guidance | **Four individual skills installed project-level from a pinned official revision** |
| Ponytail | Complementary simplicity, maintainability, and code review | **Installed and verified; lifecycle hooks not trusted/activated** |
| Superpowers subset | Debugging/TDD/verification only where it does not duplicate OpenSpec | **Three individual skills installed project-level; plugin and hooks excluded** |
| Vercel frontend skills | React/accessibility/performance guidance | Official `vercel-react-best-practices` audited; deferred until the frontend baseline exists |
| Playwright CLI/skills | Interactive browser diagnosis alongside deterministic E2E tests | Deferred until UI/E2E work begins |

Project skills are stored in `.agents/skills`. `.agents/rules` and `.agents/roles` are documented inventories only: root `AGENTS.md` remains the active rule authority, and native Codex custom agents would require reviewed `.codex/agents/*.toml` definitions. No custom role or project lifecycle hook was activated.

## 3. Ponytail installation record

| Field | Verified value |
|---|---|
| Official repository | `https://github.com/DietrichGebert/ponytail` |
| Installation method | Official Codex plugin marketplace commands |
| Scope | User-level Codex plugin |
| Plugin identity | `ponytail@ponytail` |
| Manifest version | `5.3.2` |
| Audited/installed revision | `09c434ab73ca43761c5d8b0c5d341926c2fba710` |
| License | MIT |
| Marketplace snapshot | `C:\Users\sulth\.codex\.tmp\marketplaces\ponytail` |
| Installed cache | `C:\Users\sulth\.codex\plugins\cache\ponytail\ponytail\5.3.2` |
| Node runtime | `v24.14.0` |
| Plugin status | Installed, enabled |
| Hook trust | Not granted |

The marketplace was pinned during installation to the audited commit:

```powershell
codex plugin marketplace add DietrichGebert/ponytail --ref 09c434ab73ca43761c5d8b0c5d341926c2fba710 --json
codex plugin add ponytail@ponytail --json
```

The upstream marketplace entry itself names `main`; therefore maintenance must not run an unreviewed marketplace upgrade. Before upgrading, resolve the new commit, re-audit changed manifests/skills/hooks/scripts, rerun tests, and record the new revision here.

## 4. Installed Ponytail skills

Codex discovered all six skills in a new process:

| Skill | Intended StudyAI use | Mutation behavior |
|---|---|---|
| `ponytail:ponytail` | Find the smallest complete implementation while retaining project architecture and controls | May guide implementation changes |
| `ponytail:ponytail-review` | Review a diff for correctness, security, load, missing tests, and avoidable complexity | Read-only report |
| `ponytail:ponytail-audit` | Broader repository audit with ranked findings | Read-only report |
| `ponytail:ponytail-debt` | Inventory explicit `shortcut:`/legacy `ponytail:` debt markers | Read-only unless separately asked to persist a ledger |
| `ponytail:ponytail-gain` | Display upstream benchmark claims | Display only; figures are not evidence about StudyAI |
| `ponytail:ponytail-help` | Show commands and modes | Display only |

Preferred use is explicit invocation, especially `ponytail:ponytail-review` after tests and before final verification. Do not use benchmark claims as StudyAI performance, cost, quality, or line-count evidence.

## 5. Manifest, scripts, and permission review

The Codex compatibility manifest declares only packaged skills and lifecycle hooks. It declares no MCP server, external account, API key, application connector, production access, or repository write tool.

The root package has no install/postinstall lifecycle script. Its declared npm script runs tests. The `scripts/` directory contains maintenance utilities:

- `check-rule-copies.js` and `check-versions.js`: read-only consistency checks.
- `build-openclaw-skills.js`: writes generated OpenClaw copies when manually invoked; not used by Codex installation.
- `publish-openclaw-skills.js`: invokes `clawhub` and publishes externally when manually invoked; not executed and not authorized for StudyAI setup.
- `cursor-hooks.js`: edits Cursor hook configuration when manually invoked; not executed.
- `uninstall.js`: removes Ponytail state/configuration and selected hook/statusline entries; destructive cleanup that must only be run during an explicitly approved uninstall.

Network access was used only by Codex/Git to retrieve the official marketplace. The installed skills themselves are instruction files. No dependency installation, API credential, or external account connection was required.

## 6. Lifecycle hook assessment

Ponytail's Codex hook manifest registers three events backed by local Node.js scripts:

| Event | Behavior | Access and side effects |
|---|---|---|
| `SessionStart` | Selects the default mode, injects Ponytail instructions, and builds an optional codebase map | Reads Git-tracked source; runs Node and `git ls-files`; reads at most 4,000 matching files and at most 64 KiB per file; writes mode state under Codex `PLUGIN_DATA` |
| `UserPromptSubmit` | Reads each submitted prompt to recognize Ponytail mode/default/deactivation commands | Receives prompt text; writes/removes mode state; `/ponytail default ...` can write `%APPDATA%\ponytail\config.json` |
| `SubagentStart` | Injects the current mode into subagents | Reads mode state and hook input; default scope is every subagent; optional regex scoping fails open to injection |

The audited Codex hook path contains no network request and does not edit StudyAI source. It does automatically add instructions to model context and scans source at session start, which is still security-relevant:

- Automatic injected instructions can compete with project rules even though higher-priority `AGENTS.md` remains authoritative.
- The general skill says it applies to any coding task; StudyAI narrows that behavior to appropriate simplicity/review work.
- A codebase map may expose repository symbol names to the model context and adds startup I/O.
- Hook updates change executable code. Automatic marketplace/plugin updates are not recommended without re-audit.
- Subagent injection is broad unless explicitly scoped.

Official OpenAI documentation states that installing or enabling a plugin does not automatically trust bundled hooks; Codex skips them until the user reviews and trusts the current definition: `https://developers.openai.com/plugins/build/plugins#bundled-mcp-servers-and-lifecycle-hooks`.

### Activation decision

**Current status: hooks are untrusted and inactive.**

During this audit, the open session unexpectedly contained injected Ponytail mode instructions. Inspection proved that all three Ponytail hook hashes had been trusted in the user configuration and `.ponytail-active` existed, contradicting the recorded policy. The three Ponytail-only trust records were removed and the active-state file was moved to a recoverable backup. A fresh ephemeral Codex process then discovered the skills without recreating `.ponytail-active`. No `--dangerously-bypass-hook-trust` flag was used.

Recovery backups:

- `C:\Users\sulth\.codex\config.toml.pre-studyai-hook-disable-20261019-153325.bak`
- `C:\Users\sulth\.codex\plugins\data\ponytail-ponytail\.ponytail-active.disabled-20261019-153325`

If automatic hooks are later approved:

1. Open `/hooks` in Codex and review the exact current hook definition.
2. Trust only the reviewed hash/revision.
3. Start with `PONYTAIL_DEFAULT_MODE=off` so activation remains explicit.
4. Consider `PONYTAIL_MAP=0` to disable startup source mapping.
5. Scope `PONYTAIL_SUBAGENT_MATCHER` if only selected agent types should receive the rules.
6. Start a new thread and verify the mode/state behavior before normal development.

Do not use `--dangerously-bypass-hook-trust` for routine StudyAI work.

## 7. StudyAI conflict resolution

Ponytail's simplification rule is accepted only after these non-negotiable requirements are satisfied:

- Data Space access is resolved server-side for every relevant path.
- Tenant-safe composite keys and adversarial ownership tests remain.
- AI source-version provenance and citation truthfulness remain.
- Usage reservation, settlement/release, entitlement, rate-limit, and cost-cap controls remain.
- Document versions, private storage, lifecycle, retention, and recovery behavior remain.
- Domain/module boundaries, explicit contracts, acceptance criteria, and proportionate tests remain.
- The public Campus V1 scope remains all nine modules.

Required abstractions or controls are not over-engineering merely because they add lines. When Ponytail conflicts with an accepted ADR, PRD behavior, architecture invariant, or `AGENTS.md`, ignore the Ponytail instruction and record the conflict.

## 8. Verification evidence

| Check | Result |
|---|---|
| Official `main` revision resolved | PASS — `09c434ab73ca43761c5d8b0c5d341926c2fba710` |
| Marketplace snapshot revision | PASS — same commit |
| Installed cache revision | PASS — same commit |
| Manifest version | PASS — 5.3.2 |
| Codex plugin inventory | PASS — `ponytail@ponytail`, installed and enabled |
| Installed `SKILL.md` count | PASS — six |
| Fresh-process skill discovery | PASS — all six names returned |
| Explicit `ponytail:ponytail-help` invocation | PASS — returned the help title and all six skill names in an ephemeral read-only process |
| Upstream test suite | PASS — 144 root tests + 30 Pi tests, zero failures |
| Hook manifest SHA-256 | `72E6C54FF7D428D8C969F9929672902F145C042A88B35F848E17787A61936C82` |
| Base skill SHA-256 | `F18008471528F0ADDE90A560D053F85935EED5CEE26C447B7177AD24472BCF32` |
| Automatic hook execution | Was observed at audit start due to persisted trust; trust records removed and active state disabled; fresh-process check did not reactivate it |
| Application/schema changes | None |
| Git commit | None |

Fresh-process discovery used an ephemeral, read-only Codex invocation and returned:

```json
["ponytail:ponytail","ponytail:ponytail-audit","ponytail:ponytail-debt","ponytail:ponytail-gain","ponytail:ponytail-help","ponytail:ponytail-review"]
```

The fresh CLI process acts as a reload verification. The currently open Codex desktop process may still require a normal app restart before its UI lists the newly installed plugin; restarting it was not forced because that would interrupt this active task.

The discovery run also emitted non-blocking warnings about existing bundled-runtime marketplace snapshots and unrelated icon metadata. Ponytail discovery completed successfully with exit code 0, but those warnings should be rechecked after the next normal Codex desktop restart if they persist.

## 9. Maintenance and removal

- Keep automatic updates disabled for Ponytail. Review upstream changes and pin the next approved commit before upgrade.
- Re-run upstream tests and fresh-process skill discovery after every update.
- Re-review hook hashes whenever `/hooks` reports a changed definition.
- To remove the plugin: `codex plugin remove ponytail`, then remove the marketplace only if no longer needed: `codex plugin marketplace remove ponytail`.
- If hooks are ever activated and state is created, review and run Ponytail's cleanup script before plugin removal only with explicit approval.

## 10. Recommended StudyAI configuration

- Plugin installed and skills available.
- Lifecycle hooks untrusted/inactive.
- Explicitly invoke `ponytail:ponytail-review` for code-review and simplification passes.
- Use base Ponytail mode only after requirements, OpenSpec change artifacts, and architecture constraints are already clear.
- Never accept deletion or simplification of required security, data-integrity, provenance, metering, lifecycle, or test behavior.
- Reassess always-on hooks only after a non-critical pilot demonstrates benefit without instruction conflicts.

## 11. Project-level installation inventory

### OpenSpec

| Field | Verified value |
|---|---|
| Official repository | `https://github.com/Fission-AI/OpenSpec` |
| Audited revision | `c794978f4f8845ce59693373c9dd882ee776588c` |
| Package/version | `@fission-ai/openspec` 1.14.3 |
| npm integrity | `sha512-ALFW1d5bSm6t27Kmcle93SDFuqUc8fLwy5ZdBfIiNffo0Yk9AEOJxw6xDqBv38eMM7l7TeOZBB/7C08luxYGfg==` |
| Execution method | Version-pinned `npx`; no global installation |
| Telemetry during setup | Disabled with process-scoped `OPENSPEC_TELEMETRY=0` |
| Project configuration | `openspec/config.yaml`, schema `spec-driven` |
| Generated location | `.agents/skills/` |
| Generated skills | `openspec-propose`, `openspec-explore`, `openspec-update-change`, `openspec-apply-change`, `openspec-sync-specs`, `openspec-archive-change` |

The configuration makes OpenSpec the change-specific specification workflow while explicitly subordinating it to `AGENTS.md`, `docs/INDEX.md`, the PRD, Architecture v1.2, and accepted ADRs. `openspec status` returned no active changes. `openspec validate --all --strict` returned “No items found to validate,” which is expected for a newly initialized empty specification tree and is not evidence that a change artifact passed validation.

OpenSpec installs no lifecycle hook here, but several skills are intentionally write-capable when explicitly invoked: propose/update write planning artifacts, apply edits implementation files, sync can modify or remove canonical OpenSpec specs according to an approved delta, and archive moves a completed change into the archive tree. Those operations remain subject to the user's requested scope, the repository rules, and ordinary destructive-action safeguards.

### Official .NET skills

Source: `https://github.com/dotnet/skills`, pinned revision `10d85e25fb42d5f65c568d5e979a005ee69eecb3`.

| Installed skill | Source path | StudyAI use |
|---|---|---|
| `csharp-expert` | `plugins/dotnet/skills/csharp-expert` | C#/.NET routing and language fallback |
| `dotnet-webapi` | `plugins/dotnet-aspnetcore/skills/dotnet-webapi` | ASP.NET Core API guidance and validation |
| `optimizing-ef-core-queries` | `plugins/dotnet-data/skills/optimizing-ef-core-queries` | EF Core query correctness/performance review; not a migration generator |
| `run-tests` | `plugins/dotnet-test/skills/run-tests` | Correct .NET test-platform detection, execution, filtering, and evidence reporting |

The complete plugins were not installed. This avoids unrelated agents, host-specific LSP declarations, MCP servers, and the much larger .NET catalog. Byte-for-byte SHA-256 file comparison against the audited checkout passed for all four installed directories.

### Superpowers subset

Source: `https://github.com/obra/superpowers`, pinned revision `bb92a77741419a4ab5f06e711a283343f1ada0c3`, upstream manifest version 7.0.0.

Installed individual skills:

- `systematic-debugging`
- `test-driven-development`
- `verification-before-completion`

The Superpowers plugin, session-start hook, planning/execution workflows, worktree workflow, and subagent workflows were not installed. The installed debugging folder includes `find-polluter.sh`; it was reviewed but not executed. It enumerates tests and runs `npm test` per file, so it must only be used after confirming the repository and test runner match its assumptions. Byte-for-byte comparison against the audited checkout passed for all three installed directories.

## 12. Context7 decision

The official repository was audited at revision `ae4871e9a8cbed00bc0f4a40b0b5b61e2d37378c`; the current npm MCP package reported version 4.4.3. The supported Codex integration choices either:

- install the Context7 plugin and open an OAuth login, or
- add an MCP server using a Context7 API key in user- or project-level Codex configuration.

Both require external-account authorization or a credential. Because this task prohibits granting external-account permissions and no API key was provided, Context7 was not installed or configured. No placeholder secret, unauthenticated endpoint, or runtime `npx @latest` command was added. When approved, prefer the official hosted plugin/OAuth path or a project-scoped `.codex/config.toml` with a securely supplied API key; never commit the credential.

## 13. Vercel React evaluation

The official `vercel-labs/agent-skills` repository was audited at revision `063bee94c3f4df8453406c830b0a7df0f2860278`. `vercel-react-best-practices` version 1.0.0 is the only recommended future addition for the current architecture. It contains 70 React/Next.js performance rules and no required lifecycle hook.

Installation is deferred because `apps/web` is currently empty and there is no Next.js baseline to inspect or test. Deployment, token, and Vercel-cost skills are not approved because Vercel is not the selected production host. Reassess `vercel-react-best-practices` when Phase 3 creates the frontend and pins its Next.js/React versions.

## 14. Discovery and invocation verification

A fresh ephemeral Codex process loaded the project and returned these 19 relevant discoverable skills:

```json
["csharp-expert","dotnet-webapi","openspec-apply-change","openspec-archive-change","openspec-explore","openspec-propose","openspec-sync-specs","openspec-update-change","optimizing-ef-core-queries","run-tests","systematic-debugging","test-driven-development","verification-before-completion","ponytail:ponytail","ponytail:ponytail-audit","ponytail:ponytail-debt","ponytail:ponytail-gain","ponytail:ponytail-help","ponytail:ponytail-review"]
```

An explicit read-only invocation smoke test returned:

```json
{"openspec-explore":"loaded","csharp-expert":"loaded","systematic-debugging":"loaded"}
```

The other ten project-level skills were confirmed discoverable by exact name and have valid `SKILL.md` files, but were not behaviorally invoked because doing so would create or act on a change, execute tests, or require implementation context. `ponytail:ponytail-help` had already passed explicit invocation in the earlier installation verification.

The fresh-process checks emitted two unrelated, non-blocking icon-path warnings from an installed plugin and a PowerShell shell-snapshot warning. They exited successfully. The current open desktop thread loaded its skill catalog before these project files existed; use a new thread or restart/reload the app for UI discovery.

## 15. Installed plugin/session audit

`codex plugin list --json` reported these enabled installed plugins: `documents`, `pdf`, `spreadsheets`, `presentations`, `template-creator`, `codex-app-tools`, `browser`, `unified-computer-use`, `computer-use`, `code-review`, `visualize`, and `ponytail`. OpenSpec, the .NET subset, and the Superpowers subset are project skills rather than installed plugins. Context7 and Vercel are not installed.

The local installed-plugin inventory completed, but the command could not refresh the remote ChatGPT plugin catalog in the sandboxed check. That warning does not invalidate the local installed list, but it means this audit does not claim a complete live catalog of every remotely available plugin.

At task start, the current desktop session already exposed bundled/system skills plus the six Ponytail skills. It did not yet expose the new project skills because discovery occurred before installation. Fresh-process verification is the authoritative post-install discovery evidence.

No project plugin manifest, MCP manifest, or hook manifest exists under `.agents`. No system-wide setting, application source, database schema, PRD, or Core Architecture file was changed. No Git commit was created.

## 16. Remaining blockers and next safe step

- Context7 needs explicit approval for OAuth/external-account connection or a securely supplied API key.
- Vercel React guidance should wait for the Next.js baseline.
- No custom Codex role is justified yet; native role activation would require a separate `.codex/agents/*.toml` review.
- OpenSpec has no active change artifacts yet, so strict artifact validation has nothing to validate.
- Phase 2 remains blocked by the architecture/database gates and ADR decisions recorded in the implementation roadmap and database runtime report; this setup does not resolve them.

The next safe development step is to use `openspec-explore` or `openspec-propose` for a narrowly scoped, non-implementation proposal that closes the remaining Phase 1/Phase 2 prerequisites. Do not scaffold the backend or create EF Core migrations as part of this tooling task.
