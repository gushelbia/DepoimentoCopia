# Log sem texto de depoimento (rodada 8): filtro do AppLog.Write e modo diagnóstico.
#   pwsh -NoProfile -File maintenance/log-privacy/Test-LogFilter.ps1
# Usa SemanticGuard.compiled.dll (a mesma lógica embutida no motor). Frases inventadas.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/semantic-fix/SemanticGuard.compiled.dll')
$G = [DepoimentoLocal.Windows.SemanticGuard]
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
function Normal($event, $details) { $env:DEPOIMENTOLOCAL_LOG_DIAGNOSTICO = $null; [void]$G::TakeRepairs(); return $G::LogMessage($event, $details) }
function Same($event, $details, $name) { $r = Normal $event $details; Check ($r -ceq $details) $name $r }
function Becomes($event, $details, $expected, $name) { $r = Normal $event $details; Check ($r -ceq $expected) $name $r }

# Uso normal: só informação técnica.
Becomes 'LOCAL_REPAIR' 'block=1/1; before=fidelidade: possível omissão de oração: Eu cheguei cedo na Xavantina.; after=ok' 'block=1/1; before=fidelidade: possível omissão de oração: [texto omitido]; after=ok' 'LOCAL_REPAIR: categoria mantida, frase do original omitida'
Becomes 'BLOCK_RETRY_START' 'block=1/2; issue=fidelidade: sujeito de outra pessoa apagado: no original «Meu marido Valdomiro foi preso», na saída «relatou que foi preso»' 'block=1/2; issue=fidelidade: sujeito de outra pessoa apagado: [texto omitido]' 'motivo da nova tentativa: categoria mantida, trechos «…» omitidos'
Becomes 'BLOCK_REJECTED' 'block=1/1; detail=fidelidade: possível primeira pessoa residual: fiquei' 'block=1/1; detail=fidelidade: possível primeira pessoa residual: [texto omitido]' 'rejeição: palavra do original omitida'
Becomes 'GENERATION_FAILED' "sourceChars=58; elapsedMs=9000`r`nSystem.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: papéis trocados: no original «ele estava me enforcando», na saída «o depoente estava enforcando». A reformulação foi interrompida.`r`n   at DepoimentoLocal.Windows.MainForm.Reformulate_Click()" "sourceChars=58; elapsedMs=9000`r`nSystem.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: papéis trocados: [texto omitido]`r`n   at DepoimentoLocal.Windows.MainForm.Reformulate_Click()" 'erro: tipo, categoria e pilha mantidos; trecho omitido'
Becomes 'GENERATION_FAILED' 'O bloco 2 ainda precisa de revisão: a frase do Gumercindo ficou estranha' 'O bloco 2 ainda precisa de revisão: [texto omitido]' 'revisão sem categoria conhecida: tudo depois de «revisão:» omitido'
Becomes 'BLOCK_ACCEPTED_WITH_WARNING' 'block=1/1; detail=algo: "fala do Gumercindo"' 'block=1/1; detail=algo: [texto omitido]' 'detalhe com fala entre aspas omitido'
Same 'BLOCK_START' 'block=1/3; chars=812; maxTokens=900' 'dados técnicos ficam iguais (bloco, tamanho, tokens)'
Same 'BLOCK_RETRY_START' 'block=1/1; issue=a saída ainda contém primeira pessoa' 'motivo sem trecho de texto fica igual'
Same 'LOCAL_REPAIR' 'block=1/1; before=ok; after=ok' 'reparo sem problema fica igual'
Same 'MODEL_LOAD_START' 'file=..\modelo\qwen2.5-3b-instruct-q4_k_m.gguf; bytes=1929903264' 'carregamento do modelo fica igual'
Same 'LLAMASHARP' 'print_info: model type = 3B' 'mensagem nativa técnica fica igual'
$r = Normal 'APP_START' 'version=1.0.5'
Check ($r -ceq 'version=1.0.5; log=técnico (sem texto do depoimento)') 'APP_START registra que o log está no modo técnico' $r

# Nomes dos reparos aplicados (no uso normal também).
$env:DEPOIMENTOLOCAL_LOG_DIAGNOSTICO = $null; [void]$G::TakeRepairs()
[void]$G::RepairSourceAnchors('Meu marido foi preso e eu fiquei sozinha.', 'Relatou que foi preso e o depoente ficou sozinho.')
$r = $G::LogMessage('BLOCK_OK', 'block=1/1; outputChars=80')
Check ($r -match '; repairs=.*sujeito-apagado' -and $r -match 'abertura' -and $r -notmatch 'marido') 'BLOCK_OK lista os reparos aplicados, sem texto' $r
$r = $G::LogMessage('BLOCK_OK', 'block=2/2; outputChars=80')
Check ($r -ceq 'block=2/2; outputChars=80') 'a lista é zerada a cada bloco' $r
[void]$G::RepairFirstPerson('Fiquei na sala.')
$r = $G::LogMessage('LOCAL_REPAIR', 'block=1/1; before=ok; after=ok')
Check ($r -match 'repairs=primeira-pessoa') 'reparo da primeira pessoa registrado' $r
$r = $G::LogMessage('MODEL_CONFIG', 'context=4096')
Check ($r -ceq 'context=4096') 'outros eventos não recebem lista de reparos' $r

# Modo diagnóstico (só os testes ligam): texto completo.
$env:DEPOIMENTOLOCAL_LOG_DIAGNOSTICO = '1'; [void]$G::TakeRepairs()
$full = 'block=1/2; issue=fidelidade: sujeito de outra pessoa apagado: no original «Meu marido Valdomiro foi preso»'
Check ($G::LogMessage('BLOCK_RETRY_START', $full) -ceq $full) 'modo diagnóstico: texto completo mantido' ''
Check ($G::LogMessage('APP_START', 'version=1.0.5') -match 'log=diagnóstico') 'APP_START registra o modo diagnóstico' ''
$env:DEPOIMENTOLOCAL_LOG_DIAGNOSTICO = '0'
Check ($G::LogMessage('BLOCK_RETRY_START', $full) -notmatch 'Valdomiro') 'qualquer valor diferente de 1 = modo técnico' ''
$env:DEPOIMENTOLOCAL_LOG_DIAGNOSTICO = $null

# O filtro não altera a reformulação.
Check ($G::RepairSourceAnchors('Ele me atacou.', 'Relatou que ele o atacou.') -ceq 'O depoente relatou que foi atacado por ele.') 'reparos continuam produzindo o mesmo texto' ''

$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'filter-results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
