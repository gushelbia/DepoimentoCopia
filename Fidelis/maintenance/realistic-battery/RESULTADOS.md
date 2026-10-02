# Bateria realista — resultados no motor atual

Depoimentos inventados. Saída final = texto que a janela mostra (com a conversão do gênero). Tempo = geração no motor.

## R01 — Gênero do depoente: Feminino — 22.4 s

**Entrada:** Então, eu sou aluna do quinto período e o professor Heitor me chamou na sala dele depois da aula, isso foi numa quinta, dia 14 de agosto, umas cinco e pouco da tarde. Ele fechou a porta e me disse que se eu não fosse no aniversário dele ele ia me reprovar. Eu fiquei muito nervosa e falei que não ia. Aí ele me segurou pelo braço e eu me soltei e saí correndo. Depois eu contei pra minha colega Bianca e ela me disse que ele já tinha feito isso com ela também.

**Saída final:** A depoente relatou que então, a depoente é aluna do quinto período e o professor Heitor chamou a depoente na sala dele depois da aula, isso foi numa quinta-feira, dia 14 de agosto, umas cinco e pouco da tarde. Ele fechou a porta e lhe disse que se a depoente não fosse no aniversário dele ele ia reprovar a depoente. A depoente ficou muito nervosa e falou que não ia. Aí ele segurou a depoente pelo braço e a depoente se soltou e saiu correndo. Depois a depoente contou para sua colega Bianca e ela lhe disse que ele já tinha feito isso com ela também.

*Saída do motor (antes da conversão):* O depoente relatou que então, o depoente é aluna do quinto período e o professor Heitor chamou o depoente na sala dele depois da aula, isso foi numa quinta-feira, dia 14 de agosto, umas cinco e pouco da tarde. Ele fechou a porta e lhe disse que se o depoente não fosse no aniversário dele ele ia reprovar o depoente. O depoente ficou muito nervosa e falou que não ia. Aí ele segurou o depoente pelo braço e o depoente se soltou e saiu correndo. Depois o depoente contou para sua colega Bianca e ela lhe disse que ele já tinha feito isso com ela também.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: umas cinco e pouco

**Chave automática:** ok

**Chave (fatos a preservar):**
- A depoente é aluna (mulher): «nervosa», «a depoente».
- Quem chamou, ameaçou e segurou: o professor Heitor.
- Quem se soltou e saiu correndo: a depoente.
- «ela me disse que ele já tinha feito isso com ela»: Bianca relatou que Heitor fez o mesmo com Bianca.
- Data 14/08 (quinta); horário aproximado («cinco e pouco») não pode virar horário exato.

## R02 — Gênero do depoente: Masculino — 18.1 s

**Entrada:** Olha, eu sou o professor Heitor e eu nego tudo. A aluna me procurou porque queria revisão de nota, ela que me pediu pra conversar. Eu nunca disse que ia reprovar ninguém. Ela me acusou de segurar o braço dela, mas quem estava nervoso ali era eu, porque ela começou a gritar comigo. Eu não fechei a porta, a porta fica fechada por causa do ar-condicionado.

**Saída final:** O depoente relatou que é o professor Heitor e nega tudo. A aluna o procurou porque queria revisão de nota, ela que lhe pediu para conversar. O depoente nunca disse que ia reprovar ninguém. O depoente foi acusado por ela de segurar o braço dela, mas quem estava nervoso ali era ele, porque ela começou a gritar com ele. O depoente não fechou a porta, a porta fica fechada por causa do ar-condicionado.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- O depoente (homem) é o investigado e NEGA as acusações.
- Quem pediu a conversa: a aluna.
- Quem acusou: a aluna acusou o depoente.
- Quem estava nervoso: o depoente («era eu»).
- Quem gritou: a aluna gritou com o depoente.
- Negações preservadas: «nunca disse», «não fechei a porta».

## R03 — Gênero do depoente: Masculino — 22.2 s

