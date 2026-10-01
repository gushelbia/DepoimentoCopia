# Gênero do depoente (rodada 7): conversão para o feminino na interface, alertas com
# «a depoente» e sugestão do campo. Frases inventadas.
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/gender/Test-GenderConverter.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -TypeDefinition (Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Conv($src, $out, $expected, $name) { $r = [GenderConverter]::ToFeminine($src, $out); Check ($r -eq $expected) $name $r }
function Doubts($src, $out) { @([GenderConverter]::Doubts($src, [GenderConverter]::ToFeminine($src, $out)) | ForEach-Object { $_.Text }) }

# Conversão: só o que se refere à depoente.
Conv 'Eu fiquei nervosa e liguei para o meu irmão.' 'O depoente relatou que ficou nervoso e ligou para o seu irmão.' 'A depoente relatou que ficou nervosa e ligou para o seu irmão.' 'adjetivo da depoente vai para o feminino'
Conv 'Fui agredida pelo meu ex-companheiro.' 'O depoente relatou que foi agredido pelo seu ex-companheiro.' 'A depoente relatou que foi agredida pelo seu ex-companheiro.' 'particípio da depoente'
Conv 'Ele me empurrou e eu caí sentada no chão.' 'O depoente relatou que foi empurrado e caiu sentada no chão.' 'A depoente relatou que foi empurrada e caiu sentada no chão.' 'regra F9: particípio depois de «foi»'
Conv 'Eu estava grávida quando ele me ameaçou.' 'O depoente relatou que estava grávida quando ele o ameaçou.' 'A depoente relatou que estava grávida quando ele a ameaçou.' 'pronome «o» vindo de «me» vira «a»'
Conv 'Ela me disse que ia embora.' 'O depoente relatou que ela disse ao depoente que ia embora.' 'A depoente relatou que ela disse à depoente que ia embora.' 'contração «ao» → «à»'
Conv 'O Carlos falou comigo.' 'O depoente relatou que o Carlos falou com o depoente.' 'A depoente relatou que o Carlos falou com a depoente.' '«com o depoente» → «com a depoente»'
Conv 'Pegaram minha bolsa.' 'O depoente relatou que pegaram a bolsa do depoente.' 'A depoente relatou que pegaram a bolsa da depoente.' '«do depoente» → «da depoente»'
Conv 'Eu fiquei nervosa e ele estava nervoso também.' 'O depoente relatou que ficou nervoso e ele estava nervoso também.' 'A depoente relatou que ficou nervosa e ele estava nervoso também.' 'termo de outra pessoa («ele estava nervoso») não muda'
Conv 'Fiquei assustada. O Carlos ficou assustado também.' 'O depoente relatou que ficou assustado. O Carlos ficou assustado também.' 'A depoente relatou que ficou assustada. O Carlos ficou assustado também.' 'nome próprio como sujeito: termo dele não muda'
Conv 'Eu estava sozinha e o vizinho estava sozinho.' 'O depoente relatou que estava sozinho e o vizinho estava sozinho.' 'A depoente relatou que estava sozinha e o vizinho estava sozinho.' '«o vizinho estava sozinho» não muda'
Conv 'Ela gritou: "estou cansado". Eu fiquei calada.' 'O depoente relatou que ela gritou: "estou cansado". O depoente ficou calado.' 'A depoente relatou que ela gritou: "estou cansado". A depoente ficou calada.' 'fala entre aspas preservada byte a byte'
Conv 'Eu sou a mãe do menino.' 'A depoente relatou que é a mãe do menino.' 'A depoente relatou que é a mãe do menino.' 'texto já no feminino: sem mudança'
$once = [GenderConverter]::ToFeminine('Eu fiquei nervosa.', 'O depoente relatou que ficou nervoso.')
Check ([GenderConverter]::ToFeminine('Eu fiquei nervosa.', $once) -eq $once) 'converter duas vezes não muda (idempotente)' $once
Conv 'Eu fiquei nervosa.' 'O depoente relatou que, depois que o filho saiu, ficou nervoso.' 'A depoente relatou que, depois que o filho saiu, ficou nervoso.' 'na dúvida (outra pessoa no meio) não converte'
$dz = Doubts 'Eu fiquei nervosa.' 'O depoente relatou que, depois que o filho saiu, ficou nervoso.'
Check ($dz -contains 'nervoso') 'na dúvida aparece aviso informativo' ($dz -join ', ')
$dz = Doubts 'O Carlos entrou depois de mim.' 'O depoente relatou que o Carlos entrou depois dele.'
Check ($dz -contains 'dele') '«dele» para «de mim»: aviso informativo, sem conversão' ($dz -join ', ')
$dz = Doubts 'Eu fiquei nervosa e ele estava nervoso também.' 'O depoente relatou que ficou nervoso e ele estava nervoso também.'
Check ($dz.Count -eq 0) 'termo claramente de outra pessoa: sem aviso' ($dz -join ', ')

# Na dúvida não converte (bateria de gênero: o modelo perdeu o sujeito de outra pessoa).
Conv 'Ele estava nervoso e eu fiquei calada.' 'O depoente relatou que estava nervoso e o depoente ficou calado.' 'A depoente relatou que estava nervoso e a depoente ficou calada.' 'F13: «nervoso» é do «ele» no original: não vira «nervosa»'
$dz = Doubts 'Ele estava nervoso e eu fiquei calada.' 'O depoente relatou que estava nervoso e o depoente ficou calado.'
Check ($dz -contains 'nervoso') 'F13: aviso informativo em «nervoso»' ($dz -join ', ')
Conv 'Ele chegou nervoso e começou a gritar comigo.' 'O depoente relatou que chegou nervoso e começou a gritar com o depoente.' 'A depoente relatou que chegou nervoso e começou a gritar com a depoente.' 'N1: «chegou nervoso» (do «ele») não muda'
Conv 'O porteiro ficou calado e não quis abrir o portão.' 'O depoente relatou que ficou calado e não quis abrir o portão.' 'A depoente relatou que ficou calado e não quis abrir o portão.' 'N5: «calado» (do porteiro) não muda'
Conv 'Minha vizinha estava assustada e bateu na minha porta.' 'O depoente relatou que estava assustado e bateu na porta da vizinha.' 'A depoente relatou que estava assustado e bateu na porta da vizinha.' 'N3: «a vizinha estava assustada» não é palavra da depoente'
Check (([GenderConverter]::NarratorFeminineWords('Minha vizinha estava assustada.')).Count -eq 0) '«estava» sem «eu» não é primeira pessoa' ''
Check (([GenderConverter]::NarratorFeminineWords('Eu estava assustada.')) -contains 'assustada') '«eu estava assustada» é da depoente' ''

# Alertas com «a depoente» e com o campo de gênero.
function Kinds($o, $r, $g) { @([RoleScanner]::Find($o, $r, $g) | ForEach-Object { ($(if ($_.Kind -eq [RoleScanner]::Note) {'aviso:'} else {'laranja:'}) + ($_.Reason -split '[:(]')[0].Trim().ToLowerInvariant()) } | Sort-Object -Unique) }
$k = Kinds 'Ele me disse que viria.' 'A depoente relatou que a depoente lhe disse que viria.' 'Feminino'
Check ($k -contains 'laranja:confira quem fez ou disse') 'inversão marcada com «a depoente»' ($k -join ' | ')
$k = Kinds 'Eu empurrei ele porque ele estava me enforcando.' 'A depoente relatou que empurrou ele porque ele estava a depoente enforcando.' 'Feminino'
Check ($k -contains 'laranja:frase quebrada') 'frase quebrada marcada com «a depoente»' ($k -join ' | ')
$k = Kinds 'Eu fiquei nervosa.' 'A depoente relatou que ficou nervosa.' 'Feminino'
Check ($k.Count -eq 0) 'texto convertido correto: nenhum alerta nem aviso' ($k -join ' | ')
$k = Kinds 'Eu fiquei nervosa.' 'O depoente relatou que ficou nervoso.' 'Feminino'
Check ($k -contains 'laranja:gênero trocado') 'campo Feminino com texto no masculino: laranja' ($k -join ' | ')
$k = Kinds 'Eu cheguei cedo.' 'O depoente relatou que chegou cedo.' 'Masculino'
Check (-not ($k -match 'presumido')) 'campo Masculino: sem aviso de gênero presumido' ($k -join ' | ')
$k = Kinds 'Eu cheguei cedo.' 'O depoente relatou que chegou cedo.' ''
Check ($k -contains 'aviso:gênero presumido') 'campo Não informado: aviso de gênero presumido continua' ($k -join ' | ')
$k = Kinds 'Eu fiquei nervosa e ele estava nervoso também.' 'O depoente relatou que ficou nervosa e ele estava nervoso também.' ''
Check (-not ($k -match 'gênero trocado: no original')) 'masculino de outra pessoa não vira alerta de gênero trocado' ($k -join ' | ')
$k = Kinds 'Acho que foi a Maria, mas posso estar enganado.' 'A depoente relatou que acredita que foi a Maria, mas pode estar enganada.' 'Feminino'
Check ($k -contains 'aviso:gênero') 'campo Feminino mas original indica homem: aviso para conferir o campo' ($k -join ' | ')

# Sugestão do campo: só com pista clara.
Check ([RoleScanner]::FirstFeminine('Eu fiquei nervosa e liguei para o irmão.') -eq 'nervosa' -and $null -eq [RoleScanner]::FirstMasculine('Eu fiquei nervosa e liguei para o irmão.')) 'pista clara de mulher («fiquei nervosa»)' ''
Check ($null -eq [RoleScanner]::FirstFeminine('A porta estava fechada quando cheguei.') -and $null -eq [RoleScanner]::FirstMasculine('A porta estava fechada quando cheguei.')) 'sem pista («a porta estava fechada»): nenhuma sugestão' ''
Check ($null -eq [RoleScanner]::FirstFeminine('Ele estava nervoso.')) '«ele estava nervoso» não é pista da depoente' ''

# Texto de homens e de gênero não informado: a conversão nunca roda (identidade exata).
$same = 0; $total = 0
foreach ($c in (Get-Content -Raw -Encoding UTF8 (Join-Path $root 'maintenance/pronoun-diagnosis/resultados-depois.json') | ConvertFrom-Json)) {
    $total++
    if (([GenderConverter]::ForGender('Masculino', $c.input, $c.output) -ceq $c.output) -and ([GenderConverter]::ForGender('', $c.input, $c.output) -ceq $c.output)) { $same++ }
}
Check ($total -eq 20 -and $same -eq 20) '20 frases aprovadas: Masculino e Não informado idênticos' "$same/$total"

$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'converter-results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
