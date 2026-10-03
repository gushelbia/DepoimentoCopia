# Bateria realista 3 (casos novos; depoimentos inventados em estilo de fala transcrita): roda cada
# caso no motor atual pelo mesmo caminho do botão Reformular (QA/QaRunner.cs),
# aplica a conversão do campo «Gênero do depoente» da interface e calcula os
# alertas que a janela mostraria (ReviewScanner.Compare com o gênero). Não altera nada.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/realistic-battery-3/Run-RealisticBattery.ps1 [-Only R01,R05]
param([int]$TimeoutSeconds = 900, [string]$Only)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$runner = [IO.File]::ReadAllText((Join-Path $root 'QA/QaRunner.cs'))
Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = Join-Path $root 'engine'
$before = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash | ForEach-Object Hash)

# Casos: «R01», «GÊNERO: …», «TEXTO:» (até «CHAVE:»), «CHAVE:» (até a linha de =====).
$lines = [IO.File]::ReadAllLines((Join-Path $PSScriptRoot 'bateria-realista-3.txt'), [Text.Encoding]::UTF8)
$cases = @(); $cur = $null; $mode = ''
foreach ($l in $lines) {
    if ($l -match '^[A-Z]\d+\s*$') { if ($cur) { $cases += $cur }; $cur = [ordered]@{ id = $l.Trim(); gender = ''; text = ''; key = @() }; $mode = ''; continue }
    if (-not $cur) { continue }
    if ($l -match '^GÊNERO:\s*(.+)$') { $g = $Matches[1].Trim(); $cur.gender = $(if ($g -eq 'Não informado') { '' } else { $g }); continue }
    if ($l -match '^TEXTO:') { $mode = 'text'; continue }
    if ($l -match '^CHAVE:') { $mode = 'key'; continue }
    if ($l -match '^=+$') { $mode = ''; continue }
    if ($mode -eq 'text' -and $l.Trim()) { $cur.text = ($cur.text + ' ' + $l.Trim()).Trim() }
    if ($mode -eq 'key' -and $l.Trim()) { $cur.key += $l.Trim() }
}
if ($cur) { $cases += $cur }
if ($Only) { $wanted = @($Only -split '[,; ]+'); $cases = @($cases | Where-Object { $wanted -contains $_.id }) }

