# Database Security Bootstrap and Credential Rotation Runbook

Status: supported administrator workflow for clean PostgreSQL clusters and StudyAI database-role credential rotation. The commands intentionally do not contain credential values.

## Safety boundary

- Run this workflow only against a new/disposable cluster or an explicitly approved StudyAI database.
- Load every password from the approved secret manager into process-scoped environment variables. Never put a secret in source, shell history, a command argument, a checked-in `.env` file, or an artifact log.
- Role bootstrap must run **before** EF migrations because historical migrations grant privileges to the pre-existing `studyai_app` role; they do not create roles or embed credentials.
- Do not add role passwords to migrations. The supported workflow pre-creates or rotates roles from process-scoped administrator configuration before applying the migration chain.
- Apply the runtime privilege allowlist after every migration deployment. Migration execution is administrative; the application runtime role must never own schema objects or mutate EF migration history.

## Required administrator configuration

Populate these process environment variables from the secret manager:

```text
STUDYAI_DB_ADMIN_HOST
STUDYAI_DB_ADMIN_PORT
STUDYAI_DB_ADMIN_DATABASE
STUDYAI_DB_ADMIN_USER
STUDYAI_DB_ADMIN_PASSWORD
STUDYAI_DB_MIGRATOR_PASSWORD
STUDYAI_DB_APP_PASSWORD
STUDYAI_DB_WORKER_PASSWORD
STUDYAI_DB_READONLY_PASSWORD
```

The scripts validate presence but never print the values. Native execution requires `psql` on `PATH`.

## Clean-room deployment order

From the repository root:

```powershell
pwsh -NoProfile -File scripts/Initialize-DatabaseRoles.ps1

$env:STUDYAI_MIGRATION_CONNECTION_STRING = '<administrator connection string from the secret manager>'
dotnet ef database update `
  --project src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj `
  --startup-project apps/api/StudyAI.Api/StudyAI.Api.csproj `
  --no-build
Remove-Item Env:STUDYAI_MIGRATION_CONNECTION_STRING

pwsh -NoProfile -File scripts/Set-DatabaseRuntimePrivileges.ps1
```

For the repository's disposable Docker validation container, both administrator scripts also accept `-DockerContainerName <exact-container-name>`. This mode is for explicit local validation and still reads role secrets from the same environment variables.

Validate the no-volume clean-room assumption independently:

```powershell
pwsh -NoProfile -File scripts/Test-CleanRoomPostgresBootstrap.ps1
```

The test creates a uniquely named container with a tmpfs data directory, checks default ACL and absence of StudyAI roles/schemas, and removes only the container and temporary env-file created by that run.

## Runtime privilege verification

After `Set-DatabaseRuntimePrivileges.ps1`:

- `studyai_app` may connect, use approved application schemas, and perform `SELECT`, `INSERT`, `UPDATE`, and `DELETE` only on the reviewed table allowlist.
- It has no table ownership, grant option, elevated role membership, database TEMP privilege, public-schema CREATE, application-schema CREATE, or access to unapproved tables.
- It cannot read or mutate `ops.__ef_migrations_history`, create/alter schema objects, or grant its table privileges to another role.

Run `ApplicationRoleHasOnlyReviewedRuntimePrivileges` plus the full real-PostgreSQL integration suite after a privilege change.

## Credential rotation

Rotation is an administrator-coordinated maintenance action:

1. Create new random values in the approved secret manager for the affected StudyAI roles.
2. Load the new values into the process environment without echoing them.
3. Run `Initialize-DatabaseRoles.ps1`. It alters existing roles as well as creating missing roles, so previous passwords become invalid immediately.
4. Update the API/worker deployment secret references and restart or roll the affected processes so new connections use the rotated credentials.
5. Re-run `Set-DatabaseRuntimePrivileges.ps1`; rotation must not widen privileges.
6. Verify a new connection succeeds, an old credential fails, and application health/integration tests pass.
7. Remove process-scoped secret variables and retire the old secret versions according to the approved retention policy.

The current script performs immediate replacement, not dual-password overlap. Schedule a maintenance window or a coordinated rollout; a zero-downtime dual-role rotation requires a separate reviewed change.

## Logging and incident response

- Script output may contain role names and success/failure status, but never credential values or full connection strings.
- Treat any accidental credential output as an incident: stop deployment, rotate the exposed role, remove unsafe artifacts through the approved retention process, and document the event without copying the secret.
- A missing variable, absent `psql`, failed migration, or failed privilege probe is a hard stop. Do not bypass bootstrap by relying on the historical migration literal.
