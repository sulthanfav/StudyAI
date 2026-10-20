[CmdletBinding()]
param(
    [string]$ContainerName = 'studyai-db-validation-postgres-1',
    [string]$DatabaseName = 'studyai_persistence_test'
)

$ErrorActionPreference = 'Stop'
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " StudyAI Campus: Persistence Subset Parity Verification   " -ForegroundColor Cyan
Write-Host " Target Database: $DatabaseName                           " -ForegroundColor Cyan
Write-Host " Container:       $ContainerName                          " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$expectedTables = @(
    'identity.users',
    'ownership.data_spaces',
    'ownership.data_space_memberships',
    'workspace.content_spaces',
    'knowledge.knowledge_items',
    'ai.embedding_models',
    'documents.storage_objects',
    'documents.documents',
    'documents.document_versions',
    'documents.processing_runs',
    'documents.document_chunks',
    'documents.chunk_embeddings'
)

$expectedRlsTables = @(
    'workspace.content_spaces',
    'knowledge.knowledge_items',
    'documents.storage_objects',
    'documents.documents',
    'documents.document_versions',
    'documents.processing_runs',
    'documents.document_chunks',
    'documents.chunk_embeddings'
)

$results = [System.Collections.Generic.List[PSCustomObject]]::new()

function Record-Parity {
    param(
        [string]$CheckId,
        [string]$Description,
        [string]$Expected,
        [string]$Actual,
        [bool]$Passed
    )
    $status = if ($Passed) { "PASS" } else { "FAIL" }
    $color = if ($Passed) { "Green" } else { "Red" }
    Write-Host "  [$status] ${CheckId}: $Description" -ForegroundColor $color
    if (-not $Passed) {
        Write-Host "    Expected: $Expected" -ForegroundColor Yellow
        Write-Host "    Actual:   $Actual" -ForegroundColor Yellow
    }
    $results.Add([PSCustomObject]@{
        CheckId = $CheckId
        Description = $Description
        Expected = $Expected
        Actual = $Actual
        Status = $status
    })
}

# 1. Extensions
$extSql = "SELECT string_agg(extname, ',' ORDER BY extname) FROM pg_extension WHERE extname IN ('vector', 'pg_trgm', 'btree_gist');"
$extActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $extSql).Trim()
$passedExt = ($extActual -eq 'btree_gist,pg_trgm,vector')
Record-Parity "SUB-PAR-01" "Required extensions (vector, pg_trgm, btree_gist)" "btree_gist,pg_trgm,vector" $extActual $passedExt

# 2. Implemented Table Count
$tableSql = "SELECT string_agg(table_schema || '.' || table_name, ',' ORDER BY table_schema, table_name) FROM information_schema.tables WHERE table_schema || '.' || table_name IN ('" + ($expectedTables -join "','") + "');"
$tableActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $tableSql).Trim()
$expectedTableStr = ($expectedTables | Sort-Object) -join ','
$passedTables = ($tableActual -eq $expectedTableStr)
Record-Parity "SUB-PAR-02" "Approved initial subset tables (12 tables)" $expectedTableStr $tableActual $passedTables

# 3. Primary Keys
$pkSql = @"
SELECT count(DISTINCT tc.table_schema || '.' || tc.table_name)
FROM information_schema.table_constraints tc
WHERE (tc.table_schema || '.' || tc.table_name) IN ('$($expectedTables -join "','")')
  AND tc.constraint_type = 'PRIMARY KEY';
"@
$pkActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $pkSql).Trim()
$passedPk = ($pkActual -eq '12')
Record-Parity "SUB-PAR-03" "Primary key coverage across 12 subset tables" "12" $pkActual $passedPk

# 4. Foreign Key Constraints Count
$fkSql = @"
SELECT count(*)
FROM pg_constraint
WHERE contype = 'f' AND conrelid::regclass::text IN ('$($expectedTables -join "','")');
"@
$fkActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $fkSql).Trim()
$passedFk = ($fkActual -eq '14')
Record-Parity "SUB-PAR-04" "Foreign keys across subset tables equals 14" "14" $fkActual $passedFk

# 5. Row-Level Security Enabled and Forced
$rlsSql = @"
SELECT count(*)
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE (n.nspname || '.' || c.relname) IN ('$($expectedRlsTables -join "','")')
  AND c.relrowsecurity = true
  AND c.relforcerowsecurity = true;
"@
$rlsActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $rlsSql).Trim()
$passedRls = ($rlsActual -eq '8')
Record-Parity "SUB-PAR-05" "RLS enabled and forced on 8 tenant-scoped tables" "8" $rlsActual $passedRls

# 6. RLS Policies Configured
$policySql = @"
SELECT count(*)
FROM pg_policies
WHERE (schemaname || '.' || tablename) IN ('$($expectedRlsTables -join "','")')
  AND policyname = 'tenant_isolation_policy'
  AND roles = '{studyai_app}';
"@
$policyActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $policySql).Trim()
$passedPolicy = ($policyActual -eq '8')
Record-Parity "SUB-PAR-06" "tenant_isolation_policy on 8 tenant tables for studyai_app" "8" $policyActual $passedPolicy

# 7. Immutability Trigger
$trgSql = "SELECT count(*) FROM pg_trigger WHERE tgname = 'trg_document_chunk_immutable';"
$trgActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $trgSql).Trim()
$passedTrg = ($trgActual -eq '1')
Record-Parity "SUB-PAR-07" "trg_document_chunk_immutable trigger present" "1" $trgActual $passedTrg

# 8. Vector Column Type
$vecSql = "SELECT udt_name FROM information_schema.columns WHERE table_schema = 'documents' AND table_name = 'chunk_embeddings' AND column_name = 'embedding';"
$vecActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $vecSql).Trim()
$passedVec = ($vecActual -eq 'vector')
Record-Parity "SUB-PAR-08" "chunk_embeddings.embedding is vector" "vector" $vecActual $passedVec

Write-Host "`n=== Subset Parity Summary ===" -ForegroundColor Cyan
$passedCount = ($results | Where-Object { $_.Status -eq 'PASS' }).Count
$totalCount = $results.Count
Write-Host "Total Checks: $totalCount"
Write-Host "Passed:       $passedCount" -ForegroundColor Green
Write-Host "Failed:       $($totalCount - $passedCount)" -ForegroundColor $(if ($passedCount -eq $totalCount) { "Green" } else { "Red" })

Write-Host "`n=== Canonical Coverage Accounting ===" -ForegroundColor Cyan
Write-Host "Covered in Subset: 11 tables (identity.users, ownership.data_spaces, workspace.content_spaces, knowledge.knowledge_items, ai.embedding_models, documents.*)" -ForegroundColor Green
Write-Host "Remaining in Canonical Schema v1.1: 57 tables (workspace.workspaces/folders/placements, knowledge.notes/revisions/saved_outputs, research.*, learning.*, writing.*, presentation.*, integrity.*, planner.*, billing.*, ops.*)" -ForegroundColor Yellow

if ($passedCount -ne $totalCount) {
    throw "Subset schema parity verification failed."
}
