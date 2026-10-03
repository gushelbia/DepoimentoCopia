# Bateria realista 2 — resultados no motor atual

Depoimentos inventados. Saída final = texto que a janela mostra (com a conversão do gênero). Tempo = geração no motor.

## B01 — Gênero do depoente: Feminino — 57.1 s — nova tentativa — **SAÍDA INCOMPLETA (rejeitada)**

**Entrada:** Eu sou bolsista do projeto de extensão desde o ano passado. A coordenadora, a professora Ivone, sempre foi muito exigente, mas em maio começou a ficar diferente comigo. Ela me tirou do grupo de mensagens do projeto sem avisar. Quando eu perguntei por quê, ela falou: "você sabe muito bem o que você fez". Eu não sabia, juro. Aí a Kátia, que também é bolsista, me contou que a Ivone achava que eu tinha falado mal dela pro diretor. Eu nunca falei com o diretor, eu nem conheço ele pessoalmente. Depois disso a Ivone cortou a minha bolsa em junho, e a bolsa da Kátia continuou. Eu me lembro que fiquei uns três dias sem conseguir dormir direito.

**Saída final:** [SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO] / Motivo: O bloco 1 ainda precisa de revisão: fidelidade: primeira pessoa fora das aspas: «conheço». A reformulação foi interrompida: os blocos seguintes não foram processados e o trecho na tela foi marcado como incompleto. /  / A depoente relatou que é bolsista do projeto de extensão desde o ano passado. A coordenadora, a professora Ivone, sempre foi muito exigente, mas em maio começou a ficar diferente com a depoente. Ela tirou a depoente do grupo de mensagens do projeto sem avisar. Quando a depoente perguntou por quê, ela falou: "você sabe muito bem o que você fez". A depoente não sabia. Aí a Kátia, que também é bolsista, lhe contou que a Ivone achava que a depoente tinha falado mal dela pro diretor. A depoente nunca falou com o diretor, nem conheço pessoalmente. Depois disso, a Ivone cortou a bolsa da depoente em junho, e a bolsa da Kátia continuou. A depoente se lembra que ficou uns três dias sem conseguir dormir direito. /  / [FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]

