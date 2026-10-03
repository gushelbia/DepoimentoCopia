# Rodada 10 (bateria realista 3): outras formas de primeira pessoa, aproximação,
# primeira pessoa do plural, marcadores da fala, nome na abertura e primeira pessoa restante.
#   pwsh -NoProfile -File maintenance/forms-fix/Test-FormsRepairs.ps1
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

# 1) «comigo», «mim», «foi eu», «foi meu».
Repair 'O Lauro já tinha brigado comigo antes.' 'Relatou que o Lauro já tinha brigado com ele antes.' 'Relatou que o Lauro já tinha brigado com o depoente antes.' 'C05: «brigado com ele» → «brigado com o depoente»'
Repair 'Eu expliquei que não tinha sido eu.' 'Relatou que o depoente explicou que não tinha sido ele.' 'Relatou que o depoente explicou que não tinha sido o depoente.' 'C07: «sido ele» → «sido o depoente»'
Repair 'Era um atraso que nem foi meu, foi do Anselmo.' 'Relatou que era um atraso que nem foi seu, foi do Anselmo.' 'Relatou que era um atraso que nem foi do depoente, foi do Anselmo.' 'C01: «nem foi seu» → «nem foi do depoente»'
Repair 'O Carlos entrou depois de mim e eu já estava lá.' 'Relatou que o Carlos entrou depois dele e o depoente já estava lá.' 'Relatou que o Carlos entrou depois do depoente e o depoente já estava lá.' '«depois dele» (de mim) → «depois do depoente»'
Repair 'Ela acreditou em mim.' 'Relatou que ela acreditou nele.' 'Relatou que ela acreditou no depoente.' '«acreditou nele» (em mim) → «no depoente»'
Repair 'O Lauro tinha brigado comigo.' 'Relatou que o Lauro tinha brigado com ele e tinha brigado com ele.' 'Relatou que o Lauro tinha brigado com ele e tinha brigado com ele.' 'duas saídas «brigado com ele»: não adivinha'
Issue 'O Lauro tinha brigado comigo.' 'O depoente relatou que o Lauro tinha brigado com ele e com ele.' 'depoente trocado por outra pessoa' 'duas ocorrências: validador acusa'

# 2) Aproximação.
Repair 'O carro bateu umas sete e meia da noite.' 'Relatou que o carro bateu às sete e meia da noite.' 'Relatou que o carro bateu por volta das sete e meia da noite.' 'C02: «às sete e meia» → «por volta das sete e meia»'
Repair 'Esperei uns vinte minutos.' 'Relatou que o depoente esperou vinte minutos.' 'Relatou que o depoente esperou cerca de vinte minutos.' '«uns vinte minutos» → «cerca de vinte minutos»'
Repair 'Esperei uns vinte minutos.' 'Relatou que o depoente esperou uns vinte minutos.' 'Relatou que o depoente esperou uns vinte minutos.' 'aproximação mantida: sem mudança'
Issue 'Chegou lá pelas dez e eu saí às dez.' 'O depoente relatou que ele chegou às dez e o depoente saiu às dez.' 'aproximação perdida' 'duas ocorrências: validador acusa'

# 3) Primeira pessoa do plural: número explícito → «os quatro»; membro nomeado na
# mesma frase ou na anterior → «o depoente e a Olívia»; na dúvida → «o grupo».
Repair 'A gente estava em quatro no carro, eu, a Débora e o Gilson. Um carro bateu atrás da gente. O motorista disse que a culpa era nossa. Ele nos ameaçou.' 'Relatou que a gente estava em quatro no carro, eu, a Débora e o Gilson. Um carro bateu atrás da gente. O motorista disse que a culpa era nossa. Ele nos ameaçou.' 'Relatou que os quatro estavam no carro, o depoente, a Débora e o Gilson. Um carro bateu atrás dos quatro. O motorista disse que a culpa era do grupo. Ele ameaçou o grupo.' 'C02: número na frase ou na anterior → «os quatro»; depois → «o grupo»'
Repair 'A Olívia veio falar comigo. Ela acreditou em mim e a gente foi junto falar com a Noêmia.' 'Relatou que Olívia veio falar com o depoente. Ela acreditou no depoente e a gente foi junto falar com a Noêmia.' 'Relatou que Olívia veio falar com o depoente. Ela acreditou no depoente e o depoente e a Olívia foram juntos falar com a Noêmia.' 'C07: membro nomeado na frase anterior → «o depoente e a Olívia foram juntos»'
Repair 'Eu e o meu irmão, o Otávio, estávamos voltando. Na saída três caras pararam a gente. Um deles falou que a gente tinha mexido com a namorada dele.' 'Relatou que o depoente e o seu irmão, o Otávio, estavam voltando. Na saída três caras pararam a gente. Um deles falou que a gente tinha mexido com a namorada dele.' 'Relatou que o depoente e o seu irmão, o Otávio, estavam voltando. Na saída três caras pararam o depoente e o Otávio. Um deles falou que o grupo tinha mexido com a namorada dele.' 'B02: «eu e o meu irmão, o Otávio» → objeto «o depoente e o Otávio»; sem nome perto → «o grupo»'
Repair 'Eu fui com ela na delegacia e a gente fez o boletim. Era no nome do Edvaldo.' 'Relatou que o depoente foi com ela na delegacia e a gente fez o boletim. Era no nome do Edvaldo.' 'Relatou que o depoente foi com ela na delegacia e o grupo fez o boletim. Era no nome do Edvaldo.' 'B08: membro sem nome («com ela») → «o grupo»'
Repair 'O Caio e a Rita chegaram. A gente saiu junto.' 'Relatou que o Caio e a Rita chegaram. A gente saiu junto.' 'Relatou que o Caio e a Rita chegaram. O grupo saiu junto.' 'dois nomes possíveis: na dúvida, «o grupo»'
Repair 'Eu fui com a Lia ao mercado e a gente encontrou a Rosa lá.' 'Relatou que o depoente foi com a Lia ao mercado e a gente encontrou a Rosa lá.' 'Relatou que o depoente foi com a Lia ao mercado e o depoente e a Lia encontraram a Rosa lá.' '«com a Lia» antes de «a gente»; a Rosa, depois, não é membro'
Repair 'Nós estávamos voltando.' 'Relatou que nós estávamos voltando.' 'Relatou que o grupo estava voltando.' '«nós estávamos» → «o grupo estava»'
Repair 'Nós três estávamos voltando.' 'Relatou que nós três estávamos voltando.' 'Relatou que os três estavam voltando.' '«nós três» → «os três estavam»'
Repair 'Ela disse: "a gente vai embora".' 'Relatou que ela disse: "a gente vai embora".' 'Relatou que ela disse: "a gente vai embora".' 'fala entre aspas preservada'

