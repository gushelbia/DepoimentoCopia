# Alertas de papéis e pronomes (laranja, «conferir») — só na interface; o motor não muda.
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/role-alerts/Test-RoleAlerts.ps1
# Usa as 20 frases inventadas de maintenance/pronoun-diagnosis (saídas reais do modelo 3B),
# versões corrigidas (sem falso alarme), citações e as 10 saídas aprovadas da bateria QA.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -TypeDefinition (Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Kinds($original, $output) {
    $k = @()
    foreach ($i in [RoleScanner]::Find($original, $output)) {
        foreach ($part in ($i.Reason -split ' Também: ')) {
            $p = $part.ToLowerInvariant()
            if ($p.StartsWith('confira quem')) { $k += 'inversão' }
            elseif ($p.StartsWith('o original é reflexivo')) { $k += 'reflexivo' }
            elseif ($p.StartsWith('frase quebrada')) { $k += 'quebrada' }
            elseif ($p.StartsWith('«lhe» com verbo')) { $k += 'lhe' }
            elseif ($p.StartsWith('pronome ambíguo')) { $k += 'ambíguo' }
            elseif ($p.StartsWith('sujeito')) { $k += 'sujeito' }
            elseif ($p.StartsWith('gênero trocado')) { $k += 'gênero trocado' }
            elseif ($p.StartsWith('gênero presumido')) { $k += 'gênero presumido' }
            else { $k += '?' }
        }
    }
    return @($k | Sort-Object -Unique)
}
function Expect($name, $original, $output, [string[]]$expected) {
    $got = Kinds $original $output
    $exp = @($expected | Sort-Object -Unique)
    Check ((($got -join ',') -eq ($exp -join ','))) $name ("esperado: " + ($exp -join ', ') + " | obtido: " + ($got -join ', '))
}

# 1) As 20 frases da bateria (saídas reais do 3B).
$expected = @{
  1=@('sujeito','gênero presumido','ambíguo'); 2=@('sujeito','gênero presumido','ambíguo'); 3=@('inversão','sujeito','gênero presumido');
  4=@('gênero presumido','ambíguo'); 5=@('sujeito','gênero presumido','quebrada'); 6=@('lhe','gênero presumido'); 7=@('inversão','sujeito','gênero presumido');
  8=@('lhe','gênero presumido'); 9=@('sujeito','gênero presumido'); 10=@('inversão','sujeito','gênero presumido'); 11=@('sujeito','gênero trocado','ambíguo');
  12=@('sujeito','gênero trocado'); 13=@('sujeito'); 14=@('lhe','gênero presumido'); 15=@('sujeito','gênero presumido'); 16=@('sujeito','gênero presumido');
  17=@('sujeito','gênero presumido'); 18=@('lhe','reflexivo','gênero presumido'); 19=@('sujeito','gênero trocado'); 20=@('sujeito') }
$battery = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-20-frases.json') | ConvertFrom-Json
foreach ($c in $battery) {
    $out = ($c.output -split "`n" | Where-Object { $_ -match '^Relatou' } | Select-Object -First 1)
    Expect ("bateria {0}: {1}" -f $c.number, $c.input) $c.input $out $expected[[int]$c.number]
}
# Os erros graves da bateria têm alerta (nenhum dos 8 com erro de sentido passa sem marca).
$serious = 0
foreach ($n in 1,3,4,5,7,10,11,18) { $c = $battery | Where-Object number -eq $n; $out = ($c.output -split "`n" | Where-Object { $_ -match '^Relatou' } | Select-Object -First 1); $k = Kinds $c.input $out; if ($k | Where-Object { $_ -in 'inversão','reflexivo','quebrada','ambíguo' }) { $serious++ } }
Check ($serious -eq 8) 'os 8 casos com erro de sentido na bateria recebem alerta de sentido' "$serious de 8"

# 2) Versões corretas: sem falso alarme do tipo corrigido.
Expect 'corrigido 1: voz passiva, sujeito explícito' 'Eu estava correndo na praia quando ele me atacou por trás.' 'O depoente relatou que estava correndo na praia quando foi atacado por trás por ele.' @('gênero presumido')
Expect 'corrigido 7: «ele lhe disse» é o certo' 'Ele me disse que tinha visto ela sair com o carro.' 'Relatou que ele lhe disse que tinha visto ela sair com o carro.' @()
Expect 'corrigido 10: «ela lhe contou»' 'Ela me contou que ele tinha batido nela na noite anterior.' 'Relatou que ela lhe contou que ele tinha batido nela na noite anterior.' @()
Expect 'corrigido 18: «atacou o depoente» e «se defendeu»' 'Ela me atacou primeiro e eu só me defendi.' 'Relatou que ela atacou o depoente primeiro e o depoente só se defendeu.' @('gênero presumido')
Expect 'corrigido 5: «enforcando o depoente»' 'Eu empurrei ele porque ele estava me enforcando.' 'O depoente relatou que empurrou ele porque ele estava enforcando o depoente.' @('gênero presumido')
Expect 'corrigido 12: «a depoente … nervosa»' 'Eu fiquei nervosa e liguei para o meu irmão.' 'Relatou que a depoente ficou nervosa e ligou para o seu irmão.' @('sujeito')
Expect 'corrigido 19: «a depoente foi agredida»' 'Fui agredida pelo meu ex-companheiro na frente dos meus filhos.' 'A depoente relatou que foi agredida pelo seu ex-companheiro na frente dos seus filhos.' @()
Expect 'homem explícito («sozinho»): sem alerta de gênero' 'Eu estava sozinho em casa.' 'O depoente relatou que estava sozinho em casa.' @()
Expect '«lhe deu» é destinatário normal' 'Ele me deu uma rasteira.' 'Relatou que ele lhe deu uma rasteira.' @()
Expect 'sem «me» no original: «lhe» não é marcado' 'O gerente entregou a chave ao porteiro e depois o chamou.' 'O gerente entregou a chave ao porteiro e depois lhe telefonou.' @()
Expect 'fala entre aspas não é marcada' 'Eu ouvi ela gritar: “o depoente lhe bateu”.' 'O depoente relatou que ouviu ela gritar: “o depoente lhe bateu”.' @('gênero presumido')
Expect 'textos vazios: nenhum alerta' '' '' @()
Expect 'texto longo com outro «lhe»: inversão ainda é marcada' 'O gerente lhe entregou a chave antes. Depois ele me disse que tinha visto ela sair com o carro.' 'O gerente lhe entregou a chave antes. Depois o depoente lhe disse que tinha visto ela sair com o carro.' @('inversão','gênero presumido')
Expect '«posso estar enganado» indica homem: sem gênero presumido' 'Acho que foi a Maria, mas posso estar enganado.' 'O depoente acredita que foi a Maria, mas pode estar enganado.' @()
Expect '«posso estar enganada» indica mulher' 'Acho que foi o João, mas posso estar enganada.' 'O depoente acredita que foi o João, mas pode estar enganado.' @('gênero trocado')

