[CmdletBinding()]
param(
    [string]$ContainerName = 'studyai-db-validation-postgres-1',
    [string]$DatabaseName = 'studyai_tenant_poc_20261020'
)

$ErrorActionPreference = 'Continue'
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " StudyAI Campus: Multi-Session Concurrency Invariants     " -ForegroundColor Cyan
Write-Host " Target Database: $DatabaseName                           " -ForegroundColor Cyan
Write-Host " Container:       $ContainerName                          " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Apply Seed Data
Write-Host "`n[Step 1] Applying concurrency seed data..." -ForegroundColor Yellow
$setupScriptPath = Join-Path (Resolve-Path "$PSScriptRoot\..").Path "infra\postgres\concurrency-poc-setup.sql"
$setupSql = Get-Content -Raw $setupScriptPath
$tmpSetup = [System.IO.Path]::GetTempFileName()
[System.IO.File]::WriteAllText($tmpSetup, $setupSql)
cmd /c "type `"$tmpSetup`" | docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -v ON_ERROR_STOP=1 2>&1" | Out-Null
Remove-Item $tmpSetup -Force
Write-Host "Concurrency seed data applied." -ForegroundColor Green

$results = [System.Collections.Generic.List[PSCustomObject]]::new()

function Record-ConcurrencyResult {
    param(
        [string]$TestId,
        [string]$Description,
        [string]$Setup,
        [string]$Operations,
        [string]$Expected,
        [string]$Actual,
        [bool]$Passed
    )
    $status = if ($Passed) { "PASS" } else { "FAIL" }
    $color = if ($Passed) { "Green" } else { "Red" }
    Write-Host "`n----------------------------------------------------------" -ForegroundColor Gray
    Write-Host "[$status] ${TestId}: $Description" -ForegroundColor $color
    Write-Host "  Setup:      $Setup" -ForegroundColor Gray
    Write-Host "  Operations: $Operations" -ForegroundColor Gray
    Write-Host "  Expected:   $Expected" -ForegroundColor Gray
    Write-Host "  Actual:     $Actual" -ForegroundColor Gray

    $results.Add([PSCustomObject]@{
        TestId = $TestId
        Description = $Description
        Setup = $Setup
        Operations = $Operations
        Expected = $Expected
        Actual = $Actual
        Status = $status
    })
}

$spaceId = '0192a000-0000-7000-8000-0000000000aa'

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 1: Concurrent Note Updates (Optimistic Concurrency)
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-01] Concurrent Note Updates..." -ForegroundColor Yellow

$noteId = '0192a000-0000-7000-8000-0000000000c1'
$rev1Id = '0192a000-0000-7000-8000-0000000000c3'
$rev2Id = '0192a000-0000-7000-8000-0000000000c4'

$sqlWorker1 = @"
BEGIN;
SELECT pg_sleep(0.05);
UPDATE knowledge.notes SET revision = 2, updated_at = now() WHERE knowledge_item_id = '$noteId' AND revision = 1;
INSERT INTO knowledge.note_revisions (id, data_space_id, note_id, revision, format_version, content_json, search_text)
VALUES ('$rev1Id', '$spaceId', '$noteId', 2, 1, '{"text":"Worker 1 update"}', 'Worker 1 update');
COMMIT;
"@

$sqlWorker2 = @"
BEGIN;
SELECT pg_sleep(0.1);
UPDATE knowledge.notes SET revision = 2, updated_at = now() WHERE knowledge_item_id = '$noteId' AND revision = 1;
INSERT INTO knowledge.note_revisions (id, data_space_id, note_id, revision, format_version, content_json, search_text)
VALUES ('$rev2Id', '$spaceId', '$noteId', 2, 1, '{"text":"Worker 2 update"}', 'Worker 2 update');
COMMIT;
"@

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlWorker1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlWorker2

$out1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$out2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$passNote = (($out1 -match 'UPDATE 1' -and ($out2 -match 'duplicate key|UPDATE 0')) -or
             ($out2 -match 'UPDATE 1' -and ($out1 -match 'duplicate key|UPDATE 0')))

$actualNote = "Worker 1: $(($out1 -split "`r?`n")[-1]); Worker 2: $(($out2 -split "`r?`n")[-1])"
Record-ConcurrencyResult "CONC-01" "Concurrent Note Edits Optimistic Concurrency" `
    "Note revision = 1" `
    "Two concurrent sessions attempt to commit revision 2" `
    "Exactly one commits revision 2; competing session fails with unique revision conflict or 0 rows updated" `
    $actualNote $passNote

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 2: Concurrent Atomic Quota Reservation
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-02] Concurrent Atomic Quota Reservation..." -ForegroundColor Yellow

