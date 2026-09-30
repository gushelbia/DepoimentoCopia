# Comparação semântica e técnica — DepoimentoLocal

Entradas idênticas ao anexo, sem alterações. Saídas obtidas pelo OriginalAppBridge de produção; nenhuma saída foi editada. Execução anterior preservada em `round-1-final/Results`. A aprovação semântica abaixo é uma revisão manual desta execução, não uma garantia geral do modelo ou do validador.

| Teste | Antes: problema relevante | Depois: revisão semântica | Estado técnico / retry |
|---|---|---|---|
| 1 | Recupera destinatário da agressividade e preserva fontes; introduz ambiguidade no destinatário da fala de Carlos. | **Aprovado na revisão desta execução** — Destinatário explícito, locativo preservado e ação separada da estimativa; não há perda semântica identificada. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 2 | Primeira pessoa corrigida e fontes recuperadas no retry; persistem perda de referente, redução do horário e falso começo. | **Aprovado na revisão desta execução** — Referente e horário completos; autocorreção consolidada com dúvida e referência explícita à saída do depoente. | CONCLUÍDO COM ALERTAS / True |
| 3 | Tempo composto, cadeia de relato, versões conflitantes e sujeito do depoimento preservados. | **Aprovado na revisão desta execução** — Versões conflitantes, fonte em cadeia, anterioridade e percepção permanecem preservadas. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 4 | Preserva as ambiguidades e torna explícito o depoente nas percepções e dúvidas. | **Aprovado na revisão desta execução** — Ambiguidades Marcos/Felipe/diretor e voz não identificada preservadas; depoente explícito nas percepções. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 5 | Recupera papéis explícitos, fontes e primeira pessoa; persistem elipses de sujeito e uma construção inadequada. | **Aprovado na revisão desta execução** — Sujeito pós-verbal corrigido e depoente explícito nas elipses perigosas; relato longo preservado. | CONCLUÍDO COM ALERTAS / True |
| 6 | Preserva lhe disseram, participação, ausência, fontes nomeadas e graus de certeza. | **Aprovado na revisão desta execução** — Fonte indireta, certeza, memória e ausência de participação preservadas, sem regressão. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 7 | Recupera estava perto, preserva citações e separa percepção direta de relato posterior. | **Aprovado na revisão desta execução** — Citações literais, proximidade, audição e fonte posterior preservadas. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 8 | Preserva a percepção visual, nomes/cargos, fonte indireta e dúvidas. | **Aprovado na revisão desta execução** — Percepção visual, cargos, fonte anônima e desconhecimento preservados. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 9 | Recupera quarta/quinta, conselho, mesmo dia/dia seguinte e antes do almoço; conclui após retry. | **Aprovado na revisão desta execução** — Início temporal completo, fonte indireta e foco de só preservados. | CONCLUÍDO COM ALERTAS / True |
| 10 | Mantém a negação da percepção, sem afirmar inexistência do fato. | **Aprovado na revisão desta execução** — Negação de percepção preservada, sem convertê-la em inexistência do fato. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |

## Métricas da mesma bateria

| Métrica | Antes | Depois |
|---|---:|---:|
| Total de testes previstos | 10 | 10 |
| Testes executados | 10 | 10 |
| Concluídos (GENERATION_OK) | 10 | 10 |
| Com warning do validador | 3 | 3 |
| Com primeira pessoa fora de citações (indício) | 0 | 0 |
| Com retry | 3 | 3 |
| Com erro | 0 | 0 |
| Com indício de truncamento | 0 | 0 |
| Com duplicação integral | 0 | 0 |
| Com parágrafos repetidos | 0 | 0 |
| Com ContextOverflowException | 0 | 0 |
| Tempo total (s) | 682.277 | 688.93 |

Warnings incluem tentativas rejeitadas e recuperadas no retry. Ausência de warning não equivale a aprovação semântica. Rejeição final significa que a tarefa de reformulação daquele caso não foi concluída, mesmo quando a proteção evitou aceitar um erro.

## Evidências por caso

### Teste 1

A nova saída diz “o depoente chegou lá; o depoente acredita que era umas duas e pouco” e “Carlos falou para o depoente”, eliminando os dois problemas principais da execução anterior. Mantém horário aproximado, falta de lembrança, aparência de discussão e ausência de audição do início; Carlos continua sendo a fonte da batida, que o depoente não viu. Mantém a dúvida bater/colocar a mão, Maria como fonte da agressividade com ela, ausência de relato de agressão física e de contato visto pelo depoente. Conserva a visão da entrada do chefe, a dúvida sobre quem o chamou, a saída antes do fim e a segunda reunião recebida de terceiros. O “ele” da fala de Maria permanece ambíguo como na entrada; o destinatário do relato de Carlos agora é explícito. Há repetição estilística de “o depoente”, sem duplicação de fatos. Não se identificou alteração de fonte, sujeito, percepção, negação, incerteza, temporalidade ou referente.