# 3) Saídas aprovadas da bateria QA (10 casos): nenhum alerta de sentido (inversão, reflexivo, frase quebrada, «lhe»).
foreach ($n in 1..10) {
    $j = Get-Content -Raw -Encoding UTF8 (Join-Path $root ('QA/Results/test-{0:D2}.json' -f $n)) | ConvertFrom-Json
    $k = Kinds $j.input $j.output
    $bad = @($k | Where-Object { $_ -notin 'sujeito','gênero presumido' })
    Check ($bad.Count -eq 0) ("QA {0}: sem falso alarme de sentido" -f $n) ("alertas: " + ($k -join ', '))
}

# 4) Resumo e lista.
$r = [ReviewScanner]::Compare('Ele me disse que tinha visto ela sair com o carro.', 'Relatou que o depoente lhe disse que tinha visto ela sair com o carro.')
Check ([ReviewScanner]::Summary($r) -eq '1 alerta de papéis e pronomes; 1 aviso informativo.') 'resumo: alertas e avisos contados separados' ([ReviewScanner]::Summary($r))
$det = [ReviewScanner]::Details($r)
Check ($det.StartsWith('Papéis e pronomes') -and $det.Contains('Confira quem fez ou disse') -and $det.Contains('Avisos informativos') -and $det.IndexOf('Avisos informativos') -gt $det.IndexOf('Papéis e pronomes')) 'lista ao clicar: alertas primeiro, avisos informativos em seção própria' ($det -split "`n")[1]
Check ($r.Roles[0].Alert -and -not $r.Roles[0].Unmatched) 'alerta de papel usa a cor laranja (alerta), nunca a vermelha' ''
Check (-not $r.Notes[0].Alert -and -not $r.Notes[0].Unmatched) 'aviso informativo não tem cor' ''
$paintedBad = 0; $notesBad = 0
foreach ($c in $battery) { $out = ($c.output -split "`n" | Where-Object { $_ -match '^Relatou' } | Select-Object -First 1); $rr = [ReviewScanner]::Compare($c.input, $out)
  foreach ($i in $rr.Roles) { if ($i.Reason -match '^(sujeito|gênero presumido)' -or -not $i.Alert) { $paintedBad++ } }
  foreach ($i in $rr.Notes) { if ($i.Alert -or $i.Reason -notmatch '^(sujeito|gênero presumido)') { $notesBad++ } } }
Check ($paintedBad -eq 0) 'bateria: laranja só para inversão, reflexivo, frase quebrada, «lhe», pronome ambíguo e gênero trocado' ("$paintedBad itens indevidos")
Check ($notesBad -eq 0) 'bateria: sujeito e gênero presumido ficam só como aviso informativo' ("$notesBad itens indevidos")

foreach ($n in 11,12,19) { $c = $battery | Where-Object number -eq $n; $out = ($c.output -split "`n" | Where-Object { $_ -match '^Relatou' } | Select-Object -First 1); $rr = [ReviewScanner]::Compare($c.input, $out)
  $swapped = @($rr.Roles | Where-Object { $_.Alert -and $_.Reason -match 'gênero trocado' })
  Check ($swapped.Count -ge 1 -and -not ($rr.Notes | Where-Object { $_.Reason -match 'gênero trocado' })) ("bateria {0}: gênero trocado pintado de laranja (contradiz o original)" -f $n) (($swapped | ForEach-Object { $_.Text }) -join ', ') }
$rr = [ReviewScanner]::Compare('Eu fiquei nervosa e liguei para o meu irmão.', 'Relatou que o depoente ficou nervoso e ligou para o seu irmão.')
Check (@($rr.Roles | Where-Object { $_.Text -eq 'nervoso' }).Count -eq 1 -and @($rr.Notes | Where-Object { $_.Reason -match '^sujeito' }).Count -eq 1) '«nervoso» em laranja; «Relatou que» continua aviso sem cor' ''
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }