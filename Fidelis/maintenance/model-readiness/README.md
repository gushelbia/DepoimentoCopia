# Proteção de carregamento e comunicação assíncrona

O comando síncrono da interface moderna acionava um MessageBox no motor oculto quando o modelo não estava carregado. A interface aguardava o fechamento desse aviso e aparentava travar.

`ui/ModernShell.cs` agora mantém uma confirmação de carregamento, obtida apenas após o status de sucesso e a liberação dos controles do motor. Reformular verifica essa confirmação antes de enviar texto ou iniciar uma operação. A ponte também rejeita Reformular e Cancelar sem modelo pronto. Selecionar outro modelo, iniciar nova carga, falhar a carga ou perder a comunicação invalida a confirmação.

Na abertura, Reformular aceita o clique para exibir a mensagem “Carregue o modelo antes de reformular o texto.” na área de status e retorna imediatamente. Durante carregamento e geração o botão fica desabilitado; carga bem-sucedida habilita, falha mantém desabilitado. Carregamento/seleção de modelo não podem ocorrer durante geração. Ações sobre textos já existentes continuam disponíveis sem depender do modelo.

A comunicação durante as operações usa uma fila assíncrona serializada. Comandos são postados ao motor; leituras/escritas usam timeout fora da thread da interface. O timer não acumula consultas sobrepostas. O cancelamento solicitado antes de o motor habilitar seu botão é reenviado na próxima consulta. O aviso conhecido de falha de carga do processo oculto é fechado após sua mensagem ser refletida no status, permitindo nova tentativa.

Layout, prompts, modelo, DLLs de inferência e perfis CPU/Vulkan foram preservados. `engine-before.json` registra os hashes anteriores; `verification.txt` registra a comparação final. A DLL gráfica instalada é `engine/DepoimentoLocal.Desktop.dll`; backups da versão anterior foram preservados na mesma pasta.

## Validação

Executar no Windows PowerShell, com a aplicação de teste fechada:

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/model-readiness/Test-Readiness.ps1
    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/model-readiness/Test-Executable.ps1

O primeiro teste abre uma instância própria do motor e valida: clique sem modelo sem envio de entrada, bloqueio também na ponte, resposta do timer da UI, falha real com arquivo GGUF inválido, nova carga com GGUF real, duas gerações, eventos duplicados, tentativa de carga durante geração, cancelamento e encerramento do motor. O diretório do processo .NET é configurado explicitamente após a compilação para resolver o modelo relativo como no launcher.

O segundo inicia o executável instalado a partir de outro diretório e exercita os botões nativos: aviso sem modelo, carga real, geração real, responsividade e fechamento. Resultados em `results.txt` e `executable-results.txt`. Os testes encerram somente as instâncias que iniciam.

A compilação de produção utiliza o mesmo Roslyn e as referências .NET 8 do `launcher/Build-Launcher.ps1`, com emissão em arquivo temporário antes da substituição da DLL. A compilação pelo Windows PowerShell também confirma a compatibilidade da interface com os iniciadores legados.
