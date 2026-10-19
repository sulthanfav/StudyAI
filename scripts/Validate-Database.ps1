[CmdletBinding()]
param(
    [string]$ProjectName = 'studyai-db-validation',
    [string]$DatabaseName = '',
    [ValidateRange(1024, 65535)]
    [int]$HostPort = 55432,
    [ValidateRange(10, 300)]
    [int]$WaitSeconds = 90,
    [switch]$SkipBackupRestore,
    [switch]$StopContainerAfter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$composeFile = Join-Path $repoRoot 'infra\compose\compose.database.yml'
$environmentFile = Join-Path $repoRoot 'infra\compose\.env.database.local'
$schemaFileInContainer = '/workspace/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql'
$smokeFileInContainer = '/workspace/database/StudyAI_DB_Smoke_Tests_v1.1.sql'
$verificationFileInContainer = '/workspace/validation/verify-runtime.sql'
$image = 'pgvector/pgvector:0.8.7-pg16-bookworm'
$adminUser = 'studyai_admin'
$script:redactionValues = @()

if ([string]::IsNullOrWhiteSpace($DatabaseName)) {
    $DatabaseName = 'studyai_validation_{0}' -f (Get-Date -Format 'yyyyMMdd_HHmmss')
}

if ($DatabaseName -notmatch '^[a-z][a-z0-9_]{0,62}$') {
    throw "DatabaseName must match ^[a-z][a-z0-9_]{0,62}$."
}

$restoreDatabaseName = '{0}_restore' -f $DatabaseName
if ($restoreDatabaseName.Length -gt 63) {
    throw 'DatabaseName is too long to create the restore-verification database.'
}

$runStamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$artifactDirectory = Join-Path $repoRoot "artifacts\database-validation\$runStamp"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null

function Write-Section {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host ''
    Write-Host "=== $Message ===" -ForegroundColor Cyan
}

function Invoke-LoggedNative {
    param(
        [Parameter(Mandatory)][string]$Command,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$LogPath,
        [switch]$AllowFailure
    )

    $display = "$Command $($Arguments -join ' ')"
    $output = & $Command @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    $outputLines = @($output | ForEach-Object { $_.ToString() })

    foreach ($secret in $script:redactionValues) {
        if (-not [string]::IsNullOrEmpty($secret)) {
            $display = $display.Replace($secret, '<redacted>')
            $outputLines = @($outputLines | ForEach-Object { $_.Replace($secret, '<redacted>') })
        }
    }

    @(
        "COMMAND: $display"
        "EXIT_CODE: $exitCode"
        ''
        $outputLines
    ) | Set-Content -LiteralPath $LogPath -Encoding utf8NoBOM

    $outputLines | ForEach-Object { Write-Host $_ }

    if (-not $AllowFailure -and $exitCode -ne 0) {
        throw "Command failed with exit code $exitCode. See $LogPath"
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = $outputLines
    }
}

function Get-ComposeArguments {
    param([string[]]$Arguments)
    return @(
        'compose',
        '--env-file', $environmentFile,
        '-f', $composeFile,
        '-p', $ProjectName
    ) + $Arguments
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker CLI was not found on PATH.'
}

Write-Section 'Docker connectivity'
$dockerVersion = Invoke-LoggedNative -Command 'docker' -Arguments @('version') -LogPath (Join-Path $artifactDirectory '01-docker-version.log')

Write-Section 'Local validation credentials'
if (-not (Test-Path -LiteralPath $environmentFile)) {
    $randomBytes = [byte[]]::new(32)
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($randomBytes)
    $password = [Convert]::ToHexString($randomBytes)

    @(
        "STUDYAI_POSTGRES_ADMIN_USER=$adminUser"
        "STUDYAI_POSTGRES_ADMIN_PASSWORD=$password"
        "STUDYAI_POSTGRES_PORT=$HostPort"
    ) | Set-Content -LiteralPath $environmentFile -Encoding utf8NoBOM

    Write-Host "Created ignored local environment file: $environmentFile"
} else {
    Write-Host "Reusing ignored local environment file: $environmentFile"
}

$envData = ConvertFrom-StringData (Get-Content -Raw -LiteralPath $environmentFile)
if (-not $envData.ContainsKey('STUDYAI_POSTGRES_ADMIN_USER') -or
    -not $envData.ContainsKey('STUDYAI_POSTGRES_ADMIN_PASSWORD')) {
    throw "Required database variables are missing from $environmentFile"
}
$adminUser = $envData['STUDYAI_POSTGRES_ADMIN_USER']
$script:redactionValues = @($envData['STUDYAI_POSTGRES_ADMIN_PASSWORD'])

Write-Section 'Compose configuration'
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @('config')) -LogPath (Join-Path $artifactDirectory '02-compose-config.log') | Out-Null

