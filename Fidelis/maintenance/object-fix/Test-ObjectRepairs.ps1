# Rodada 8 (bateria realista): o depoente como objeto e dono, valores por extenso,
# frase quebrada por repetição e regência. Reparos no motor e o que o validador acusa.
#   pwsh -NoProfile -File maintenance/object-fix/Test-ObjectRepairs.ps1
# Usa SemanticGuard.compiled.dll (a mesma lógica embutida no motor). Frases inventadas;
# a entrada é o texto interno do motor antes do reparo final («Relatou que …»).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/semantic-fix/SemanticGuard.compiled.dll')
$G = [DepoimentoLocal.Windows.SemanticGuard]
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Repair($src, $out, $expected, $name) { $r = $G::RepairSourceAnchors($src, $out); $e = $G::FinalLead($expected); Check ($r -ceq $e) $name $r }
function Issue($src, $out, $kind, $name) { $i = $G::Validate($src, $out); Check ([bool]($i -and $i.Contains($kind))) $name $i }
function NoIssue($src, $out, $name) { $i = $G::Validate($src, $out); Check ($null -eq $i) $name $i }

# 1) Depoente apagado ou trocado como objeto.
Repair 'Aí ele tirou uma faca da cintura e me ameaçou.' 'Relatou que aí ele tirou uma faca da cintura e ameaçou.' 'Relatou que aí ele tirou uma faca da cintura e ameaçou o depoente.' 'R05: objeto apagado («e ameaçou.») → «ameaçou o depoente»'
Repair 'Ele desceu e veio me xingando.' 'Relatou que ele desceu e veio se xingando.' 'Relatou que ele desceu e veio xingando o depoente.' 'R05: «veio se xingando» → «veio xingando o depoente»'
Repair 'A professora Cecília me aceitou como orientando.' 'Relatou que a professora Cecília aceitou como orientando.' 'Relatou que a professora Cecília aceitou o depoente como orientando.' 'R07: «aceitou como orientando» → «aceitou o depoente como orientando»'
Repair 'Um homem de moto parou e me perguntou as horas.' 'Relatou que um homem de moto parou e perguntou as horas.' 'Relatou que um homem de moto parou e lhe perguntou as horas.' 'R11: «perguntou as horas» → «lhe perguntou as horas» (objeto indireto)'
Repair 'Eu fiquei até tarde e ele me atacou na saída.' 'Relatou que o depoente ficou até tarde e lhe atacou na saída.' 'Relatou que o depoente ficou até tarde e ele atacou o depoente na saída.' 'N7: «e lhe atacou» → «e ele atacou o depoente» (sujeito e objeto do original)'
Issue 'O Diego me pediu duzentos reais emprestado em março.' 'O depoente relatou que recebeu duzentos reais emprestados do Diego em março.' 'depoente omitido ou trocado como objeto' 'R10: «me pediu» virou «recebeu … do Diego»: validador acusa'
Issue 'Ele me ameaçou e depois me ameaçou de novo.' 'O depoente relatou que ele ameaçou e depois ameaçou de novo.' 'depoente omitido' 'duas ocorrências sem o depoente: não adivinha, validador acusa'
Repair 'Ele me ameaçou e depois me ameaçou de novo.' 'Relatou que ele ameaçou e depois ameaçou de novo.' 'Relatou que ele ameaçou e depois ameaçou de novo.' 'duas ocorrências: o reparo não adivinha'
# Não deve agir nem acusar.
NoIssue 'Ele me empurrou e eu caí.' 'O depoente relatou que foi empurrado e caiu.' 'passiva «relatou que foi empurrado» conta como o depoente'
NoIssue 'Eles me cercaram na saída do bar.' 'O depoente relatou que foi cercado na saída do bar.' 'passiva «foi cercado» conta como o depoente'
NoIssue 'Uma moça me ajudou e me levou até o ambulatório.' 'O depoente relatou que uma moça ajudou o depoente e o levou até o ambulatório.' '«ajudou o depoente» e «o levou»: depoente presente'
Repair 'Uma moça me levou até o ambulatório.' 'Relatou que uma moça o levou até o ambulatório.' 'Relatou que uma moça o levou até o ambulatório.' '«levar» tem as duas regências: sem mudança'
NoIssue 'Eu me machuquei no joelho.' 'O depoente relatou que se machucou no joelho.' 'reflexivo do depoente («me machuquei» → «se machucou»): sem acusação'
NoIssue 'Eu estava me arrumando quando ele chegou.' 'O depoente relatou que estava se arrumando quando ele chegou.' 'gerúndio reflexivo do depoente («eu estava me arrumando»): sem acusação'
NoIssue 'Ela me disse que viria.' 'O depoente relatou que ela lhe disse que viria.' 'verbo de fala com «lhe»: sem acusação'
Repair 'O Tiago se levantou e me defendeu.' 'Relatou que o Tiago se levantou e o defendeu.' 'Relatou que o Tiago se levantou e o defendeu.' '«o defendeu» certo: não vira «lhe» («defendeu» não é «deu»)'

