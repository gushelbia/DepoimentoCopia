# Relatório integral — bateria inédita do DepoimentoLocal

Avaliação em 29/09/2026. Quinze casos novos, uma execução por entrada, com o retry interno normal. Software, prompt, reparos locais e validador congelados. Nenhuma saída foi corrigida. Execução real pelo OriginalAppBridge e botão Reformular; logs e saídas literais preservados em Results/.

Os casos são sintéticos e foram preparados antes da execução, pelo mesmo assistente que conhece as correções anteriores. São textos inéditos, mas não constituem uma avaliação cega independente ou amostra estatística. Resultados não devem ser extrapolados como taxa de acerto em depoimentos reais.

## Conclusão

A bateria inédita não reproduziu o desempenho integral da bateria anterior: 10/15 gerações concluíram, cinco foram rejeitadas e somente quatro casos receberam aprovação integral nesta revisão (7, 8, 9 e 13). O caso 3 manteve o conteúdo corrigido recuperável, mas não consolidou os falsos começos. O caso 2 foi rejeitado apesar de um rascunho aparentemente fiel, indicando provável rejeição lexical excessiva.

Foram identificadas alterações, omissões ou ambiguidades novas em nove saídas, incluindo rascunhos rejeitados: 1, 4, 5, 6, 10, 11, 12, 14 e 15. Cinco dessas saídas foram aceitas pelo programa (4, 6, 10, 11 e 14); quatro passaram sem qualquer warning (6, 10, 11 e 14). Essas contagens incluem ambiguidade introduzida — não significam nove inversões factuais inequívocas. Os achados de gravidade alta estão nos casos 1 (agente da conferência/assinatura virou percepção), 10 (fonte/destinatário de relato corrompidos) e 12 (omissão no trecho gerado e interrupção com grande parte do relato ausente). Há problemas moderados de elipse, destinatário e primeira pessoa nos demais casos indicados. Não foi identificada inversão crítica do tipo “não viu acontecer” → “não aconteceu” nesta execução.

O validador conteve quatro rascunhos com primeira pessoa (1, 5, 12 e 15), mas deixou passar “o depoente li” no caso 14. A revisão manual encontrou primeira pessoa fora de citações em cinco casos, enquanto o analisador automático externo indicou apenas um. Isso demonstra uma lacuna também na medição automática. O texto longo parou por rejeição no bloco 1/5; não houve ContextOverflowException. Sua incompletude não foi marcada pelo detector automático de truncamento, pois o rascunho termina com ponto. Não se observou duplicação factual ou integral.

Houve acertos nas negativas encadeadas, alternativas temporais, versões conflitantes e retratação explícita. As citações literais foram preservadas no caso 10, apesar do defeito de atribuição fora das aspas. Os nomes e cargos próximos foram preservados no caso 11, apesar da ambiguidade sobre quem ouviu. Os detalhes aparentemente irrelevantes do caso 14 também permaneceram.

A conclusão é de generalização parcial: há proteções úteis, mas persistem falhas não sinalizadas e rejeições que impedem concluir tarefas. Esta evidência não sustenta aprovação geral de fidelidade nem uso sem conferência integral da entrada e da saída. O software não foi modificado e nenhum caso foi repetido para selecionar melhor resultado. A etapa termina com documentação dos achados e aguarda nova instrução, inclusive a classificação final de gravidade pelo usuário.

## Métricas técnicas

| Métrica automática | Quantidade |
|---|---:|
| Total de testes previstos | 15 |
| Testes executados | 15 |
| Concluídos (GENERATION_OK) | 10 |
| Com warning do validador | 6 |
| Com primeira pessoa fora de citações (indício) | 1 |
| Com retry | 6 |
| Com erro | 5 |
| Com indício de truncamento | 0 |
| Com duplicação integral | 0 |
| Com parágrafos repetidos | 0 |
| Com ContextOverflowException | 0 |
| Tempo total (s) | 575.14 |

A detecção automática de primeira pessoa é lexical e incompleta. A contagem manual abaixo prevalece para a avaliação. Erro/exceção de rejeição semântica não significa necessariamente queda do processo. Rascunho visível após rejeição não é resultado aprovado.

## Revisão manual — resultados e gravidade

| Caso | Conclusão técnica | Avaliação manual | Gravidade | Alteração semântica identificada |
|---|---|---|---|---|
| 1 | ERRO | Reprovado | Alta + falha operacional | True |
| 2 | ERRO | Não concluído; rascunho semanticamente preservado | Falha operacional | False |
| 3 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Ressalva de redação; não aprovado integralmente | Moderada — autocorreção não consolidada | False |
| 4 | CONCLUÍDO COM ALERTAS | Reprovado na revisão semântica | Moderada — ambiguidade nova de sujeito | True |
| 5 | ERRO | Reprovado | Moderada + falha operacional | True |
| 6 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Reprovado na revisão semântica | Moderada — destinatário omitido | True |
| 7 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Aprovado | Sem erro semântico identificado | False |
| 8 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Aprovado | Sem erro semântico identificado | False |
| 9 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Aprovado | Sem erro semântico identificado | False |
| 10 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Reprovado na revisão semântica | Alta — atribuição de relato comprometida | True |
| 11 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Reprovado na revisão semântica | Moderada — ambiguidade nova de percepção | True |
| 12 | ERRO | Reprovado; saída parcial | Alta + falha operacional | True |
| 13 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Aprovado | Sem erro semântico identificado | False |
| 14 | CONCLUÍDO SEM ALERTAS AUTOMÁTICOS | Reprovado | Moderada — primeira pessoa e destinatário omitido | True |
| 15 | ERRO | Reprovado | Moderada + falha operacional | True |

- Aprovados integralmente na revisão: 4
- Com alteração semântica identificada, incluindo rascunhos rejeitados: 9
- Com primeira pessoa fora de citações na revisão manual: 5
- Com corte/omissão de trecho final na revisão manual: 1
- Com duplicação factual na revisão manual: 0

- Casos com retry: 1, 2, 4, 5, 12, 15.
- Casos com warning do validador: 1, 2, 4, 5, 12, 15.
- Casos com alteração semântica: 1, 4, 5, 6, 10, 11, 12, 14, 15.
- Casos aprovados integralmente: 7, 8, 9, 13.

Gravidade: crítica para inversão de percepção/negação ou falsa atribuição grave; alta para troca de fonte/sujeito, certeza indevida, mudança cronológica ou omissão relevante; moderada para ambiguidade introduzida ou problema estrutural; estilo para redação sem mudança identificada de conteúdo. Falhas operacionais são registradas separadamente. A classificação é preliminar e permite a revisão do usuário.

## Evidências integrais por caso

### Caso 1 — Relato simples e direto

Objetivo: Preservar ações observadas, participantes e sequência.

**Resultado:** Reprovado. **Gravidade:** Alta + falha operacional.