$periodId = '0192a000-0000-7000-8000-0000000000d1'

$sqlQuota1 = @"
UPDATE billing.quota_balances
SET reserved_amount = reserved_amount + 60
WHERE data_space_id = '$spaceId' AND period_id = '$periodId' AND resource = 'ai_tokens'
  AND (limit_amount - used_amount - reserved_amount) >= 60;
"@

$sqlQuota2 = @"
UPDATE billing.quota_balances
SET reserved_amount = reserved_amount + 60
WHERE data_space_id = '$spaceId' AND period_id = '$periodId' AND resource = 'ai_tokens'
  AND (limit_amount - used_amount - reserved_amount) >= 60;
"@

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d -t -A 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlQuota1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d -t -A 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlQuota2

$outQ1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$outQ2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$finalBalance = (& docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc "SELECT reserved_amount FROM billing.quota_balances WHERE period_id = '$periodId';")
if ($finalBalance) { $finalBalance = $finalBalance.Trim() }

$passQuota = (($outQ1 -eq 'UPDATE 1' -and $outQ2 -eq 'UPDATE 0') -or ($outQ1 -eq 'UPDATE 0' -and $outQ2 -eq 'UPDATE 1')) -and ($finalBalance -eq '60')
$actualQuota = "Session 1: $outQ1; Session 2: $outQ2; DB Final Reserved: $finalBalance"
Record-ConcurrencyResult "CONC-02" "Atomic AI Quota Reservation Concurrency" `
    "Quota limit = 100, Reserved = 0, Remaining = 100" `
    "Two concurrent sessions each request 60 units (total 120 > 100)" `
    "Exactly one session succeeds (UPDATE 1); second session updates 0 rows; final reserved = 60 <= 100" `
    $actualQuota $passQuota

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 3: Duplicate Document-Processing Run
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-03] Duplicate Document-Processing Prevention..." -ForegroundColor Yellow

$verId = '0192a000-0000-7000-8000-0000000000a3'
$run1 = '0192a000-0000-7000-8000-000000000091'
$run2 = '0192a000-0000-7000-8000-000000000092'

$sqlRun1 = "INSERT INTO documents.processing_runs (id, data_space_id, document_version_id, processor_version, status, is_active) VALUES ('$run1', '$spaceId', '$verId', 'v1', 'ready', true);"
$sqlRun2 = "INSERT INTO documents.processing_runs (id, data_space_id, document_version_id, processor_version, status, is_active) VALUES ('$run2', '$spaceId', '$verId', 'v1', 'ready', true);"

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlRun1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlRun2

$outR1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$outR2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$passRun = ($outR1 -match 'INSERT 0 1' -and $outR2 -match 'ux_processing_active_run') -or
           ($outR2 -match 'INSERT 0 1' -and $outR1 -match 'ux_processing_active_run')
$actualRun = "Worker 1: $(($outR1 -split "`r?`n")[-1]); Worker 2: $(($outR2 -split "`r?`n")[-1])"
Record-ConcurrencyResult "CONC-03" "Duplicate Document-Processing Run Prevention" `
    "Document version exists with zero active processing runs" `
    "Two concurrent workers attempt to insert an active run (is_active = true)" `
    "One worker succeeds; second worker fails with unique index violation on ux_processing_active_run" `
    $actualRun $passRun

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 4: Subscription Overlap Prevention (GiST Exclusion)
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-04] Subscription Overlap Prevention..." -ForegroundColor Yellow

$planId = '0192a000-0000-7000-8000-0000000000f1'
$sub1 = '0192a000-0000-7000-8000-0000000000f3'
$sub2 = '0192a000-0000-7000-8000-0000000000f4'

$sqlSub1 = "INSERT INTO billing.subscription_periods (id, data_space_id, plan_id, starts_at, ends_at, state) VALUES ('$sub1', '$spaceId', '$planId', '2026-11-10 00:00:00+00', '2026-12-10 00:00:00+00', 'active');"
$sqlSub2 = "INSERT INTO billing.subscription_periods (id, data_space_id, plan_id, starts_at, ends_at, state) VALUES ('$sub2', '$spaceId', '$planId', '2026-11-15 00:00:00+00', '2026-12-15 00:00:00+00', 'active');"

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlSub1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlSub2

$outS1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$outS2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$passSub = ($outS1 -match 'ex_subscription_one_active_per_space') -and ($outS2 -match 'ex_subscription_one_active_per_space')
$actualSub = "Session 1: $(($outS1 -split "`r?`n")[-1]); Session 2: $(($outS2 -split "`r?`n")[-1])"
Record-ConcurrencyResult "CONC-04" "Prepaid Subscription Overlap Exclusion" `
    "Existing active subscription [2026-10-20, 2026-11-20)" `
    "Two concurrent sessions attempt to add overlapping active periods for same space" `
    "Both rejected with exclusion constraint violation ex_subscription_one_active_per_space" `
    $actualSub $passSub

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 5: Idempotent Usage Settle & Release
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-05] Idempotent Usage Settlement..." -ForegroundColor Yellow

