# Comparação semântica e técnica — DepoimentoLocal

Entradas idênticas ao anexo, sem alterações. Saídas obtidas pelo OriginalAppBridge de produção; nenhuma saída foi editada. O baseline foi conferido contra as dez saídas integrais do relatório anexado. A aprovação semântica abaixo é uma revisão manual desta execução, não uma garantia geral do modelo ou do validador.

| Teste | Antes: problema relevante | Depois: revisão semântica | Estado técnico / retry |
|---|---|---|---|
| 1 | Agressividade perdeu o destinatário; pronomes ambíguos na saída. | **Não aprovado integralmente** — Recupera destinatário da agressividade e preserva fontes; introduz ambiguidade no destinatário da fala de Carlos. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 2 | Warning de contagem de não; autocorreção consolidada, fontes e negativas majoritariamente preservadas. | **Não aprovado integralmente** — Primeira pessoa corrigida e fontes recuperadas no retry; persistem perda de referente, redução do horário e falso começo. | CONCLUÍDO COM ALERTAS / True |
| 3 | tinha batido → batia; disse que ouviu → ouviu que; depoente virou ele após citar Carlos. | **Fiel nos pontos avaliados** — Tempo composto, cadeia de relato, versões conflitantes e sujeito do depoimento preservados. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 4 | Ambiguidades centrais preservadas; sujeito do depoente frequentemente elíptico. | **Fiel nos pontos avaliados** — Preserva as ambiguidades e torna explícito o depoente nas percepções e dúvidas. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 5 | Patrícia tomou o lugar do depoente; destinatário virou ela; restou verifiquei; perdeu alcance de toda a conversa. | **Correção substancial, sem aprovação integral** — Recupera papéis explícitos, fontes e primeira pessoa; persistem elipses de sujeito e uma construção inadequada. | CONCLUÍDO COM ALERTAS / True |
| 6 | Me disseram virou Informou que; warning de fonte indireta aceito sem retry. | **Fiel nos pontos avaliados** — Preserva lhe disseram, participação, ausência, fontes nomeadas e graus de certeza. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 7 | Perdeu a proximidade e a condição de testemunha na primeira oração. | **Fiel nos pontos avaliados** — Recupera estava perto, preserva citações e separa percepção direta de relato posterior. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 8 | Eu vi a professora entrando virou simples afirmação da entrada. | **Fiel nos pontos avaliados** — Preserva a percepção visual, nomes/cargos, fonte indireta e dúvidas. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |
| 9 | Perdeu todo o início temporal; validador rejeitou, mas o retry repetiu a omissão. | **Fiel nos pontos avaliados** — Recupera quarta/quinta, conselho, mesmo dia/dia seguinte e antes do almoço; conclui após retry. | CONCLUÍDO COM ALERTAS / True |
| 10 | Eu não vi Pedro ameaçar ninguém virou Pedro não ameaçou ninguém, sem warning. | **Fiel nos pontos avaliados** — Mantém a negação da percepção, sem afirmar inexistência do fato. | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS / False |

## Métricas da mesma bateria

| Métrica | Antes | Depois |
|---|---:|---:|
| Total de testes previstos | 10 | 10 |
| Testes executados | 10 | 10 |
| Concluídos (GENERATION_OK) | 9 | 10 |
| Com warning do validador | 3 | 3 |
| Com primeira pessoa fora de citações (indício) | 1 | 0 |
| Com retry | 1 | 3 |
| Com erro | 1 | 0 |
| Com indício de truncamento | 0 | 0 |
| Com duplicação integral | 0 | 0 |
| Com parágrafos repetidos | 0 | 0 |
| Com ContextOverflowException | 0 | 0 |
| Tempo total (s) | 637.472 | 682.277 |

Warnings incluem tentativas rejeitadas e recuperadas no retry. Ausência de warning não equivale a aprovação semântica. Rejeição final significa que a tarefa de reformulação daquele caso não foi concluída, mesmo quando a proteção evitou aceitar um erro.

## Evidências por caso

### Teste 1

A saída preserva o horário aproximado, Carlos como fonte da batida na mesa, a negativa de percepção do depoente, a dúvida entre bater e colocar a mão, Maria como fonte da agressividade com ela, a ausência de agressão física observada, a dúvida sobre quem chamou o chefe e a reunião posterior recebida de terceiros. Porém “Carlos falou pra mim” vira “Carlos falou para ele”, deixando um destinatário que era explícito menos claro após citar João. A primeira frase também tem pontuação inadequada (“chegou lá acredita...”); “Depoente foi embora” perde o artigo. Não se identificou primeira pessoa, truncamento ou duplicação. A pendência de referente impede aprovação integral, apesar da ausência de warning.

