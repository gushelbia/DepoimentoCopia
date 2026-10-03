# Bateria realista 3 — resultados no motor atual

Depoimentos inventados. Saída final = texto que a janela mostra (com a conversão do gênero). Tempo = geração no motor.

## C01 — Gênero do depoente: Masculino — 21.9 s

**Entrada:** Ele mandou me chamar na sala da direção na segunda de manhã. Quando eu cheguei, o diretor Fausto e a vice, a Glória, já estavam lá. Ele me fez assinar uma advertência sem me deixar ler direito. Eu queria ler com calma, mas ele disse que não tinha tempo. Se ele tivesse me explicado o motivo, eu teria assinado sem problema. Depois eu fiquei sabendo pela Glória que a advertência era por causa de um atraso que nem foi meu, foi do meu colega de turno, o Anselmo.

**Saída final:** O depoente relatou que ele mandou chamar o depoente na sala da direção na segunda de manhã. Quando o depoente chegou, o diretor Fausto e a vice, a Glória, já estavam lá. Ele fez o depoente assinar uma advertência sem deixar o depoente ler direito. O depoente queria ler com calma, mas ele disse que não tinha tempo. Se ele tivesse lhe explicado o motivo, o depoente teria assinado sem problema. Depois o depoente ficou sabendo pela Glória que a advertência era por causa de um atraso que nem foi do depoente, foi do colega do depoente de turno, o Anselmo.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- «mandou me chamar», «me fez assinar», «sem me deixar ler»: o diretor Fausto é quem age; o depoente é quem foi chamado, obrigado a assinar e impedido de ler.
- Hipótese preservada: SE Fausto tivesse explicado, o depoente TERIA assinado sem problema (não é fato ocorrido).
- Quem informou o motivo: Glória, ao depoente.
- O atraso NÃO foi do depoente; foi de Anselmo, colega de turno do depoente.

## C02 — Gênero do depoente: Feminino — 22.6 s

**Entrada:** A gente estava em quatro no carro, eu, a Débora, a Fernanda e o Gilson, que estava dirigindo. Um carro preto bateu atrás da gente no sinal da avenida, umas sete e meia da noite. O motorista desceu e veio gritando que a culpa era nossa. Ele nos ameaçou dizendo que sabia onde a gente morava. O Gilson anotou a placa, PNQ2H67, e eu filmei com o meu celular. Ninguém se machucou, só a Fernanda que bateu o joelho no banco da frente.

**Saída final:** A depoente relatou que os quatro estavam no carro, a depoente, a Débora, a Fernanda e o Gilson, que estava dirigindo. Um carro preto bateu atrás dos quatro no sinal da avenida, umas sete e meia da noite. O motorista desceu e veio gritando que a culpa era do grupo. Ele ameaçou o grupo dizendo que sabia onde o grupo morava. O Gilson anotou a placa, PNQ2H67, e a depoente filmou com o seu celular. Ninguém se machucou, só a Fernanda que bateu o joelho no banco da frente.

*Saída do motor (antes da conversão):* O depoente relatou que os quatro estavam no carro, o depoente, a Débora, a Fernanda e o Gilson, que estava dirigindo. Um carro preto bateu atrás dos quatro no sinal da avenida, umas sete e meia da noite. O motorista desceu e veio gritando que a culpa era do grupo. Ele ameaçou o grupo dizendo que sabia onde o grupo morava. O Gilson anotou a placa, PNQ2H67, e o depoente filmou com o seu celular. Ninguém se machucou, só a Fernanda que bateu o joelho no banco da frente.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: umas sete e meia da noite

**Chave automática:** ok

**Chave (fatos a preservar):**
- «a gente» / «nos» / «nossa» = os quatro ocupantes (a depoente, Débora, Fernanda e Gilson).
- Quem dirigia: Gilson. Quem bateu atrás: o carro preto.
- Quem desceu, gritou e ameaçou os quatro: o motorista do carro preto.
- Quem anotou a placa PNQ2H67: Gilson. Quem filmou com o próprio celular: a depoente.
- Ferimento: só Fernanda bateu o joelho; a depoente NÃO se machucou.
- «umas sete e meia» é aproximado.

