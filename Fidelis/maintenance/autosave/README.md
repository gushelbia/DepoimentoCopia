# Proteções do depoimento consolidado

Melhorias aplicadas em `ui/ModernShell.cs`, mantendo todas as proteções anteriores da interface:

1. «Limpar consolidado» pede confirmação; «Não» é o botão padrão.
2. Ao fechar com o consolidado não salvo: Sim / Não / Cancelar. «Sim» abre a janela de salvar (Word ou .txt); cancelar essa janela mantém o programa aberto. O motor só é encerrado depois da decisão.
3. Salvamento automático 1,5 s após a última alteração em `%LOCALAPPDATA%\DepoimentoLocal\recuperacao\consolidado.txt` (fora da pasta do programa e do repositório). A cópia é apagada ao salvar, exportar, limpar ou escolher «Não» ao fechar.
4. Ao abrir, se existir a cópia, o programa oferece a recuperação com data e hora.
5. Em desligamento do Windows ou encerramento pelo Gerenciador de Tarefas, nada é perguntado e a cópia é mantida.

## Teste

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/autosave/Test-Autosave.ps1

Usa o motor real, sem carregar modelo, e responde às caixas de diálogo por uma rotina em paralelo. A cópia de recuperação real do usuário, se existir, é guardada antes e devolvida no fim. Os arquivos salvos pelo teste vão para uma pasta temporária; se uma janela de salvar usar o nome e a pasta padrão, o teste remove esse arquivo (só quando é recente e contém a frase do teste) e marca FALHA. Resultado em `results.txt`.

Janelas do programa aparecem e somem na tela durante cerca de um minuto.