**Entrada:** Eu trabalho na portaria do bloco F. Naquele dia, acho que era terça, não, quarta, dia 3 de setembro, por volta das dez da noite, dois alunos começaram a discutir na escada. O mais alto empurrou o outro e ele caiu. Um deles me pediu pra chamar a segurança, acho que foi o que caiu. Eu liguei pro ramal 4417. O Rafael, que era o mais alto, foi levado pela segurança e o outro rapaz ficou sentado com a mão na cabeça.

**Saída final:** O depoente relatou que trabalha na portaria do bloco F. Naquele dia, o depoente acredita que era terça, não, quarta, dia 3 de setembro, por volta das dez da noite, dois alunos começaram a discutir na escada. O mais alto empurrou o outro e ele caiu. Um deles pediu ao depoente para chamar a segurança, o depoente acredita que foi o que caiu. O depoente ligou para o ramal 4417. O Rafael, que era o mais alto, foi levado pela segurança e o outro rapaz ficou sentado com a mão na cabeça.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: por volta das dez da noite

**Chave automática:** ok

**Chave (fatos a preservar):**
- A autocorreção vale: quarta-feira, 03/09 (não terça).
- Quem empurrou: o aluno mais alto (Rafael). Quem caiu: o outro aluno.
- Quem pediu ajuda ao depoente: provavelmente o que caiu (o depoente demonstra dúvida: «acho»).
- Quem foi levado pela segurança: Rafael (NÃO o depoente).
- Quem ficou sentado com a mão na cabeça: o outro aluno (NÃO o depoente).
- «por volta das dez» é aproximado.

## R04 — Gênero do depoente: Feminino — 39.4 s — nova tentativa

**Entrada:** Na prova de Cálculo II a Larissa sentou do meu lado e me passou um papel dobrado. Eu não abri. Eu devolvi pra ela e ela ficou brava comigo. Depois da prova ela me disse que eu era uma dedo-duro e que todo mundo ia saber. Eu não contei pra professora naquele dia, eu só contei uma semana depois, quando ela me ameaçou no grupo da turma.

**Saída final:** A depoente relatou que na prova de Cálculo II, Larissa sentou do lado da depoente e lhe passou um papel dobrado. A depoente não abriu. A depoente devolveu o papel para ela e ela ficou brava com a depoente. Depois da prova, ela lhe disse que a depoente era uma dedo-duro e que todo mundo ia saber. A depoente não contou à professora naquele dia; a depoente contou apenas uma semana depois, quando ela ameaçou a depoente no grupo da turma.

*Saída do motor (antes da conversão):* O depoente relatou que na prova de Cálculo II, Larissa sentou do lado do depoente e lhe passou um papel dobrado. O depoente não abriu. O depoente devolveu o papel para ela e ela ficou brava com o depoente. Depois da prova, ela lhe disse que o depoente era uma dedo-duro e que todo mundo ia saber. O depoente não contou à professora naquele dia; o depoente contou apenas uma semana depois, quando ela ameaçou o depoente no grupo da turma.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: eu devolvi pra ela e ela ficou brava comigo.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem passou o papel: Larissa. Quem não abriu e devolveu: a depoente.
- Quem ficou brava: Larissa.
- Ofensa: Larissa chamou a depoente de «dedo-duro» (a depoente NÃO chamou Larissa).
- Negações preservadas: «não abri», «não contei naquele dia».
- Quem ameaçou no grupo: Larissa ameaçou a depoente.

## R05 — Gênero do depoente: Masculino — 21.7 s

**Entrada:** Eu sou vigilante. Eram umas duas e quarenta da manhã quando um rapaz parou um carro prata, placa RJX4C21, perto do portão. Ele desceu e veio me xingando, falando que eu tinha que abrir o portão pra ele. Eu disse que sem crachá não podia. Aí ele tirou uma faca da cintura e me ameaçou. Eu me afastei e chamei a viatura pelo rádio. Ele entrou no carro e foi embora antes da viatura chegar.