**Achados:** A geração foi rejeitada, mas o rascunho permanece na tela. «Conferi os números das etiquetas e assinei o recibo» virou «O depoente viu conferir os números das etiquetas e assinar o recibo»: execução pelo depoente virou percepção de ação com agente indeterminado. «Vi Otávio colocar» virou afirmação direta «Otávio colocou», sem marcar a percepção. O cargo de zeladora foi omitido. «Berenice estava comigo» virou «Viu Berenice com o depoente», com sujeito elíptico e construção ambígua. Há primeira pessoa residual que o analisador externo não detectou; o validador do aplicativo detectou «fechei» e bloqueou.

**Demais verificações semânticas:** Duas caixas lacradas, localização, distribuição das caixas, horários oito/oito e vinte e negativa de abertura constam do rascunho, embora esta última permaneça em primeira pessoa. Não há citação, data ou grau de incerteza na entrada. Não há duplicação ou corte textual identificado.

**Redação:** Concordância inadequada em «o depoente abri».

**Primeira pessoa fora de citações — revisão manual:** abri; Fechei; Não abri.

**Registro técnico:** ERRO; retry=True; erro=True; exceção=True; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 38.769 s. Entrada: 317 caracteres. Saída: 344 caracteres. Captura conferida por GENERATION_OK: não disponível; rascunho capturado da tela.

**Warnings do validador:**

