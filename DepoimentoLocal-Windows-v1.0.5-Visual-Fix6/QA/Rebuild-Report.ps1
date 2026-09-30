# Reanalyze saved evidence without invoking generation or modifying any output.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Report.ps1')
$results = Join-Path $PSScriptRoot 'Results'
$oldSummary = Get-Content -Raw -Encoding UTF8 (Join-Path $results 'summary.json') | ConvertFrom-Json
$rows = @(Get-ChildItem $results -Filter 'test-??.json' | Sort-Object Name | ForEach-Object {
    $saved = Get-Content -Raw -Encoding UTF8 $_.FullName | ConvertFrom-Json
    $stem = 'test-{0:d2}' -f [int]$saved.number
    $log = [IO.File]::ReadAllText((Join-Path $results "$stem-log.txt"))
    $output = [IO.File]::ReadAllText((Join-Path $results "$stem-output.txt"))
    $generated = [pscustomobject]@{Output=$output;Log=$log;Exception=$saved.exception;CompletedNormally=$saved.completedNormally;GenerationSeconds=$saved.generationSeconds;TotalSeconds=$saved.totalSeconds}
    $row = Get-QaResult $saved $generated
    $row | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $_.FullName
    $row
})
Write-QaReport $rows $results $oldSummary.'Tempo total (s)' ($rows.Count -eq 10)