**Saída final:** O depoente relatou que é vigilante. Eram umas duas e quarenta da manhã quando um rapaz parou um carro prata, placa RJX4C21, perto do portão. Ele desceu e veio xingando o depoente, falando que o depoente tinha que abrir o portão para ele. O depoente disse que sem crachá não podia. Aí ele tirou uma faca da cintura e ameaçou o depoente. O depoente se afastou e chamou a viatura pelo rádio. Ele entrou no carro e foi embora antes da viatura chegar.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: umas duas e quarenta da manhã

**Chave automática:** ok

**Chave (fatos a preservar):**
- Placa RJX4C21; carro prata; horário aproximado («umas duas e quarenta»).
- Quem xingou, exigiu e ameaçou com faca: o rapaz.
- Quem recusou, se afastou e chamou a viatura: o depoente.
- «veio me xingando» → o rapaz xingando o depoente (gerúndio, armadilha da P5).
- Quem foi embora: o rapaz.

## R06 — Gênero do depoente: Feminino — 37.6 s — nova tentativa

**Entrada:** Eu sou técnica administrativa na secretaria. A minha chefe, a Dona Rosângela, me humilhou na frente de todo mundo. Ela disse que eu era incompetente e que eu tinha perdido o processo do aluno. Mas fui eu que achei o processo, ele estava na mesa dela. Eu fiquei calada porque fiquei com medo. Depois a Rosângela me mandou um e-mail pedindo desculpas, no dia 22 de setembro.

**Saída final:** A depoente relatou que é técnica administrativa na secretaria. A chefe da depoente, a Dona Rosângela, humilhou a depoente na frente de todo mundo. Ela disse que a depoente era incompetente e que a depoente tinha perdido o processo do aluno. Mas foi a depoente que achou o processo, ele estava na mesa dela. A depoente ficou calada porque ficou com medo. Depois a Rosângela lhe mandou um e-mail pedindo desculpas, no dia 22 de setembro.

*Saída do motor (antes da conversão):* O depoente relatou que é técnica administrativa na secretaria. A chefe do depoente, a Dona Rosângela, humilhou o depoente na frente de todo mundo. Ela disse que o depoente era incompetente e que o depoente tinha perdido o processo do aluno. Mas foi o depoente que achou o processo, ele estava na mesa dela. O depoente ficou calada porque ficou com medo. Depois a Rosângela lhe mandou um e-mail pedindo desculpas, no dia 22 de setembro.

**Alertas laranja (papéis e pronomes):**
- «ele estava» — pronome ambíguo: «ele» pode ser lido como o próprio depoente. Confira a quem se refere.

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: possível omissão de oração: eu fiquei calada porque fiquei com medo.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem humilhou e acusou: Rosângela (a chefe).
- «ela disse que eu era incompetente»: Rosângela chamou a DEPOENTE de incompetente (armadilha «eu era culpada»).
- Quem achou o processo: a depoente. Onde estava: na mesa de Rosângela.
- Quem ficou calada e com medo: a depoente («calada»).
- Quem pediu desculpas por e-mail: Rosângela, em 22/09.

## R07 — Gênero do depoente: Masculino — 16.9 s

**Entrada:** Meu orientador, o professor Wanderley, foi afastado em agosto. Eu não sei o motivo direito. O coordenador ficou calado na reunião e ninguém explicou nada pra gente. Eu fiquei sem orientador quase dois meses. Aí a professora Cecília me aceitou como orientando, mas ela disse que não podia assinar o relatório antigo.

**Saída final:** O depoente relatou que o orientador do depoente, o professor Wanderley, foi afastado em agosto. O depoente não sabe o motivo exato. No encontro, o coordenador ficou calado e ninguém explicou nada para os alunos. O depoente ficou sem orientador por quase dois meses. A professora Cecília aceitou o depoente como orientando, mas ela disse que não podia assinar o relatório antigo.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem foi afastado: o professor Wanderley (NÃO o depoente). Armadilha do sujeito apagado.
- Quem ficou calado: o coordenador (NÃO o depoente).
- Quem ficou sem orientador: o depoente.
- Quem aceitou e quem disse que não podia assinar: a professora Cecília.

