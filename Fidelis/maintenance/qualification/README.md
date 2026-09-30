# Qualificação, Word formatado e rascunho reabrível

Mudanças só em `ui/ModernShell.cs`. O motor e o texto gerado não mudam. As proteções anteriores continuam valendo: salvamento automático, recuperação, pergunta ao fechar, destaques, lado a lado e cancelamento.

## Campos de qualificação

- Painel «Qualificação» recolhível, fechado por padrão. Fechado, mostra uma linha de resumo: procedimento, depoente, condição e data.
- Campos:
  - nº do procedimento, unidade e local;
  - data e hora, preenchidas com o momento em que o programa abre e editáveis;
  - nome do depoente, documento (RG/CPF) e condição (vítima, testemunha, investigado ou declarante);
  - endereço e telefone;
  - autoridade e escrivão.
- CPF: os dígitos verificadores são conferidos quando o documento parece um CPF. Um CPF inválido gera aviso no campo e no resumo, mas não impede salvar nem exportar. RG não é conferido.
- Com o painel aberto numa janela baixa ou estreita, a janela ganha rolagem vertical em vez de espremer os quadros de texto (mínimo de cerca de 140 px por quadro). Com o painel fechado, o layout é o de antes.
- «Limpar campos» pede confirmação («Não» é o padrão) e não mexe no texto dos quadros.
- **Os campos nunca vão para o modelo.** Eles só entram no cabeçalho do Word e no rascunho. O teste faz uma geração real com marcadores em todos os campos e confere o que o motor recebeu.

## Word formatado (`SimpleDocx`)

- Papel A4, Times New Roman 12, texto justificado e espaçamento 1,5.
- Margens: 3 cm à esquerda e em cima, 2 cm à direita e embaixo.
- Rodapé «Página X de Y».
- Título conforme a condição:
  - vítima ou declarante: TERMO DE DECLARAÇÕES;
  - investigado: TERMO DE INTERROGATÓRIO;
  - testemunha ou sem condição: TERMO DE DEPOIMENTO.
- Cabeçalho com os campos preenchidos, com rótulo em negrito. Campos vazios não aparecem.
- Cada linha do consolidado vira um parágrafo, sem alteração. Uma linha em branco vira espaço entre os parágrafos.
- No final, linhas de assinatura do depoente, da autoridade e do escrivão, com os nomes quando preenchidos. Elas não se separam entre páginas.
- «Exportar Word» e a opção Word ao fechar usam os campos da tela.

## Rascunho

- «Salvar rascunho» grava um `.txt` legível no Bloco de Notas. O arquivo tem:
  - a linha `[DEPOIMENTO LOCAL - RASCUNHO]`;
  - um campo por linha (`depoente: …`);
  - a linha `[TEXTO]`, seguida do texto consolidado sem alteração.
- Também salva quando só os campos estão preenchidos.
- «Abrir rascunho», ao lado, reabre texto e campos.
  - Um `.txt` antigo, sem o cabeçalho, abre como texto. Se houver campos preenchidos, o programa pergunta se deve limpá-los, para não misturar os dados de um depoimento com o texto de outro. «Sim, limpar» é o padrão, e a data e a hora são renovadas.
  - Um arquivo que não é texto é recusado sem apagar nada.
- Se houver alteração não salva (no texto ou nos campos), o programa pergunta antes de substituir. «Não» é o padrão.
- O salvamento automático e a recuperação guardam e devolvem texto e campos. Mudar só um campo também dispara o salvamento automático. Cópias antigas, só com texto, continuam sendo recuperadas.
- Em janelas estreitas, «Adicionar ao depoimento» vira «Adicionar» (o nome completo fica na dica), para todos os botões caberem.

## Testes

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/qualification/Test-Qualification.ps1
    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/word-export/Test-WordExport.ps1
    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/draft/Test-Draft.ps1

Os testes de janela usam o motor real, sem modelo. A exceção é o teste de qualificação, que carrega o modelo para a prova de que os campos não vazam. As janelas de diálogo são respondidas por `maintenance/dialogs/DialogKit.cs`, que digita o nome do arquivo e só confirma se o nome na caixa for exatamente o pedido. A escolha de tema e a cópia de recuperação do usuário são guardadas e devolvidas.

Prints:
- `Print-Screens.ps1` grava `print-*.png` nesta pasta.
- `maintenance/word-export/Render-DocxPreview.ps1` desenha uma prévia em PNG de um `.docx`, porque o Word não está instalado nesta máquina. A prévia é uma aproximação feita a partir do arquivo, não a renderização do Word.
