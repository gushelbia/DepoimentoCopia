# Correção da etapa de 1ª pessoa (rodada 6): sujeito trocado e reflexivo.
#   pwsh -NoProfile -File maintenance/first-person-fix/Test-SwappedSubject.ps1
# Usa SemanticGuard.compiled.dll (a mesma lógica embutida no motor). Frases inventadas.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/semantic-fix/SemanticGuard.compiled.dll')
$G = [DepoimentoLocal.Windows.SemanticGuard]
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Repair($src, $out, $expected, $name) { $r = $G::RepairSourceAnchors($src, $out); $e = $G::FinalLead($expected); Check ($r -eq $e) $name $r }
function Issue($src, $out, [bool]$expected, $name) { $i = $G::Validate($src, $out); Check ([bool]($i -and $i.Contains('autor da ação trocado')) -eq $expected) $name $i }

# Inversões da bateria (texto real do motor antes do reparo final).
Repair 'Ele me bateu com um pedaço de pau e depois fugiu.' 'Relatou que o depoente lhe bateu com um pedaço de pau e depois fugiu.' 'Relatou que ele bateu no depoente com um pedaço de pau e depois fugiu.' 'caso 3: «o depoente lhe bateu» → «ele bateu no depoente»'
Repair 'Ela me xingou e eu respondi que ia chamar a polícia.' 'Relatou que o depoente lhe xingou e respondeu que ia chamar a polícia.' 'Relatou que ela xingou o depoente e o depoente respondeu que ia chamar a polícia.' 'caso 6: restaura «ela» e mantém «o depoente respondeu»'
Repair 'Ele me disse que tinha visto ela sair com o carro.' 'Relatou que o depoente lhe disse que tinha visto ela sair com o carro.' 'Relatou que ele lhe disse que tinha visto ela sair com o carro.' 'caso 7: «o depoente lhe disse» → «ele lhe disse»'
Repair 'Ela me contou que ele tinha batido nela na noite anterior.' 'Relatou que o depoente lhe disse que ele tinha batido nela na noite anterior.' 'Relatou que ela lhe disse que ele tinha batido nela na noite anterior.' 'caso 10: verbo de fala diferente (contou/disse) também restaura'
Repair 'Ele me ameaçou com uma faca e eu corri para dentro de casa.' 'Relatou que o depoente lhe ameaçou com uma faca e o depoente correu para dentro de casa.' 'Relatou que ele ameaçou o depoente com uma faca e o depoente correu para dentro de casa.' 'caso 14: «ameaçou o depoente»'
Repair 'Ela me atacou primeiro e eu só me defendi.' 'Relatou que o depoente lhe atacou primeiro e só se defendeu.' 'Relatou que ela atacou o depoente primeiro e o depoente só se defendeu.' 'caso 18 (1ª passagem): restaura «ela» e o sujeito de «se defendeu»'
Repair 'Ela me atacou primeiro e eu só me defendi.' 'Relatou que ela lhe atacou primeiro e o depoente só lhe defendeu.' 'Relatou que ela atacou o depoente primeiro e o depoente só se defendeu.' 'caso 18 (2ª tentativa): reflexivo «lhe defendeu» → «se defendeu»; «lhe atacou» → «atacou o depoente» (rodada 8)'
Repair 'Eu me escondi atrás do carro.' 'Relatou que o depoente me escondeu atrás do carro.' 'Relatou que o depoente se escondeu atrás do carro.' 'reflexivo com «me» restante → «se»'
Repair 'Ela me disse: “eu não volto”.' 'Relatou que o depoente lhe disse: “eu não volto”.' 'Relatou que ela lhe disse: “eu não volto”.' 'fala entre aspas preservada byte a byte'