## R08 — Gênero do depoente: Não informado — 19.7 s

**Entrada:** Eu caí da escada do prédio da reitoria porque o corrimão estava solto. Me machuquei no joelho. Uma moça que estava passando me ajudou a levantar e me levou até o ambulatório. Lá a enfermeira me disse que eu tinha que fazer raio-x. Eu já tinha avisado a manutenção do corrimão no dia 5, pelo telefone (21) 3938-1100.

**Saída final:** O depoente relatou que caiu da escada do prédio da reitoria porque o corrimão estava solto. O depoente se machucou no joelho. Uma moça que passava ajudou o depoente a levantar e o levara até o ambulatório. Lá, a enfermeira disse ao depente que o depoente tinha que fazer raio-x. O depoente já tinha avisado a manutenção do corrimão no dia 5, pelo telefone (21) 3938-1100.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:**
- «O depoente» — gênero presumido: o original não diz se quem fala é homem ou mulher; «o depoente» supõe homem. Confira.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem caiu e se machucou: o/a depoente (reflexivo «me machuquei» → «se machucou»).
- Quem ajudou e levou ao ambulatório: a moça.
- Quem disse que precisava de raio-x: a enfermeira; quem precisava fazer: o/a depoente.
- Quem avisou a manutenção: o/a depoente, no dia 5; telefone preservado.
- Gênero não informado: aviso informativo esperado; não pode virar feminino sozinho.

## R09 — Gênero do depoente: Feminino — 15.4 s

**Entrada:** A Paula e a Renata brigaram no laboratório. A Paula disse que a Renata tinha pegado o caderno dela. Ela negou e disse que o caderno era dela mesmo. Eu tentei separar e acabei levando um tapa sem querer. Eu acho que foi da Renata, mas não tenho certeza.

**Saída final:** A depoente relatou que a Paula e a Renata brigaram no laboratório. A Paula disse que a Renata tinha pegado o caderno dela. Ela negou e disse que o caderno era dela mesmo. A depoente tentou separar e acabou levando um tapa sem querer. A depoente acredita que foi da Renata, mas a depoente não tem certeza.

*Saída do motor (antes da conversão):* O depoente relatou que a Paula e a Renata brigaram no laboratório. A Paula disse que a Renata tinha pegado o caderno dela. Ela negou e disse que o caderno era dela mesmo. O depoente tentou separar e acabou levando um tapa sem querer. O depoente acredita que foi da Renata, mas o depoente não tem certeza.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Ambiguidade real do original: «Ela negou» = Renata; «o caderno era dela» = de Renata.
- A saída NÃO pode resolver a dúvida de quem deu o tapa: a depoente «acha» que foi Renata, sem certeza.
- Quem tentou separar e levou o tapa: a depoente.
- Esperado: alerta laranja de pronome ambíguo se a saída mantiver «ela… dela».

## R10 — Gênero do depoente: Masculino — 37 s — nova tentativa — **SAÍDA INCOMPLETA (rejeitada)**

**Entrada:** O Diego me pediu duzentos reais emprestado em março, falou que ia me pagar em abril. Eu dei o dinheiro em espécie. Ele não me pagou. Em junho eu cobrei e ele me disse que já tinha me dado cento e cinquenta. Isso é mentira, ele não me deu nada. Aí ele começou a espalhar que eu é que devia pra ele.