## C03 — Gênero do depoente: Feminino — 40.7 s — nova tentativa

**Entrada:** Eu, Marta, sou secretária do departamento há oito anos. Eu fui a primeira a chegar naquele dia e encontrei a porta da sala do professor Hélio arrombada. Não mexi em nada. Liguei pro Hélio e ele me disse pra não deixar ninguém entrar até a segurança chegar. Uns vinte minutos depois chegaram dois seguranças. Ninguém nunca me falou o que foi levado. O próprio Hélio me disse depois que não sabe se sumiu alguma coisa.

**Saída final:** A depoente, Marta, relatou que é secretária do departamento há oito anos e foi a primeira a chegar naquele dia e encontrou a porta da sala do professor Hélio arrombada. A depoente não mexeu em nada. A depoente ligou para Hélio e ele lhe disse para não deixar ninguém entrar até a segurança chegar. Uns vinte minutos depois chegaram dois seguranças. Ninguém nunca lhe falou o que foi levado. O próprio Hélio lhe disse depois que não sabe se sumiu alguma coisa.

*Saída do motor (antes da conversão):* O depoente, Marta, relatou que é secretária do departamento há oito anos e foi a primeira a chegar naquele dia e encontrou a porta da sala do professor Hélio arrombada. O depoente não mexeu em nada. O depoente ligou para Hélio e ele lhe disse para não deixar ninguém entrar até a segurança chegar. Uns vinte minutos depois chegaram dois seguranças. Ninguém nunca lhe falou o que foi levado. O próprio Hélio lhe disse depois que não sabe se sumiu alguma coisa.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: ninguém nunca me falou o que foi levado.

**Chave automática:** ok

**Chave (fatos a preservar):**
- A depoente se identifica como Marta, secretária há oito anos (feminino: «a primeira a chegar»).
- Quem encontrou a porta arrombada: a depoente. A sala é do professor Hélio.
- Negação: a depoente NÃO mexeu em nada.
- Quem ligou: a depoente ligou para Hélio; quem orientou a não deixar entrar: Hélio, à depoente.
- «Ninguém nunca me falou»: ninguém informou à depoente o que foi levado (dupla negação = nenhuma informação).
- Hélio NÃO sabe se sumiu algo. «Uns vinte minutos» é aproximado.

## C04 — Gênero do depoente: Masculino — 42.4 s — nova tentativa

**Entrada:** O Ícaro chegou perto de mim no intervalo e falou: "eu vou te pegar lá fora, seu dedo-duro". Eu não respondi nada. Ele achava que eu tinha contado pro professor que ele colou na prova. Eu não contei. Quem contou foi outra pessoa, eu não sei quem. Depois da aula ele estava me esperando no portão, mas o inspetor Jurandir viu e foi comigo até o ponto de ônibus.

**Saída final:** O depoente relatou que o Ícaro chegou perto do depoente no intervalo e falou: "eu vou te pegar lá fora, seu dedo-duro". O depoente não respondeu nada. Ele achava que o depoente tinha contado pro professor que ele colou na prova. O depoente não contou. Quem contou foi outra pessoa, o depoente não sabe quem. Depois da aula ele estava esperando o depoente no portão, mas o inspetor Jurandir viu e foi com o depoente até o ponto de ônibus.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: participante explícito omitido ou substituído na oração: o ícaro chegou perto de mim no intervalo e falou: .

**Chave automática:** ok

**Chave (fatos a preservar):**
- A fala entre aspas é de Ícaro, dirigida ao depoente; o «eu» e o «te» dentro das aspas NÃO devem ser convertidos.
- Quem não respondeu: o depoente.
- Ícaro ACHAVA que o depoente tinha contado; o depoente NÃO contou; quem contou é desconhecido.
- Quem colou na prova (segundo a acusação): Ícaro.
- Quem esperava no portão: Ícaro, esperando o depoente.
- Quem viu e acompanhou o depoente até o ponto: o inspetor Jurandir.

