# Atalhos de teclado

Mudanças só em `ui/ModernShell.cs`. O motor não muda.

| Atalho | Ação |
|---|---|
| Ctrl+Enter | Reformular (sem quebrar linha no editor; sem modelo, só o aviso) |
| Esc | Cancelar a geração, só enquanto estiver gerando; fora disso, não faz nada |
| Ctrl+S | Salvar rascunho |
| Ctrl+O | Abrir rascunho |
| Ctrl+E | Exportar Word |
| Ctrl+D | Ligar e desligar os destaques |

- Os atalhos valem na janela toda, inclusive com o foco nos campos de qualificação. Eles fazem o mesmo que os botões e respeitam os mesmos estados, com as mesmas perguntas e proteções.
- A dica (tooltip) de cada botão mostra o atalho.
- Nos três editores, os atalhos nativos de formatação do RichTextBox ficam bloqueados, e o texto continua sem formatação:
  - Ctrl+E, Ctrl+R, Ctrl+L e Ctrl+J (alinhamento);
  - Ctrl+1, Ctrl+2 e Ctrl+5 (espaçamento);
  - Ctrl+Shift+L (marcadores);
  - Ctrl+= e Ctrl+Shift+= (subscrito e sobrescrito);
  - Ctrl+Shift+< e Ctrl+Shift+> (tamanho da fonte).
- Copiar, recortar, colar, selecionar tudo e desfazer continuam funcionando.

## Teste

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/shortcuts/Test-Shortcuts.ps1

As teclas são simuladas só na thread do teste, pelo mesmo caminho do laço de mensagens, sem injetar entrada global. Um controle confere que a simulação alinha o texto num RichTextBox comum, sem o bloqueio. O teste carrega o modelo para conferir Ctrl+Enter e Esc de verdade.