*Saída do motor (antes da conversão):* [SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO] / Motivo: O bloco 1 ainda precisa de revisão: fidelidade: primeira pessoa fora das aspas: «conheço». A reformulação foi interrompida: os blocos seguintes não foram processados e o trecho na tela foi marcado como incompleto. /  / O depoente relatou que é bolsista do projeto de extensão desde o ano passado. A coordenadora, a professora Ivone, sempre foi muito exigente, mas em maio começou a ficar diferente com o depoente. Ela tirou o depoente do grupo de mensagens do projeto sem avisar. Quando o depoente perguntou por quê, ela falou: "você sabe muito bem o que você fez". O depoente não sabia. Aí a Kátia, que também é bolsista, lhe contou que a Ivone achava que o depoente tinha falado mal dela pro diretor. O depoente nunca falou com o diretor, nem conheço pessoalmente. Depois disso, a Ivone cortou a bolsa do depoente em junho, e a bolsa da Kátia continuou. O depoente se lembra que ficou uns três dias sem conseguir dormir direito. /  / [FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: a coordenadora, a professora ivone, sempre foi muito exigente, mas em maio começou a ficar diferente comigo. | [BLOCK_REJECTED] block=1/1; detail=fidelidade: primeira pessoa fora das aspas: «conheço»

**Chave automática:** FALHA — não devia ter: primeira pessoa fora das aspas

**Chave (fatos a preservar):**
- Quem tirou a depoente do grupo e cortou a bolsa dela: Ivone.
- Fala citada entre aspas pertence a Ivone e se dirige à depoente.
- Quem contou: Kátia contou à depoente que Ivone achava que a depoente tinha falado mal de Ivone ao diretor.
- Negações: a depoente NUNCA falou com o diretor e NÃO o conhece pessoalmente.
- A bolsa cortada é a da depoente; a de Kátia continuou.
- «me lembro» é pronominal: «a depoente se lembra» / «recorda» (não é ação de outra pessoa).
- «uns três dias» é aproximado.

## B02 — Gênero do depoente: Masculino — 27.4 s

**Entrada:** Eu e o meu irmão, o Otávio, estávamos voltando da festa da atlética, era sábado, umas três da manhã. Na saída do estacionamento três caras pararam a gente. Um deles, de boné vermelho, empurrou o Otávio e falou que a gente tinha mexido com a namorada dele. A gente não tinha mexido com ninguém. O de boné me deu um soco no rosto e os outros dois seguraram o meu irmão. Eu caí e bati a cabeça no meio-fio. Quem chamou o segurança foi uma menina que estava no carro do lado. Eles fugiram a pé quando o segurança chegou. Eu fiquei com o olho roxo e o Otávio não se machucou.

**Saída final:** O depoente relatou que o depoente e o seu irmão, o Otávio, estavavam voltando da festa da atlética, era sábado, umas três da manhã. Na saída do estacionamento três caras pararam a gente. Um deles, de boné vermelho, empurrou o Otávio e falou que a gente tinha mexido com a namorada dele. A gente não tinha mexido com ninguém. O de boné lhe deu um soco no rosto e os outros dois seguraram o irmão do depoente. O depoente caiu e bateu a cabeça no meio-fio. Quem chamou o segurança foi uma menina que estava no carro do lado. Eles fugiram a pé quando o segurança chegou. O depoente ficou com o olho roxo e o Otávio não se machucou.

**Alertas laranja (papéis e pronomes):**
- «estavavam» — palavra inexistente: «estavavam» não está no dicionário nem no original. Confira a grafia.

**Avisos informativos:** nenhum

**Chave automática:** FALHA — não devia ter: palavra inexistente

**Chave (fatos a preservar):**
- «a gente» = o depoente e o irmão Otávio (ambos parados pelos três).
- Quem empurrou Otávio e acusou: o de boné vermelho.
- Negação: o depoente e Otávio NÃO tinham mexido com ninguém.
- Quem deu o soco no depoente: o de boné vermelho. Quem segurou Otávio: os outros dois.
- Quem caiu e bateu a cabeça: o depoente.
- Quem chamou o segurança: a menina do carro ao lado (NÃO o depoente).
- Quem fugiu: os três. Quem ficou com olho roxo: o depoente. Otávio NÃO se machucou.

## B03 — Gênero do depoente: Feminino — 25.7 s

**Entrada:** Ontem de manhã, antes da aula de Anatomia, eu fui no banheiro do segundo andar e quando eu voltei a minha mochila não estava mais na cadeira. Dentro tinha meu notebook, minha carteira com cento e oitenta reais e meu crachá. O Bruno, que senta atrás de mim, disse que viu um rapaz de moletom cinza pegando a mochila, mas achou que era meu namorado. Eu não tenho namorado. Eu fui na segurança e eles me mostraram a câmera, mas a imagem estava muito escura. Depois, por volta do meio-dia, a minha carteira apareceu no achados e perdidos, sem o dinheiro.

**Saída final:** A depoente relatou que ontem de manhã, antes da aula de Anatomia, a depoente foi no banheiro do segundo andar e quando voltou a mochila da depoente não estava mais na cadeira. Dentro havia o notebook da depoente, sua carteira com ciento e oitenta reais e seu crachá. O Bruno, que senta atrás da depoente, disse que viu um rapaz de moletom cinza pegando a mochila, mas achou que era o namorado da depoente. A depoente não tem namorado. A depoente foi na segurança e eles lhe mostraram a câmera, mas a imagem estava muito escura. Depois, por volta do meio-dia, a carteira da depoente apareceu no achados e perdidos, sem o dinheiro.

*Saída do motor (antes da conversão):* O depoente relatou que ontem de manhã, antes da aula de Anatomia, o depoente foi no banheiro do segundo andar e quando voltou a mochila do depoente não estava mais na cadeira. Dentro havia o notebook do depoente, sua carteira com ciento e oitenta reais e seu crachá. O Bruno, que senta atrás do depoente, disse que viu um rapaz de moletom cinza pegando a mochila, mas achou que era o namorado do depoente. O depoente não tem namorado. O depoente foi na segurança e eles lhe mostraram a câmera, mas a imagem estava muito escura. Depois, por volta do meio-dia, a carteira do depoente apareceu no achados e perdidos, sem o dinheiro.

**Alertas laranja (papéis e pronomes):**
- «ciento» — palavra inexistente: «ciento» não está no dicionário nem no original. Confira a grafia.

**Avisos informativos:** nenhum

**Datas, números e nomes:**
- não encontrado no original: valor: oitenta reais
- aproximado ou ambíguo: horário: por volta do meio-dia
- sumiu do reformulado: valor: cento e oitenta reais

**Chave automática:** FALHA — falta: valor; não devia ter: palavra inexistente

**Chave (fatos a preservar):**
- A mochila, o notebook, a carteira (com R$ 180) e o crachá são da depoente.
- Quem viu o rapaz de moletom cinza pegando a mochila: Bruno.
- Bruno ACHOU que o rapaz era namorado da depoente; a depoente NÃO tem namorado.
- Quem mostrou a câmera: a segurança, à depoente.
- A carteira reapareceu SEM o dinheiro, «por volta do meio-dia» (aproximado).
- «Ontem» deve continuar relativo (não inventar data).

## B04 — Gênero do depoente: Masculino — 47.9 s — nova tentativa

**Entrada:** Eu sou servidor do almoxarifado há doze anos. No dia 2 de outubro o senhor Alcides, que é meu chefe, me mandou assinar o recebimento de quarenta cadeiras. Eu falei pra ele que só tinham chegado vinte e oito. Ele disse que as outras doze iam chegar depois e que era pra eu assinar assim mesmo. Eu não assinei. Ele ficou irritado e falou que eu ia me arrepender. Na semana seguinte eu fui transferido pro turno da noite. Eu acho que foi por causa disso, mas ninguém me falou o motivo.

**Saída final:** O depoente relatou que é servidor do almoxarifado há doze anos. No dia 2 de outubro o senhor Alcides, que é o chefe do depoente, lhe mandou assinar o recebimento de quarenta cadeiras. O depoente falou pra ele que só tinham chegado vinte e oito. Ele disse que as outras doze iam chegar depois e que era pra o depoente assinar assim mesmo. O depoente não assinou. Ele ficou irritado e falou que o depoente ia se arrepender. Na semana seguinte o depoente foi transferido pro turno da noite. O depoente acredita que foi por causa disso, mas ninguém lhe falou o motivo.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: depoente omitido ou trocado como objeto: no original «me mandou»; repairs=primeira-pessoa,possessivo,abertura

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem mandou assinar: Alcides (chefe). Quem recusou: o depoente («não assinei»).
- Números: 40 cadeiras declaradas; só 28 chegaram; as «outras doze» viriam depois.
- Quem ficou irritado e ameaçou («ia se arrepender»): Alcides; quem «ia se arrepender»: o depoente.
- Quem foi transferido: o depoente.
- A causa é suposição do depoente («acho»), não fato; ninguém informou o motivo.

## B05 — Gênero do depoente: Não informado — 21.8 s

**Entrada:** Eu moro no alojamento, quarto 214. A minha colega de quarto, a Simone, começou a levar o namorado dela pra dormir lá quase todo dia. Eu pedi pra ela não fazer isso porque é proibido e porque eu não me sinto bem. Ela disse que eu era chata e que se eu reclamasse com a administração ela ia fazer da minha vida um inferno. Eu reclamei mesmo assim, no dia 18. Desde então ela não fala comigo e escondeu o meu carregador duas vezes.

**Saída final:** O depoente relatou que mora no alojamento, quarto 214. A colega de quarto, a Simone, começou a levar o namorado dela para dormir lá quase todo dia. O depoente pediu pra ela não fazer isso porque é proibido e porque ele não se sente bem. Ela disse que o depoente era chato e que se o depoente reclamasse com a administração ela ia fazer da vida do depoente um inferno. O depoente reclamou mesmo assim, no dia 18. Desde então ela não fala com ele e escondeu o carregador do depoente duas vezes.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:**
- «O depoente» — gênero presumido: o original não diz se quem fala é homem ou mulher; «o depoente» supõe homem. Confira.

**Chave automática:** ok

**Chave (fatos a preservar):**
- Gênero não informado: NÃO converter para feminino; aviso informativo esperado. («colega de quarto» é feminino porque se refere a Simone, não à pessoa depoente.)
- Quem levava o namorado: Simone. O namorado é de Simone.
- Quem pediu: o/a depoente.
- Quem disse que o/a depoente era chato(a) e ameaçou: Simone.
- Quem reclamou à administração no dia 18: o/a depoente.
- Quem parou de falar e escondeu o carregador (do/da depoente) duas vezes: Simone.

## B06 — Gênero do depoente: Feminino — 22.8 s

**Entrada:** Eu estava dando monitoria quando o aluno Jefferson entrou na sala gritando. Ele dizia que eu tinha dado a nota errada pra ele de propósito. Eu expliquei que quem dá a nota é o professor, não a monitora. Ele não quis ouvir. Ele bateu com a mão na mesa e chegou muito perto de mim. A Luana, que é a outra monitora, saiu pra chamar o professor Emanuel. Quando o professor chegou, o Jefferson já tinha ido embora. O professor me perguntou se eu queria registrar a ocorrência e eu falei que sim.

**Saída final:** A depoente relatou que estava dando monitoria quando o aluno Jefferson entrou na sala gritando. Ele dizia que a depoente tinha dado a nota errada para ele de propósito. A depoente explicou que quem dá a nota é o professor, não a monitora. Jefferson não quis ouvir. Ele bateu com a mão na mesa e chegou muito perto da depoente. A Luana, que é a outra monitora, saiu pra chamar o professor Emanuel. Quando o professor chegou, Jefferson já tinha ido embora. O professor perguntou se a depoente queria registrar a ocorrência e a depoente disse que sim.

*Saída do motor (antes da conversão):* O depoente relatou que estava dando monitoria quando o aluno Jefferson entrou na sala gritando. Ele dizia que o depoente tinha dado a nota errada para ele de propósito. O depoente explicou que quem dá a nota é o professor, não a monitora. Jefferson não quis ouvir. Ele bateu com a mão na mesa e chegou muito perto do depoente. A Luana, que é a outra monitora, saiu pra chamar o professor Emanuel. Quando o professor chegou, Jefferson já tinha ido embora. O professor perguntou se o depoente queria registrar a ocorrência e o depoente disse que sim.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- A depoente é monitora («a monitora», feminino).
- Quem entrou gritando e acusou: Jefferson. A acusação: que a DEPOENTE deu a nota errada a ele de propósito.
- Quem explicou: a depoente. Quem não quis ouvir, bateu na mesa e chegou perto: Jefferson.
- Quem saiu para chamar o professor: Luana (NÃO a depoente).
- Quem já tinha ido embora: Jefferson.
- Quem perguntou: o professor Emanuel perguntou à depoente; quem disse que sim: a depoente.

## B07 — Gênero do depoente: Masculino — 24.5 s

**Entrada:** Eu não vi a briga, eu só ouvi. Eu estava na biblioteca, no terceiro andar, e ouvi alguém gritando "me solta, me solta" no corredor. Quando eu cheguei na porta, tinha um rapaz no chão e uma moça de cabelo curto do lado dele. Ela falou pra mim: "ele tentou me agarrar". Ele levantou e disse que era mentira e saiu andando. Eu não conhecia nenhum dos dois. Eu fiquei com a moça até a segurança chegar, uns quinze minutos depois.

**Saída final:** O depoente relatou que não viu a briga, apenas ouviu. O depoente estava na biblioteca, no terceiro andar, e ouviu alguém gritando "me solta, me solta" no corredor. Quando o depoente chegou à porta, havia um rapaz no chão e uma moça de cabelo curto do lado dele. A moça falou ao depoente: "ele tentou me agarrar". O rapaz levantou-se e disse que era mentira, depois saiu andando. O depoente não conhecia nenhum dos dois. O depoente ficou com a moça até que a segurança chegasse, cerca de quinze minutos depois.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Negação essencial: o depoente NÃO viu a briga, só ouviu.
- «me solta, me solta» é fala de outra pessoa, entre aspas (NÃO do depoente).
- «ele tentou me agarrar» é fala da moça: o rapaz teria tentado agarrar a MOÇA (não o depoente).
- Quem negou e saiu: o rapaz.
- O depoente não conhecia nenhum dos dois.
- Quem ficou com a moça: o depoente. «uns quinze minutos» é aproximado.

## B08 — Gênero do depoente: Feminino — 43.3 s — nova tentativa

**Entrada:** A minha mãe me ligou dizendo que tinham ligado pra ela do meu celular pedindo dinheiro. Só que o meu celular estava comigo o tempo todo. Ela já tinha feito um Pix de quinhentos e cinquenta reais pra uma conta no nome de um tal de Edvaldo. Eu fui com ela na delegacia no mesmo dia, dia 25 de agosto, e a gente fez o boletim de ocorrência. O banco disse pra ela que ia tentar estornar, mas até hoje não devolveram nada.

**Saída final:** A depoente relatou que a mãe da depoente lhe ligou dizendo que tinham ligado para ela do celular da depoente pedindo dinheiro. Só que o celular da depoente estava com a depoente o tempo todo. Ela já tinha feito um Pix de quinhentos e cinquenta reais pra uma conta no nome de um tal de Edvaldo. A depoente foi com ela na delegacia no mesmo dia, dia 25 de agosto, e a gente fez o boletim de ocorrência. O banco disse para ela que ia tentar estornar, mas até hoje não devolveram nada.

*Saída do motor (antes da conversão):* O depoente relatou que a mãe do depoente lhe ligou dizendo que tinham ligado para ela do celular do depoente pedindo dinheiro. Só que o celular do depoente estava com o depoente o tempo todo. Ela já tinha feito um Pix de quinhentos e cinquenta reais pra uma conta no nome de um tal de Edvaldo. O depoente foi com ela na delegacia no mesmo dia, dia 25 de agosto, e a gente fez o boletim de ocorrência. O banco disse para ela que ia tentar estornar, mas até hoje não devolveram nada.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: possível omissão de oração: só que o meu celular estava comigo o tempo todo.; repairs=abertura

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem ligou para a depoente: a mãe dela.
- Ligaram para a MÃE dizendo ser do celular da depoente; o celular estava COM a depoente.
- Quem fez o Pix de R$ 550: a mãe (NÃO a depoente), para conta em nome de Edvaldo.
- Quem foi à delegacia: a depoente e a mãe, em 25/08; «a gente» = as duas.
- O banco disse à mãe que ia tentar estornar; nada foi devolvido.

## B09 — Gênero do depoente: Masculino — 21.4 s

**Entrada:** O professor Rogério me chamou pra ser monitor dele no semestre passado e eu aceitei. No começo foi tudo bem. Depois ele começou a me pedir pra corrigir as provas inteiras sozinho, inclusive as notas finais. Eu falei que não me sentia confortável, porque isso é função dele. Ele disse que se eu não quisesse tinha uma fila de gente querendo a vaga. Eu continuei corrigindo porque precisava da declaração de monitoria. Eu me arrependo de não ter falado antes com a coordenação.

**Saída final:** O depoente relatou que o professor Rogério chamou o depoente pra ser monitor dele no semestre passado e o depoente aceitou. No começo foi tudo bem. Depois ele começou a lhe pedir pra corrigir as provas inteiras sozinho, inclusive as notas finais. O depoente falou que não se sentia confortável, porque isso é função dele. Ele disse que se o depoente não quisesse tinha uma fila de gente querendo a vaga. O depoente continuou corrigindo porque precisava da declaração de monitoria. O depoente se arrepende de não ter falado antes com a coordenação.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem chamou e quem aceitou: Rogério chamou o depoente; o depoente aceitou.
- Quem pedia para corrigir as provas: Rogério; quem corrigia: o depoente.
- Quem não se sentia confortável: o depoente; «isso é função dele» = função de Rogério.
- Quem respondeu com a «fila de gente»: Rogério.
- «me arrependo» é pronominal: o depoente se arrepende (de não ter falado antes).

## B10 — Gênero do depoente: Feminino — 53.5 s — nova tentativa

**Entrada:** Na reunião do colegiado do dia 9 de setembro, eu pedi a palavra pra falar sobre a carga horária dos técnicos. O professor Nestor, que presidia a reunião, me disse que aquilo não estava na pauta. Eu mostrei que estava sim, no item 4. Aí ele falou que eu podia falar, mas só por dois minutos. Enquanto eu falava, ele e o professor Clóvis ficaram conversando alto e rindo. Eu parei e pedi que eles me respeitassem. O Clóvis disse que não estava rindo de mim. Depois da reunião, a professora Dalva veio me pedir desculpas pelos colegas e me disse que ia levar o assunto pra direção.

**Saída final:** A depoente relatou que na reunião do colegiado do dia 9 de setembro, a depoente pediu a palavra pra falar sobre a carga horária dos técnicos. O professor Nestor, que presidia a reunião, lhe disse que aquilo não estava na pauta. A depoente mostrou que estava sim, no item 4. Aí ele falou que a depoente podia falar, mas só por dois minutos. Enquanto a depoente falava, ele e o professor Clóvis ficaram conversando alto e rindo. A depoente parou e pediu que eles respeitassem a depoente. O Clóvis disse que não estava rindo da depoente. Depois da reunião, a professora Dalva veio lhe pedir desculpas pelos colegas e lhe disse que ia levar o assunto pra direção.

*Saída do motor (antes da conversão):* O depoente relatou que na reunião do colegiado do dia 9 de setembro, o depoente pediu a palavra pra falar sobre a carga horária dos técnicos. O professor Nestor, que presidia a reunião, lhe disse que aquilo não estava na pauta. O depoente mostrou que estava sim, no item 4. Aí ele falou que o depoente podia falar, mas só por dois minutos. Enquanto o depoente falava, ele e o professor Clóvis ficaram conversando alto e rindo. O depoente parou e pediu que eles respeitassem o depoente. O Clóvis disse que não estava rindo do depoente. Depois da reunião, a professora Dalva veio lhe pedir desculpas pelos colegas e lhe disse que ia levar o assunto pra direção.

**Alertas laranja (papéis e pronomes):** nenhum

**Avisos informativos:** nenhum

**Motor:** [BLOCK_RETRY_START] block=1/1; issue=fidelidade: depoente omitido ou trocado como objeto: no original «eles me respeitassem»

**Chave automática:** ok

**Chave (fatos a preservar):**
- Quem pediu a palavra: a depoente.
- Quem disse que não estava na pauta: Nestor, à depoente. Quem mostrou o item 4: a depoente.
- Quem permitiu «só por dois minutos»: Nestor.
- Quem conversou alto e riu: Nestor e Clóvis (NÃO a depoente).
- Quem pediu respeito: a depoente pediu que Nestor e Clóvis a respeitassem.
- Quem negou estar rindo da depoente: Clóvis.
- Quem pediu desculpas pelos colegas e disse que ia levar à direção: Dalva, à depoente.
- Data: 09/09.