Antes: 955 caracteres; depois: 1123 caracteres. Logs e saídas integrais em `baseline/test-01*` e `../../QA/Results/test-01*`.

### Teste 2

O retry mantém a percepção negada, o grito informado por Carlos, a pessoa não identificada como fonte da porta, a ordem incerta de saída e o desconhecimento sobre a conversa da chefia. Os reparos agora preservam “o depoente não entendeu”, “o depoente não ouviu esse grito” e “o depoente não participou”. Porém a saída omite “ela” em “não sabe o que quis dizer com isso”, deixando o sujeito menos claro, e mantém outros sujeitos elípticos após mencionar Paulo. O horário “três, três e pouco” fica reduzido a “uns três e pouco”. Mantém ainda o falso começo “Então saiu... Não, acredita que Carlos entrou antes de sua saída. Sim, o Carlos entrou antes”, enquanto o baseline consolidava melhor a autocorreção. Não se trata de duplicação integral, mas há repetição discursiva. São pendências reais; conclusão técnica não equivale a aprovação semântica.

Antes: 1061 caracteres; depois: 1180 caracteres. Logs e saídas integrais em `baseline/test-02*` e `../../QA/Results/test-02*`.

### Teste 3

A nova saída mantém 'Ana lhe disse que Roberto tinha batido', 'Carlos ... disse que ouviu Roberto falar', as duas versões sobre a porta e sobre sala/reunião, a falta de compreensão da frase inteira e a incerteza da ordem de saída. O depoente permanece explicitamente como quem estava perto, não viu os dois saindo juntos e não participou da conversa. As versões de Ana e Carlos sobre o dia permanecem atribuídas, sem escolher uma delas. Nenhuma omissão relevante, primeira pessoa ou duplicação identificada.

Antes: 1130 caracteres; depois: 1304 caracteres. Logs e saídas integrais em `baseline/test-03*` e `../../QA/Results/test-03*`.

### Teste 4

Mantém não identificar quem falou, a hipótese de Marcos ter respondido, a dúvida Felipe/diretor, a fala literal 'ele já sabe disso' e a ambiguidade Marcos/Felipe na informação de Renata. Mantém os três falando quase ao mesmo tempo, o depoente saindo antes e desconhecendo o que aconteceu depois. Não atribui referentes por adivinhação. Nenhuma perda relevante, primeira pessoa fora de aspas ou duplicação identificada.

Antes: 1355 caracteres; depois: 1479 caracteres. Logs e saídas integrais em `baseline/test-04*` e `../../QA/Results/test-04*`.

### Teste 5

O validador rejeitou no bloco 3/6 a substituição da ação explícita de Patrícia; o retry recuperou esse papel. Na saída final, o depoente volta a ser quem ouviu Fernanda, não entendeu o endereço, não ouviu mais discussão, não presenciou conversa particular e recebeu o comentário de Marcelo. Preserva o alcance de 'não prestou atenção em toda a conversa' e corrige 'não verifiquei' para 'o depoente não verificou'. Mantém horários aproximados, durações, sequência antes/depois do almoço, suposição sobre Fernanda/coordenador, relatos posteriores com fontes e dúvidas, a ausência de participação em reuniões e o encerramento integral. Não se identificaram omissões factuais relevantes, duplicação ou truncamento. Contudo, 'Ouviu o depoente ela dizer' é uma construção inadequada na passagem Patrícia/Marcelo; o reparo que conserva eu pode produzir ordem pós-verbal ruim. Também persistem elipses como 'Não sabe quem ligou para ela' após Patrícia sair e 'Não perguntou exatamente...' após Eduardo falar, tornando o depoente menos explícito que no original. As trocas nominais graves e a primeira pessoa do baseline foram corrigidas, mas não há aprovação integral de fidelidade/referência.

Antes: 6097 caracteres; depois: 6283 caracteres. Logs e saídas integrais em `baseline/test-05*` e `../../QA/Results/test-05*`.

### Teste 6

