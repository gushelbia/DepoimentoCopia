# Inversões de papel (rodada 7): reparos no motor e o que o validador acusa.
#   pwsh -NoProfile -File maintenance/role-fix/Test-RoleRepairs.ps1
# Usa SemanticGuard.compiled.dll (a mesma lógica embutida no motor). Frases inventadas.
# Os textos de entrada são o texto interno do motor antes do reparo final («Relatou que …»).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/semantic-fix/SemanticGuard.compiled.dll')
$G = [DepoimentoLocal.Windows.SemanticGuard]
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Repair($src, $out, $expected, $name) { $r = $G::RepairSourceAnchors($src, $out); $e = $G::FinalLead($expected); Check ($r -ceq $e) $name $r }
function Issue($src, $out, $kind, $name) { $i = $G::Validate($src, $out); Check ([bool]($i -and $i.Contains($kind))) $name $i }
function NoIssue($src, $out, $name) { $i = $G::Validate($src, $out); Check ($null -eq $i) $name $i }

# 1) Sujeito de outra pessoa apagado: devolve o sujeito do original.
Repair 'Ele estava nervoso e eu fiquei calada.' 'Relatou que estava nervoso e o depoente ficou calado.' 'Relatou que ele estava nervoso e o depoente ficou calado.' 'F13: devolve «ele»'
Repair 'Meu marido foi preso e eu fiquei sozinha com as crianças.' 'Relatou que foi preso e o depoente ficou sozinho com as crianças.' 'Relatou que o marido do depoente foi preso e o depoente ficou sozinho com as crianças.' 'F14: «meu marido» → «o marido do depoente»'
Repair 'Ela estava nervosa e eu fiquei calado.' 'Relatou que estava nervoso e o depoente ficou calado.' 'Relatou que ela estava nervosa e o depoente ficou calado.' 'M13: devolve «ela» e a concordância do original («nervosa»)'
Repair 'Minha esposa foi presa e eu fiquei sozinho com as crianças.' 'Relatou que foi presa e o depoente ficou sozinho com as crianças.' 'Relatou que a esposa do depoente foi presa e o depoente ficou sozinho com as crianças.' 'M14: «minha esposa» → «a esposa do depoente»'
Repair 'Ele chegou nervoso e começou a gritar comigo.' 'Relatou que chegou nervoso e começou a gritar com o depoente.' 'Relatou que ele chegou nervoso e começou a gritar com o depoente.' 'N1: devolve «ele»'
Repair 'O porteiro ficou calado e não quis abrir o portão.' 'Relatou que ficou calado e não quis abrir o portão.' 'Relatou que o porteiro ficou calado e não quis abrir o portão.' 'N5: devolve «o porteiro»'
Repair 'O porteiro ficou calado e eu fui embora.' 'Relatou que o depoente ficou calado e o depoente foi embora.' 'Relatou que o porteiro ficou calado e o depoente foi embora.' '«o depoente ficou calado» → «o porteiro ficou calado»'
# Não deve agir.
Repair 'Minha vizinha estava assustada e bateu na minha porta.' 'Relatou que estava assustado e bateu na porta da vizinha.' 'Relatou que estava assustado e bateu na porta da vizinha.' 'N3: a vizinha já aparece na frase (dupla inversão): não adivinha'
Issue 'Minha vizinha estava assustada e bateu na minha porta.' 'O depoente relatou que estava assustado e bateu na porta da vizinha.' 'sujeito de outra pessoa apagado' 'N3: validador acusa (nova tentativa)'
Repair 'O porteiro ficou calado e eu fiquei calado também.' 'Relatou que o porteiro ficou calado e o depoente ficou calado também.' 'Relatou que o porteiro ficou calado e o depoente ficou calado também.' 'depoente também faz a ação no original: sem mudança'
Repair 'Ele ficou calado. O porteiro ficou calado.' 'Relatou que ficou calado.' 'Relatou que ficou calado.' 'dois sujeitos com o mesmo predicado: não adivinha'
Repair 'Ele estava nervoso e eu fiquei calada.' 'Relatou que ele estava nervoso e o depoente ficou calado.' 'Relatou que ele estava nervoso e o depoente ficou calado.' 'texto já correto: sem mudança'
Repair 'Ela gritou: "ele foi preso".' 'Relatou que ela gritou: "ele foi preso".' 'Relatou que ela gritou: "ele foi preso".' 'fala entre aspas preservada'
NoIssue 'Ele estava nervoso e eu fiquei calada.' 'O depoente relatou que ele estava nervoso e o depoente ficou calado.' 'texto correto: validador não acusa'
Repair 'Ela pegou sua bolsa. Eu peguei minha bolsa.' 'Ela pegou sua bolsa. O depoente pegou sua bolsa.' 'Ela pegou sua bolsa. O depoente pegou sua bolsa.' 'depoente faz o mesmo verbo («peguei minha bolsa»): sem mudança'
NoIssue 'Ela pegou sua bolsa. Eu peguei minha bolsa.' 'Ela pegou sua bolsa. O depoente pegou sua bolsa.' 'depoente faz o mesmo verbo: validador não acusa'

