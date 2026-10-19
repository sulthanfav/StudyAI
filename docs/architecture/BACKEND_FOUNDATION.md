# Backend Foundation — Iteration 1

Status: implemented foundation contract; not an authentication, persistence, tenant-security, or production-readiness claim.  
OpenSpec change: `backend-foundation-iteration-1`  
Toolchain decision: [ADR-015](../adr/ADR-015_BACKEND_FOUNDATION_TOOLCHAIN.md)

## Scope

This iteration supplies one ASP.NET Core API host, explicit modular-monolith project boundaries, validated foundation configuration, safe structured request metadata, liveness/readiness endpoints, Problem Details for failures, and automated unit/architecture/API tests.

It intentionally contains no authentication, Data Space authorization, EF Core, `DbContext`, migration, database connection, RLS policy, queue, storage provider, worker, AI/RAG, billing, academic module, or frontend implementation. The existing PostgreSQL clean-slate candidate is unchanged.

## Project responsibilities

| Project | Responsibility | Allowed production references |
|---|---|---|
| `StudyAI.Domain` | Future domain types and rules that do not depend on application or infrastructure concerns. | None |
| `StudyAI.Contracts` | Future public API and integration contracts independent of implementation layers. | None |
| `StudyAI.Application` | Future use cases and domain orchestration. | `StudyAI.Domain` |
| `StudyAI.Infrastructure` | Future implementations of accepted infrastructure contracts. | `StudyAI.Application`, `StudyAI.Domain` |
| `StudyAI.Api` | The single HTTP deployable and composition root. | All four production projects |
| `StudyAI.UnitTests` | Fast tests for isolated foundation behavior. | `StudyAI.Api` |
| `StudyAI.ArchitectureTests` | Exact project-reference graph enforcement. | No production project reference; it inspects project files |
| `StudyAI.Api.IntegrationTests` | In-process tests of startup and the observable HTTP host contract. | `StudyAI.Api` |

The API is the only composition root. Domain, Contracts, Application, and Infrastructure must never reference `StudyAI.Api`. A future feature module must expose an explicit registration contract instead of reaching into another module's internals.

## Configuration

ASP.NET Core applies its normal layered precedence: `appsettings.json`, optional environment-specific JSON, development secrets when explicitly configured, environment variables, then command-line values. Only non-secret foundation settings exist in this iteration:

| Key | Rule |
|---|---|
| `Foundation:ServiceName` | Required; ASCII letters, digits, dots, and hyphens; maximum 64 characters. |
| `Foundation:MaxCorrelationIdLength` | Integer from 16 through 128. |

Options are validated during host startup. Diagnostics name invalid keys and rules but never echo supplied values. Do not add placeholder provider credentials or connection strings before their owning ADR/change is accepted. Real secrets remain outside Git.

## Logging and correlation

The default console provider emits structured JSON with UTC timestamps. Framework scope serialization is disabled so raw request paths from framework scopes do not bypass the route-template policy. The middleware still creates an internal correlation scope for downstream logs, while its request-completion event records only:

- HTTP method;
- controlled route template, or `(unmatched)`;
- response status;
- elapsed milliseconds;
- bounded correlation identifier.

`X-Correlation-ID` accepts only ASCII letters, digits, dots, underscores, and hyphens within the configured limit. Missing or invalid values are replaced and the same value is returned in the response and attached to the request log scope.

Default logs must not record query strings, authorization headers, cookies, request or response bodies, credentials, connection strings, tokens, signed URLs, prompts, notes, or document text. New telemetry fields require a privacy/security review and a test proving private values remain absent.

## HTTP surface

| Route | Meaning |
|---|---|
| `GET /health/live` | Process-only liveness. It intentionally runs no registered external dependency check. |
| `GET /health/ready` | Foundation readiness. It runs checks tagged `ready` and returns only service name, aggregate status, and safe component names/statuses. |
| `GET /openapi/{documentName}.json` | Framework-generated metadata in the Development environment only. |