# Não deve agir.
Repair 'Eu lhe disse que ia embora.' 'Relatou que o depoente lhe disse que ia embora.' 'Relatou que o depoente lhe disse que ia embora.' 'depoente que fala de fato («eu lhe disse»): sem mudança'
Repair 'Eu contei para ela e ela me disse que já sabia.' 'Relatou que o depoente lhe contou e ela lhe disse que já sabia.' 'Relatou que o depoente lhe contou e ela lhe disse que já sabia.' 'depoente também fala no original: sem mudança'
Repair 'Ela se defendeu quando ele me empurrou.' 'Relatou que ela se defendeu quando ele empurrou o depoente.' 'Relatou que ela se defendeu quando ele empurrou o depoente.' 'texto já correto: sem mudança'
Repair 'Ele me disse que viria. Ela me disse que não.' 'Relatou que o depoente lhe disse que viria. Ela lhe disse que não.' 'Relatou que o depoente lhe disse que viria. Ela lhe disse que não.' 'dois candidatos no original: não adivinha'
Repair 'O Marcos me segurou pelo braço.' 'Relatou que o Marcos lhe segurou pelo braço.' 'Relatou que o Marcos segurou o depoente pelo braço.' 'sujeito correto mantido; regência «lhe segurou» → «segurou o depoente» (rodada 8)'
Repair 'Depois me disseram que houve reunião.' 'Relatou que depois lhe disseram que houve reunião.' 'Relatou que depois lhe disseram que houve reunião.' '«Depois me disseram» não vira sujeito'
Repair 'Eu bati nele porque ele me ameaçou.' 'Relatou que o depoente bateu nele porque ele o ameaçou.' 'Relatou que o depoente bateu nele porque o depoente foi ameaçado por ele.' 'ação do próprio depoente («eu bati»): sujeito mantido; «ele o ameaçou» vira passiva (rodada 7)'

# Validador: troca que não pôde ser reparada vira erro (nova tentativa / rejeição).
Issue 'Ele me disse que viria. Ela me disse que não.' 'Relatou que o depoente lhe disse que viria. Ela lhe disse que não.' $true 'validador: troca não reparável é acusada'
Issue 'Ela me contou que ele tinha batido nela.' 'Relatou que o depoente lhe disse que ele tinha batido nela.' $true 'validador: caso 10 sem reparo seria acusado'
Issue 'Eu lhe disse que ia embora.' 'Relatou que o depoente lhe disse que ia embora.' $false 'validador: depoente que fala de fato não é acusado'
Issue 'Ela me atacou primeiro e eu só me defendi.' 'Relatou que ela atacou o depoente primeiro e o depoente só se defendeu.' $false 'validador: texto corrigido do caso 18 aceito'
Issue 'Ele me disse que viria.' 'Relatou que ele lhe disse que viria.' $false 'validador: texto correto aceito'
Check ($G::Validate('Eu cheguei.', '') -eq 'fidelidade: saída vazia') 'saída vazia continua rejeitada' ''
Check ($G::Validate('Eu cheguei.', '   ') -eq 'fidelidade: saída vazia') 'saída só com espaços continua rejeitada' ''

# Abertura (opção A), aplicada no fim do reparo final: «O depoente relatou que …».
function Lead($in, $expected, $name) { $r = $G::FinalLead($in); Check ($r -eq $expected) $name $r }
Lead 'Relatou que o depoente estava correndo na praia.' 'O depoente relatou que estava correndo na praia.' 'abertura: «Relatou que o depoente estava» → «O depoente relatou que estava»'
Lead 'Relatou que o depoente não viu quem o acertou.' 'O depoente relatou que não viu quem o acertou.' 'abertura: com negação'
Lead 'Relatou que ele lhe disse que viria.' 'O depoente relatou que ele lhe disse que viria.' 'abertura: outro sujeito fica como está'
Lead 'Relatou que quando o depoente chegou, ele já estava caído.' 'O depoente relatou que quando o depoente chegou, ele já estava caído.' 'abertura: «o depoente» que não abre a frase fica'
Lead 'Relatou que o depoente e a esposa chegaram.' 'O depoente relatou que o depoente e a esposa chegaram.' 'abertura: sujeito composto fica'
Lead 'O depoente relatou que estava em casa.' 'O depoente relatou que estava em casa.' 'abertura: aplicar de novo não muda (idempotente)'
Lead 'Ele chegou depois.' 'Ele chegou depois.' 'blocos seguintes (sem abertura): sem mudança'
Check ($G::RepairSourceAnchors('Ele me bateu.', 'Relatou que o depoente lhe bateu.') -eq 'O depoente relatou que ele bateu no depoente.') 'ordem: o sujeito trocado é corrigido antes da abertura final' ($G::RepairSourceAnchors('Ele me bateu.', 'Relatou que o depoente lhe bateu.'))
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }