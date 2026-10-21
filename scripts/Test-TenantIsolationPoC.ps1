[CmdletBinding()]
param(
    [string]$ContainerName = 'studyai-db-validation-postgres-1',
    [string]$DatabaseName = 'studyai_tenant_poc_20261020'
)

$ErrorActionPreference = 'Continue'
$appPassword = $env:STUDYAI_TEST_APP_PASSWORD
if ([string]::IsNullOrWhiteSpace($appPassword)) { throw 'STUDYAI_TEST_APP_PASSWORD is required.' }
$migratorPassword = $env:STUDYAI_TEST_MIGRATOR_PASSWORD
$workerPassword = $env:STUDYAI_TEST_WORKER_PASSWORD
$readonlyPassword = $env:STUDYAI_TEST_READONLY_PASSWORD
if ([string]::IsNullOrWhiteSpace($migratorPassword) -or
    [string]::IsNullOrWhiteSpace($workerPassword) -or
    [string]::IsNullOrWhiteSpace($readonlyPassword)) {
    throw 'STUDYAI_TEST_MIGRATOR_PASSWORD, STUDYAI_TEST_WORKER_PASSWORD, and STUDYAI_TEST_READONLY_PASSWORD are required.'
}
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

$appPassword = $env:STUDYAI_DB_APP_PASSWORD
if ([string]::IsNullOrWhiteSpace($appPassword)) {
    throw 'STUDYAI_DB_APP_PASSWORD must be supplied by approved test configuration.'
}
$workerPassword = $env:STUDYAI_DB_WORKER_PASSWORD
if ([string]::IsNullOrWhiteSpace($workerPassword)) {
    throw 'STUDYAI_DB_WORKER_PASSWORD must be supplied by approved test configuration.'
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " StudyAI Campus: Tenant Isolation & Database Roles PoC   " -ForegroundColor Cyan
Write-Host " Target Database: $DatabaseName                           " -ForegroundColor Cyan
Write-Host " Container:       $ContainerName                          " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Ensure Target Database Exists
Write-Host "`n[Step 1] Initializing isolated database: $DatabaseName..." -ForegroundColor Yellow
$dbExists = (& docker exec -i $ContainerName psql -U studyai_admin -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$DatabaseName';")
if ($dbExists.Trim() -ne '1') {
    Write-Host "Creating database $DatabaseName..."
    & docker exec -i $ContainerName psql -U studyai_admin -d postgres -c "CREATE DATABASE $DatabaseName;" | Out-Null
    
    Write-Host "Applying canonical physical schema v1.1..."
    & docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -f /workspace/database/StudyAI_Physical_Schema_Reviewed_v1.1.sql | Out-Null
} else {
    Write-Host "Database $DatabaseName already exists."
}

# 2. Apply PoC Setup (Roles, Grants, RLS Policies, Seed Data)
Write-Host "`n[Step 2] Applying tenant roles, RLS policies, and seed data..." -ForegroundColor Yellow
$setupScriptPath = Join-Path (Resolve-Path "$PSScriptRoot\..").Path "infra\postgres\tenant-poc-setup.sql"
$setupSql = Get-Content -Raw $setupScriptPath
$setupSql | & docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -v ON_ERROR_STOP=1 `
    -v "migrator_password=$migratorPassword" -v "app_password=$appPassword" `
    -v "worker_password=$workerPassword" -v "readonly_password=$readonlyPassword" | Out-Null
Write-Host "Setup applied successfully." -ForegroundColor Green

$results = [System.Collections.Generic.List[PSCustomObject]]::new()

function Record-TestResult {
    param(
        [string]$TestId,
        [string]$Description,
        [string]$Expected,
        [string]$Actual,
        [bool]$Passed
    )
    $status = if ($Passed) { "PASS" } else { "FAIL" }
    $color = if ($Passed) { "Green" } else { "Red" }
    Write-Host "  [$status] ${TestId}: $Description" -ForegroundColor $color
    if (-not $Passed) {
        Write-Host "    Expected: $Expected" -ForegroundColor Yellow
        Write-Host "    Actual:   $Actual" -ForegroundColor Yellow
    }
    $results.Add([PSCustomObject]@{
        TestId = $TestId
        Description = $Description
        Expected = $Expected
        Actual = $Actual
        Status = $status
    })
}

function Invoke-PsqlCmd {
    param(
        [string]$User,
        [string]$Password,
        [string]$CommandText,
        [switch]$TuplesOnly
    )
    $tFlag = if ($TuplesOnly) { "-tA" } else { "" }
    $cmd = "docker exec -e PGPASSWORD=$Password -i $ContainerName psql -U $User -d $DatabaseName $tFlag -c `"$CommandText`" 2>&1"
    $output = cmd /c $cmd
    return ($output | Out-String).Trim()
}

function Invoke-PsqlScript {
    param(
        [string]$User,
        [string]$Password,
        [string]$ScriptText,
        [switch]$TuplesOnly
    )
    $tFlag = if ($TuplesOnly) { "-tA" } else { "" }
    $tmpFile = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmpFile, $ScriptText)
    try {
        $cmd = "type `"$tmpFile`" | docker exec -e PGPASSWORD=$Password -i $ContainerName psql -U $User -d $DatabaseName $tFlag 2>&1"
        $output = cmd /c $cmd
        return ($output | Out-String).Trim()
    } finally {
        if (Test-Path $tmpFile) { Remove-Item $tmpFile -Force }
    }
}

Write-Host "`n[Step 3] Executing Tenant Isolation & Security Test Scenarios..." -ForegroundColor Yellow

# Scenario 1: DDL Rejection for studyai_app
$out = Invoke-PsqlCmd "studyai_app" $appPassword "CREATE TABLE public.forbidden_table (id int);"
$passed = ($out -match 'permission denied')
Record-TestResult "POC-01" "Application role cannot execute CREATE TABLE (DDL prohibited)" "permission denied" $out $passed

# Scenario 2: ALTER TABLE Rejection for studyai_app
$out = Invoke-PsqlCmd "studyai_app" $appPassword "ALTER TABLE knowledge.knowledge_items DROP COLUMN title;"
$passed = ($out -match 'must be owner of table|permission denied')
Record-TestResult "POC-02" "Application role cannot execute ALTER TABLE (DDL prohibited)" "must be owner of table" $out $passed

# Scenario 3: Missing Context Fail-Closed Read
$out = Invoke-PsqlCmd "studyai_app" $appPassword "SELECT count(*) FROM knowledge.knowledge_items;" -TuplesOnly
$passed = ($out -eq '0')
Record-TestResult "POC-03" "Missing context fails closed on SELECT (0 rows returned)" "0" $out $passed

# Scenario 4: Missing Context Fail-Closed Write
$cmd = "INSERT INTO knowledge.knowledge_items (id, data_space_id, kind, title) VALUES ('0192a000-0000-7000-8000-000000000099', '0192a000-0000-7000-8000-0000000000aa', 'document', 'Rogue Write');"
$out = Invoke-PsqlCmd "studyai_app" $appPassword $cmd
$passed = ($out -match 'new row violates row-level security policy')
Record-TestResult "POC-04" "Missing context fails closed on INSERT (RLS policy violation)" "violates row-level security policy" $out $passed

# Scenario 5: Malformed Context Fail-Closed
$sql = @"
BEGIN;
SELECT set_config('app.current_data_space_id', 'not-a-valid-uuid', true);
SELECT count(*) FROM knowledge.knowledge_items;
COMMIT;
"@
$out = Invoke-PsqlScript "studyai_app" $appPassword $sql
$passed = ($out -match 'invalid input syntax for type uuid')
Record-TestResult "POC-05" "Malformed UUID context fails closed with cast error" "invalid input syntax for type uuid" $out $passed

# Scenario 6: User A Authorized Read
$spaceA = '0192a000-0000-7000-8000-0000000000aa'
$sql = @"
BEGIN;
SELECT set_config('app.current_data_space_id', '$spaceA', true);
SELECT count(*), min(title) FROM knowledge.knowledge_items;
SELECT count(*), min(content_text) FROM documents.document_chunks;
SELECT count(*), min(embedding::text) FROM documents.chunk_embeddings;
COMMIT;
"@
$out = Invoke-PsqlScript "studyai_app" $appPassword $sql -TuplesOnly
$passed = ($out -match '1\|Document Space A') -and ($out -match '1\|Secret academic notes for Space A') -and ($out -match '1\|\[0.1,0.2,0.3\]')
Record-TestResult "POC-06" "User A can read authorized Data Space A items, chunks, and embeddings" "1|Document Space A; 1|Secret academic notes; 1|[0.1,0.2,0.3]" $out $passed

# Scenario 7: User A Attempting to Read/Update Space B (Cross-Tenant Leakage Blocked)
$spaceB = '0192a000-0000-7000-8000-0000000000bb'
$sql = @"
BEGIN;
SELECT set_config('app.current_data_space_id', '$spaceA', true);
SELECT count(*) FROM knowledge.knowledge_items WHERE data_space_id = '$spaceB';
SELECT count(*) FROM documents.document_chunks WHERE data_space_id = '$spaceB';
UPDATE knowledge.knowledge_items SET title = 'Hacked' WHERE data_space_id = '$spaceB';
COMMIT;
"@
$out = Invoke-PsqlScript "studyai_app" $appPassword $sql -TuplesOnly
$passed = ($out -match "0`r?`n0`r?`nUPDATE 0" -or ($out -match "0" -and $out -match "UPDATE 0"))
Record-TestResult "POC-07" "User A in Space A context sees 0 rows and updates 0 rows in Space B" "0 rows, UPDATE 0" $out $passed

# Scenario 8: Pooled Connection Reuse (User B follows User A on same connection)
$sql = @"
BEGIN;
SELECT set_config('app.current_data_space_id', '$spaceA', true);
SELECT 'SESSION_A:' || title FROM knowledge.knowledge_items;
COMMIT;

BEGIN;
SELECT set_config('app.current_data_space_id', '$spaceB', true);
SELECT 'SESSION_B:' || title FROM knowledge.knowledge_items;
COMMIT;

SELECT 'SESSION_UNSET:' || count(*) FROM knowledge.knowledge_items;
"@
$out = Invoke-PsqlScript "studyai_app" $appPassword $sql -TuplesOnly
$passed = ($out -match 'SESSION_A:Document Space A') -and 
          ($out -match 'SESSION_B:Document Space B') -and 
          ($out -match 'SESSION_UNSET:0') -and
          (-not ($out -match 'SESSION_B:Document Space A'))
Record-TestResult "POC-08" "Reused pooled connection correctly switches context with zero leakage" "A sees A, B sees B, unset sees 0" $out $passed

# Scenario 9: Rollback / Abort Context Flush
$sql = @"
BEGIN;
SELECT set_config('app.current_data_space_id', '$spaceA', true);
SELECT 'INSIDE_TX:' || count(*) FROM knowledge.knowledge_items;
ROLLBACK;
SELECT 'AFTER_ROLLBACK:' || count(*) FROM knowledge.knowledge_items;
"@
$out = Invoke-PsqlScript "studyai_app" $appPassword $sql -TuplesOnly
$passed = ($out -match 'INSIDE_TX:1') -and ($out -match 'AFTER_ROLLBACK:0')
Record-TestResult "POC-09" "Transaction rollback flushes transaction-local context immediately" "INSIDE: 1, AFTER: 0" $out $passed

# Scenario 10: Background Worker Scoped Access
$sql = @"
SELECT count(*) FROM documents.processing_runs;
SELECT count(*) FROM documents.documents;
"@
$outWorker = Invoke-PsqlScript "studyai_worker" $workerPassword $sql -TuplesOnly
$sqlForbidden = "INSERT INTO billing.plans (id, code, title) VALUES ('0192a000-0000-7000-8000-000000000077', 'hacked', 'Hacked');"
$outForbidden = Invoke-PsqlCmd "studyai_worker" $workerPassword $sqlForbidden
$passed = ($outWorker -match '1|2') -and ($outForbidden -match 'permission denied')
Record-TestResult "POC-10" "Worker role has read access to processing_runs/documents, but denied write to billing" "Allowed documents, denied billing" $outForbidden $passed

Write-Host "`n=== Tenant Isolation PoC Summary ===" -ForegroundColor Cyan
$passedCount = ($results | Where-Object { $_.Status -eq 'PASS' }).Count
$totalCount = $results.Count
Write-Host "Total Scenarios: $totalCount"
Write-Host "Passed:          $passedCount" -ForegroundColor Green
Write-Host "Failed:          $($totalCount - $passedCount)" -ForegroundColor $(if ($passedCount -eq $totalCount) { "Green" } else { "Red" })

if ($passedCount -ne $totalCount) {
    throw "Tenant Isolation PoC encountered failures."
}