- [2026-09-29 17:25:44.504 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: a zeladora berenice estava comigo.
- [2026-09-29 17:26:03.850 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: possível primeira pessoa residual: fechei

**Warnings nativos:** 4. Constam no [log integral](Results/test-01-log.txt), preservados separadamente dos warnings do validador.

**Erro/exceção integral registrado:**

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=317; elapsedMs=38769
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: possível primeira pessoa residual: fechei. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

**Entrada integral:**

```text
Eu abri o almoxarifado às oito horas. A zeladora Berenice estava comigo. Vi Otávio colocar duas caixas lacradas sobre o balcão. Conferi os números das etiquetas e assinei o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. Fechei a porta às oito e vinte. Não abri nenhuma das caixas.
```

**Saída integral, sem edição — rascunho de geração rejeitada:**

```text
Relatou que o depoente abri o almoxarifado às oito horas. Viu Berenice com o depoente. Otávio colocou duas caixas lacradas sobre o balcão. O depoente viu conferir os números das etiquetas e assinar o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. Fechei a porta às oito e vinte. Não abri nenhuma das caixas.
```

SHA-256 da saída: `342474A705AF7EEFA5E93581E24F2152D912AEA7FC495D9A9150B8B845A99A29`. [Saída original](Results/test-01-output.txt) · [Registro completo](Results/test-01.json).

### Caso 2 — Coloquial e desorganizado

Objetivo: Preservar aproximações, destinatários e ordem sem polir fatos.

**Resultado:** Não concluído; rascunho semanticamente preservado. **Gravidade:** Falha operacional.

**Achados:** O validador rejeitou a oração «eu falei que não tava com a chave», embora o rascunho a expresse como «O depoente respondeu que não tinha a chave». No contexto do episódio, a negativa de posse naquele momento permanece; trata-se de provável rejeição excessivamente lexical, e não de omissão factual observada. A geração não foi aceita, portanto o caso não é aprovado operacionalmente. «Respondeu»/«atendeu» são reformulações do diálogo e da ida atender, sem mudança relevante identificada nesta leitura.

**Demais verificações semânticas:** Pátio e proximidade do bebedouro, depois do sinal, horário aproximado, Davi como fonte/destinatário explícito, visão anterior de Elisa e alternativa abriu/olhou, chave desaparecida segundo Davi, negativa do depoente, ônibus/barulho/audição incompleta, chave vista sem identificação e fim do acompanhamento foram preservados. Não há citações ou cargos a preservar. Sem primeira pessoa, duplicação ou corte identificado.

**Redação:** Mantém «foi assim», «por aí» e outras marcas coloquiais.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** ERRO; retry=True; erro=True; exceção=True; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 57.485 s. Entrada: 533 caracteres. Saída: 640 caracteres. Captura conferida por GENERATION_OK: não disponível; rascunho capturado da tela.

**Warnings do validador:**

- [2026-09-29 17:26:35.819 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: marcador temporal ou de certeza omitido: perto
- [2026-09-29 17:27:04.196 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: possível omissão de oração: eu falei que não tava com a chave.

**Warnings nativos:** 4. Constam no [log integral](Results/test-02-log.txt), preservados separadamente dos warnings do validador.

**Erro/exceção integral registrado:**

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=533; elapsedMs=57485
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: possível omissão de oração: eu falei que não tava com a chave.. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

**Entrada integral:**

```text
Foi assim, eu tava no pátio, perto do bebedouro, isso já depois do sinal, umas dez para as onze, por aí. O Davi veio falar comigo do portão. Antes disso eu tinha visto a Elisa mexendo no cadeado, mas não sei se ela abriu ou só olhou. Aí o Davi falou pra mim que a chave tinha sumido. Eu falei que não tava com a chave. Tinha um ônibus ligado, fazia barulho, então uma parte do que ele disse eu perdi. No fim a Elisa passou com uma chave na mão, só que eu não sei se era aquela. Depois fui atender um telefonema e não acompanhei mais.
```

**Saída integral, sem edição — rascunho de geração rejeitada:**

```text
Relatou que foi assim, o depoente estava no pátio, perto do bebedouro, isso já depois do sinal, por volta de dez para as onze, por aí. O Davi veio falar com o depoente do portão. Antes disso, o depoente já tinha visto a Elisa mexendo no cadeado, mas o depoente não sabe se ela abriu ou só olhou. Então o Davi falou ao depoente que a chave tinha sumido. O depoente respondeu que não tinha a chave. Havia um ônibus ligado, fazia barulho, então uma parte do que o Davi disse o depoente perdeu. No final, a Elisa passou com uma chave na mão, mas o depoente não sabe se era aquela. Depois, o depoente atendeu um telefonema e não acompanhou mais.
```

SHA-256 da saída: `F83313DA474FD4E954FBBF4333CEA197A5BE1F41380178875C79955A71D649B0`. [Saída original](Results/test-02-output.txt) · [Registro completo](Results/test-02.json).

### Caso 3 — Autocorreções de transcrição

Objetivo: Distinguir fragmento abandonado, confirmação e dúvida não resolvida.

**Resultado:** Ressalva de redação; não aprovado integralmente. **Gravidade:** Moderada — autocorreção não consolidada.

**Achados:** Concluiu sem warning, mas reproduziu os fragmentos abandonados seguidos das correções, em vez de expressar somente a versão efetiva. A data correta e o horário de saída continuam recuperáveis pelas correções explícitas, portanto não classifico como inversão factual consumada. O comportamento geral de consolidação de autocorreções não generalizou para estas formas. A construção «Entregou para...» mantém elipse do depoente, recuperável pelo contexto.

**Demais verificações semânticas:** Quinta como data corrigida, folga na sexta, função efetiva de estagiária cobrindo recepção, nome desconhecido, chegada às dez e saída aproximada perto das onze, envelope azul e dupla marca de dúvida, além de não retratar a própria incerteza. Sem troca de fonte, certeza indevida, citações, omissão relevante ou duplicação factual identificadas.

**Redação:** Mantém três falsos começos/autocorreções: sexta/quinta, recepcionista/estagiária e saída às dez/chegada às dez. Capitalização inadequada após ponto e vírgula.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 22.956 s. Entrada: 444 caracteres. Saída: 525 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-03-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu entreguei a pasta na sexta... não, na quinta-feira. Foi quinta, porque sexta eu estava de folga. Entreguei para a recepcionista, quer dizer, para a estagiária que estava cobrindo a recepção. Não sei o nome dela. Eu saí às dez... corrigindo, isso foi quando cheguei; saí perto das onze. Acho que o envelope azul ficou dentro da pasta, mas não tenho certeza. Não estou corrigindo essa dúvida: eu realmente não lembro se coloquei o envelope lá.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente entregou a pasta na sexta... Não, na quinta-feira. Foi quinta, porque sexta ele estava de folga. Entregou para a recepcionista, quer dizer, para a estagiária que estava cobrindo a recepção. O depoente não sabe o nome dela. O depoente saiu às dez... Corrigindo, isso foi quando chegou; O depoente saiu perto das onze. O depoente acredita que o envelope azul ficou dentro da pasta, mas o depoente não tem certeza. Não está corrigindo essa dúvida: o depoente realmente não lembra se colocou o envelope lá.
```

SHA-256 da saída: `7B54A515FE5847A9EF2881B43E6D962EDFB2FAA28C75877AD8FE733A81D01F6A`. [Saída original](Results/test-03-output.txt) · [Registro completo](Results/test-03.json).

### Caso 4 — Três participantes do mesmo gênero

Objetivo: Manter sujeito do depoimento e distinguir pronomes definidos de ambíguos.

**Resultado:** Reprovado na revisão semântica. **Gravidade:** Moderada — ambiguidade nova de sujeito.

**Achados:** Após «Vicente lhe perguntou se o depoente tinha visto a nota fiscal», a saída usa «Respondeu que não». O original «Respondi que não» identifica inequivocamente o depoente; a nova elipse permite continuar com Vicente como sujeito da oração principal anterior. O contexto sugere o depoente, mas a atribuição ficou menos explícita justamente no caso de risco de pronomes. Não há troca nominal inequívoca, e sim ambiguidade introduzida. O retry restaurou o destinatário da fala posterior de Leandro.

**Demais verificações semânticas:** Presença inicial, possuidor desconhecido da ferramenta, alternativa Leandro/Natan para voltar no sábado, saída de Vicente da oficina, permanência do depoente com Natan, ausência de pergunta sobre a nota, retorno solitário de Leandro e fonte indireta da saída do prédio, que o depoente não viu. Ambiguidades originais permanecem, datas e negativas presentes, sem corte ou duplicação.

**Redação:** Construção geral legível, mas a elipse de resposta é perigosa em relato com vários homens.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO COM ALERTAS; retry=True; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 50.271 s. Entrada: 526 caracteres. Saída: 597 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

- [2026-09-29 17:27:58.533 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: leandro voltou sozinho e me disse que vicente já tinha ido embora.

**Warnings nativos:** 4. Constam no [log integral](Results/test-04-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu estava com Leandro e Natan quando Vicente entrou na oficina. Leandro pediu a Natan que guardasse a ferramenta dele. Eu não sei de quem era a ferramenta. Vicente me perguntou se eu tinha visto a nota fiscal. Respondi que não. Natan disse a Leandro que ele precisava voltar no sábado. Não ficou claro para mim qual dos dois precisava voltar. Depois Vicente saiu, e eu fiquei com Natan. Eu não perguntei a Natan sobre a nota. Leandro voltou sozinho e me disse que Vicente já tinha ido embora. Eu não vi Vicente sair do prédio.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente estava com Leandro e Natan quando Vicente entrou na oficina. Leandro pediu a Natan que guardasse a ferramenta dele. O depoente não sabe de quem era a ferramenta. Vicente lhe perguntou se o depoente tinha visto a nota fiscal. Respondeu que não. Natan disse a Leandro que ele precisava voltar no sábado. Não ficou claro para o depoente qual dos dois precisava voltar. Depois Vicente saiu, e o depoente ficou com Natan. O depoente não perguntou a Natan sobre a nota. Leandro voltou sozinho e lhe disse que Vicente já tinha ido embora. O depoente não viu Vicente sair do prédio.
```

SHA-256 da saída: `1C6363627DED3A7668584A73B315508FBF03A5754AC4C81A5BA934D342034EE8`. [Saída original](Results/test-04-output.txt) · [Registro completo](Results/test-04.json).

### Caso 5 — Percepção e relato em cadeia

Objetivo: Preservar cada camada de fonte e distinguir leitura, audição e observação.

**Resultado:** Reprovado. **Gravidade:** Moderada + falha operacional.

**Achados:** Rejeitado por primeira pessoa residual, que também existe em «li» e não foi sinalizada pelo analisador externo. A cadeia «Sílvia lhe contou que Raul lhe disse» mantém os dois nomes, mas o primeiro «lhe» passa a designar o depoente, tornando menos claro se o segundo designa Sílvia ou o depoente. Na entrada, «Sílvia me contou que Raul lhe disse» distinguia os destinatários. Há ambiguidade nova de fonte/destinatário, sem troca comprovada de nome. O validador bloqueou pela forma verbal, não por essa ambiguidade.

**Demais verificações semânticas:** Copo quebrado junto da impressora, autoria desconhecida da queda, relato de Raul por Sílvia, ausência de audição direta, mensagem lida de Noemi e distinção ouviu/não viu, hipótese de trinca anterior, não exame dos cacos e limite final de conhecimento estão presentes. Funções técnica/motorista e ordem temporal permanecem. Sem corte ou duplicação; primeira pessoa impede conformidade formal.

**Redação:** «O depoente não examinei» apresenta erro de concordância.

**Primeira pessoa fora de citações — revisão manual:** li; examinei.

**Registro técnico:** ERRO; retry=True; erro=True; exceção=True; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 43.331 s. Entrada: 426 caracteres. Saída: 492 caracteres. Captura conferida por GENERATION_OK: não disponível; rascunho capturado da tela.

**Warnings do validador:**

- [2026-09-29 17:28:46.345 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: possível primeira pessoa residual: examinei
- [2026-09-29 17:29:08.779 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: possível primeira pessoa residual: examinei

**Warnings nativos:** 4. Constam no [log integral](Results/test-05-log.txt), preservados separadamente dos warnings do validador.

**Erro/exceção integral registrado:**

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=426; elapsedMs=43331
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: possível primeira pessoa residual: examinei. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

**Entrada integral:**

```text
Vi um copo quebrado ao lado da impressora. Não vi quem o derrubou. A técnica Sílvia me contou que o motorista Raul lhe disse que alguém esbarrou na mesa. Eu não ouvi Raul falar isso. Mais tarde li uma mensagem de Noemi dizendo que ela tinha ouvido a queda, mas não tinha visto o copo cair. Sílvia acrescentou que achava que o copo já estava trincado. Eu não examinei os cacos. Sei apenas que havia vidro no chão quando entrei.
```

**Saída integral, sem edição — rascunho de geração rejeitada:**

```text
Relatou que o depoente viu um copo quebrado ao lado da impressora. O depoente não viu quem o derrubou. A técnica Sílvia lhe contou que o motorista Raul lhe disse que alguém esbarrou na mesa. O depoente não ouviu Raul falar isso. Mais tarde li uma mensagem de Noemi dizendo que ela tinha ouvido a queda, mas não tinha visto o copo cair. Sílvia acrescentou que achava que o copo já estava trincado. O depoente não examinei os cacos. O depoente sabe apenas que havia vidro no chão quando entrou.
```

SHA-256 da saída: `C62ED3EEEE48FF2EDA5EE28972401179657017103B738242343D28D0569D27CD`. [Saída original](Results/test-05-output.txt) · [Registro completo](Results/test-05.json).

### Caso 6 — Negação da percepção

Objetivo: Não transformar não ver/não ouvir em negativa do acontecimento.

**Resultado:** Reprovado na revisão semântica. **Gravidade:** Moderada — destinatário omitido.

**Achados:** As negações de percepção passaram corretamente, inclusive a ressalva explícita de que não ver não permite concluir se ocorreu. Entretanto, «A gerente Yara me disse» virou «A gerente Yara disse»: a fonte nomeada permanece, mas desaparece que a informação foi transmitida ao depoente. Não há invenção de outro destinatário; há perda de uma relação explícita de relato. A geração foi aceita sem warning apesar dessa omissão.

**Demais verificações semânticas:** Não ver retirada, não saber se houve retirada, visão de Ícaro perto do balcão com sacola fechada, não ouvir autorização, não poder negar pedido por ausência temporária, valor de cinquenta reais, falta de conferência e dúvida sobre diferença anterior. Nome e cargo preservados, sem certeza indevida, primeira pessoa, corte ou duplicação.

**Redação:** Redação legível, com repetição de «o depoente».

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 21.436 s. Entrada: 408 caracteres. Saída: 488 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-06-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu não vi Ícaro retirar dinheiro da gaveta. Isso não significa que eu saiba se ele retirou ou não. Vi Ícaro perto do balcão, com uma sacola fechada. Não ouvi ele pedir autorização à gerente. Também não posso afirmar que ele não pediu, porque fiquei fora da loja por alguns minutos. A gerente Yara me disse que faltavam cinquenta reais, mas eu não conferi o caixa e não sei se essa diferença já existia antes.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente não viu Ícaro retirar dinheiro da gaveta. Isso não significa que o depoente saiba se ele retirou ou não. O depoente viu Ícaro perto do balcão, com uma sacola fechada. O depoente não ouviu ele pedir autorização à gerente. O depoente também não pode afirmar que ele não pediu, porque o depoente ficou fora da loja por alguns minutos. A gerente Yara disse que faltavam cinquenta reais, mas o depoente não conferiu o caixa e não sabe se essa diferença já existia antes.
```

SHA-256 da saída: `3D1D19AFD728425E5F96F6BAEC21A5C704F896BFBF77E6526E70E999580D1E08`. [Saída original](Results/test-06-output.txt) · [Registro completo](Results/test-06.json).

### Caso 7 — Negações encadeadas

Objetivo: Preservar escopo de negativas e ausência de confirmação ou desmentido.

**Resultado:** Aprovado. **Gravidade:** Sem erro semântico identificado.

**Achados:** As negativas encaixadas foram preservadas: não disse que Celso não entregou, mas disse que não viu; houve aviso, não telefônico; Lídia não negou e declarou desconhecimento; depoente não confirmou nem desmentiu, nunca entrou sem acompanhamento e não autorizou uso do nome. Nenhuma inversão de escopo identificada.

**Demais verificações semânticas:** Sujeitos, cargo de supervisora, bilhete e autoria desconhecida, fonte da declaração e limites de conhecimento permanecem. «Naquele depósito» ficou «no depósito», sem outro local concorrente no relato; não identificada troca de referente. Não há datas ou citações literais. Sem omissão factual relevante, acréscimo, primeira pessoa, corte ou duplicação.

**Redação:** «Lhe avisou» é coloquial; elipses continuam ligadas ao depoente no contexto.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 20.338 s. Entrada: 418 caracteres. Saída: 467 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-07-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu não disse que Celso não entregou o crachá. Disse que não vi a entrega. Não é verdade que ninguém me avisou da vistoria: recebi um aviso, mas não pelo telefone. Não lembro quem deixou o bilhete. A supervisora Lídia não negou que o armário estivesse aberto; ela disse que não sabia. Eu não confirmei nem desmenti a versão dela. Nunca entrei naquele depósito sem acompanhamento e não autorizei ninguém a usar meu nome.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente não disse que Celso não entregou o crachá. Disse que o depoente não viu a entrega. Não é verdade que ninguém lhe avisou da vistoria: recebeu um aviso, mas não pelo telefone. O depoente não lembra quem deixou o bilhete. A supervisora Lídia não negou que o armário estivesse aberto; ela disse que não sabia. O depoente não confirmou nem desmentiu a versão dela. Nunca entrou no depósito sem acompanhamento e não autorizou ninguém a usar seu nome.
```

SHA-256 da saída: `6584507B1BF08C6B8EBC4FC6A295EC56BF6C6CEF388A75DC364DDDC47A0B1C94`. [Saída original](Results/test-07-output.txt) · [Registro completo](Results/test-07.json).

### Caso 8 — Datas e ordem incertas

Objetivo: Preservar alternativas de data, intervalos e limites de lembrança.

**Resultado:** Aprovado. **Gravidade:** Sem erro semântico identificado.

**Achados:** Conserva todas as alternativas temporais e a negação de oito e meia. Não escolhe entre 12/13, entre nove/nove e quinze ou entre dois/três dias. Explicita a chegada do depoente como marco do caminhão.

**Demais verificações semânticas:** Feriado municipal no dia 15, certeza da anterioridade, intervalo sete e meia/oito, chegada do caminhão entre chegada do depoente e abertura do portão, ligação posterior de Tainá, cancelamento segundo ela na véspera da ligação e data desconhecida. Fontes, ordem, certeza e incertezas intactas. Sem citações, troca de cargo, omissão, acréscimo, primeira pessoa, corte ou duplicação.

**Redação:** Repetição de «o depoente», sem duplicação de fatos.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 26.694 s. Entrada: 505 caracteres. Saída: 594 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-08-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Acho que visitei o arquivo em 12 de agosto, talvez tenha sido no dia 13. Sei que foi antes do feriado municipal de 15 de agosto. Cheguei entre sete e meia e oito horas, não às oito e meia. O caminhão apareceu depois da minha chegada, mas antes de abrirem o portão lateral. Não lembro se fiquei até nove horas ou até nove e quinze. Dois ou três dias depois, recebi uma ligação de Tainá sobre as caixas. Ela disse que a retirada tinha sido cancelada na véspera da ligação. Não sei a data desse cancelamento.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente acredita que visitou o arquivo em 12 de agosto, talvez tenha sido no dia 13. O depoente sabe que foi antes do feriado municipal de 15 de agosto. O depoente chegou entre sete e meia e oito horas, não às oito e meia. O caminhão apareceu depois da chegada do depoente, mas antes de abrirem o portão lateral. O depoente não lembra se ficou até nove horas ou até nove e quinze. Dois ou três dias depois, o depoente recebeu uma ligação de Tainá sobre as caixas. Ela disse que a retirada tinha sido cancelada na véspera da ligação. O depoente não sabe a data desse cancelamento.
```

SHA-256 da saída: `D3CE0A279BE9A29360157BDABC306DFC9DB64A120BB7AF3DF011F1AA0C3C8526`. [Saída original](Results/test-08-output.txt) · [Registro completo](Results/test-08.json).

### Caso 9 — Versões incompatíveis

Objetivo: Preservar conflito, atribuições e certeza de cada participante.

**Resultado:** Aprovado. **Gravidade:** Sem erro semântico identificado.

**Achados:** Preserva as versões incompatíveis sem escolher uma. Não usa a hipótese do segurança como explicação confirmada. Mantém Hugo como fonte na segunda, Iracema na terça e o depoente ausente do local.

**Demais verificações semânticas:** Janela fechada antes da chuva versus aberta durante a chuva, esclarecimento copa/não corredor, reafirmação de Iracema sobre a copa, dúvida do depoente e ausência de percepção da eventual reabertura. Relato de Hugo ao depoente preservado por «lhe». Sem troca de sujeito, fonte, negação, cronologia ou grau de certeza; sem omissão, acréscimo, primeira pessoa, corte ou duplicação.

**Redação:** Introdução «Relatou que na segunda-feira» poderia receber pontuação mais natural; conteúdo legível.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 23.255 s. Entrada: 507 caracteres. Saída: 548 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-09-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Na segunda-feira, Hugo me disse que a janela havia sido fechada antes da chuva. Na terça-feira, Iracema afirmou que encontrou a mesma janela aberta durante a chuva. Eu não estava no prédio naquele período. Hugo depois explicou que fechou a janela da copa, não a do corredor. Iracema disse que estava falando da janela da copa. Não sei qual versão está correta. Um segurança comentou que talvez alguém tivesse aberto a janela novamente, mas deixou claro que era uma hipótese. Eu não presenciei essa abertura.
```

**Saída integral, sem edição:**

```text
Relatou que na segunda-feira, Hugo lhe disse que a janela havia sido fechada antes da chuva. Na terça-feira, Iracema afirmou que encontrou a mesma janela aberta durante a chuva. O depoente não estava no prédio naquele período. Hugo depois explicou que fechou a janela da copa, não a do corredor. Iracema disse que estava falando da janela da copa. O depoente não sabe qual versão está correta. Um segurança comentou que talvez alguém tivesse aberto a janela novamente, mas deixou claro que era uma hipótese. O depoente não presenciou essa abertura.
```

SHA-256 da saída: `3EC662BB200FD7B3FC940BFF76B1F0FF1D1C3E2A4DBA1BFEEA938A8DAF854546`. [Saída original](Results/test-09-output.txt) · [Registro completo](Results/test-09.json).

### Caso 10 — Citações e primeira pessoa legítima

Objetivo: Preservar palavras entre aspas e identidade de quem as pronunciou.

**Resultado:** Reprovado na revisão semântica. **Gravidade:** Alta — atribuição de relato comprometida.

**Achados:** «Mirela me disse mais tarde» virou «Mirela o depoente disse mais tarde». O destinatário não foi convertido para uma construção gramatical válida, e «o depoente» aparece contíguo a «disse», concorrendo com Mirela como sujeito. A relação fonte/destinatário ficou comprometida; não é apenas polimento de estilo. O aplicativo aceitou sem warning. A elipse em «Não sabe» é recuperável pelo depoente da oração anterior e não foi tratada como outra falha.

**Demais verificações semânticas:** As duas falas principais e «nós já avisamos» permaneceram literais, inclusive a primeira pessoa legítima nas aspas; «amanhã» e a dúvida sobre seu dia também. Audição das duas frases, voz atrás do depoente não identificada, negativa sobre contrato e ausência de pergunta permanecem, embora a fonte da negativa tenha sido prejudicada. Sem corte, duplicação ou primeira pessoa fora de aspas.

**Redação:** «Atrás de o depoente» é contração inadequada, sem mudar a posição espacial. «Mirela o depoente disse» compromete a estrutura da fala.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 22.184 s. Entrada: 396 caracteres. Saída: 464 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-10-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Ouvi Mirela dizer a Jonas: “Eu não prometi entregar hoje; prometi tentar”. Jonas respondeu: “Você me disse amanhã, não hoje”. Eu ouvi as duas frases. Não sei a que dia Jonas se referia com “amanhã”. Depois alguém atrás de mim falou “nós já avisamos”, mas não identifiquei a voz. Mirela me disse mais tarde que não estava falando do contrato. Eu não perguntei de qual documento ela estava falando.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente ouviu Mirela dizer a Jonas: “Eu não prometi entregar hoje; prometi tentar”. Jonas respondeu: “Você me disse amanhã, não hoje”. O depoente ouviu as duas frases. Não sabe a que dia Jonas se referia com “amanhã”. Depois alguém atrás de o depoente falou: “nós já avisamos”, mas o depoente não identificou a voz. Mirela o depoente disse mais tarde que não estava falando do contrato. O depoente não perguntou de qual documento ela estava falando.
```

SHA-256 da saída: `08AF2C0CFE8DF3DCCBFE6878C5F6C3EFF48D917714F73BE49CFA4813117E0281`. [Saída original](Results/test-10-output.txt) · [Registro completo](Results/test-10.json).

### Caso 11 — Nomes e funções semelhantes

Objetivo: Não fundir nomes próximos nem trocar cargos, objetos ou responsabilidade.

**Resultado:** Reprovado na revisão semântica. **Gravidade:** Moderada — ambiguidade nova de percepção.

**Achados:** «A diretora Célia ... não participou da conversa que ouvi» virou «A diretora Célia ... não participou da conversa que ouviu». A primeira pessoa identificava o depoente como ouvinte; a terceira pessoa sem sujeito permite atribuir a audição a Célia, sujeito da oração principal. Não se afirma que ocorreu troca inequívoca, mas houve perda da atribuição explícita de percepção. Aceito sem warning.

**Demais verificações semânticas:** Lívia/Lídia, Ari/Arlindo e Célia/Célio não foram fundidos. Todos os cargos, recebimento do formulário, assinaturas em campos diferentes, entrega por Ari não vista e relatada depois, presença de Célia no corredor, telefonema do vice-diretor à tarde e limites da função/autorização do depoente foram preservados. Sem citação, data específica, negação invertida, corte, duplicação ou primeira pessoa residual.

**Redação:** Demais construções legíveis.

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 22.35 s. Entrada: 489 caracteres. Saída: 547 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-11-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu trabalho como auxiliar de biblioteca. A bibliotecária Lívia recebeu o formulário da assistente Lídia. Vi as duas assinarem campos diferentes. O professor Ari levou o formulário para o secretário Arlindo. Não vi a entrega ao secretário; Ari me contou isso depois. A diretora Célia estava no corredor, mas não participou da conversa que ouvi. O vice-diretor Célio telefonou para mim à tarde e perguntou pelo formulário. Eu não sou secretário e não tinha autorização para aprovar o pedido.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente trabalha como auxiliar de biblioteca. A bibliotecária Lívia recebeu o formulário da assistente Lídia. O depoente viu as duas assinarem campos diferentes. O professor Ari levou o formulário para o secretário Arlindo. O depoente não viu a entrega ao secretário; Ari lhe disse isso depois. A diretora Célia estava no corredor, mas não participou da conversa que ouviu. O vice-diretor Célio telefonou para o depoente à tarde e perguntou pelo formulário. O depoente não é secretário e não tinha autorização para aprovar o pedido.
```

SHA-256 da saída: `FCF3EF46D87FA48DE1C736C72460922246B08F0093580EA45A828301D55F5370`. [Saída original](Results/test-11-output.txt) · [Registro completo](Results/test-11.json).

### Caso 12 — Relato longo e mudanças de assunto

Objetivo: Verificar cobertura dos blocos, identidade do depoente, fontes, temporalidade e final.

**Resultado:** Reprovado; saída parcial. **Gravidade:** Alta + falha operacional.

**Achados:** A execução parou por rejeição do bloco 1/5, após retry, por «consultei». Somente o início ficou na tela, até a entrada de Vilma com as sacolas. O relógio parado foi omitido já nesse trecho; a explicação do uso do celular perdeu sua causa. Todo o restante da entrada ficou ausente da saída disponível: guarda/devolução das sacolas, ligação e atraso sem cancelamento, impressão e destinatário das folhas, extensão, serviço elétrico, pincel e limites da percepção, veículo/grupo incerto, conversa no almoço, chave, fim do expediente, mensagens posteriores e ressalvas finais. Trata-se de interrupção por validação documentada, não evidência de estouro de contexto ou corte por limite de tokens. Não é possível avaliar como o modelo reformularia os quatro blocos não processados.

**Demais verificações semânticas:** No trecho disponível, constam data 18 de maio, chegada aproximada, nomes/cargos iniciais, mochila, ausência de discussão, pergunta e fonte da lista, não visão do envio e endereço desconhecido, consulta restrita à caixa de entrada, Neide sem resposta, relato de Bento, chiado ouvido com causa incerta, pedido para aguardar eletricista e ausência de afirmação de equipamento queimado. Esses acertos parciais não compensam a incompletude. Sem duplicação identificada. O detector automático marcou «nossa», mas não todas as formas de primeira pessoa; não marcou truncamento porque o rascunho termina com ponto.

**Redação:** «O depoente trabalho» e a mistura «Consultei seu celular» rompem a pessoa gramatical. Elipses após a enumeração de participantes também reduzem clareza.

**Primeira pessoa fora de citações — revisão manual:** trabalho; Consultei (duas ocorrências); nossa.

**Registro técnico:** ERRO; retry=True; erro=True; exceção=True; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 91.712 s. Entrada: 5184 caracteres. Saída: 1242 caracteres. Captura conferida por GENERATION_OK: não disponível; rascunho capturado da tela.

**Warnings do validador:**

- [2026-09-29 17:32:30.176 -03:00] [WARN] [BLOCK_RETRY_START] block=1/5; issue=fidelidade: possível primeira pessoa residual: consultei
- [2026-09-29 17:33:14.637 -03:00] [WARN] [BLOCK_REJECTED] block=1/5; detail=fidelidade: possível primeira pessoa residual: consultei

**Warnings nativos:** 4. Constam no [log integral](Results/test-12-log.txt), preservados separadamente dos warnings do validador.

**Erro/exceção integral registrado:**

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=5184; elapsedMs=91712
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: possível primeira pessoa residual: consultei. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

**Entrada integral:**

```text
Eu trabalho na recepção de um centro cultural. Na manhã de 18 de maio, cheguei por volta das sete e quarenta. O relógio da parede estava parado, por isso consultei meu celular. A produtora Solange estava na entrada, o técnico Bento estava no palco e a auxiliar Neide arrumava as cadeiras. Cumprimentei os três e deixei minha mochila sob o balcão. Não percebi discussão quando entrei.

Solange me perguntou se a lista dos convidados já tinha chegado. Respondi que eu ainda não tinha recebido nenhuma lista. Ela disse que o coordenador Gaspar tinha enviado uma versão na noite anterior. Não vi esse envio e não sei para qual endereço teria sido encaminhado. Consultei somente a caixa de entrada da recepção, não a pasta de mensagens indesejadas. Neide ouviu nossa conversa, mas não respondeu.

Alguns minutos depois, Bento veio até o balcão com um cabo na mão. Disse que uma das caixas de som estava falhando. Ouvi um chiado quando ele fez um teste, mas não sei avaliar se o problema era no cabo ou na caixa. Solange pediu que ele aguardasse a chegada do eletricista. Eu não ouvi ninguém dizer que o equipamento estava queimado.

Enquanto isso, entrou a professora Vilma com duas sacolas. Ela pediu que eu guardasse os materiais da oficina. Coloquei as sacolas no armário atrás da recepção, sem abrir nenhuma delas. Vilma disse que voltaria antes das dez. Não sei o que havia dentro. Mais tarde Neide comentou que uma das sacolas parecia conter tintas, mas ela também não abriu a sacola na minha presença.

Perto das oito e meia, atendi uma ligação. A pessoa se apresentou como representante da empresa de transporte, mas não lembro o nome. Informou que o veículo do grupo estava atrasado. Eu anotei a informação e a passei para Solange. Não me disseram a causa do atraso nem quanto tempo levaria. Não afirmei que o grupo tinha cancelado a visita.

Saí para beber água. Quando voltei, Gaspar estava ao lado de Solange. Não vi a chegada dele. Os dois olharam uma folha, mas eu estava longe e não consegui ler. Gaspar me pediu que imprimisse uma lista nova. Recebi um arquivo no computador e imprimi três páginas. Não comparei essa lista com qualquer versão anterior. Entreguei as folhas a Solange, não a Gaspar.

Depois ouvi Neide perguntar a Bento sobre uma extensão elétrica. Ele disse que tinha deixado a extensão perto da porta de serviço. Eu não vi onde ele a colocou. Neide saiu em direção à porta e voltou sem nada nas mãos. Não sei se não encontrou a extensão ou se foi fazer outra coisa. Eu não perguntei.

O eletricista chegou, acho que pouco depois das nove. Não sei seu nome. Vi ele conversar com Bento e desligar uma tomada. Não acompanhei o restante do serviço porque comecei a atender os visitantes. Mais tarde Bento me disse que o som estava funcionando. Eu ouvi música no salão, mas não sei se saía da mesma caixa que tinha apresentado chiado.

Por volta de nove e meia, Vilma voltou ao balcão. Entreguei a ela as duas sacolas. Ela conferiu algo dentro de uma delas e disse que estava faltando um pincel. Não sei se esse pincel estava na sacola quando ela chegou. Vilma não disse que alguém tinha retirado o objeto. Eu não vi ninguém mexer nas sacolas enquanto estavam no armário, mas houve um período em que fiquei longe do balcão.

Antes do almoço, Solange me contou que a empresa havia enviado outro veículo. Não falei novamente com a empresa. Um grupo chegou pouco depois, acompanhado de uma monitora. Não tenho certeza de que era o mesmo grupo da ligação. A monitora entregou um papel a Gaspar. Vi a entrega, mas não li o papel.

Saí para almoçar às doze e dez e voltei por volta de uma hora. Na volta, Neide disse que tinha havido uma conversa entre Vilma e Gaspar sobre materiais. Não participei dessa conversa e não sei se trataram do pincel. Neide disse que ouviu apenas o final. Não me contou as palavras usadas por nenhum dos dois.

Durante a tarde, ajudei a separar crachás para a atividade seguinte. Bento perguntou se eu tinha visto uma chave pequena. Respondi que não sabia de qual chave ele estava falando. Ele mostrou uma fotografia, mas eu não reconheci a chave. Não disse que ela nunca tinha passado pela recepção. Solange comentou que talvez estivesse com o pessoal da limpeza; isso foi apresentado como possibilidade.

No fim do expediente, Gaspar agradeceu à equipe e disse que faria uma avaliação do evento em outro dia. Não marcou uma data na minha presença. Fui embora por volta das cinco e vinte. Quando saí, Solange e Bento ainda estavam no salão. Não sei quem fechou o prédio naquela noite.

Na manhã seguinte, recebi uma mensagem de Vilma dizendo que tinha encontrado o pincel em casa. Foi ela quem informou isso; eu não vi o objeto. Não recebi notícia de que alguém tivesse sido responsabilizado pelo desaparecimento. Na semana seguinte, ouvi de um funcionário cujo nome não lembro que a avaliação havia sido adiada. Não sei se essa informação estava correta.

Não participei de reunião de avaliação e não tive acesso a atas. Não posso afirmar que o problema do som, o atraso do transporte e a falta do pincel tivessem relação entre si. Meu relato reúne o que observei, as informações que me passaram e os pontos que não consegui confirmar.
```

**Saída integral, sem edição — rascunho de geração rejeitada:**

```text
Relatou que o depoente trabalho na recepção de um centro cultural. Na manhã de 18 de maio, o depoente chegou por volta das sete e quarenta. Consultei seu celular para verificar o horário. A produtora Solange estava na entrada, o técnico Bento no palco e a auxiliar Neide arrumava as cadeiras. Cumprimentou os três e deixou sua mochila sob o balcão. O depoente não percebeu discussão quando entrou. Solange lhe perguntou se a lista dos convidados já tinha chegado. Respondeu que ainda não tinha recebido nenhuma lista. Ela disse que o coordenador Gaspar tinha enviado uma versão na noite anterior. O depoente não viu esse envio e não sabe para qual endereço teria sido encaminhado. Consultei somente a caixa de entrada da recepção, não a pasta de mensagens indesejadas. Neide ouviu nossa conversa, mas não respondeu. Alguns minutos depois, Bento veio até o balcão com um cabo na mão. Disse que uma das caixas de som estava falhando. O depoente ouviu um chiado quando ele fez um teste, mas o depoente não sabe avaliar se o problema era no cabo ou na caixa. Solange pediu que ele aguardasse a chegada do eletricista. O depoente não ouviu ninguém dizer que o equipamento estava queimado. Enquanto isso, entrou a professora Vilma com duas sacolas.
```

SHA-256 da saída: `A144B67227F262B72C16404EC3F50F69B873A0ACA316FA2C944CA59CBAB61773`. [Saída original](Results/test-12-output.txt) · [Registro completo](Results/test-12.json).

### Caso 13 — Correção explícita do próprio depoente

Objetivo: Aplicar retratação efetiva sem manter alegação abandonada ou inventar certeza.

**Resultado:** Aprovado. **Gravidade:** Sem erro semântico identificado.

**Achados:** A correção Rubens→Amaro permanece explicitamente marcada, sem apresentar Rubens como autor confirmado da entrega. Amaro continua porteiro; certeza da entrega justificada pela assinatura é separada da dúvida do horário. «Fechado com fita» não vira «lacrado», e a negativa de afirmar lacre permanece. Diferentemente dos falsos começos do caso 3, aqui o histórico da declaração anterior é parte de uma retratação completa e inteligível, preservada como tal.

**Demais verificações semânticas:** Destinatário da chave, presença de Rubens ao lado de Amaro, assinatura diante de Amaro, alternativas seis/seis e meia, fita, falta de verificação de lacre e recusa de afirmar lacrado. Não há fonte indireta nova, resolução de dúvida, omissão relevante, primeira pessoa, corte ou duplicação factual.

**Redação:** Mantém o histórico explícito da retratação, em vez de condensá-lo; acrescenta aspas metalinguísticas em «lacrado».

**Primeira pessoa fora de citações — revisão manual:** nenhuma identificada.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 22.543 s. Entrada: 458 caracteres. Saída: 506 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-13-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu disse há pouco que Rubens me entregou a chave, mas preciso corrigir isso: quem me entregou foi o porteiro Amaro. Rubens estava ao lado dele. Tenho certeza de quem fez a entrega porque assinei o livro diante de Amaro. Sobre o horário, mantenho a dúvida: pode ter sido às seis ou às seis e meia. Também preciso corrigir a palavra lacrado; o pacote estava fechado com fita, mas eu não verifiquei se havia lacre. Não quero afirmar que o pacote estava lacrado.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente disse há pouco que Rubens lhe entregou a chave, mas precisa corrigir isso: quem lhe entregou foi o porteiro Amaro. Rubens estava ao lado dele. O depoente tem certeza de quem fez a entrega porque assinou o livro diante de Amaro. Sobre o horário, mantém a dúvida: pode ter sido às seis ou às seis e meia. Também precisa corrigir a palavra "lacrado"; o pacote estava fechado com fita, mas o depoente não verificou se havia lacre. O depoente não quer afirmar que o pacote estava lacrado.
```

SHA-256 da saída: `E8362C8A1B0A0DA015C86F99E6B081011D28B4E652043F6108F92BC20BD0FFDA`. [Saída original](Results/test-13-output.txt) · [Registro completo](Results/test-13.json).

### Caso 14 — Detalhes laterais relevantes ou não

Objetivo: Não apagar conteúdo sem autorização, mesmo quando parece irrelevante.

**Resultado:** Reprovado. **Gravidade:** Moderada — primeira pessoa e destinatário omitido.

**Achados:** Aceito sem warning apesar de «o depoente li». O analisador externo também não marcou «li». «Ela me entregou uma via» virou «ela entregou uma via com o nome do depoente»: o destinatário explícito da entrega desaparece; o nome na via e a conferência permitem inferir a entrega ao depoente, mas não expressam a mesma relação. Os detalhes laterais não foram apagados.

**Demais verificações semânticas:** Pedido de segunda via, chuva, guarda-chuva amarelo pingando perto da porta, opinião de irrelevância da cor e lembrança do detalhe, espera solicitada por Dora, cartaz de vacinação, água, entrega observada a homem de boné com dúvida sobre qual recibo, nome e conferência da via, saída sem reclamar/conversar. Cargo de atendente e percepção visual preservados; sem corte ou duplicação.

**Redação:** «O depoente li um cartaz» apresenta primeira pessoa residual e concordância incorreta.

**Primeira pessoa fora de citações — revisão manual:** li.

**Registro técnico:** CONCLUÍDO SEM ALERTAS AUTOMÁTICOS; retry=False; erro=False; exceção=False; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 26.239 s. Entrada: 500 caracteres. Saída: 627 caracteres. Captura conferida por GENERATION_OK: True.

**Warnings do validador:**

Nenhum.

**Warnings nativos:** 3. Constam no [log integral](Results/test-14-log.txt), preservados separadamente dos warnings do validador.

**Entrada integral:**

```text
Eu fui ao balcão pedir uma segunda via do recibo. Estava chovendo, e meu guarda-chuva amarelo ficou pingando perto da porta. Não acho que a cor do guarda-chuva tenha relação com o atendimento, mas lembro desse detalhe. A atendente Dora pediu que eu esperasse. Enquanto esperava, li um cartaz sobre vacinação e tomei água. Vi Dora entregar um papel a um homem de boné; não sei se era o meu recibo. Depois ela me entregou uma via com meu nome, que conferi. Saí sem reclamar e sem conversar com o homem.
```

**Saída integral, sem edição:**

```text
Relatou que o depoente foi ao balcão pedir uma segunda via do recibo. Estava chovendo, e o guarda-chuva amarelo do depoente ficou pingando perto da porta. O depoente não acredita que a cor do guarda-chuva tenha relação com o atendimento, mas o depoente lembra desse detalhe. A atendente Dora pediu que o depoente esperasse. Enquanto esperava, o depoente li um cartaz sobre vacinação e tomou água. O depoente viu Dora entregar um papel a um homem de boné; não sabe se era o recibo do depoente. Depois ela entregou uma via com o nome do depoente, que o depoente conferiu. O depoente saiu sem reclamar e sem conversar com o homem.
```

SHA-256 da saída: `3525B9AA05E0623E50D42FF9DC46DA6DB1ABFEC6612351CEA2A02392861ACA63`. [Saída original](Results/test-14-output.txt) · [Registro completo](Results/test-14.json).

### Caso 15 — Ambiguidade real a preservar

Objetivo: Não resolver possuidor, destinatário, pessoa ou local por inferência.

**Resultado:** Reprovado. **Gravidade:** Moderada + falha operacional.

**Achados:** Rejeitado após retry por «levei», forma não detectada pelo analisador externo. As ambiguidades originais foram preservadas, mas «Não perguntei quem era ela» virou «Não perguntou quem era ela» após «Vera lhe disse...». A elipse pode atribuir a ausência de pergunta a Vera em vez do depoente; a entrada era explícita pela flexão de primeira pessoa. Não há solução inventada para irmã/carro/ela/problema, mas há uma ambiguidade nova sobre quem não perguntou.

**Demais verificações semânticas:** Presença na sala, fala Inês→Vera, irmã e carro desconhecidos, pedido de levar bolsa com autora desconhecida, depoente de costas, negativa de levar, fala de uma sobre a outra, problema desconhecido e relato posterior de Vera ao depoente. A palavra citada «ela» permanece literal. Sem corte ou duplicação identificados.

**Redação:** Primeira pessoa residual em «Não levei a bolsa».

**Primeira pessoa fora de citações — revisão manual:** levei.

**Registro técnico:** ERRO; retry=True; erro=True; exceção=True; overflow=False; indício automático de truncamento=False; duplicação integral=False; parágrafos repetidos=False.

Tempo de geração: 46.165 s. Entrada: 477 caracteres. Saída: 562 caracteres. Captura conferida por GENERATION_OK: não disponível; rascunho capturado da tela.

**Warnings do validador:**

- [2026-09-29 17:34:33.629 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: possível perda de negação na oração: não levei a bolsa.
- [2026-09-29 17:34:57.419 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: possível primeira pessoa residual: levei

**Warnings nativos:** 4. Constam no [log integral](Results/test-15-log.txt), preservados separadamente dos warnings do validador.

**Erro/exceção integral registrado:**

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=477; elapsedMs=46165
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: possível primeira pessoa residual: levei. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

**Entrada integral:**

```text
Eu estava na sala quando Inês disse a Vera que sua irmã esperava no carro. Não sei de quem era a irmã nem qual carro era. Depois ela pediu que eu levasse a bolsa para lá. Não consegui identificar se quem fez o pedido foi Inês ou Vera, porque eu estava de costas. Não levei a bolsa. Ouvi uma delas dizer que a outra já sabia do problema. Não sei qual problema. Mais tarde Vera me disse que ela tinha resolvido aquilo. Não perguntei quem era “ela” nem o que tinha sido resolvido.
```

**Saída integral, sem edição — rascunho de geração rejeitada:**

```text
Relatou que o depoente estava na sala quando Inês disse a Vera que sua irmã esperava no carro. O depoente não sabe de quem era a irmã nem qual carro era. Depois ela pediu que o depoente levasse a bolsa para lá. O depoente não conseguiu identificar se quem fez o pedido foi Inês ou Vera, porque o depoente estava de costas. Não levei a bolsa. O depoente ouviu uma delas dizer que a outra já sabia do problema. O depoente não sabe qual problema. Mais tarde Vera lhe disse que ela tinha resolvido aquilo. Não perguntou quem era “ela” nem o que tinha sido resolvido.
```

SHA-256 da saída: `B9CA3850959B21B10C6EA08481098E27594E833EA61C84421B2EF37F086B6861`. [Saída original](Results/test-15-output.txt) · [Registro completo](Results/test-15.json).

## Integridade e limites

PASS: 307 arquivos do aplicativo e da manutenção existente mantiveram os hashes anteriores à bateria.
PASS: as quinze entradas permaneceram idênticas aos arquivos congelados antes da execução.
PASS: quinze entradas executadas conferem com os fixtures e quinze saídas literais conferem com os registros estruturados. As dez gerações concluídas conferem também com o tamanho registrado pelo motor.
PASS: binários, interface, launcher e modelo conferem com os registros de integridade e o manifesto da execução.
PASS: QaRunner.cs é idêntico ao executor anterior. Nenhum código do aplicativo foi alterado.

A revisão comparou integralmente entradas e saídas quanto a sujeito, fonte, percepção, negação, incerteza, ambiguidade, temporalidade, citações, omissões, acréscimos, nomes e cargos. Nenhum warning foi usado como substituto dessa comparação. O log não expõe a razão de parada EOS/limite de tokens; não se atribui causa de truncamento sem evidência. O aplicativo não foi corrigido durante ou após a avaliação. A etapa está encerrada aguardando nova instrução.