Antes: 1123 caracteres; depois: 1232 caracteres. Logs e saídas integrais em `round-1-final/Results/test-01*` e `../../QA/Results/test-01*`.

### Teste 2

Mantém “umas três, três e pouco” e “o que ela quis dizer”, sem reduzir o horário nem retirar o sujeito da oração subordinada. A correção efetiva ficou “O depoente acredita que Carlos entrou antes da saída do depoente”, preservando a crença e removendo o fragmento de saída abandonado e sua confirmação repetida. Carlos permanece o participante que entra e pergunta o que estava acontecendo; a ida buscar café fica explicitamente atribuída ao depoente. A pausa inicial em “então chegou...” foi mantida porque a chegada não foi retratada; ela não é o falso começo negado da saída. Conserva duração aproximada, talvez menos, ausência de percepção de contato, Carlos como fonte do grito não ouvido, fonte anônima da porta e ausência de visão desse ato, dúvida da ordem de saída e do dia da conversa com a chefia, não participação e ausência de relato sobre seu conteúdo. A frase citada está intacta. O retry recuperou a percepção negada; o warning final legado corresponde à remoção do “não” corretivo, não a uma negação factual perdida. Persistem formas coloquiais e algumas elipses com referente recuperável no contexto; não se identificou troca de fonte, sujeito, percepção, negação, grau de certeza, tempo ou referente nesta saída.

Antes: 1180 caracteres; depois: 1306 caracteres. Logs e saídas integrais em `round-1-final/Results/test-02*` e `../../QA/Results/test-02*`.

### Teste 3

Preserva Ana como fonte de “tinha batido”, Carlos como fonte da versão de porta já fechada e o depoente sem saber qual está correta. Mantém “Carlos disse que ouviu Roberto”, sem eliminar a camada intermediária, e a divergência sala/reunião. Mantém proximidade e incompreensão do depoente, dúvida sobre a ordem de saída, a servidora como fonte da saída conjunta não presenciada e versões conflitantes sobre o dia da conversa com a chefia. As mudanças em relação à execução anterior explicitam o depoente em lembrança, possibilidade de engano e ouvir dizer; não transferem ações entre pessoas. Nenhuma mudança relevante de conteúdo, fonte, negação, incerteza, tempo ou referente foi identificada.

Antes: 1304 caracteres; depois: 1337 caracteres. Logs e saídas integrais em `round-1-final/Results/test-03*` e `../../QA/Results/test-03*`.

### Teste 4

Preserva desconhecer quem chegou primeiro, a voz não identificada, a hipótese de Marcos ter respondido com possibilidade de engano e as dúvidas sobre Felipe/diretor e Marcos/Felipe. Não escolhe referentes para “ele já sabe disso”; a citação está intacta. Conserva a reunião anterior, as três pessoas falando quase ao mesmo tempo, a falta de compreensão, o depoente não ter perguntado, sua saída antes dos demais e o desconhecimento do desfecho. A explicitação do depoente nas limitações de percepção/memória não resolve as ambiguidades dos outros participantes. A grafia “entrou” da execução anterior foi preservada na versão final; o erro de uma tentativa com outro prompt foi descartado. Não se identificou regressão semântica ou omissão.

Antes: 1479 caracteres; depois: 1512 caracteres. Logs e saídas integrais em `round-1-final/Results/test-04*` e `../../QA/Results/test-04*`.

### Teste 5

A construção inadequada virou “O depoente ouviu ela dizer que aquilo já tinha acontecido antes”. Agora aparecem “O depoente não sabe quem ligou para ela”, “O depoente não sabe dizer quanto tempo depois”, “O depoente não perguntou exatamente a que situação ele estava se referindo” e “O depoente não sabe se essa conversa realmente aconteceu”. As ações nomeadas de Patrícia, Eduardo, Marcelo, Fernanda e do coordenador permanecem com seus sujeitos; o retry do bloco 3 recuperou a atribuição explícita de Patrícia. A revisão cobriu os seis blocos: chegada aproximada antes das dez, intervalos, documento do dia anterior, atenção parcial, alternativas de endereço e de motivo da saída, tela não vista, tom alto sem afirmar gritos, conversas simultâneas, ligação, reenvio, informação de Fernanda sem percepção da conversa particular e sem certeza de sua fonte, retorno após o almoço e ordem de chegada, suposição sobre o coordenador, reunião de meia hora não presenciada, comentário incerto de Marcelo, falas posteriores sem intenção/punição/denúncia, entrega informada sem verificação, conversa posterior não confirmada, observação de Fernanda e limites finais de conhecimento. Início, meio e final estão presentes. Não há primeira pessoa fora de citações, trocas nominais, perda factual relevante, truncamento ou duplicação identificados. Continuam formas coloquiais, repetições de “o depoente” e elipses com sujeito sustentado pelo contexto; não se resolveu por adivinhação a referência das falas originalmente ambíguas.

Antes: 6283 caracteres; depois: 6492 caracteres. Logs e saídas integrais em `round-1-final/Results/test-05*` e `../../QA/Results/test-05*`.

