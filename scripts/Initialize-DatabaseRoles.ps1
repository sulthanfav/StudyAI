[CmdletBinding()]
param(
    [string]$PsqlPath = 'psql',
    [string]$DockerContainerName = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$requiredVariables = @(
    'STUDYAI_DB_ADMIN_HOST',
    'STUDYAI_DB_ADMIN_PORT',
    'STUDYAI_DB_ADMIN_DATABASE',
    'STUDYAI_DB_ADMIN_USER',
    'STUDYAI_DB_ADMIN_PASSWORD',
    'STUDYAI_DB_MIGRATOR_PASSWORD',
    'STUDYAI_DB_APP_PASSWORD',
    'STUDYAI_DB_WORKER_PASSWORD',
    'STUDYAI_DB_READONLY_PASSWORD'
)

$missingVariables = @($requiredVariables | Where-Object {
    [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_))
})
if ($missingVariables.Count -gt 0) {
    throw "Missing required database bootstrap configuration: $($missingVariables -join ', ')."
}

$bootstrapSql = (Resolve-Path (Join-Path $PSScriptRoot '..\infra\postgres\bootstrap-roles.sql')).Path
$psqlCommand = $null
if ([string]::IsNullOrWhiteSpace($DockerContainerName)) {
    $psqlCommand = Get-Command $PsqlPath -ErrorAction SilentlyContinue
    if ($null -eq $psqlCommand) {
        throw "PostgreSQL psql client '$PsqlPath' was not found on PATH."
    }
} elseif ($null -eq (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker CLI was not found on PATH.'
}

$libpqVariableNames = @('PGHOST', 'PGPORT', 'PGDATABASE', 'PGUSER', 'PGPASSWORD', 'PGAPPNAME')
$previousLibpqValues = @{}
foreach ($name in $libpqVariableNames) {
    $previousLibpqValues[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
}

try {
    if (-not [string]::IsNullOrWhiteSpace($DockerContainerName)) {
        Get-Content -Raw -LiteralPath $bootstrapSql | & docker exec -i `
            -e STUDYAI_DB_MIGRATOR_PASSWORD `
            -e STUDYAI_DB_APP_PASSWORD `
            -e STUDYAI_DB_WORKER_PASSWORD `
            -e STUDYAI_DB_READONLY_PASSWORD `
            $DockerContainerName `
            psql -X --quiet --set ON_ERROR_STOP=1 `
            -U $env:STUDYAI_DB_ADMIN_USER `
            -d $env:STUDYAI_DB_ADMIN_DATABASE
        if ($LASTEXITCODE -ne 0) {
            throw "Database role bootstrap failed with docker exec exit code $LASTEXITCODE."
        }

        Write-Output 'StudyAI database role bootstrap completed without emitting credential values.'
        return
    }

    $env:PGHOST = $env:STUDYAI_DB_ADMIN_HOST
    $env:PGPORT = $env:STUDYAI_DB_ADMIN_PORT
    $env:PGDATABASE = $env:STUDYAI_DB_ADMIN_DATABASE
    $env:PGUSER = $env:STUDYAI_DB_ADMIN_USER
    $env:PGPASSWORD = $env:STUDYAI_DB_ADMIN_PASSWORD
    $env:PGAPPNAME = 'studyai-role-bootstrap'

    & $psqlCommand.Source -X --quiet --set ON_ERROR_STOP=1 --file $bootstrapSql
    if ($LASTEXITCODE -ne 0) {
        throw "Database role bootstrap failed with psql exit code $LASTEXITCODE."
    }

    Write-Output 'StudyAI database role bootstrap completed without emitting credential values.'
}
finally {
    foreach ($name in $libpqVariableNames) {
        [Environment]::SetEnvironmentVariable($name, $previousLibpqValues[$name], 'Process')
    }
}