# 4) Marcadores da fala.
Repair 'Olha, sei lá, tipo, foi tudo muito rápido, né. Eu tava saindo.' 'Relatou que olha, sabe lá, tipo, foi tudo muito rápido, né. O depoente tava saindo.' 'Relatou que foi tudo muito rápido. O depoente estava saindo.' 'C06: marcadores removidos; «tava» → «estava»'
Repair 'Sei lá se foi ele.' 'Relatou que sabe lá se foi ele.' 'Relatou que não sabe se foi ele.' '«sei lá se» (dúvida) → «não sabe se»'
NoIssue 'Olha, sei lá, tipo, foi tudo muito rápido, né.' 'O depoente relatou que foi tudo muito rápido.' 'marcadores retirados não contam como omissão'
Repair 'Ele disse: "olha, tipo, sei lá".' 'Relatou que ele disse: "olha, tipo, sei lá".' 'Relatou que ele disse: "olha, tipo, sei lá".' 'marcadores entre aspas preservados'

# 5) Nome próprio e abertura.
Check ($G::RepairSourceAnchors('Eu, Marta, sou secretária há oito anos. Eu fui a primeira a chegar.', 'Relatou que marta, secretária há oito anos, o depoente foi a primeira a chegar.') -ceq 'O depoente, Marta, relatou que é secretária há oito anos e foi a primeira a chegar.') 'C03: «Eu, Marta, sou…» → «O depoente, Marta, relatou que é…»' ''
Check ($G::RepairSourceAnchors('Eu, Marta, sou secretária. Eu cheguei cedo.', 'Relatou que o depoente é secretária. O depoente chegou cedo.') -ceq 'O depoente, Marta, relatou que é secretária. O depoente chegou cedo.') 'nome perdido na abertura: «O depoente, Marta, relatou que…»' ''
Repair 'Falei com a Rosa ontem.' 'Relatou que o depoente falou com a rosa ontem.' 'Relatou que o depoente falou com a Rosa ontem.' 'nome do original em minúscula volta à maiúscula'
Repair 'A rosa estava na mesa da Rosa.' 'Relatou que a rosa estava na mesa da Rosa.' 'Relatou que a rosa estava na mesa da Rosa.' 'palavra comum igual ao nome no original: não muda'

# 6) Primeira pessoa singular que sobrou.
Repair 'Eu nunca falei com o diretor, eu nem conheço ele.' 'Relatou que o depoente nunca falou com o diretor, nem conheço pessoalmente.' 'Relatou que o depoente nunca falou com o diretor, nem conhece pessoalmente.' 'B01: «nem conheço» → «nem conhece»'
Repair 'Eu não contei.' 'Relatou que o depoente não contei.' 'Relatou que o depoente não contou.' 'C04: «o depoente não contei» → «não contou»'
Repair 'Ele disse que eu conheço o lugar.' 'Relatou que ele disse que conheço o lugar.' 'Relatou que ele disse que conheço o lugar.' 'sem «o depoente» como sujeito da oração: não adivinha'
Issue 'Ele disse que eu conheço o lugar.' 'O depoente relatou que ele disse que conheço o lugar.' 'primeira pessoa fora das aspas' 'e o validador acusa'

# 7) O depoente escrito como «ele/ela».
Repair 'Eu devolvi pra ela e eu não contei pra ninguém.' 'Relatou que ele devolveu para ela e ele não contou a ninguém.' 'Relatou que o depoente devolveu para ela e o depoente não contou a ninguém.' 'R04: «ele devolveu», «ele não contou» → «o depoente»'
Repair 'Eu nunca falei com o diretor, eu nem conheço ele.' 'Relatou que o depoente nunca falou com o diretor, ele nem conhece pessoalmente.' 'Relatou que o depoente nunca falou com o diretor, o depoente nem conhece pessoalmente.' 'B01: «ele nem conhece» → «o depoente nem conhece»'
Repair 'Eu cheguei cedo e ele chegou depois.' 'Relatou que ele chegou depois.' 'Relatou que ele chegou depois.' 'outra pessoa faz o mesmo no original: não adivinha'
Repair 'Eu não sabia, juro.' 'Relatou que o depoente não sabia, jura.' 'Relatou que o depoente não sabia.' '«jura» isolado (de «juro») removido'
Issue 'Eu vi a Ana sair.' '' 'saída vazia' 'saída vazia continua rejeitada'
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