# Regression: automatic key (chave-automatica.tsv) and the approved outputs (aprovados.json).
$keyFile = Join-Path $PSScriptRoot 'chave-automatica.tsv'
$checks = @(); if (Test-Path $keyFile) { $checks = @(Get-Content -Encoding UTF8 $keyFile | Where-Object { $_ -and -not $_.StartsWith('#') } | ForEach-Object { $p = $_ -split "`t"; [pscustomobject]@{ id = $p[0]; must = ($p[1] -eq 'deve'); rx = $p[2]; what = $p[3] } }) }
$approvedPath = Join-Path $PSScriptRoot 'aprovados.json'
$approved = @{}
if (Test-Path $approvedPath) { foreach ($a in (Get-Content -Raw -Encoding UTF8 $approvedPath | ConvertFrom-Json)) { $approved[$a.id] = $a.shown } }
$keyFailures = 0; $changed = 0
function Items($list) { @($list | ForEach-Object { '«' + $_.Text.Trim() + '» — ' + $_.Reason }) }
$rows = @()
$md = New-Object Text.StringBuilder
[void]$md.AppendLine('# Bateria realista 3 — resultados no motor atual'); [void]$md.AppendLine()
[void]$md.AppendLine('Depoimentos inventados. Saída final = texto que a janela mostra (com a conversão do gênero). Tempo = geração no motor.'); [void]$md.AppendLine()
foreach ($c in $cases) {
    $g = [QaRunner]::Run($root, $c.text, $TimeoutSeconds)
    $shown = [GenderConverter]::ForGender($c.gender, $c.text, $g.Output)
    $review = [ReviewScanner]::Compare($c.text, $shown, $c.gender)
    $retry = $g.Log.Contains('[BLOCK_RETRY_START]')
    $warn = @($g.Log -split '\r?\n' | Where-Object { $_ -match '\[(BLOCK_ACCEPTED_WITH_WARNING|BLOCK_REJECTED|BLOCK_RETRY_START)\]' } | ForEach-Object { ($_ -replace '^\[[^\]]*\]\s*\[[A-Z]+\]\s*', '') })
    $incomplete = $g.Output.StartsWith('[SAÍDA INCOMPLETA')
    $orange = Items $review.Roles
    $notes = Items $review.Notes
    $approx = @($review.Reformulated | Where-Object { $_.Alert -and -not $_.Unmatched } | ForEach-Object { $_.Kind + ': ' + $_.Text.Trim() })
    $unmatched = @($review.Reformulated | Where-Object { $_.Unmatched } | ForEach-Object { $_.Kind + ': ' + $_.Text.Trim() })
    $missing = @($review.MissingFromReformulated | ForEach-Object { $_.Kind + ': ' + $_.Text.Trim() })
    $field = $(if ($c.gender) { $c.gender } else { 'Não informado' })
    $row = [ordered]@{ id = $c.id; gender = $field; input = $c.text; engine = $g.Output; shown = $shown; seconds = [math]::Round($g.GenerationSeconds, 1)
        completed = $g.CompletedNormally; incomplete = $incomplete; keyFailures = @(); sameAsApproved = $null; retry = $retry; engineWarnings = $warn; exception = $g.Exception
        orange = $orange; notes = $notes; approximate = $approx; notInOriginal = $unmatched; missing = $missing; summary = [ReviewScanner]::Summary($review); key = $c.key }
    $fails = @()
    # Only the reformulated text: the incomplete mark («Motivo: …») may quote the original.
    $body = (($shown -split "`r?`n") | Where-Object { $_ -notmatch '^\[SAÍDA INCOMPLETA|^Motivo:|^\[FIM DO TRECHO' }) -join "`n"
    foreach ($k in ($checks | Where-Object id -eq $c.id)) { $hit = [regex]::IsMatch($body, $k.rx, 'IgnoreCase'); if ($hit -ne $k.must) { $fails += ($(if ($k.must) { 'falta: ' } else { 'não devia ter: ' }) + $k.what) } }
    $keyFailures += $fails.Count
    $row.keyFailures = $fails
    $row.sameAsApproved = $(if ($approved.ContainsKey($c.id)) { $approved[$c.id] -ceq $shown } else { $null })
    if ($row.sameAsApproved -eq $false) { $changed++ }
    Write-Output ("   chave automática: " + $(if ($fails.Count) { 'FALHA — ' + ($fails -join '; ') } else { 'ok' }) + $(if ($null -ne $row.sameAsApproved) { '; igual ao aprovado: ' + $row.sameAsApproved } else { '' }))
    $rows += [pscustomobject]$row
    Write-Output ("{0} [{1}] {2:N1}s retry={3} incompleta={4} laranja={5} informativos={6}" -f $c.id, $field, $row.seconds, $retry, $incomplete, $orange.Count, $notes.Count)
    [void]$md.AppendLine("## $($c.id) — Gênero do depoente: $field — $($row.seconds) s" + $(if ($retry) { ' — nova tentativa' } else { '' }) + $(if ($incomplete) { ' — **SAÍDA INCOMPLETA (rejeitada)**' } else { '' })); [void]$md.AppendLine()
    [void]$md.AppendLine('**Entrada:** ' + $c.text); [void]$md.AppendLine()
    [void]$md.AppendLine('**Saída final:** ' + ($shown -replace "`r?`n", ' / ')); [void]$md.AppendLine()
    if ($shown -cne $g.Output) { [void]$md.AppendLine('*Saída do motor (antes da conversão):* ' + ($g.Output -replace "`r?`n", ' / ')); [void]$md.AppendLine() }
    [void]$md.AppendLine('**Alertas laranja (papéis e pronomes):**' + $(if ($orange.Count) { '' } else { ' nenhum' })); foreach ($x in $orange) { [void]$md.AppendLine('- ' + $x) }; [void]$md.AppendLine()
    [void]$md.AppendLine('**Avisos informativos:**' + $(if ($notes.Count) { '' } else { ' nenhum' })); foreach ($x in $notes) { [void]$md.AppendLine('- ' + $x) }; [void]$md.AppendLine()
    if ($approx.Count -or $unmatched.Count -or $missing.Count) {
        [void]$md.AppendLine('**Datas, números e nomes:**')
        foreach ($x in $unmatched) { [void]$md.AppendLine('- não encontrado no original: ' + $x) }
        foreach ($x in $approx) { [void]$md.AppendLine('- aproximado ou ambíguo: ' + $x) }
        foreach ($x in $missing) { [void]$md.AppendLine('- sumiu do reformulado: ' + $x) }
        [void]$md.AppendLine()
    }
    if ($warn.Count) { [void]$md.AppendLine('**Motor:** ' + ($warn -join ' | ')); [void]$md.AppendLine() }
    [void]$md.AppendLine('**Chave automática:** ' + $(if (-not $checks.Count) { '(esta bateria ainda não tem chave automática; conferência pela chave abaixo)' } elseif ($row.keyFailures.Count) { 'FALHA — ' + ($row.keyFailures -join '; ') } else { 'ok' })); [void]$md.AppendLine()
    [void]$md.AppendLine('**Chave (fatos a preservar):**'); foreach ($k in $c.key) { [void]$md.AppendLine($k) }; [void]$md.AppendLine()
    $rows | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'resultados.json')
    [IO.File]::WriteAllText((Join-Path $PSScriptRoot 'RESULTADOS.md'), $md.ToString(), (New-Object Text.UTF8Encoding $false))
}
$after = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash | ForEach-Object Hash)
Write-Output ('motor inalterado: ' + (-not (Compare-Object $before $after)))
Write-Output ("chave automática: " + $(if ($keyFailures) { "$keyFailures falha(s)" } else { 'todas ok' }) + $(if ($approved.Count) { "; diferentes do aprovado: $changed" } else { '' }))
