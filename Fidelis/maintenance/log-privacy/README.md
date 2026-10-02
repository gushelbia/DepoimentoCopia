# Rodada 8: log sem texto de depoimento

O log fica em `%LOCALAPPDATA%\DepoimentoLocal\logs\depoimento-local.log` e é aberto pelo botão «Abrir log».

## O que o log gravava (levantamento)

Todo o log passa por um só ponto do motor, `AppLog.Write(nível, evento, detalhes)`. A interface Fidelis não grava log próprio; o botão «Abrir log» abre o log do motor.

| Evento | Conteúdo | Tinha texto do depoimento? |
|---|---|---|
| `APP_START`, `NATIVE_LOG`, `MODEL_LOAD_*`, `MODEL_CONFIG`, `CONTEXT_BUDGET` | versão, sistema, arquivo e tamanho do modelo, configuração, tokens | não |
| `GENERATION_START`, `BLOCK_START`, `BLOCK_FIRST_PASS`, `BLOCK_OK`, `GENERATION_OK`, `GENERATION_CANCELLED` | blocos, tamanhos (caracteres), tempos, trechos gerados | não |
| `LOCAL_REPAIR` (`before=`/`after=`) | motivo do validador antes e depois dos reparos | **sim**: o motivo cita a frase do original ou da saída |
| `BLOCK_RETRY_START`/`BLOCK_RETRY_END` (`issue=`) | motivo da nova tentativa | **sim** |
| `BLOCK_REJECTED`/`BLOCK_ACCEPTED_WITH_WARNING` (`detail=`) | motivo da rejeição ou do alerta | **sim** |
| `GENERATION_FAILED` | erro com a pilha; a mensagem de revisão cita o motivo | **sim** |
| `NATIVE-*` (llama.cpp) | carregamento de bibliotecas e do modelo, desempenho | não (verificado nas 8.571 linhas do log antigo) |

**Quem lia o log:**
- Leem o texto: `QA/QaRunner.cs` e `QA/Report.ps1` (marcas `[BLOCK_*]`, linhas de alerta, configuração do modelo) e, por meio do QaRunner, `Run-Diagnosis.ps1` (20 frases), `Run-GenderBattery.ps1` e `model-7b-test/Run-Model.ps1`.
- Só procuram marcas como `[GENERATION_OK]`: as sondas antigas `aspect-fix/Test3Probe.cs` e `context-fix/AutomaticRetryTest.cs`.
- Fica de fora: a bateria antiga `QA-Inedita-20260929` tem seu próprio QaRunner, que roda no modo técnico (as marcas continuam lá; os motivos ficam sem o trecho).

Nenhum teste lê logs antigos; todos leem só o que a própria execução gravou.

## O que mudou

- `SemanticGuard.LogMessage`, chamado no início de `AppLog.Write` (`Apply-SemanticFix.ps1`).
- **Uso normal:** de cada motivo do validador fica só a categoria («fidelidade: sujeito de outra pessoa apagado: [texto omitido]»), e trechos entre aspas viram «…». Tudo o mais continua igual: eventos, horários, tamanhos, status, erros com a pilha e o tipo da exceção.
- **Novo:** os eventos de bloco ganham `repairs=` com os nomes dos reparos aplicados (por exemplo `repairs=abertura,sujeito-apagado`), também no uso normal.
- **Modo diagnóstico (texto completo):** só com a variável de ambiente `DEPOIMENTOLOCAL_LOG_DIAGNOSTICO=1`, que o Fidelis.exe nunca define. Só o `QA/QaRunner.cs` a liga, para os testes. O `APP_START` registra o modo: `log=técnico (sem texto do depoimento)` ou `log=diagnóstico`.
- A reformulação não muda: o filtro age só no que vai para o arquivo de log.

## Testes

- `Test-LogFilter.ps1` (pwsh): o filtro, o modo diagnóstico e os nomes dos reparos.
- `Test-LogPrivacy.ps1` (STA, modelo real): gerações no uso normal com palavras-sentinela inventadas. O log não pode conter nenhuma palavra da entrada nem da saída, e o modo diagnóstico é conferido.

## Backup

`backup-before/` guarda o motor, o SemanticGuard, o script de aplicação, o QaRunner, os inicializadores e o SHA256SUMS anteriores.
