# Diagnóstico de pronomes, sujeito e gênero: roda frases INVENTADAS no motor atual,
# pelo mesmo caminho do botão Reformular (QA/QaRunner.cs), sem alterar nada.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/pronoun-diagnosis/Run-Diagnosis.ps1
param([int]$TimeoutSeconds = 600, [string]$Only, [string]$Out = 'resultados.json')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$runner = [IO.File]::ReadAllText((Join-Path $root 'QA/QaRunner.cs'))
Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = Join-Path $root 'engine'
$before = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
$cases = Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'frases.json') | ConvertFrom-Json
$rows = @()
if ($Only) { $wanted = @($Only -split '[,; ]+' | Where-Object { $_ } | ForEach-Object { [int]$_ }); $cases = @($cases | Where-Object { $wanted -contains [int]$_.number }) }
foreach ($c in $cases) {
    $g = [QaRunner]::Run($root, $c.input, $TimeoutSeconds)
    # Same criteria as QA/Report.ps1.
    $retry = $g.Log.Contains('[BLOCK_RETRY_START]')
    $warn = @($g.Log -split '\r?\n' | Where-Object { $_ -match '\[(BLOCK_ACCEPTED_WITH_WARNING|BLOCK_REJECTED)\]' })
    $row = [ordered]@{ number=$c.number; gender=$c.gender; input=$c.input; output=$g.Output; seconds=[math]::Round($g.GenerationSeconds,1);
        completed=$g.CompletedNormally; retry=[bool]$retry; warnings=$warn; exception=$g.Exception;
        repairs=@($g.Log -split '\r?\n' | Where-Object { $_ -match '\[(LOCAL_REPAIR|BLOCK_RETRY_START|BLOCK_REJECTED|BLOCK_ACCEPTED_WITH_WARNING|BLOCK_FIRST_PASS|BLOCK_OK)\]' } | ForEach-Object { ($_ -replace '^\[[^\]]*\]\s*','') }) }
    $rows += [pscustomobject]$row
    Write-Output ("{0,2} [{1}] {2,5:N1}s  {3}`n        -> {4}" -f $c.number, $c.gender, $row.seconds, $c.input, $g.Output)
    $rows | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot $Out)
}
$after = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
Write-Output ("motor inalterado: " + (-not (Compare-Object ($before | % Hash) ($after | % Hash))))