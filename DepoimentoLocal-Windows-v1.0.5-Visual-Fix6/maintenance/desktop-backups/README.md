# Cópias da interface compilada (antes de cada mudança)

Cópias de `engine/DepoimentoLocal.Desktop.dll` guardadas antes de cada recompilação da interface. O nome indica a mudança e a data e hora (`before-<mudança>-<aaaammddhhmmss>.dll`).

Estavam em `engine/` e foram movidas para cá, porque o programa não as usa:
- o launcher abre os arquivos pelo nome exato;
- elas não constam do `.deps.json`;
- o programa em execução não carrega nenhuma delas.

Voltar a uma versão: feche o programa, copie o arquivo desejado para `engine/DepoimentoLocal.Desktop.dll` e abra o `Fidelis.exe`.

As DLLs não vão para o GitHub (estão no `.gitignore`), porque podem ser recompiladas a partir do histórico do `ui/ModernShell.cs`. A bateria de QA registra os hashes delas em `QA/Results/engine-integrity.json`, junto com os do motor.