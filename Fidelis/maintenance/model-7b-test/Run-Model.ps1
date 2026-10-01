# Testa outro modelo (ex.: Qwen2.5 7B) nas mesmas baterias, SEM trocar o modelo do programa:
# usa o runner da QA (QA/QaRunner.cs) com o caminho do modelo trocado só nesta compilação,
# grava resultados nesta pasta (não em QA/Results) e devolve o last-model.txt do programa.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/model-7b-test/Run-Model.ps1 -Model D:\...\arquivo.gguf -Tag 7b [-Suite frases|qa|ambas] [-Only 1,2]
param([Parameter(Mandatory=$true)][string]$Model, [string]$Tag = '7b', [string]$Suite = 'ambas', [string]$Only, [int]$TimeoutSeconds = 2400)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path -LiteralPath $Model)) { throw "Modelo não encontrado: $Model" }
$source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$runner = [IO.File]::ReadAllText((Join-Path $root 'QA/QaRunner.cs'))
$literal = '@"..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf"'
if (-not $runner.Contains($literal)) { throw 'Caminho do modelo não encontrado no runner.' }
$runner = $runner.Replace($literal, '@"' + $Model.Replace('"', '""') + '"')
Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
. (Join-Path $root 'QA/Report.ps1')
[Environment]::CurrentDirectory = Join-Path $root 'engine'
$lastModel = Join-Path $env:LOCALAPPDATA 'DepoimentoLocal\last-model.txt'
$savedLast = if (Test-Path -LiteralPath $lastModel) { [IO.File]::ReadAllBytes($lastModel) } else { $null }
$before = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
$wanted = if ($Only) { @($Only -split '[,; ]+' | Where-Object { $_ } | ForEach-Object { [int]$_ }) } else { @() }
try {
    if ($Suite -in 'frases','ambas') {
        $rows = @()
        foreach ($c in (Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/frases.json') | ConvertFrom-Json)) {
            if ($wanted.Count -and $wanted -notcontains [int]$c.number) { continue }
            $g = [QaRunner]::Run($root, $c.input, $TimeoutSeconds)
            $rows += [pscustomobject][ordered]@{ number=$c.number; gender=$c.gender; input=$c.input; output=$g.Output; seconds=[math]::Round($g.GenerationSeconds,1);
                completed=$g.CompletedNormally; retry=$g.Log.Contains('[BLOCK_RETRY_START]'); exception=$g.Exception;
                repairs=@($g.Log -split '\r?\n' | Where-Object { $_ -match '\[(LOCAL_REPAIR|BLOCK_RETRY_START|BLOCK_REJECTED|BLOCK_ACCEPTED_WITH_WARNING|MODEL_LOAD_OK|MODEL_LOAD_FAILED)\]' } | ForEach-Object { $_ -replace '^\[[^\]]*\]\s*','' }) }
            Write-Output ("frase {0,2}: {1,6:N1}s  {2}" -f $c.number, $g.GenerationSeconds, ($g.Output -replace "`r?`n",' / '))
            $rows | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot "frases-$Tag.json")
        }
    }
    if ($Suite -in 'qa','ambas') {
        $rows = @()
        foreach ($file in (Get-ChildItem (Join-Path $root 'QA/TestCases') -Filter '*.json' | Sort-Object Name)) {
            $case = Get-Content -Raw -Encoding UTF8 $file.FullName | ConvertFrom-Json
            if ($case.inputFile) { $case.input = [IO.File]::ReadAllText((Join-Path $file.DirectoryName $case.inputFile)).TrimEnd("`r", "`n") }
            if ($wanted.Count -and $wanted -notcontains [int]$case.number) { continue }
            try { $g = [QaRunner]::Run($root, $case.input, $TimeoutSeconds) }
            catch { $g = [pscustomobject]@{Output='';Log='';Exception=$_.Exception.ToString();GenerationSeconds=0;TotalSeconds=0;CompletedNormally=$false} }
            $row = Get-QaResult $case $g
            $rows += $row
            Write-Output ("QA {0,2}: {1}; {2:N1}s; warnings={3}; retry={4}" -f $case.number, $row.status, $row.generationSeconds, $row.warnings.Count, $row.retry)
            $rows | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot "qa-$Tag.json")
        }
    }
} finally {
    if ($savedLast -ne $null) { [IO.File]::WriteAllBytes($lastModel, $savedLast) }
    $after = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
    Write-Output ("motor inalterado: " + (-not (Compare-Object ($before | % Hash) ($after | % Hash))))
    Write-Output ("modelo lembrado pelo programa: " + $(if (Test-Path -LiteralPath $lastModel) { [IO.File]::ReadAllText($lastModel).Trim() } else { '(nenhum)' }))
}