Write-Section 'Start PostgreSQL validation service'
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @('up', '-d')) -LogPath (Join-Path $artifactDirectory '03-compose-up.log') | Out-Null

$deadline = (Get-Date).AddSeconds($WaitSeconds)
$ready = $false
do {
    $healthArguments = Get-ComposeArguments @(
        'exec', '-T', 'postgres',
        'pg_isready', '-U', $adminUser, '-d', 'postgres'
    )
    & docker @healthArguments *> $null
    if ($LASTEXITCODE -eq 0) {
        $ready = $true
        break
    }
    Start-Sleep -Seconds 2
} while ((Get-Date) -lt $deadline)

if (-not $ready) {
    $statusArguments = Get-ComposeArguments @('ps')
    $logArguments = Get-ComposeArguments @('logs', '--no-color', 'postgres')
    & docker @statusArguments
    & docker @logArguments
    throw "PostgreSQL did not become ready within $WaitSeconds seconds."
}
Write-Host 'PostgreSQL is accepting connections.'

Write-Section "Create isolated database $DatabaseName"
$createDatabaseSql = 'CREATE DATABASE "{0}";' -f $DatabaseName
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
    'exec', '-T', 'postgres',
    'psql', '-X', '-v', 'ON_ERROR_STOP=1',
    '-U', $adminUser, '-d', 'postgres',
    '-c', $createDatabaseSql
)) -LogPath (Join-Path $artifactDirectory '04-create-database.log') | Out-Null

Write-Section 'Apply reviewed schema v1.1'
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
    'exec', '-T', 'postgres',
    'psql', '-X', '-v', 'ON_ERROR_STOP=1',
    '-U', $adminUser, '-d', $DatabaseName,
    '-f', $schemaFileInContainer
)) -LogPath (Join-Path $artifactDirectory '05-schema-apply.log') | Out-Null

Write-Section 'Verify runtime objects'
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
    'exec', '-T', 'postgres',
    'psql', '-X', '-v', 'ON_ERROR_STOP=1',
    '-U', $adminUser, '-d', $DatabaseName,
    '-f', $verificationFileInContainer
)) -LogPath (Join-Path $artifactDirectory '06-runtime-inventory.log') | Out-Null

Write-Section 'Run database smoke tests'
$smokeLog = Join-Path $artifactDirectory '07-smoke-tests.log'
Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
    'exec', '-T', 'postgres',
    'psql', '-X', '-v', 'ON_ERROR_STOP=1',
    '-U', $adminUser, '-d', $DatabaseName,
    '-f', $smokeFileInContainer
)) -LogPath $smokeLog | Out-Null

$smokeText = Get-Content -Raw -LiteralPath $smokeLog
$passMatches = [regex]::Matches(
    $smokeText,
    'PASS (?:01|02|03A|03B|03|04|05|06|07|08):'
)
$failMatches = [regex]::Matches($smokeText, 'FAIL [^\r\n]*')
if ($passMatches.Count -ne 10 -or $failMatches.Count -ne 0) {
    throw "Expected 10 PASS notices and zero FAIL notices; found $($passMatches.Count) PASS and $($failMatches.Count) FAIL."
}
Write-Host 'Smoke result: 10 PASS notices, 0 FAIL notices.'

Write-Section 'Verify smoke fixture rollback'
$rollbackLog = Join-Path $artifactDirectory '08-smoke-rollback-check.log'
$rollbackResult = Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
    'exec', '-T', 'postgres',
    'psql', '-X', '-At', '-v', 'ON_ERROR_STOP=1',
    '-U', $adminUser, '-d', $DatabaseName,
    '-c', 'SELECT count(*) FROM identity.users;'
)) -LogPath $rollbackLog
$rollbackRows = @($rollbackResult.Output | Where-Object { $_ -match '^\d+$' })
if ($rollbackRows.Count -ne 1 -or $rollbackRows[0] -ne '0') {
    throw 'Smoke-test transaction did not roll back to an empty identity.users table.'
}
Write-Host 'Smoke fixture rollback verified.'