Unmatched routes return RFC 7807 Problem Details. Health responses omit descriptions, exceptions, configuration values, connection strings, hostnames, and private content. No database readiness signal exists until the gated persistence change introduces and verifies one.

## Local commands

Prerequisite: a .NET 10 SDK compatible with `global.json`.

```powershell
dotnet restore StudyAI.slnx --locked-mode
dotnet build StudyAI.slnx --no-restore
dotnet test --solution StudyAI.slnx --no-restore
dotnet run --project apps/api/StudyAI.Api/StudyAI.Api.csproj --no-restore
```

The native Microsoft Testing Platform command uses `--solution`; do not add the VSTest bridge separator. The development launch profile listens on the URLs recorded in the API project.

## Remaining gates

- ADR-001 authentication/session design before identity behavior.
- ADR-003 runtime roles/RLS/pooling design plus adversarial PostgreSQL evidence before tenant-sensitive persistence.
- EF Core/Npgsql/pgvector pinning, schema ownership, incremental migrations, populated upgrade/rollback, and real PostgreSQL parity tests before persistence implementation.
- Accepted queue, storage, AI, billing, quota, and production-operability decisions before their respective capabilities.

Passing this host suite proves only the foundation contract described above. It does not establish tenant isolation, database readiness, security readiness, performance capacity, or production readiness.

## Verification evidence

Fresh verification on 2026-10-19 produced:

| Verification | Evidence |
|---|---|
| Clean | `dotnet clean StudyAI.slnx` exited `0`. |
| Locked restore | `dotnet restore StudyAI.slnx --locked-mode` exited `0`. |
| Build | `dotnet build StudyAI.slnx --no-restore` exited `0` with 0 warnings and 0 errors. |
| Automated tests | `dotnet test --solution StudyAI.slnx --no-restore` exited `0`: 28 passed, 0 failed, 0 skipped. |
| Live host | Kestrel returned HTTP 200 from `/health/live` and `/health/ready`; the caller correlation identifier was echoed. |
| Development metadata | OpenAPI `3.1.1` listed `/health/live` and `/health/ready`. |
| Invalid startup | A malformed `Foundation:ServiceName` produced a non-zero process exit and identified the key without echoing the canary value. |
| Database parity guard | The reviewed schema SHA-256 remained `0545E8A38069D8DCF739070FD65B4C6C129AC5B390724506FE17D7177538F7DE`. |

The suite contains 11 unit cases, 5 architecture cases, and 12 API integration cases. No Docker, database, persistent volume, production/shared infrastructure, or remote service was changed by this implementation.

### OpenSpec scenario reconciliation

| `platform/api-host` scenario | Evidence |
|---|---|
| Valid foundation configuration | In-process host and live Kestrel startup succeed using only foundation settings. |
| Invalid explicit configuration | Unit/startup tests and a process run prove non-zero startup failure with key-only diagnostics. |
| Process is alive | `/health/live` API test and live HTTP 200; component list is empty. |
| Foundation is ready | `/health/ready` API test and live HTTP 200 with only the `foundation` component. |
| Required readiness check fails | Injected unhealthy tagged check returns HTTP 503 and omits its private description. |
| Valid caller correlation identifier | Unit/API tests preserve, echo, scope, and structurally log the same identifier. |
| Missing or invalid correlation identifier | Unit/API cases cover missing, blank, space, control-character, and oversized inputs. |
| Sensitive request material | Canary authorization, signed URL, credential, prompt, note, and document values remain absent from captured logs. |
| Secret-setting validation | Startup diagnostics contain the setting key/rule and not the supplied canary value. |
| Foundation dependency rules | Five architecture cases enforce the exact production reference graph. |
| Future module composition | `AddStudyAIFoundation` is an explicit composition contract; no reflection discovery or cross-module internal access exists. |
| Foundation-only surface | API allowlist test permits only liveness, readiness, and Development OpenAPI metadata. |
| Foundation verification | Locked restore, build, 28-test solution run, live HTTP checks, and documented commands passed. |
| Later database behavior | No database or fake relational package/code exists; later database/security tests remain required to use real PostgreSQL. |