# 2) Papéis trocados.
Repair 'O Carlos entrou depois de mim e ficou me encarando.' 'Relatou que entrou depois do Carlos e ficou lhe encarando.' 'Relatou que o Carlos entrou depois do depoente e ficou encarando o depoente.' 'F7: «depois de mim» e «me encarando» restaurados'
Repair 'Ele disse que eu era culpada pelo que aconteceu.' 'Relatou que disse que ele era culpado pelo que aconteceu.' 'Relatou que ele disse que o depoente era culpado pelo que aconteceu.' 'F5: «eu era culpada» → «o depoente era culpado» e sujeito de «disse» devolvido'
Repair 'Ele disse que eu era culpado pelo que aconteceu.' 'Relatou que disse que o depoente era culpado pelo que aconteceu.' 'Relatou que ele disse que o depoente era culpado pelo que aconteceu.' 'M5: sujeito de «disse» devolvido'
Repair 'A Maria chegou antes de mim.' 'Relatou que o depoente chegou antes da Maria.' 'Relatou que a Maria chegou antes do depoente.' '«antes de mim» com «o depoente» como sujeito trocado'
Issue 'O Carlos entrou depois de mim.' 'O depoente relatou que entrou depois do Carlos.' 'papéis trocados' 'troca que ficou sem reparo: validador acusa'
Repair 'Eu entrei depois do Carlos.' 'Relatou que o depoente entrou depois do Carlos.' 'Relatou que o depoente entrou depois do Carlos.' 'original já tem «depois do Carlos»: sem mudança'
Issue 'Ele disse que eu era culpada.' 'O depoente relatou que ela disse que ele era culpado e ele era culpado.' 'depoente trocado' '«ele era culpado» duas vezes: validador acusa'
NoIssue 'Eu estava cansado e ele estava cansado.' 'O depoente relatou que estava cansado e ele estava cansado.' 'outra pessoa com o mesmo predicado no original: sem acusação'

