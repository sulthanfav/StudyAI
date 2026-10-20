[CmdletBinding()]
param(
    [string]$ContainerName = 'studyai-db-validation-postgres-1',
    [string]$DatabaseName = 'studyai_tenant_poc_20261020'
)

$ErrorActionPreference = 'Stop'
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " StudyAI Campus: PostgreSQL Schema Parity Verification    " -ForegroundColor Cyan
Write-Host " Target Database: $DatabaseName                           " -ForegroundColor Cyan
Write-Host " Canonical SQL:   StudyAI_Physical_Schema_Reviewed_v1.1.sql" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$expectedSchemas = @(
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
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

# Check 1: Extensions
$extSql = "SELECT string_agg(extname, ',' ORDER BY extname) FROM pg_extension WHERE extname IN ('vector', 'pg_trgm', 'btree_gist');"
$extActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $extSql).Trim()
$passedExt = ($extActual -eq 'btree_gist,pg_trgm,vector')
Record-Parity "PAR-01" "Required extensions (vector, pg_trgm, btree_gist)" "btree_gist,pg_trgm,vector" $extActual $passedExt

# Check 2: Schema Count
$schemaSql = "SELECT string_agg(schema_name, ',' ORDER BY schema_name) FROM information_schema.schemata WHERE schema_name IN ('" + ($expectedSchemas -join "','") + "');"
$schemaActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $schemaSql).Trim()
$expectedSchemaStr = ($expectedSchemas | Sort-Object) -join ','
$passedSchema = ($schemaActual -eq $expectedSchemaStr)
Record-Parity "PAR-02" "All 14 application schemas present" $expectedSchemaStr $schemaActual $passedSchema

# Check 3: Table Count across 14 schemas
$tableSql = "SELECT count(*) FROM information_schema.tables WHERE table_schema IN ('" + ($expectedSchemas -join "','") + "') AND table_type = 'BASE TABLE';"
$tableActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $tableSql).Trim()
$passedTables = ($tableActual -eq '68')
Record-Parity "PAR-03" "Total canonical tables count equals 68" "68" $tableActual $passedTables

# Check 4: Primary Keys Count
$pkSql = @"
SELECT count(DISTINCT tc.table_schema || '.' || tc.table_name)
FROM information_schema.table_constraints tc
WHERE tc.table_schema IN ('$($expectedSchemas -join "','")')
  AND tc.constraint_type = 'PRIMARY KEY';
"@
$pkActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $pkSql).Trim()
$passedPk = ($pkActual -eq '68')
Record-Parity "PAR-04" "Primary key coverage across all 68 tables" "68" $pkActual $passedPk

# Check 5: Critical GiST Exclusion Constraint
$gistSql = "SELECT count(*) FROM pg_constraint WHERE conname = 'ex_subscription_one_active_per_space';"
$gistActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $gistSql).Trim()
$passedGist = ($gistActual -eq '1')
Record-Parity "PAR-05" "GiST subscription exclusion constraint present" "1" $gistActual $passedGist

# Check 6: Immutability triggers present
$trgSql = "SELECT count(*) FROM pg_trigger WHERE tgname IN ('trg_note_revision_immutable', 'trg_document_version_immutable', 'trg_document_chunk_immutable', 'trg_embedding_dimensions');"
$trgActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $trgSql).Trim()
$passedTrg = ($trgActual -eq '4')
Record-Parity "PAR-06" "Immutability & dimension guard triggers present" "4" $trgActual $passedTrg

# Check 7: Vector column verification
$vecSql = "SELECT udt_name FROM information_schema.columns WHERE table_schema = 'documents' AND table_name = 'chunk_embeddings' AND column_name = 'embedding';"
$vecActual = (docker exec -i $ContainerName psql -U studyai_admin -d $DatabaseName -tAc $vecSql).Trim()
$passedVec = ($vecActual -eq 'vector')
Record-Parity "PAR-07" "Chunk embedding column type is pgvector" "vector" $vecActual $passedVec

Write-Host "`n=== Parity Verification Summary ===" -ForegroundColor Cyan
$passedCount = ($results | Where-Object { $_.Status -eq 'PASS' }).Count
$totalCount = $results.Count
Write-Host "Total Checks: $totalCount"
Write-Host "Passed:       $passedCount" -ForegroundColor Green
Write-Host "Failed:       $($totalCount - $passedCount)" -ForegroundColor $(if ($passedCount -eq $totalCount) { "Green" } else { "Red" })

if ($passedCount -ne $totalCount) {
    throw "Schema parity verification failed."
}
