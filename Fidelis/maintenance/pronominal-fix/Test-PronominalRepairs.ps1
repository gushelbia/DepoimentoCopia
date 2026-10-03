# Rodada 9 (bateria realista 2): verbos pronominais do depoente, primeira pessoa que
# sobrou, possessivo com outra pessoa na frase e destinatário perdido.
#   pwsh -NoProfile -File maintenance/pronominal-fix/Test-PronominalRepairs.ps1
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

# 1) Verbos pronominais do depoente: «me» vira «se».
Repair 'Eu falei que não me sentia confortável.' 'Relatou que o depoente falou que não lhe sentia confortável.' 'Relatou que o depoente falou que não se sentia confortável.' 'B09: «não lhe sentia» → «não se sentia»'
Repair 'Eu me arrependo de não ter falado antes.' 'Relatou que o depoente lhe arrependo de não ter falado antes.' 'Relatou que o depoente se arrepende de não ter falado antes.' 'B09: «o depoente lhe arrependo» → «se arrepende»'
Repair 'Eu me arrependo de não ter falado antes.' 'Relatou que o depoente arrependo o depoente de não ter falado antes.' 'Relatou que o depoente se arrepende de não ter falado antes.' 'B09 (forma da rodada 8): «arrependo o depoente» → «se arrepende»'
Repair 'Eu me lembro que fiquei sem dormir.' 'Relatou que o depoente lhe lembra que ficou sem dormir.' 'Relatou que o depoente se lembra que ficou sem dormir.' 'B01: «lhe lembra» → «se lembra»'
Repair 'Ele falou que eu ia me arrepender.' 'Relatou que ele falou que o depoente ia lhe arrepender.' 'Relatou que ele falou que o depoente ia se arrepender.' 'B04: «ia lhe arrepender» → «ia se arrepender»'
NoIssue 'Eu pedi porque eu não me sinto bem.' 'O depoente relatou que pediu porque ele não se sente bem.' 'B05: «não me sinto» é do depoente: sem falso alarme'
NoIssue 'Eu me machuquei no joelho.' 'O depoente relatou que se machucou no joelho.' '«me machuquei» → «se machucou»: sem acusação'
Issue 'Eu me arrependo de não ter falado.' 'O depoente relatou que o depoente lhe arrepende de não ter falado.' 'verbo pronominal do depoente sem «se»' 'pronominal com «lhe» que ficou: validador acusa'
# Não é pronominal do depoente (é outra pessoa agindo sobre ele).
Repair 'O professor Heitor me chamou na sala.' 'Relatou que o professor Heitor lhe chamou na sala.' 'Relatou que o professor Heitor chamou o depoente na sala.' '«Heitor me chamou»: outra pessoa → «chamou o depoente» (não «se chamou»)'
Repair 'Ele me disse que já tinha me dado o dinheiro.' 'Relatou que ele lhe disse que já tinha lhe dado o dinheiro.' 'Relatou que ele lhe disse que já tinha lhe dado o dinheiro.' '«ele me disse»: fica «lhe disse»'
Repair 'Eu parei e pedi que eles me respeitassem.' 'Relatou que o depoente parou e pediu que eles respeitassem o depoente.' 'Relatou que o depoente parou e pediu que eles respeitassem o depoente.' '«eles me respeitassem»: não vira «se respeitassem»'
NoIssue 'Depois algumas pessoas me disseram que houve reunião.' 'O depoente relatou que depois algumas pessoas lhe disseram que houve reunião.' '«pessoas me disseram»: sem falso alarme'

# 2) Primeira pessoa fora das aspas.
Repair 'Eu não sabia, juro.' 'Relatou que o depoente não sabia, juro.' 'Relatou que o depoente não sabia.' 'B01: «juro» isolado entre vírgulas removido'
Issue 'Eu juro que não sabia.' 'O depoente relatou que juro que não sabia.' 'primeira pessoa fora das aspas' '«juro que…» (não isolado): validador acusa'
Issue 'Eu nunca falei com ele, eu nem conheço ele.' 'O depoente relatou que nunca falou com ele, nem conheço ele.' 'primeira pessoa fora das aspas' 'B01: «nem conheço» acusado'
NoIssue 'Ela gritou: "eu juro que não fui".' 'O depoente relatou que ela gritou: "eu juro que não fui".' '«juro» entre aspas: sem acusação'
NoIssue 'Eu trabalho na portaria.' 'O depoente relatou que trabalha na portaria e o trabalho é cansativo.' '«o trabalho» (substantivo): sem acusação'

# 3) Possessivo com outra pessoa na frase.
Repair 'O Bruno disse que achou que era meu namorado.' 'Relatou que o Bruno disse que o depoente achou que era seu namorado.' 'Relatou que o Bruno disse que o depoente achou que era o namorado do depoente.' 'B03: «seu namorado» com o Bruno na frase → «o namorado do depoente»'
Repair 'Ela ia fazer da minha vida um inferno.' 'Relatou que ela disse ao depoente que ia fazer da sua vida um inferno.' 'Relatou que ela disse ao depoente que ia fazer da vida do depoente um inferno.' 'B05: «da sua vida» com «ela» → «da vida do depoente»'
Repair 'Eu contei pra minha colega Bianca.' 'Relatou que o depoente contou para sua colega Bianca.' 'Relatou que o depoente contou para sua colega Bianca.' '«sua colega Bianca»: o nome já identifica: sem mudança'
Repair 'Eu fui pra minha mesa.' 'Relatou que o depoente foi para sua mesa.' 'Relatou que o depoente foi para sua mesa.' 'só o depoente na frase: «sua mesa» fica'

# 4) Destinatário perdido.
Repair 'Tinham ligado pra ela do meu celular.' 'Relatou que tinham ligado do celular do depoente.' 'Relatou que tinham ligado para ela do celular do depoente.' 'B08: «ligado pra ela» devolvido pelo original'
Repair 'O banco disse pra ela que ia estornar.' 'Relatou que o banco disse que ia estornar.' 'Relatou que o banco disse para ela que ia estornar.' 'B08: «disse pra ela» devolvido pelo original'
Issue 'Ele disse pra ela que sim e depois disse pra ela que não.' 'O depoente relatou que ele disse que sim e depois disse que não.' 'destinatário perdido' 'duas frases iguais no original: não adivinha, validador acusa'
Issue 'O banco disse pra ela que ia estornar.' 'O depoente relatou que o banco disse que ia estornar.' 'destinatário perdido' 'B08: «disse pra ela» perdido: acusado'
NoIssue 'O banco disse pra ela que ia estornar.' 'O depoente relatou que o banco disse à mãe que ia estornar.' 'destinatário mantido («disse à mãe»): sem acusação'
NoIssue 'Ouvi a Fernanda dizer para ele ficar tranquilo.' 'O depoente relatou que ouviu a Fernanda dizer para ele ficar tranquilo.' '«para ele ficar» (finalidade): fora desta regra'

# 5) Regência com «-aram».
Repair 'Eu fui na segurança e eles me mostraram a câmera.' 'Relatou que o depoente foi na segurança e eles o mostraram a câmera.' 'Relatou que o depoente foi na segurança e eles lhe mostraram a câmera.' 'B03: «eles o mostraram» → «eles lhe mostraram»'

Issue 'Eu vi a Ana sair.' '' 'saída vazia' 'saída vazia continua rejeitada'
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
