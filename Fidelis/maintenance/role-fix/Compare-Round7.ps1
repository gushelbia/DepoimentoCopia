# Antes × depois da rodada 7 (inversões de papel), caso a caso.
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/role-fix/Compare-Round7.ps1
# Marca INESPERADO qualquer mudança fora dos casos dos quatro problemas.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location -LiteralPath (Split-Path -Parent $root)
$expected = @{
    'P1' = 'pronome ambíguo → passiva'; 'P2' = 'pronome ambíguo → passiva'; 'P5' = 'frase quebrada → corrigida pelo original'; 'P11' = 'pronome ambíguo → passiva'
    'F5' = 'papéis trocados + sujeito apagado'; 'M5' = 'sujeito apagado'; 'F7' = 'papéis trocados'; 'M7' = 'papéis trocados'
    'F11' = 'pronome ambíguo → passiva'; 'M11' = 'pronome ambíguo → passiva'
    'F13' = 'sujeito apagado'; 'F14' = 'sujeito apagado'; 'M13' = 'sujeito apagado'; 'M14' = 'sujeito apagado'
    'N1' = 'sujeito apagado'; 'N3' = 'sujeito apagado (dupla inversão): validador acusou, nova tentativa saiu correta'; 'N5' = 'sujeito apagado'
}
function Words($t) { @([regex]::Split(($t -replace '\s+', ' ').Trim(), ' ')) }
function Show-Change($a, $b) {
    if ($a -ceq $b) { return 'igual' }
    $x = Words $a; $y = Words $b; $i = 0
    while ($i -lt $x.Count -and $i -lt $y.Count -and $x[$i] -ceq $y[$i]) { $i++ }
    $j = 0
    while ($j -lt ($x.Count - $i) -and $j -lt ($y.Count - $i) -and $x[$x.Count - 1 - $j] -ceq $y[$y.Count - 1 - $j]) { $j++ }
    $old = ($x[$i..($x.Count - 1 - $j)] -join ' '); $new = ($y[$i..($y.Count - 1 - $j)] -join ' ')
    if ($i -gt $x.Count - 1 - $j) { $old = '' }; if ($i -gt $y.Count - 1 - $j) { $new = '' }
    $ctxStart = [Math]::Max(0, $i - 3)
    $ctx = if ($i -gt 0) { '…' + ($x[$ctxStart..($i - 1)] -join ' ') + ' ' } else { '' }
    return "$ctx«$old» → «$new»"
}
function Verdict($id, $changed) {
    if (-not $changed) { if ($expected.ContainsKey($id)) { return 'sem mudança' } else { return 'igual' } }
    if ($expected.ContainsKey($id)) { return $expected[$id] }
    return '**INESPERADO**'
}
$unexpected = 0
$sb = New-Object Text.StringBuilder
[void]$sb.AppendLine('# Rodada 7: inversões de papel, antes × depois'); [void]$sb.AppendLine()
[void]$sb.AppendLine('## 20 frases inventadas'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Entrada | Antes | Depois | O que mudou | Problema | Retry antes → depois |'); [void]$sb.AppendLine('|---|---|---|---|---|---|---|')
$before = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/role-fix/antes/20-frases.json') | ConvertFrom-Json
$after = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-depois.json') | ConvertFrom-Json
foreach ($a in $before) {
    $b = $after | Where-Object number -eq $a.number
    $o1 = ($a.output -replace "`r?`n", ' / ').Trim(); $o2 = ($b.output -replace "`r?`n", ' / ').Trim()
    $v = Verdict ("P" + $a.number) ($o1 -cne $o2); if ($v -like '*INESPERADO*') { $unexpected++ }
    [void]$sb.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5} | {6} → {7} |" -f $a.number, $a.input, $o1, $o2, (Show-Change $o1 $o2), $v, $a.retry, $b.retry))
}
[void]$sb.AppendLine(); [void]$sb.AppendLine('## Bateria de gênero (saída do motor, antes da conversão da interface)'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Entrada | Antes | Depois | O que mudou | Problema |'); [void]$sb.AppendLine('|---|---|---|---|---|---|')
$gb = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/role-fix/antes/bateria-genero.json') | ConvertFrom-Json
$ga = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/gender/bateria-resultados.json') | ConvertFrom-Json
foreach ($a in $gb) {
    $b = $ga | Where-Object id -eq $a.id
    $v = Verdict $a.id ($a.engine -cne $b.engine); if ($v -like '*INESPERADO*') { $unexpected++ }
    [void]$sb.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5} |" -f $a.id, $a.input, $a.engine, $b.engine, (Show-Change $a.engine $b.engine), $v))
}
[void]$sb.AppendLine(); [void]$sb.AppendLine('## Bateria QA (10 casos): texto aprovado (git) × motor novo'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Caso | Status antes → depois | Retry antes → depois | O que mudou no texto |'); [void]$sb.AppendLine('|---|---|---|---|---|')
foreach ($n in 1..10) {
    $f = 'Fidelis/QA/Results/test-{0:D2}.json' -f $n
    $git = (git show "HEAD:$f" | Out-String).TrimStart([char]0xFEFF) | ConvertFrom-Json; $now = Get-Content -Raw -Encoding UTF8 $f | ConvertFrom-Json
    $what = Show-Change $git.output $now.output
    if ($what -ne 'igual') { $unexpected++; $what = '**INESPERADO** ' + $what }
    [void]$sb.AppendLine(("| {0} | {1} | {2} → {3} | {4} → {5} | {6} |" -f $n, $now.name, $git.status, $now.status, $git.retry, $now.retry, $what))
}
[void]$sb.AppendLine(); [void]$sb.AppendLine("Mudanças inesperadas: $unexpected")
$out = Join-Path $PSScriptRoot 'ANTES-DEPOIS.md'
[IO.File]::WriteAllText($out, $sb.ToString(), (New-Object Text.UTF8Encoding $false))
Write-Output "Mudanças inesperadas: $unexpected"
Write-Output $out
