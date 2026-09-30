# Lado a lado e destaques de conferência

Mudanças só em `ui/ModernShell.cs`. O motor e o texto gerado não mudam, e todas as proteções anteriores (salvamento automático, prontidão do modelo, cancelamento, menu de contexto) continuam.

## Layout

A partir de 1180 px de largura (ajustado ao DPI), original e reformulado ficam lado a lado; a barra «Reformular», os botões e o consolidado ocupam a largura toda embaixo. Mais estreito, volta o layout empilhado anterior.

## Destaques

- Itens: datas (12/08/2026, 12/08, 12-08-2026, 12 de agosto de 2026, dia 13), horários (14h, 14h00, 14h30min, 14:00, 14 horas, «às oito e vinte», meio-dia), CPF (formatado ou após a palavra CPF), placas antigas e Mercosul (maiúsculas), valores em reais (R$ 1.234,56, 50 reais, cinquenta reais) e telefones. Nomes próprios ficam de fora.
- Três cores:
  - **amarelo** (bate): item com equivalente exato no outro texto. Equivalentes: 14h = 14h00 = 14:00; R$ 50,00 = 50 reais = cinquenta reais; 12/08 = 12 de agosto; telefone com e sem DDD; placa com e sem hífen;
  - **laranja** (alerta, conferir): horário aproximado, relativo ou ambíguo, sempre marcado inteiro — «umas dez para as onze, por aí», «meio-dia e pouco», «umas duas e pouco», «por volta das 14h30», «depois das duas», e horários por extenso sem período («às duas e meia» = 2h30 ou 14h30). Nunca conta como «bate», nem quando a mesma expressão aparece nos dois textos. Um horário exato cujo único correspondente é aproximado (até 20 min) ou ambíguo também fica laranja;
  - **vermelho**: item do reformulado sem correspondência no original, e item do original que sumiu do reformulado.
- Horários em números (8h, 14h30) são exatos, no relógio de 24 horas.
- Resumo na segunda linha da barra de status, em até duas linhas, sem «…»: completo quando cabe; senão, só as contagens e «Clique para ver a lista». Clicar no resumo abre a lista completa (não encontrados, aproximados, sumiram e conferidos).
- Aplicado quando a geração termina (nunca durante a geração) e 0,6 s depois de uma edição.
- Botão «Destaques: ligados/desligados» no cabeçalho do texto reformulado.
- Só visual: a cor está na formatação do editor, não no texto. Copiar, Recortar, Ctrl+C, Ctrl+X, «Adicionar», .txt, Word e motor recebem texto simples. Cursor, seleção, rolagem e o histórico do Ctrl+Z não são alterados.

## Testes

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/review-highlight/Test-ReviewScanner.ps1
    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/review-highlight/Test-HighlightUi.ps1

O primeiro testa o detector sem janela (formatos, falsos positivos, equivalências, resumo). O segundo usa o motor real, sem modelo, e grava `print-claro.png`, `print-escuro.png` e `print-empilhado.png`; guarda e devolve a escolha de tema, a área de transferência e a cópia de recuperação do usuário. Janelas aparecem na tela por alguns segundos durante os prints.

Observação: o teste antigo `maintenance/context-menu` falha de vez em quando no passo do Ctrl+C, também com a interface anterior (4 de 6 execuções passaram com ela; 5 de 6 com a nova). A cópia agora tenta de novo por cerca de 1 s quando outro programa está usando a área de transferência.