**Saída final:** [SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO] / Motivo: O bloco 1 ainda precisa de revisão: fidelidade: participante explícito omitido ou substituído na oração: o diego me pediu duzentos reais emprestado em março, falou que ia me pagar em abril.. A reformulação foi interrompida: os blocos seguintes não foram processados e o trecho na tela foi marcado como incompleto. /  / O depoente relatou que lhe pediu duzentos reais emprestado em março, falou que ia lhe pagar em abril. O depoente deu o dinheiro em espécie. Ele não lhe pagou. Em junho o depoente cobrou e ele lhe disse que já tinha lhe dado cento e cinquenta. Isso é mentira, ele não lhe deu nada. Aí ele começou a espalhar que o depoente é que devia pra ele. /  / [FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: depoente omitido ou trocado como objeto: no original «Diego me pediu», ação ausente na saída; repairs=valores,abertura | [BLOCK_REJECTED] block=1/1; detail=fidelidade: participante explícito omitido ou substituído na oração: o diego me pediu duzentos reais emprestado em março, falou que ia me pagar em abril.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem pediu emprestado: Diego. Quem emprestou: o depoente (R$ 200).
- Quem não pagou: Diego.
- Alegação de Diego: que teria pago R$ 150 ao depoente; o depoente NEGA («é mentira»).
- Quem espalhou boato: Diego, dizendo que o DEPOENTE devia a Diego.
- Valores: duzentos reais e cento e cinquenta reais.

## R11 — Gênero do depoente: Feminino — 20.1 s

**Entrada:** Eu estava no ponto de ônibus em frente ao alojamento quando um homem de moto parou e me perguntou as horas. Quando eu fui olhar o celular ele puxou da minha mão e saiu. Eu gritei e um rapaz que estava na lanchonete correu atrás dele, mas não conseguiu pegar. Eu fiquei muito assustada. Foi umas onze e meia, onze e quarenta, por aí.

**Saída final:** A depoente relatou que estava no ponto de ônibus em frente ao alojamento quando um homem de moto parou e lhe perguntou as horas. Quando a depoente foi olhar o celular, ele puxou da mão da depoente e saiu. A depoente gritou e um rapaz que estava na lanchonete correu atrás dele, mas não conseguiu pegar. A depoente ficou muito assustada. Foi por volta de onze e meia, onze e quarenta, por aí.

*Saída do motor (antes da conversão):* O depoente relatou que estava no ponto de ônibus em frente ao alojamento quando um homem de moto parou e lhe perguntou as horas. Quando o depoente foi olhar o celular, ele puxou da mão do depoente e saiu. O depoente gritou e um rapaz que estava na lanchonete correu atrás dele, mas não conseguiu pegar. O depoente ficou muito assustado. Foi por volta de onze e meia, onze e quarenta, por aí.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: por volta de onze e meia
- aproximado ou ambíguo: horário: onze e quarenta, por aí

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem parou e perguntou as horas e quem puxou o celular: o homem de moto.
- Quem gritou e ficou assustada: a depoente («assustada»).
- Quem correu atrás e não conseguiu pegar: o rapaz da lanchonete.
- Horário aproximado («por aí»): não pode virar horário exato.

## R12 — Gênero do depoente: Masculino — 19.7 s

**Entrada:** Eu sou representante de turma. Na assembleia do dia 10, a professora Márcia me interrompeu várias vezes e disse que eu não tinha legitimidade pra falar pelos alunos. Eu respondi que tinha sido eleito. Aí o Tiago, que é aluno também, se levantou e me defendeu. A Márcia mandou ele sentar e ele sentou. No final ela me pediu pra mandar a ata por e-mail, e eu mandei no mesmo dia.

**Saída final:** O depoente relatou que é representante de turma. Na assembleia do dia 10, a professora Márcia o interrompeu várias vezes e disse que o depoente não tinha legitimidade para falar pelos alunos. O depoente respondeu que tinha sido eleito. Aí o Tiago, que também é aluno, se levantou e o defendeu. A Márcia mandou ele sentar e ele sentou. No final ela lhe pediu pra mandar a ata por e-mail, e o depoente mandou no mesmo dia.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem interrompeu e questionou a legitimidade: a professora Márcia.
- Quem respondeu que foi eleito: o depoente.
- Quem se levantou e defendeu o depoente: Tiago («me defendeu» = defendeu o depoente; NÃO «se defendeu»).
- Quem mandou sentar: Márcia; quem sentou: Tiago.
- Quem pediu a ata: Márcia; quem enviou no mesmo dia: o depoente.

