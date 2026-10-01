# Bateria de gênero: roda as frases inventadas no motor real (mesmo caminho do
# botão Reformular, QA/QaRunner.cs) e aplica o que a tela mostra para o campo
# Gênero do depoente (GenderConverter.ForGender, o mesmo código da janela).
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/gender/Run-GenderBattery.ps1
# F = Feminino: nenhuma forma masculina da depoente, nenhum alerta laranja de gênero.
# M = Masculino e N = Não informado: texto da tela idêntico ao do motor.
# Também confere as 20 frases aprovadas (pronoun-diagnosis/resultados-depois.json)
# e as 10 saídas aprovadas da QA (QA/Results): com Masculino e Não informado, nada muda.
param([int]$TimeoutSeconds = 600, [string]$Only)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$runner = [IO.File]::ReadAllText((Join-Path $root 'QA/QaRunner.cs'))
Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = Join-Path $root 'engine'
$failures = 0
$report = Join-Path $PSScriptRoot 'bateria-resultados.txt'
Set-Content -Encoding UTF8 $report ('START ' + (Get-Date -Format s))
function Log($m) { Write-Output $m; Add-Content -Encoding UTF8 $report $m }
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; Log (($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''}))) }
function FieldOf($id) { switch ($id.Substring(0,1)) { 'F' { 'Feminino' } 'M' { 'Masculino' } default { '' } } }
function Alerts($o, $r, $g) { @([RoleScanner]::Find($o, $r, $g) | ForEach-Object { $(if ($_.Kind -eq [RoleScanner]::Note) {'aviso: '} else {'LARANJA: '}) + $_.Reason + ' [' + $_.Text + ']' }) }
$masc = '\b(?:o|do|ao|no|pelo)\s+depoente\b'

# 1. Textos aprovados: Masculino e Não informado não mudam nada.
$approved = @()
foreach ($r in (Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-depois.json') | ConvertFrom-Json)) { $approved += [pscustomobject]@{ name = "frase $($r.number)"; input = $r.input; output = $r.output } }
foreach ($n in 1..10) { $q = Get-Content -Raw -Encoding UTF8 (Join-Path $root ('QA/Results/test-{0:D2}.json' -f $n)) | ConvertFrom-Json; $approved += [pscustomobject]@{ name = "QA $n"; input = $q.input; output = $q.output } }
$same = 0
foreach ($a in $approved) {
    if ((([GenderConverter]::ForGender('Masculino', $a.input, $a.output)) -ceq $a.output) -and (([GenderConverter]::ForGender('', $a.input, $a.output)) -ceq $a.output)) { $same++ } else { Log "DIFERENTE: $($a.name)" }
}
Check ($same -eq $approved.Count) 'textos aprovados (20 frases + 10 QA) idênticos com Masculino e Não informado' "$same/$($approved.Count)"

# 2. Bateria nova no motor.
$cases = @(Get-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'frases-genero.tsv') | Where-Object { $_ -and -not $_.StartsWith('#') } | ForEach-Object { $p = $_ -split "`t"; [pscustomobject]@{ id = $p[0]; pending = $p[1]; input = $p[2] } })
if ($Only) { $wanted = @($Only -split '[,; ]+'); $cases = @($cases | Where-Object { $wanted -contains $_.id }) }
$rows = @()
foreach ($c in $cases) {
    $g = [QaRunner]::Run($root, $c.input, $TimeoutSeconds)
    $field = FieldOf $c.id
    $shown = [GenderConverter]::ForGender($field, $c.input, $g.Output)
    $alerts = Alerts $c.input $shown $field
    $row = [ordered]@{ id = $c.id; field = $field; pending = $c.pending; input = $c.input; engine = $g.Output; shown = $shown; alerts = $alerts; completed = $g.CompletedNormally; asFeminine = $null }
    Log ''
    Log ('{0} [{1}]{2}  {3}' -f $c.id, $(if ($field) { $field } else { 'Não informado' }), $(if ($c.pending -eq 'pendente-papel') { ' (PENDENTE: papéis trocados pelo modelo)' } elseif ($c.pending -eq 'pendente-sujeito') { ' (PENDENTE: sujeito de outra pessoa apagado pelo modelo)' } else { '' }), $c.input)
    Log ('   motor: ' + $g.Output)
    if ($shown -cne $g.Output) { Log ('   tela:  ' + $shown) }
    foreach ($a in $alerts) { Log ('   ' + $a) }
    Check $g.CompletedNormally "$($c.id): geração concluída" $null
    if ($field -eq 'Feminino') {
        Check (-not [regex]::IsMatch($shown, $masc, 'IgnoreCase')) "$($c.id): nenhum «o/do/ao/no/pelo depoente» na tela" $null
        $gender = @($alerts | Where-Object { $_ -match '^LARANJA: .*g[êe]nero' })
        Check ($gender.Count -eq 0) "$($c.id): nenhum alerta laranja de gênero" ($gender -join '; ')
    } else {
        Check ($shown -ceq $g.Output) "$($c.id): texto da tela idêntico ao do motor" $null
        $row.asFeminine = [GenderConverter]::ForGender('Feminino', $c.input, $g.Output)
        if ($c.id.StartsWith('N')) { Log ('   (se o campo fosse Feminino: ' + $row.asFeminine + ')') }
    }
    $rows += [pscustomobject]$row
    $rows | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'bateria-resultados.json')
}
Log ''
Log $(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" })
if ($failures -ne 0) { exit 1 }