## C05 — Gênero do depoente: Feminino — 21.7 s

**Entrada:** Eu não lembro se foi o Lauro ou o irmão dele, o Márcio, que jogou a garrafa. Eu só vi a garrafa vindo e me abaixei. Ela bateu na parede do meu lado. Os dois estavam juntos perto da escada. Eu sei que o Lauro já tinha brigado comigo antes, em março, mas eu não posso afirmar que foi ele. Eu prefiro não acusar ninguém sem certeza.

**Saída final:** A depoente relatou que não lembra se foi o Lauro ou o irmão dele, o Márcio, que jogou a garrafa. A depoente só viu a garrafa vindo e se abaixou. Ela bateu na parede do lado da depoente. Os dois estavam juntos perto da escada. A depoente sabe que o Lauro já tinha brigado com a depoente em março, mas a depoente não pode afirmar que foi ele. A depoente prefere não acusar ninguém sem certeza.

*Saída do motor (antes da conversão):* O depoente relatou que não lembra se foi o Lauro ou o irmão dele, o Márcio, que jogou a garrafa. O depoente só viu a garrafa vindo e se abaixou. Ela bateu na parede do lado do depoente. Os dois estavam juntos perto da escada. O depoente sabe que o Lauro já tinha brigado com o depoente em março, mas o depoente não pode afirmar que foi ele. O depoente prefere não acusar ninguém sem certeza.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Incerteza essencial: a depoente NÃO lembra se quem jogou a garrafa foi Lauro ou Márcio. A saída NÃO pode atribuir o ato a um deles.
- Quem se abaixou: a depoente («me abaixei» → «se abaixou»).
- A garrafa bateu na parede ao lado da depoente.
- Lauro já tinha brigado com a depoente em março (fato anterior), mas ela NÃO pode afirmar que foi ele.
- A depoente prefere não acusar ninguém sem certeza.

## C06 — Gênero do depoente: Masculino — 40.7 s — nova tentativa

**Entrada:** Olha, sei lá, tipo, foi tudo muito rápido, né. Eu tava saindo do prédio da Engenharia e um cara veio correndo e me deu um encontrão. Eu caí de lado. Ele nem parou. Logo atrás vinha um segurança gritando "pega, pega". Aí eu levantei e vi que o meu celular tinha caído e quebrado a tela. Eu não sei se o cara roubou alguma coisa de alguém, só sei que o segurança tava atrás dele.

**Saída final:** O depoente relatou que foi tudo muito rápido. O depoente estava saindo do prédio da Engenharia e um cara veio correndo e lhe deu um encontrão. O depoente caiu de lado. Ele nem parou. Logo atrás vinha um segurança gritando "pega, pega". O depoente levantou e viu que o seu celular tinha caído e quebrado a tela. O depoente não sabe se o cara roubou alguma coisa de alguém, o depoente só sabe que o segurança estava atrás dele.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: possível omissão de oração: olha, sei lá, tipo, foi tudo muito rápido, né.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Marcadores da fala («sei lá», «tipo», «né», «tava») não devem aparecer no texto formal nem virar fatos.
- Quem veio correndo e deu o encontrão: o «cara». Quem caiu: o depoente.
- O «cara» não parou. O segurança vinha atrás gritando «pega, pega» (fala entre aspas).
- O celular é do depoente; caiu e quebrou a tela (NÃO foi roubado).
- Incerteza: o depoente NÃO sabe se o «cara» roubou algo; sabe só que o segurança o perseguia.

## C07 — Gênero do depoente: Feminino — 20.3 s

