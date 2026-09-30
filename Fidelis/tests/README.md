# Validação de reformulações consecutivas

Execute com Windows PowerShell 5.1, no diretório da distribuição:

```powershell
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File .\tests\RepeatedGeneration.ps1
```

O teste compila `ui/ModernShell.cs` com as mesmas referências usadas por
`ModernUI.ps1`, abre uma instância própria do motor e da interface e carrega
o modelo já configurado. Usa os controles reais (`PerformClick` e o loop de
mensagens do Windows), sem substituir a IA por respostas simuladas.
Ao terminar, encerra apenas a instância criada pelo teste.

Verificações:

- Três reformulações com textos diferentes, na mesma instância do motor.
- Apagar todo o conteúdo e inserir um novo texto antes de cada chamada.
- Entrega do novo texto ao motor e obtenção de uma nova saída.
- Botão desabilitado durante a chamada e reativado ao concluir.
- Campo de entrada habilitado e editável após cada chamada.
- Recuperação de referências de controles inválidas, injetadas antes da segunda chamada.
- Cancelamento seguido de uma nova reformulação no mesmo motor.
- Recuperação do estado local quando o motor é encerrado e o envio falha.

O registro da última execução fica em `last-run.txt`. O teste não altera
prompt, modelo, parâmetros de geração ou binários do motor.

## Escopo da correção

A distribuição contém o fonte da interface e os binários do motor, sem
projeto/fonte do motor. A interface é compilada pelo inicializador a cada
abertura; portanto, a correção é aplicada ao abrir normalmente `INICIAR.bat`.

A ponte agora verifica referências nativas antes de usar os controles,
redescobre controles inválidos e envia o comando ao pai do botão. O estado
local é definido antes do envio, protegido contra reentrada e liberado tanto
na conclusão quanto em falhas de comunicação. Erros de sincronização deixam
de ser ignorados silenciosamente. O layout e as regras da IA não mudaram.

O bloqueio relatado não apareceu nas três chamadas simples da versão anterior.
A recuperação de referências inválidas foi validada por injeção dessa falha;
isso não comprova que ela tenha sido a causa específica da ocorrência original.
Não há ViewModel/CanExecuteChanged no fonte desta interface WinForms. Tokens
de cancelamento e executor pertencem ao motor, que foi preservado.