$restoreVerified = $false
if (-not $SkipBackupRestore) {
    Write-Section 'Dump and restore validation'
    $containerIdResult = Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
        'ps', '-q', 'postgres'
    )) -LogPath (Join-Path $artifactDirectory '09-container-id.log')
    $containerId = ($containerIdResult.Output | Select-Object -Last 1).Trim()
    if ([string]::IsNullOrWhiteSpace($containerId)) {
        throw 'Could not resolve the PostgreSQL container ID.'
    }

    $dumpName = "$DatabaseName.dump"
    $dumpPathInContainer = "/tmp/$dumpName"
    $dumpPathOnHost = Join-Path $artifactDirectory $dumpName

    Invoke-LoggedNative -Command 'docker' -Arguments @(
        'exec', $containerId,
        'pg_dump', '-U', $adminUser, '-d', $DatabaseName,
        '--format=custom', '--file', $dumpPathInContainer
    ) -LogPath (Join-Path $artifactDirectory '10-pg-dump.log') | Out-Null

    Invoke-LoggedNative -Command 'docker' -Arguments @(
        'cp', "${containerId}:$dumpPathInContainer", $dumpPathOnHost
    ) -LogPath (Join-Path $artifactDirectory '11-copy-dump.log') | Out-Null

    $createRestoreSql = 'CREATE DATABASE "{0}";' -f $restoreDatabaseName
    Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
        'exec', '-T', 'postgres',
        'psql', '-X', '-v', 'ON_ERROR_STOP=1',
        '-U', $adminUser, '-d', 'postgres',
        '-c', $createRestoreSql
    )) -LogPath (Join-Path $artifactDirectory '12-create-restore-database.log') | Out-Null

    Invoke-LoggedNative -Command 'docker' -Arguments @(
        'exec', $containerId,
        'pg_restore', '--exit-on-error',
        '-U', $adminUser, '-d', $restoreDatabaseName,
        $dumpPathInContainer
    ) -LogPath (Join-Path $artifactDirectory '13-pg-restore.log') | Out-Null

    $restoreCountSql = @"
SELECT count(*)
FROM pg_tables
WHERE schemaname IN (
  'identity','ownership','workspace','knowledge','documents','research','ai',
  'learning','writing','presentation','integrity','planner','billing','ops'
);
"@
    $restoreResult = Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @(
        'exec', '-T', 'postgres',
        'psql', '-X', '-At', '-v', 'ON_ERROR_STOP=1',
        '-U', $adminUser, '-d', $restoreDatabaseName,
        '-c', $restoreCountSql
    )) -LogPath (Join-Path $artifactDirectory '14-restore-table-count.log')
    $restoreRows = @($restoreResult.Output | Where-Object { $_ -match '^\d+$' })
    if ($restoreRows.Count -ne 1 -or $restoreRows[0] -ne '68') {
        throw 'Restored database did not contain the expected 68 tables.'
    }
    $restoreVerified = $true
    Write-Host "Restore verified in database $restoreDatabaseName."
}

$imageDigest = (& docker image inspect $image --format '{{index .RepoDigests 0}}' 2>$null | Select-Object -First 1)
$summary = [ordered]@{
    validated_at = (Get-Date).ToString('o')
    compose_project = $ProjectName
    image = $image
    image_digest = $imageDigest
    database = $DatabaseName
    restore_database = if ($restoreVerified) { $restoreDatabaseName } else { $null }
    persistent_volume = 'studyai-postgres-validation-data'
    schema_applied = $true
    runtime_inventory_passed = $true
    smoke_pass_count = 10
    smoke_fail_count = 0
    smoke_rollback_verified = $true
    backup_restore_verified = $restoreVerified
    artifacts = $artifactDirectory
}
$summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifactDirectory 'validation-summary.json') -Encoding utf8NoBOM

if ($StopContainerAfter) {
    Write-Section 'Stop validation container (volume retained)'
    Invoke-LoggedNative -Command 'docker' -Arguments (Get-ComposeArguments @('stop')) -LogPath (Join-Path $artifactDirectory '15-compose-stop.log') | Out-Null
}

Write-Section 'Validation completed'
Write-Host "Database: $DatabaseName"
if ($restoreVerified) {
    Write-Host "Restore database: $restoreDatabaseName"
}
Write-Host 'Persistent volume retained: studyai-postgres-validation-data'
Write-Host "Evidence: $artifactDirectory"
Write-Host 'The schema is runtime-tested as a clean-slate candidate; this does not make it production-ready.'
