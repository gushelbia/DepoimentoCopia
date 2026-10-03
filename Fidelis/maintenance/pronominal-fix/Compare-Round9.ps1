# Antes × depois da rodada 9 (bateria realista 2: pronominais, primeira pessoa, possessivo, destinatário, regência).
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/pronominal-fix/Compare-Round9.ps1
# Marca INESPERADO qualquer mudança fora dos casos destes problemas.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location -LiteralPath (Split-Path -Parent $root)
$expected = @{
    'N3' = 'possessivo: «na sua porta» → «na porta do depoente» (outra pessoa na frase)'
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
[void]$sb.AppendLine('# Rodada 9: pronominais, primeira pessoa, possessivo, destinatário e regência — antes × depois'); [void]$sb.AppendLine()
[void]$sb.AppendLine('## 20 frases inventadas'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Entrada | Antes | Depois | O que mudou | Problema | Retry antes → depois |'); [void]$sb.AppendLine('|---|---|---|---|---|---|---|')
$before = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronominal-fix/antes/20-frases.json') | ConvertFrom-Json
$after = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-rodada9.json') | ConvertFrom-Json
foreach ($a in $before) {
    $b = $after | Where-Object number -eq $a.number
    $o1 = ($a.output -replace "`r?`n", ' / ').Trim(); $o2 = ($b.output -replace "`r?`n", ' / ').Trim()
    $v = Verdict ("P" + $a.number) ($o1 -cne $o2); if ($v -like '*INESPERADO*') { $unexpected++ }
    [void]$sb.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5} | {6} → {7} |" -f $a.number, $a.input, $o1, $o2, (Show-Change $o1 $o2), $v, $a.retry, $b.retry))
}
[void]$sb.AppendLine(); [void]$sb.AppendLine('## Bateria de gênero (saída do motor, antes da conversão da interface)'); [void]$sb.AppendLine()
[void]$sb.AppendLine('| # | Entrada | Antes | Depois | O que mudou | Problema |'); [void]$sb.AppendLine('|---|---|---|---|---|---|')
$gb = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronominal-fix/antes/bateria-genero.json') | ConvertFrom-Json
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
# Both realistic batteries: before × after, with the automatic key.
function KeyChecks($file) { @(Get-Content -Encoding UTF8 $file | Where-Object { $_ -and -not $_.StartsWith('#') } | ForEach-Object { $p = $_ -split "`t"; [pscustomobject]@{ id = $p[0]; must = ($p[1] -eq 'deve'); rx = $p[2]; what = $p[3] } }) }
function KeyFails($checks, $id, $text) { @($checks | Where-Object { $_.id -eq $id -and ([regex]::IsMatch($text, $_.rx, 'IgnoreCase') -ne $_.must) } | ForEach-Object { $_.what }) }
$sb2 = New-Object Text.StringBuilder
foreach ($b in @(
    @{ title = 'Bateria realista 1 (aprovada na rodada 8)'; before = 'maintenance/pronominal-fix/antes/bateria-realista.json'; after = 'maintenance/realistic-battery/resultados.json'; key = 'maintenance/realistic-battery/chave-automatica.tsv' },
    @{ title = 'Bateria realista 2'; before = 'maintenance/pronominal-fix/antes/bateria-realista-2.json'; after = 'maintenance/realistic-battery-2/resultados.json'; key = 'maintenance/realistic-battery-2/chave-automatica.tsv' })) {
    $checks = KeyChecks (Join-Path $root $b.key)
    [void]$sb2.AppendLine(); [void]$sb2.AppendLine('## ' + $b.title + ' (saída final, já convertida para o gênero)'); [void]$sb2.AppendLine()
    [void]$sb2.AppendLine('| # | Gênero | Antes | Depois | O que mudou | Chave automática antes → depois | Tempo; nova tentativa; rejeitada |'); [void]$sb2.AppendLine('|---|---|---|---|---|---|---|')
    $rb = Get-Content -Raw -Encoding UTF8 (Join-Path $root $b.before) | ConvertFrom-Json
    $ra = Get-Content -Raw -Encoding UTF8 (Join-Path $root $b.after) | ConvertFrom-Json
    foreach ($a in $rb) {
        $x = $ra | Where-Object id -eq $a.id
        $sa = ($a.shown -replace "`r?`n", ' / ').Trim(); $sx = ($x.shown -replace "`r?`n", ' / ').Trim()
        $fa = KeyFails $checks $a.id $sa; $fx = KeyFails $checks $a.id $sx
        [void]$sb2.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5} → {6} | {7} s → {8} s; {9} → {10}; {11} → {12} |" -f $a.id, $a.gender, $sa, $sx, (Show-Change $sa $sx), $(if ($fa) { $fa -join '; ' } else { 'ok' }), $(if ($fx) { $fx -join '; ' } else { 'ok' }), $a.seconds, $x.seconds, $a.retry, $x.retry, $a.incomplete, $x.incomplete))
    }
}
[IO.File]::AppendAllText($out, $sb2.ToString(), (New-Object Text.UTF8Encoding $false))