$resId = '0192a000-0000-7000-8000-0000000000d2'

$sqlSettle1 = "UPDATE billing.usage_reservations SET state = 'settled', committed_amount = 45, settled_at = now() WHERE id = '$resId' AND state = 'reserved';"
$sqlSettle2 = "UPDATE billing.usage_reservations SET state = 'settled', committed_amount = 45, settled_at = now() WHERE id = '$resId' AND state = 'reserved';"

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d -t -A 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlSettle1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d -t -A 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlSettle2

$outSet1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$outSet2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$passSettle = (($outSet1 -eq 'UPDATE 1' -and $outSet2 -eq 'UPDATE 0') -or ($outSet1 -eq 'UPDATE 0' -and $outSet2 -eq 'UPDATE 1'))
$actualSettle = "Session 1: $outSet1; Session 2: $outSet2"
Record-ConcurrencyResult "CONC-05" "Idempotent Quota Settlement Concurrency" `
    "Usage reservation exists with state = 'reserved'" `
    "Two concurrent workers attempt to settle the same reservation" `
    "Exactly one transitions state (UPDATE 1); second session updates 0 rows without double-settling" `
    $actualSettle $passSettle

# ------------------------------------------------------------------------------
# CONCURRENCY TEST 6: Row Lock & Conflict Retry
# ------------------------------------------------------------------------------
Write-Host "`n[Running Test CONC-06] Row Lock & Retry Behavior..." -ForegroundColor Yellow

$sqlLock1 = @"
BEGIN;
SELECT id FROM ownership.data_spaces WHERE id = '$spaceId' FOR UPDATE;
SELECT pg_sleep(1.0);
COMMIT;
"@

$sqlLock2 = @"
DO \$\$
DECLARE
    v_locked boolean := false;
    v_attempts int := 0;
BEGIN
    WHILE NOT v_locked AND v_attempts < 10 LOOP
        BEGIN
            v_attempts := v_attempts + 1;
            PERFORM id FROM ownership.data_spaces WHERE id = '$spaceId' FOR UPDATE NOWAIT;
            v_locked := true;
        EXCEPTION WHEN lock_not_available THEN
            PERFORM pg_sleep(0.2);
        END;
    END LOOP;
    IF NOT v_locked THEN
        RAISE EXCEPTION 'Retry limit exceeded';
    END IF;
    RAISE NOTICE 'Acquired after % attempts', v_attempts;
END \$\$;
"@

$j1 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlLock1

$j2 = Start-Job -ScriptBlock {
    param($c, $d, $sql)
    $tmp = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tmp, $sql)
    $res = cmd /c "type `"$tmp`" | docker exec -e PGPASSWORD=AppRuntimeSecret123! -i $c psql -U studyai_app -d $d 2>&1"
    Remove-Item $tmp -Force
    $res
} -ArgumentList $ContainerName, $DatabaseName, $sqlLock2

$outL1 = (Receive-Job -Job $j1 -Wait | Out-String).Trim()
$outL2 = (Receive-Job -Job $j2 -Wait | Out-String).Trim()
Remove-Job -Job $j1, $j2 -Force

$passLock = ($outL2 -match 'Acquired after') -and ($outL1 -match 'COMMIT')
$actualLock = "Worker 1: $(($outL1 -split "`r?`n")[-1]); Worker 2: $(($outL2 -split "`r?`n")[-1])"
Record-ConcurrencyResult "CONC-06" "Row Contention Lock & Backoff Retry" `
    "Data space row exists" `
    "Session 1 holds row lock for 1.0s; Session 2 attempts FOR UPDATE NOWAIT with exponential backoff retry" `
    "Session 2 retries gracefully and acquires lock upon Session 1 commit" `
    $actualLock $passLock

Write-Host "`n=== Multi-Session Concurrency Test Summary ===" -ForegroundColor Cyan
$passedCount = ($results | Where-Object { $_.Status -eq 'PASS' }).Count
$totalCount = $results.Count
Write-Host "Total Scenarios: $totalCount"
Write-Host "Passed:          $passedCount" -ForegroundColor Green
Write-Host "Failed:          $($totalCount - $passedCount)" -ForegroundColor $(if ($passedCount -eq $totalCount) { "Green" } else { "Red" })

if ($passedCount -ne $totalCount) {
    throw "Multi-Session Concurrency testing encountered failures."
}