### Teste 6

Mantém terça-feira por presença, desconhecimento do horário exato, estimativa após as duas com possibilidade de engano, Patrícia como fonte da atribuição ao diretor e ausência de confirmação. Preserva a certeza sobre Carlos entrar depois do depoente, incerteza sobre Ana e falta de lembrança de sua entrada. A reunião de quinta-feira segue como “Depois lhe disseram”, e o depoente não participou nem sabe os presentes. A versão final mantém “depois dele”, como na execução anterior de referência; a construção “depois do seu” de uma tentativa com outro prompt foi descartada. As alterações finais explicitam o depoente nas limitações e crenças, sem trocar categorias de conhecimento ou atribuições.

Antes: 649 caracteres; depois: 715 caracteres. Logs e saídas integrais em `round-1-final/Results/test-06*` e `../../QA/Results/test-06*`.

### Teste 7

As duas citações permanecem literais, inclusive a primeira pessoa legítima dentro das aspas. Preserva o depoente perto e ouvindo claramente a primeira fala, a resposta de Ricardo não compreendida, Daniela como fonte posterior do pedido de revisão, esse pedido não ouvido pessoalmente e a dúvida sobre o destinatário da segunda fala. A diferença principal é tornar explícito quem não sabe a quem a fala foi dirigida. Nenhuma alteração de fonte, sujeito, percepção, negação ou referente foi identificada.

Antes: 503 caracteres; depois: 514 caracteres. Logs e saídas integrais em `round-1-final/Results/test-07*` e `../../QA/Results/test-07*`.

### Teste 8

A entrada de Helena continua sendo vista pelo depoente. Helena permanece professora, Paulo diretor e Marcos coordenador. A servidora continua como fonte do possível motivo da visita, sem veracidade confirmada. Mantém o desconhecimento do conteúdo das conversas, a dúvida sobre serem do mesmo assunto, a saída alguns minutos depois e o depoente não ter visto a entrada do coordenador. A negativa auditiva ficou explicitamente atribuída ao depoente. Não se identificou regressão, perda temporal ou troca de pessoa/cargo.

Antes: 532 caracteres; depois: 543 caracteres. Logs e saídas integrais em `round-1-final/Results/test-08*` e `../../QA/Results/test-08*`.

### Teste 9

Mantém a hipótese quarta/quinta, certeza de posterioridade ao conselho com dúvida mesmo dia/dia seguinte, chegada antes do almoço e conversa posterior à chegada sem intervalo conhecido. Mantém saída e retorno, ausência dos dois no retorno, relato de Carla no dia seguinte ou talvez dois dias depois e desconhecimento da data da segunda conversa. O retry recuperou o destinatário do relato de Carla: lhe contou. A expressão final é ‘o depoente só não lembra’, preservando o foco no predicado; não afirma que apenas o depoente desconhecia a data. A alteração de foco vista na tentativa intermediária não aparece nesta execução final. Não se identificou omissão, resolução indevida de ambiguidade temporal, troca de fonte ou aumento de certeza.

Antes: 552 caracteres; depois: 607 caracteres. Logs e saídas integrais em `round-1-final/Results/test-09*` e `../../QA/Results/test-09*`.

### Teste 10

A saída conserva ‘o depoente não viu Pedro ameaçar ninguém’, a ausência de audição da ameaça e de visão de contato físico, enquanto preserva ter visto a discussão. Maria continua como fonte do medo, sem relato de agressão física; Carlos continua como fonte do barulho, sem dizer que viu Pedro bater na mesa. Mantém origem desconhecida do ruído, dúvida sobre a presença do diretor, falta de lembrança de vê-lo entrar, reunião recebida de terceiros, ausência de relato de punição, não participação e desconhecimento da decisão. ‘Também’ permanece junto das limitações auditiva e de conhecimento, sem criar exclusividade do sujeito. Não se identificou transferência de negação para o fato nem perda de fonte, percepção ou incerteza.

Antes: 672 caracteres; depois: 705 caracteres. Logs e saídas integrais em `round-1-final/Results/test-10*` e `../../QA/Results/test-10*`.

## Compilação, limites e arquivos

Compilados: launcher/interface (Release x64), interface + runner e SemanticGuard.cs com referências .NET 8. O projeto/fonte completo do motor não existe nesta distribuição; as correções compiladas foram incorporadas nas três DLLs usando a infraestrutura dnlib existente. Conferência de IL preserva todos os métodos fora da lista explícita de alterações. A integridade dos binários durante a bateria está em QA/Results/engine-integrity.json.

Consulte README.md para os arquivos modificados e os comandos de reprodução, diagnosis.md para causas e limites da evidência, guard-tests.txt e embedded-tests.txt para verificações adicionais. Os testes anteriores de aspecto, quantidade e reparos passaram. O validador continua heurístico: aceitações técnicas não dispensam revisão humana, e paráfrases legítimas podem provocar rejeições conservadoras.
