[CmdletBinding()]
param(
    [string]$Image = 'pgvector/pgvector:0.8.7-pg16-bookworm',
    [int]$ReadyAttempts = 30
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

$runId = [Guid]::NewGuid().ToString('N')
$containerName = "studyai-cleanroom-$runId"
$databaseName = 'studyai_cleanroom'
$adminRole = 'studyai_bootstrap_admin'
$createdContainer = $false
$environmentFile = Join-Path ([System.IO.Path]::GetTempPath()) "$containerName.env"

$secretBytes = [byte[]]::new(32)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($secretBytes)
$adminSecret = [Convert]::ToBase64String($secretBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

try {
    @(
        "POSTGRES_DB=$databaseName"
        "POSTGRES_USER=$adminRole"
        "POSTGRES_PASSWORD=$adminSecret"
    ) | Set-Content -LiteralPath $environmentFile -Encoding Ascii

    $existingContainer = & docker ps -a --filter "name=^/$containerName$" --format '{{.Names}}'
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker availability check failed.'
    }
    if ($existingContainer) {
        throw "Refusing to reuse pre-existing container '$containerName'."
    }

    $containerId = & docker run --detach --rm `
        --name $containerName `
        --env-file $environmentFile `
        --tmpfs '/var/lib/postgresql/data:rw,noexec,nosuid,size=512m' `
        $Image
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($containerId)) {
        throw 'Disposable PostgreSQL container failed to start.'
    }
    $createdContainer = $true

    $ready = $false
    for ($attempt = 1; $attempt -le $ReadyAttempts; $attempt++) {
        & docker exec $containerName pg_isready -U $adminRole -d $databaseName *> $null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ready) {
        throw 'Disposable PostgreSQL did not become ready within the bounded retry window.'
    }

    $inspectJson = & docker inspect $containerName
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not inspect disposable PostgreSQL container.'
    }
    $inspection = @($inspectJson | ConvertFrom-Json)
    if (@($inspection).Length -ne 1) {
        throw 'Docker inspection returned an unexpected container count.'
    }
    $containerBinds = if ($inspection[0].HostConfig.PSObject.Properties['Binds']) {
        @($inspection[0].HostConfig.Binds | Where-Object { $null -ne $_ })
    } else {
        @()
    }
    $containerMounts = if ($inspection[0].PSObject.Properties['Mounts']) {
        @($inspection[0].Mounts | Where-Object { $null -ne $_ })
    } else {
        @()
    }
    $unexpectedMounts = @($containerMounts | Where-Object {
        $_.Type -ne 'tmpfs' -or $_.Destination -ne '/var/lib/postgresql/data'
    })
    if (@($containerBinds).Length -ne 0 -or @($unexpectedMounts).Length -ne 0) {
        $mountSummary = ($containerMounts | ForEach-Object { "$($_.Type):$($_.Destination)" }) -join ', '
        throw "Clean-room container unexpectedly inherited a bind or named volume. Observed mounts: $mountSummary"
    }
    $hasTmpfs = $inspection[0].HostConfig.PSObject.Properties['Tmpfs'] -and
        $inspection[0].HostConfig.Tmpfs.PSObject.Properties['/var/lib/postgresql/data']
    if (-not $hasTmpfs) {
        throw 'PostgreSQL data directory is not backed by the required disposable tmpfs.'
    }
    $unexpectedRoleCount = & docker exec $containerName psql -X -A -t -v ON_ERROR_STOP=1 `
        -U $adminRole -d $databaseName `
        -c "SELECT count(*) FROM pg_roles WHERE rolname IN ('studyai_app', 'studyai_migrator', 'studyai_worker', 'studyai_ro');"
    if ($LASTEXITCODE -ne 0 -or $unexpectedRoleCount.Trim() -ne '0') {
        throw 'Clean-room cluster contains an inherited StudyAI database role.'
    }

    $unexpectedSchemaCount = & docker exec $containerName psql -X -A -t -v ON_ERROR_STOP=1 `
        -U $adminRole -d $databaseName `
        -c "SELECT count(*) FROM pg_namespace WHERE nspname IN ('identity', 'ownership', 'workspace', 'knowledge', 'documents', 'ai', 'ops');"
    if ($LASTEXITCODE -ne 0 -or $unexpectedSchemaCount.Trim() -ne '0') {
        throw 'Clean-room database contains an inherited StudyAI schema.'
    }

    $publicCanCreate = & docker exec $containerName psql -X -A -t -v ON_ERROR_STOP=1 `
        -U $adminRole -d $databaseName `
        -c "SELECT has_schema_privilege('public', 'public', 'CREATE');"
    if ($LASTEXITCODE -ne 0 -or $publicCanCreate.Trim() -ne 'f') {
        throw 'PUBLIC unexpectedly has CREATE privilege on the public schema.'
    }

    Write-Output 'PASS: clean-room PostgreSQL used a unique container, ephemeral credential, tmpfs data directory, and no inherited StudyAI roles/schemas or permissive PUBLIC CREATE ACL.'
}
finally {
    if ($createdContainer) {
        & docker rm --force $containerName *> $null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Cleanup failed for disposable container '$containerName'."
        }
    }

    if (Test-Path -LiteralPath $environmentFile) {
        Remove-Item -LiteralPath $environmentFile -Force
    }

    [Array]::Clear($secretBytes, 0, $secretBytes.Length)
    $adminSecret = $null
}