**Entrada:** A professora Noêmia disse pra Olívia que eu tinha falado que a Olívia copiou o trabalho dela. Isso nunca aconteceu. Eu nunca disse isso pra ninguém. A Olívia veio tirar satisfação comigo no corredor e eu expliquei que não tinha sido eu. Ela acreditou em mim e a gente foi junto falar com a Noêmia. A Noêmia disse que tinha ouvido isso de um aluno e que não lembrava quem.

**Saída final:** A depoente relatou que a professora Noêmia disse a Olívia que a depoente tinha falado que a Olívia copiou o trabalho dela. Isso nunca aconteceu. A depoente nunca disse isso a ninguém. Olívia veio tirar satisfação com a depoente no corredor e a depoente explicou que não tinha sido a depoente. Ela acreditou na depoente e a depoente e a Olívia foram juntas falar com a Noêmia. A Noêmia disse que tinha ouvido isso de um aluno e que não lembrava quem.

*Saída do motor (antes da conversão):* O depoente relatou que a professora Noêmia disse a Olívia que o depoente tinha falado que a Olívia copiou o trabalho dela. Isso nunca aconteceu. O depoente nunca disse isso a ninguém. Olívia veio tirar satisfação com o depoente no corredor e o depoente explicou que não tinha sido o depoente. Ela acreditou no depoente e o depoente e a Olívia foram juntos falar com a Noêmia. A Noêmia disse que tinha ouvido isso de um aluno e que não lembrava quem.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Cadeia de discurso: Noêmia disse a Olívia que a DEPOENTE teria dito que Olívia copiou o trabalho de Noêmia («dela» = de Noêmia).
- Negação: a depoente NUNCA disse isso a ninguém.
- Quem tirou satisfação: Olívia, com a depoente. Quem explicou: a depoente.
- Quem acreditou: Olívia acreditou na depoente. «a gente» = a depoente e Olívia, que foram juntas a Noêmia.
- Noêmia disse que ouviu de um aluno e não lembra quem.

## C08 — Gênero do depoente: Masculino — 50.2 s — nova tentativa

**Entrada:** Eu sou o técnico responsável pelo laboratório de Química, matrícula 1.204.338. No dia 15 de setembro, por volta das 14h, a aluna Priscila veio me buscar dizendo que tinha cheiro de gás na sala 3. Eu fui com ela e fechei o registro geral. Depois eu abri as janelas e pedi pra todo mundo sair. A Priscila me ajudou a tirar os alunos. Ninguém passou mal. Eu acho que alguém deixou um bico de Bunsen aberto, mas eu não vi quem. Eu registrei a ocorrência no mesmo dia, às 16h20.

**Saída final:** O depoente relatou que é o técnico responsável pelo laboratório de Química, matrícula 1.204.338. No dia 15 de setembro, por volta das 14h, a aluna Priscila veio buscar o depoente dizendo que tinha cheiro de gás na sala 3. O depoente foi com ela e fechou o registro geral. Depois o depoente abriu as janelas e o depoente pediu pra todo mundo sair. A Priscila ajudou o depoente a tirar os alunos. Ninguém passou mal. O depoente acredita que alguém deixou um bico de Bunsen aberto, mas o depoente não viu quem. O depoente registrou a ocorrência no mesmo dia, às 16h20.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- aproximado ou ambíguo: horário: por volta das 14h

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: depoente omitido ou trocado como objeto: no original «veio me buscar»; repairs=ancoras-do-original,depoente-objeto,abertura

**Chave automática:** ok

**Chave (fatos a preservar):**
- Matrícula 1.204.338 preservada. Data 15/09.
- «por volta das 14h» é aproximado; «às 16h20» é exato.
- Quem veio buscar o depoente e avisou do gás: a aluna Priscila («me buscar» = buscar o depoente).
- Quem fechou o registro, abriu as janelas e pediu para saírem: o depoente.
- Quem ajudou a tirar os alunos: Priscila ajudou o depoente.
- Ninguém passou mal. A causa (bico de Bunsen aberto) é suposição («acho»); o depoente NÃO viu quem.
- Quem registrou a ocorrência às 16h20: o depoente.

