# Rodada 3 — correções a partir da bateria inédita (29/09/2026)

Base: `QA-Inedita-20260929/RELATORIO-INTEGRAL.md`. O estado anterior (fontes, DLLs, resultados das duas baterias e o relatório) está em `round-3-before/`. O prompt (`SystemPrompt.txt`) não foi alterado: a correção continua concentrada em reparo local ancorado na entrada e em validação. Nenhuma regra usa nomes, frases ou episódios das baterias; os testes auxiliares usam outros nomes e cenários.

## Achados do relatório e correção aplicada

| Achado (casos) | Causa | Correção |
|---|---|---|
| Primeira pessoa residual: «abri», «Fechei», «li», «examinei», «Consultei», «levei», «o depoente trabalho» (1, 5, 12, 14, 15) | O reparo só conhecia uma lista fechada de conjugações; o validador não conhecia formas em -i/irregulares, e «o depoente li» passava. | `RepairNarratorVerbs`: converte formas de primeira pessoa **presentes na entrada** (pretérito regular em -ei, com -quei/-guei/-cei, e lista explícita de formas em -i, irregulares e presentes). Insere «o depoente» somente em fronteira de oração sem sujeito; após sujeito nomeado apenas conjuga. Presentes que também são substantivos («trabalho», «preciso») só são convertidos logo após «o depoente». O validador passou a rejeitar essas formas e «nosso/nossa/nós» residuais. |
| Ação do depoente virou percepção: «Conferi… e assinei» → «viu conferir… e assinar»; «estava comigo» → «Viu Berenice» (1) | Não havia verificação de percepção acrescentada. | Validação rejeita mais atos de ver/ouvir/presenciar na saída do que na entrada, e «viu/ouviu + infinitivo» quando a entrada tem a ação em primeira pessoa. Não há reparo: vai para retry/revisão. |
| Destinatário omitido ou corrompido: «Yara me disse» → «Yara disse»; «Mirela me disse» → «Mirela o depoente disse»; «ela me entregou» → «ela entregou» (6, 10, 14) | A verificação de destinatário aceitava qualquer «depoente» na mesma frase. | `RepairRecipients` restaura «lhe» no mesmo falante quando a moldura é única; validação exige o destinatário ligado ao falante. |
| Dois «lhe» na mesma cadeia: «Sílvia lhe contou que Raul lhe disse» (5) | Conversão padrão me→lhe cria ambiguidade quando a entrada já tem outro «lhe». | Nesse caso o reparo usa «contou ao depoente»; a validação rejeita dois «lhe» sem o depoente explícito. |
| Sujeito elíptico após fala de outro participante: «Respondeu que não», «Não perguntou quem era», «a conversa que ouviu» (4, 11, 15; também 3 e 12) | O reparo de sujeito exigia o complemento inteiro idêntico; aspas e paráfrases o impediam. | `RepairEllipticalNarrator`: início de frase ancorado no verbo + duas palavras seguintes, e oração relativa ancorada na palavra anterior + conectivo, ambos únicos na entrada conjugada e na saída. Não insere sujeito em complemento de verbo do próprio depoente («acredita que visitou»). Validação rejeita a elipse se a frase anterior não é do depoente. |
| «ele estava de folga» para o «eu» da entrada (3) | Pronome atribui gênero e compete com participantes. | `RepairNarratorPronoun`: restaura «o depoente» apenas com moldura única e inexistente para terceiros na entrada. |
| Autocorreções não consolidadas: «sexta… não, na quinta»; «recepcionista, quer dizer, para a estagiária» (3) | A consolidação existente exigia confirmação «é,/sim,». | `RepairParallelCorrections`: remove o fragmento abandonado quando ele e a correção começam pela mesma preposição/artigo e o texto da saída coincide com a entrada. Correções não paralelas («saí às dez… corrigindo, isso foi quando cheguei») permanecem como estão. |
| Rejeição lexical excessiva: «eu falei que não tava com a chave» vs «respondeu que não tinha a chave» (2) | Âncoras de cobertura comparavam «falei»/«tava» literalmente; em empate, a frase comparada podia ser outra. | Âncoras normalizam formas do narrador e paráfrases de verbos de fala; em empate prevalece a frase mais específica. |
| Redação: «atrás de o depoente», «; O depoente» (3, 10) | — | Contração (exceto sujeito de infinitivo) e minúscula após ponto e vírgula. |

| Falha revelada pela 1ª execução desta rodada: o modelo escreveu «Eu pedi que eu guardasse» (pedido era de Vilma) e o novo reparo transformaria «o depoente pedi» em «o depoente pediu», escondendo o erro (12) | Reparo aceitava formas do dicionário ausentes da entrada. | Após «o depoente», só se converte forma presente na entrada. Nova validação rejeita «o depoente + pretérito» quando a entrada usa esse verbo apenas para outro participante. |
| «nossa conversa» (12) | Primeira pessoa do plural. | «a conversa de que o depoente participava», sem nomear os demais; só para substantivos de interação e quando a expressão está na entrada. Demais «nosso/nossa» são rejeitados. |
| «Respondeu que o depoente ainda…» para «Respondi que eu ainda…» (12) | Sujeito deslocado para a subordinada. | Reparo move o depoente para o verbo principal; validação rejeita a forma deslocada. |

