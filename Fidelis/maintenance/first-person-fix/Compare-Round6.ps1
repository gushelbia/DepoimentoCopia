# Antes × depois da rodada 6 (abertura A + correção do sujeito trocado/reflexivo).
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/first-person-fix/Compare-Round6.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location -LiteralPath (Split-Path -Parent $root)
function Words($t) { @([regex]::Split(($t -replace '\s+', ' ').Trim(), ' ')) }
# Mostra só o trecho que mudou (prefixo e sufixo comuns removidos).
function Show-Change($a, $b) {
    if ($a -eq $b) { return 'igual' }
    $x = Words $a; $y = Words $b; $i = 0
    while ($i -lt $x.Count -and $i -lt $y.Count -and $x[$i] -eq $y[$i]) { $i++ }
    $j = 0
    while ($j -lt ($x.Count - $i) -and $j -lt ($y.Count - $i) -and $x[$x.Count - 1 - $j] -eq $y[$y.Count - 1 - $j]) { $j++ }
    $old = ($x[$i..($x.Count - 1 - $j)] -join ' '); $new = ($y[$i..($y.Count - 1 - $j)] -join ' ')
    if ($i -gt $x.Count - 1 - $j) { $old = '' }; if ($i -gt $y.Count - 1 - $j) { $new = '' }
    $ctxStart = [Math]::Max(0, $i - 3)
    $ctx = if ($i -gt 0) { '…' + ($x[$ctxStart..($i - 1)] -join ' ') + ' ' } else { '' }
    return "$ctx«$old» → «$new»"
}
$sb = New-Object Text.StringBuilder
[void]$sb.AppendLine('# Rodada 6: antes × depois'); [void]$sb.AppendLine()
[void]$sb.AppendLine('## Bateria de 20 frases inventadas'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Entrada | Antes | Depois | O que mudou | Tempo antes → depois |'); [void]$sb.AppendLine('|---|---|---|---|---|---|')
$before = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-20-frases.json') | ConvertFrom-Json
$after = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-depois.json') | ConvertFrom-Json
foreach ($a in $before) {
    $b = $after | Where-Object number -eq $a.number
    $o1 = ($a.output -replace "`r?`n", ' / ').Trim(); $o2 = ($b.output -replace "`r?`n", ' / ').Trim()
    [void]$sb.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5:N1} s → {6:N1} s |" -f $a.number, $a.input, $o1, $o2, (Show-Change $o1 $o2), $a.seconds, $b.seconds))
}
[void]$sb.AppendLine(); [void]$sb.AppendLine('## Bateria QA (10 casos): texto aprovado (git) × motor novo'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Caso | Status antes → depois | Retry antes → depois | O que mudou no texto | Tempo antes → depois |'); [void]$sb.AppendLine('|---|---|---|---|---|---|')
foreach ($n in 1..10) {
    $f = 'Fidelis/QA/Results/test-{0:D2}.json' -f $n
    $git = (git show "HEAD:$f" | Out-String | ConvertFrom-Json); $now = Get-Content -Raw -Encoding UTF8 $f | ConvertFrom-Json
    $changes = @()
    $s1 = [regex]::Split(($git.output -replace '\s+',' ').Trim(), '(?<=[.!?])\s+'); $s2 = [regex]::Split(($now.output -replace '\s+',' ').Trim(), '(?<=[.!?])\s+')
    if ($s1.Count -eq $s2.Count) { for ($k = 0; $k -lt $s1.Count; $k++) { if ($s1[$k] -ne $s2[$k]) { $changes += ('frase ' + ($k + 1) + ': ' + (Show-Change $s1[$k] $s2[$k])) } } }
    elseif ($git.output -ne $now.output) { $changes += ('número de frases mudou: ' + $s1.Count + ' → ' + $s2.Count + '; ' + (Show-Change $git.output $now.output)) }
    $what = if ($changes.Count) { $changes -join '<br>' } else { 'igual' }
    [void]$sb.AppendLine(("| {0} | {1} | {2} → {3} | {4} → {5} | {6} | {7:N1} s → {8:N1} s |" -f $n, $now.name, $git.status, $now.status, $git.retry, $now.retry, $what, $git.generationSeconds, $now.generationSeconds))
}
$out = Join-Path $PSScriptRoot 'ANTES-DEPOIS.md'
[IO.File]::WriteAllText($out, $sb.ToString(), (New-Object Text.UTF8Encoding $false))
Write-Output $out