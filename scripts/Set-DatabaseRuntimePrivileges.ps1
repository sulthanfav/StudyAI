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
    'STUDYAI_DB_ADMIN_PASSWORD'
)

$missingVariables = @($requiredVariables | Where-Object {
    [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_))
})
if ($missingVariables.Count -gt 0) {
    throw "Missing required database privilege configuration: $($missingVariables -join ', ')."
}

$privilegeSql = (Resolve-Path (Join-Path $PSScriptRoot '..\infra\postgres\apply-runtime-privileges.sql')).Path
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
        Get-Content -Raw -LiteralPath $privilegeSql | & docker exec -i `
            $DockerContainerName `
            psql -X --quiet --set ON_ERROR_STOP=1 `
            -U $env:STUDYAI_DB_ADMIN_USER `
            -d $env:STUDYAI_DB_ADMIN_DATABASE
        if ($LASTEXITCODE -ne 0) {
            throw "Database runtime privilege application failed with docker exec exit code $LASTEXITCODE."
        }

        Write-Output 'StudyAI runtime privileges applied without emitting credential values.'
        return
    }

    $env:PGHOST = $env:STUDYAI_DB_ADMIN_HOST
    $env:PGPORT = $env:STUDYAI_DB_ADMIN_PORT
    $env:PGDATABASE = $env:STUDYAI_DB_ADMIN_DATABASE
    $env:PGUSER = $env:STUDYAI_DB_ADMIN_USER
    $env:PGPASSWORD = $env:STUDYAI_DB_ADMIN_PASSWORD
    $env:PGAPPNAME = 'studyai-runtime-privileges'

    & $psqlCommand.Source -X --quiet --set ON_ERROR_STOP=1 --file $privilegeSql
    if ($LASTEXITCODE -ne 0) {
        throw "Database runtime privilege application failed with psql exit code $LASTEXITCODE."
    }

    Write-Output 'StudyAI runtime privileges applied without emitting credential values.'
}
finally {
    foreach ($name in $libpqVariableNames) {
        [Environment]::SetEnvironmentVariable($name, $previousLibpqValues[$name], 'Process')
    }
}