# 3) Pronome ambíguo: voz passiva.
Repair 'Eu estava correndo na praia quando ele me atacou por trás.' 'Relatou que o depoente estava correndo na praia quando ele o atacou por trás.' 'Relatou que o depoente estava correndo na praia quando o depoente foi atacado por ele por trás.' 'P1: «ele o atacou» → «o depoente foi atacado por ele»'
Repair 'Eu estava grávida quando ele me ameaçou.' 'Relatou que o depoente estava grávida quando ele o ameaçou.' 'Relatou que o depoente estava grávida quando o depoente foi ameaçado por ele.' 'F11: «ameaçado por ele»'
Repair 'Eu estava saindo quando ela me empurrou.' 'Relatou que o depoente estava saindo quando ela o empurrou.' 'Relatou que o depoente estava saindo quando o depoente foi empurrado por ela.' '«por ela»'
Repair 'Ele não me agrediu.' 'Relatou que ele não o agrediu.' 'Relatou que o depoente não foi agredido por ele.' 'negação mantida: «não foi agredido por ele»'
Repair 'Ele me viu na rua.' 'Relatou que ele o viu na rua.' 'Relatou que ele o viu na rua.' 'particípio irregular («visto»): não converte'
Repair 'Ele me bateu.' 'Relatou que ele o bateu.' 'Relatou que ele o bateu.' '«bater» não tem passiva natural: não converte'
Repair 'Ele me atacou e depois ele me atacou de novo.' 'Relatou que ele o atacou e depois ele o atacou de novo.' 'Relatou que ele o atacou e depois ele o atacou de novo.' 'duas ocorrências: não adivinha'
Issue 'Ele me atacou e depois ele me atacou de novo.' 'O depoente relatou que ele o atacou e depois ele o atacou de novo.' 'pronome ambíguo' 'duas ocorrências: validador acusa'
Repair 'Ela me disse que viria.' 'Relatou que ela lhe disse que viria.' 'Relatou que ela lhe disse que viria.' 'verbo de fala («lhe disse»): sem mudança'
Repair 'Ele me atacou e ela o empurrou.' 'Relatou que ele atacou o depoente e ela o empurrou.' 'Relatou que ele atacou o depoente e ela o empurrou.' '«ela o empurrou» do original (outra pessoa): sem mudança'

# 4) Frase quebrada e gerúndio com o depoente como objeto («ele estava me enforcando»):
# corrige pelo original com uma única correspondência; senão o validador acusa.
Repair 'Eu empurrei ele porque ele estava me enforcando.' 'Relatou que o depoente empurrou ele porque ele estava o depoente enforcando.' 'Relatou que o depoente empurrou ele porque ele estava enforcando o depoente.' 'P5 (1ª passagem): «estava o depoente enforcando» → «estava enforcando o depoente»'
Repair 'Eu empurrei ele porque ele estava me enforcando.' 'Relatou que o depoente empurrou ele porque o depoente estava enforcando.' 'Relatou que o depoente empurrou ele porque ele estava enforcando o depoente.' 'P5 (nova tentativa): «o depoente estava enforcando» → «ele estava enforcando o depoente»'
Repair 'Ela estava me seguindo.' 'Relatou que estava seguindo.' 'Relatou que ela estava seguindo o depoente.' 'sujeito e objeto apagados: «ela estava seguindo o depoente»'
Repair 'Eu estava enforcando ele porque ele estava me enforcando.' 'Relatou que o depoente estava enforcando ele porque o depoente estava enforcando.' 'Relatou que o depoente estava enforcando ele porque o depoente estava enforcando.' 'depoente também faz a ação no original: não adivinha'
Issue 'Ele estava me enforcando e ela estava me enforcando.' 'O depoente relatou que o depoente estava enforcando.' 'papéis trocados' 'duas pessoas no original: validador acusa (rejeição e marca de incompleto)'
Issue 'Eu empurrei ele porque ele estava me enforcando.' 'O depoente relatou que empurrou ele porque o depoente estava enforcando.' 'papéis trocados' 'troca que ficou sem reparo: validador acusa'
Issue 'Eu empurrei ele porque ele estava me enforcando.' 'O depoente relatou que empurrou ele porque ele estava o depoente enforcando.' 'frase quebrada' 'frase quebrada que ficou sem reparo: validador acusa'
NoIssue 'Eu empurrei ele porque ele estava me enforcando.' 'O depoente relatou que empurrou ele porque ele estava enforcando o depoente.' 'P5 corrigida: validador aceita'
NoIssue 'Eu vi o carro parado na esquina.' 'O depoente relatou que viu o carro parado na esquina.' 'frase normal: sem acusação'

# Proteções anteriores continuam valendo.
Issue 'Eu vi a Ana sair.' '' 'saída vazia' 'saída vazia continua rejeitada'
Check ($G::RepairSourceAnchors('Ele me atacou.', 'Relatou que ele o atacou.') -ceq $G::RepairSourceAnchors('Ele me atacou.', $G::RepairSourceAnchors('Ele me atacou.', 'Relatou que ele o atacou.'))) 'reparo idempotente (aplicar duas vezes não muda)' ''

$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