A saída mantém terça-feira e presença do depoente, horário desconhecido e estimado depois das duas com possibilidade de engano, Patrícia como fonte da atribuição ao diretor e ausência de confirmação. Mantém certeza sobre Carlos chegar depois do depoente, incerteza sobre Ana e ausência de lembrança de sua entrada. A segunda reunião é explicitamente recebida de terceiros ('Depois lhe disseram que houve outra reunião na quinta-feira'); o depoente não participou e desconhece os presentes. Não transforma esse relato em conhecimento presencial. Sem primeira pessoa fora de citações, omissão relevante, truncamento ou duplicação identificados.

Antes: 569 caracteres; depois: 649 caracteres. Logs e saídas integrais em `baseline/test-06*` e `../../QA/Results/test-06*`.

### Teste 7

Mantém o depoente perto, a percepção clara da primeira frase e ambas as citações sem alterar a primeira pessoa legítima dentro das aspas. Preserva a resposta incompreendida de Ricardo, Daniela como fonte posterior do pedido e o depoente não ter ouvido pessoalmente esse pedido. Mantém a dúvida sobre a quem Ricardo dirigiu a segunda frase. Não se identificaram omissões relevantes, atribuição indevida, truncamento ou duplicação; 'Também o depoente ouviu' tem ordem pouco natural, sem perda factual.

Antes: 421 caracteres; depois: 503 caracteres. Logs e saídas integrais em `baseline/test-07*` e `../../QA/Results/test-07*`.

### Teste 8

A primeira oração agora registra 'o depoente viu a professora Helena entrando', mantendo a condição de testemunha. Preserva professora Helena, diretor Paulo e coordenador Marcos sem trocar os cargos, a servidora como fonte do motivo da visita, o desconhecimento da veracidade e do assunto posterior, a ausência de audição da conversa e de visão da entrada do coordenador. Preserva a saída alguns minutos depois. Nenhuma omissão relevante, primeira pessoa fora de citações, truncamento ou duplicação identificados.

Antes: 487 caracteres; depois: 532 caracteres. Logs e saídas integrais em `baseline/test-08*` e `../../QA/Results/test-08*`.

### Teste 9

A saída final conserva a hipótese quarta-feira versus quinta, a certeza de ter sido depois da reunião do conselho, a dúvida entre mesmo dia e dia seguinte e a chegada antes do almoço. Mantém a conversa após a chegada sem intervalo conhecido, a saída/retorno, 'no dia seguinte, ou talvez dois dias depois', Carla como fonte da segunda conversa e a incerteza de quando ocorreu. O retry desta rodada foi acionado por perda do destinatário do relato de Carla, não por perda do início; a versão final mantém 'Carla lhe contou'. Nenhum indício de omissão inicial/final, truncamento, duplicação ou primeira pessoa residual. A ausência de overflow no baseline e a inspeção dos reparos apontam omissão na geração; não há evidência para atribuir a perda antiga a falta de contexto ou a um motivo específico de parada.

Antes: 348 caracteres; depois: 552 caracteres. Logs e saídas integrais em `baseline/test-09*` e `../../QA/Results/test-09*`.

### Teste 10

A saída inicia 'o depoente não viu Pedro ameaçar ninguém'. Mantém não ter ouvido ameaça contra Maria, ter visto discussão sem ver contato físico, Maria como fonte do medo sem relato de agressão, e Carlos como fonte de um barulho sem dizer ter visto Pedro bater. Conserva desconhecimento da origem do barulho e da presença do diretor, ausência de lembrança de sua entrada, reunião informada por terceiros, ausência de informação de punição, não participação e desconhecimento da decisão. Não se identificaram inversão do alcance de negação, omissões relevantes, primeira pessoa residual, truncamento ou duplicação.

Antes: 603 caracteres; depois: 672 caracteres. Logs e saídas integrais em `baseline/test-10*` e `../../QA/Results/test-10*`.

## Compilação, limites e arquivos

Compilados: launcher/interface (Release x64), interface + runner e SemanticGuard.cs com referências .NET 8. O projeto/fonte completo do motor não existe nesta distribuição; as correções compiladas foram incorporadas nas três DLLs usando a infraestrutura dnlib existente. Conferência de IL preserva todos os métodos fora da lista explícita de alterações. A integridade dos binários durante a bateria está em QA/Results/engine-integrity.json.

Consulte README.md para os arquivos modificados e os comandos de reprodução, diagnosis.md para causas e limites da evidência, guard-tests.txt e embedded-tests.txt para verificações adicionais. Os testes anteriores de aspecto, quantidade e reparos passaram. O validador continua heurístico: aceitações técnicas não dispensam revisão humana, e paráfrases legítimas podem provocar rejeições conservadoras.