# 2) Possessivo do depoente.
Repair 'Meu orientador, o professor Wanderley, foi afastado. Eu fiquei sem orientador.' 'Relatou que o orientador, o professor Wanderley, foi afastado. O depoente ficou sem orientador.' 'Relatou que o orientador do depoente, o professor Wanderley, foi afastado. O depoente ficou sem orientador.' 'R07: «Meu orientador» → «o orientador do depoente»'
Repair 'Fiquei preocupada porque minha filha não atendia.' 'Relatou que o depoente ficou preocupado porque a filha não atendia.' 'Relatou que o depoente ficou preocupado porque a filha do depoente não atendia.' 'F8: «minha filha» → «a filha do depoente»'
Repair 'Minha filha e a filha dele brincavam.' 'Relatou que a filha e a filha dele brincavam.' 'Relatou que a filha e a filha dele brincavam.' 'outra «filha» no original: não adivinha'
Repair 'Quando eu fui olhar o celular ele puxou da minha mão e saiu.' 'Relatou que quando o depoente foi olhar o celular, ele puxou a mão do depoente e saiu.' 'Relatou que quando o depoente foi olhar o celular, ele puxou da mão do depoente e saiu.' 'R11: «puxou da minha mão» → «puxou da mão do depoente» (não «a mão»)'
Issue 'Ele puxou da minha mão e saiu.' 'O depoente relatou que ele puxou a mão do depoente e saiu.' 'papel do depoente trocado' 'R11 sem reparo: validador acusa'

# 3) Valores por extenso.
Repair 'Ele disse que já tinha me dado cento e cinquenta.' 'Relatou que ele lhe disse que já tinha pago cem e cinquenta.' 'Relatou que ele lhe disse que já tinha pago cento e cinquenta.' 'R10: «cem e cinquenta» → «cento e cinquenta»'
Repair 'Paguei cem reais.' 'Relatou que o depoente pagou cem reais.' 'Relatou que o depoente pagou cem reais.' '«cem reais» fica igual'

# 4) Frase quebrada por repetição.
Repair 'Mas fui eu que achei o processo.' 'Relatou que mas foi o depoente que o depoente achou o processo.' 'Relatou que mas foi o depoente que achou o processo.' 'R06: «foi o depoente que o depoente achou» → «foi o depoente que achou»'
Issue 'Fui eu que achei o processo.' 'O depoente relatou que o depoente que o depoente achou o processo.' 'frase quebrada por repetição' 'outra repetição (sem «foi»): validador acusa'
Repair 'Ela disse que foi a Ana que achou.' 'Relatou que ela disse que foi a Ana que achou.' 'Relatou que ela disse que foi a Ana que achou.' '«foi a Ana que achou» (sem repetição): sem mudança'
NoIssue 'Mas fui eu que achei o processo.' 'O depoente relatou que foi o depoente quem achou o processo.' 'versão correta: sem acusação'

# 5) Regência.
Repair 'O professor Heitor me chamou na sala dele.' 'Relatou que o professor Heitor lhe chamou na sala dele.' 'Relatou que o professor Heitor chamou o depoente na sala dele.' 'R01: «lhe chamou» → «chamou o depoente»'
Repair 'Ele disse que ia me reprovar.' 'Relatou que ele lhe disse que ia lhe reprovar.' 'Relatou que ele lhe disse que ia reprovar o depoente.'
Repair 'Ele me disse que se eu não fosse ele ia me reprovar.' 'Relatou que ele lhe disse que se o depoente não fosse ele ia lhe reprovar.' 'Relatou que ele lhe disse que se o depoente não fosse ele ia reprovar o depoente.' 'R01: «eu» de outra oração não torna «ia me reprovar» reflexivo'
NoIssue 'Eu fiquei me perguntando o motivo.' 'O depoente relatou que ficou se perguntando o motivo.' 'gerúndio reflexivo com auxiliar na 1ª pessoa («fiquei me perguntando»): sem acusação' 'R01: «ia lhe reprovar» → «ia reprovar o depoente»; «lhe disse» fica'
Repair 'A minha chefe, a Dona Rosângela, me humilhou na frente de todos.' 'Relatou que a chefe do depoente, a Dona Rosângela, lhe humilhou na frente de todos.' 'Relatou que a chefe do depoente, a Dona Rosângela, humilhou o depoente na frente de todos.' 'R06: «lhe humilhou» depois de vírgula → «humilhou o depoente»'
Repair 'No final ela me pediu pra mandar a ata.' 'Relatou que no final ela o pediu pra mandar a ata.' 'Relatou que no final ela lhe pediu pra mandar a ata.' 'R12: «o pediu» → «lhe pediu»'
Repair 'A aluna me procurou, ela que me pediu pra conversar.' 'Relatou que a aluna o procurou, ela que o pediu para conversar.' 'Relatou que a aluna o procurou, ela que lhe pediu para conversar.' 'R02: «o procurou» fica; «o pediu» → «lhe pediu»'
Repair 'O pedido dele foi negado. Ele me pediu ajuda.' 'Relatou que o pedido dele foi negado. Ele lhe pediu ajuda.' 'Relatou que o pedido dele foi negado. Ele lhe pediu ajuda.' '«o pedido» (substantivo) não vira «lhe»'

# Proteções anteriores continuam valendo.
Issue 'Eu vi a Ana sair.' '' 'saída vazia' 'saída vazia continua rejeitada'
$once = $G::RepairSourceAnchors('Ele me ameaçou e saiu.', 'Relatou que ele ameaçou e saiu.')
Check ($G::RepairSourceAnchors('Ele me ameaçou e saiu.', $once) -ceq $once) 'reparo idempotente' $once

$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
