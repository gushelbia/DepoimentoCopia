# Bateria de QA / regressão

Os dez casos estão em `TestCases/01.json` a `10.json`. O texto exato fornecido para o Teste 5 fica em `05.txt`, apontado por `inputFile`; os demais textos ficam no campo `input`. Edite esses arquivos para alterar os casos sem modificar o runner.

Na pasta da distribuição, execute:

```powershell
pwsh.exe -NoProfile -File .\QA\Build-Application.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File .\QA\Run-QA.ps1
```

A primeira etapa compila os fontes disponíveis da aplicação (launcher + interface) em `QA/Build`, usando as referências .NET 8 da distribuição. O motor é distribuído apenas em binários, sem seu projeto/fonte, e não é recompilado nem modificado. A segunda etapa compila novamente a interface junto ao runner e usa a classe **OriginalAppBridge original**, exatamente como o botão “Reformular texto”: insere o texto, chama `Reformular` no motor e aguarda seu evento terminal. Não há implementação alternativa de geração, prompt ou validação.

Cada caso carrega o GGUF portátil `modelo/qwen2.5-3b-instruct-q4_k_m.gguf`, usa as configurações do motor e roda em um processo próprio, sequencialmente. Ao terminar, somente esse processo é encerrado, inclusive em rejeições com diálogo modal. As instâncias do usuário não são encerradas. Isso testa os casos de geração isoladamente; o teste de gerações consecutivas na mesma instância continua em `tests/RepeatedGeneration.ps1`.

Não gere textos em outras instâncias durante a bateria: o aplicativo grava um log global sem PID. O runner detecta marcadores concorrentes e rotação inesperada como erro, mas não consegue atribuir toda linha a um PID. Ele precisa de acesso ao log em `%LOCALAPPDATA%/DepoimentoLocal/logs`. Um mutex impede duas baterias simultâneas. O limite é 180 segundos para carregar o modelo e 1.200 segundos para cada geração; ajuste somente o limite do runner com `-TimeoutSeconds`. Isso não altera o contexto nem os parâmetros do LLM.

Resultados em `Results/qa-report.md`, atualizados a cada teste, com entradas e saídas integrais. Há também JSON, saída TXT e log integral por caso, `summary.json`, registros de compilação e hashes antes/depois das DLLs de produção em `engine-integrity.json`. Uma execução substitui os resultados anteriores; copie `Results` antes de rodar novamente se quiser manter histórico.

Warnings do validador e warnings nativos ficam separados. O relatório distingue conclusão técnica (`GENERATION_OK`) de alertas. Primeira pessoa fora de citações, encerramento e truncamento são sinais para revisão, não prova automática de infidelidade. No Teste 5, três marcadores lexicais verificam o final fornecido. Nuances semânticas e possíveis paráfrases continuam para revisão humana. As saídas não passam por correções do QA; os reparos que o aplicativo já aplica continuam ativos.

Os testes do próprio analisador podem ser executados sem carregar o modelo:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\QA\Test-Report.ps1
```

Para atualizar apenas a análise e o Markdown usando as evidências já salvas, sem chamar o LLM:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\QA\Rebuild-Report.ps1
```