## Não corrigido nesta rodada

- A rejeição de um bloco continua interrompendo os blocos seguintes (caso 12). Mudar isso exige alterar a máquina de estados de `Reformulate_Click` no motor, fora do mecanismo de manutenção usado até aqui.
- Omissões de conteúdo (ex.: causa do uso do celular no caso 12) continuam dependendo do modelo; o validador rejeita, não reconstrói.
- O analisador automático do relatório (`Report.ps1`) não foi alterado, para manter as contagens comparáveis com a execução anterior.

## Verificação

- `guard-tests.txt` (61), `reference-tests.txt` (59), `embedded-tests.txt` (26): todos PASS, incluindo os testes das rodadas anteriores.
- Reaplicação dos reparos/validação sobre as saídas finais das duas baterias: nenhuma saída antes aprovada passou a ser rejeitada.
- DLLs finais: `DepoimentoLocal.dll` e `DepoimentoLocal.Vulkan8.dll` `96c6e2d4…fe4a`; `DepoimentoLocal.CPU-original.dll` `107097d8…a5de`. Inicializadores e `SHA256SUMS.txt` atualizados só nesses hashes. `SystemPrompt.txt` idêntico ao anterior (`347fadb6…`). `engine-integrity.json` confirma DLLs inalteradas durante as duas baterias.

## Resultados — execução final real (29/09/2026), uma execução por caso

O modelo roda com temperatura 0: a execução final repetiu byte a byte as saídas da tentativa anterior (`round-3-attempt-2-Results`), exceto no caso 12, onde a nova validação forçou o retry correto do bloco 2. `round-3-attempt-1-Results` e `round-3-attempt-2-Results` são tentativas intermediárias; `round-3-before/` guarda os resultados que originaram o relatório.

### Bateria inédita (15 casos) — `QA-Inedita-20260929/Results`

| Caso | Antes | Agora | Revisão manual |
|---|---|---|---|
| 1 | ERRO; rascunho com primeira pessoa e percepção inventada | ERRO | Rejeição correta: o modelo continua escrevendo «Viu Berenice» e «viu conferir… e assinar». Rascunho sem primeira pessoa. Falha operacional; nenhuma alteração passou. |
| 2 | ERRO (rejeição lexical) | Concluído, retry | Aprovado. Mesmo texto fiel do rascunho anterior, agora aceito. |
| 3 | Falsos começos mantidos | Concluído, aviso legado de contagem de negação | Aprovado com ressalva de redação: sexta/quinta e recepcionista/estagiária consolidados, «ele» → «o depoente». Permanece «saiu às dez… Corrigindo, isso foi quando o depoente chegou», correção não paralela, conteúdo recuperável. |
| 4 | «Respondeu que não» | Concluído, retry | Aprovado: «O depoente respondeu que não». |
| 5 | ERRO | Concluído sem alertas | Aprovado: «Sílvia contou ao depoente que Raul lhe disse»; «leu», «examinou» corrigidos. |
| 6 | Destinatário omitido | Concluído sem alertas | Aprovado: «Yara lhe disse». |
| 7, 8, 9, 13 | Aprovados | Concluídos sem alertas | Aprovados; 7 ganhou «o depoente recebeu um aviso». |
| 10 | «Mirela o depoente disse» | Concluído sem alertas | Aprovado: «Mirela lhe disse», «atrás do depoente», «O depoente não sabe a que dia». |
| 11 | «conversa que ouviu» | Concluído sem alertas | Aprovado: «conversa que o depoente ouviu». |
| 12 | ERRO no bloco 1/5, saída parcial | Concluído, 5/5 blocos, retry nos blocos 2 e 3 | Completo; causa do celular preservada, nenhum trecho ausente. Ressalvas moderadas: «Respondeu que ainda não tinha…» (elipse após pergunta de Solange; «Respondi que» aparece duas vezes na entrada, por isso o reparo não atua) e «na sua presença» duas vezes (Neide/Gaspar podem ser lidos como donos de «sua»). Não aprovado integralmente. |
| 14 | «o depoente li», destinatário omitido | Concluído sem alertas | Aprovado: «leu», «ela lhe entregou». |
| 15 | ERRO | Concluído sem alertas | Aprovado: «O depoente não levou», «O depoente não perguntou». |

Resumo: concluídos 14/15 (antes 10/15); aprovados integralmente na revisão 12/15 (antes 4/15); com ressalva 2 (3 e 12); rejeitado 1 (1, proteção correta). Nenhuma primeira pessoa fora de citações e nenhuma inversão de fonte ou percepção nas saídas aceitas.

### Bateria anterior (10 casos) — regressão, `QA/Results`

10/10 concluídos. Oito saídas idênticas byte a byte às aprovadas na rodada 2 (`round-3-before/QA-Results`); casos 1 e 5 só ganharam «o depoente» explícito («quando o depoente voltou», «O depoente voltou a prestar atenção»). Sem regressão identificada.

### Limite desta avaliação

Os 15 casos inéditos passaram a ser, nesta rodada, casos de desenvolvimento: os achados deles orientaram as regras, embora nenhuma regra use seus nomes ou frases. Os números acima não medem generalização. Uma nova bateria inédita, preparada sem conhecer estas regras, é necessária para isso.
