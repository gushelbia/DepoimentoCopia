# QA / regressão — DepoimentoLocal

Atualizado: 2026-10-02T07:20:56.2674645-03:00. Bateria finalizada: True.

Execução real, sequencial, via OriginalAppBridge da interface de produção e botão Reformular do motor. Mesmos binários, prompt, reparos locais, validador e retry do aplicativo; nenhum resultado foi corrigido pelo QA. Cada caso usa uma instância própria. A compilação dos fontes disponíveis (interface e runner) está em build.txt; o fonte/projeto do motor não acompanha a distribuição, portanto ele é reutilizado sem recompilação.

Truncamento, primeira pessoa e ausência do final são indícios conservadores, não avaliações semânticas. Ausência de alerta não significa fidelidade garantida. Duplicação exige repetição integral exata (espaços normalizados); parágrafos repetidos têm pelo menos 60 caracteres. Citações pareadas são excluídas do teste de primeira pessoa. Nuances verbais, fontes e ambiguidades ficam para revisão humana. O motor não expõe motivo de parada por token/EOS neste log; não se infere conclusão normal apenas pela pontuação.

Os logs nativos são separados dos warnings do validador. O log original é compartilhado pelo aplicativo e não inclui PID; não gere textos em outra instância durante a bateria. O runner rejeita atividade concorrente detectável. Tempos de geração excluem carregamento; tempo total inclui carregamento e encerramento das instâncias.

Compilação adicional do launcher + interface: application-build.txt. Integridade das DLLs antes/depois: engine-integrity.json. Captura integral conferida pelo número de caracteres publicado em GENERATION_OK. Os trechos citados abaixo não foram abreviados.

## TESTE 1 — Linguagem coloquial e desorganizada

Objetivo: Testar terceira pessoa, incerteza, relato indireto e preservação das fontes.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 41,054s de geração; 43,577s incluindo preparação.

Entrada: 1014 caracteres / 196 palavras. Saída: 1243 caracteres / 228 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-01-log.txt](test-01-log.txt).

### Entrada completa

```text
Eu cheguei lá acho que era umas duas e pouco, não lembro direito. Quando entrei o João já tava falando com a Maria e parecia que eles tavam discutindo, mas eu não sei porque eu não ouvi o começo. Aí eu ouvi o João falar alguma coisa tipo que aquilo não podia continuar, mas não lembro as palavras certinhas. A Maria respondeu alguma coisa mais alto também, só que tinha gente falando perto e eu não entendi tudo. Depois eu saí pra ir no banheiro e quando voltei o Carlos falou pra mim que o João tinha batido na mesa. Eu não vi isso. Também não sei se ele bateu mesmo ou se só colocou a mão na mesa. Depois a Maria veio falar comigo e disse que ele tinha sido agressivo com ela, mas ela não falou de agressão física. Eu também não vi ninguém encostando em ninguém. Mais tarde eu vi o chefe entrando na sala, mas não sei quem chamou ele. Acho que foi a Maria, mas posso estar enganado. Eu fui embora antes de terminar e no outro dia me disseram que eles tiveram outra reunião. Não sei quem participou dessa reunião.
```

### Saída completa

```text
O depoente relatou que chegou lá; o depoente acredita que era umas duas e pouco, o depoente não lembra direito. Quando o depoente entrou o João já estava falando com a Maria e parecia que eles estavam discutindo, mas o depoente não sabe porque o depoente não ouviu o começo. O depoente ouviu o João falar alguma coisa tipo que aquilo não podia continuar, mas não lembra as palavras certas. A Maria respondeu alguma coisa mais alto também, só que tinha gente falando perto e o depoente não entendeu tudo. Depois o depoente saiu para ir no banheiro e quando o depoente voltou o Carlos falou para o depoente que o João tinha batido na mesa. O depoente não viu isso. O depoente também não sabe se ele bateu mesmo ou se só colocou a mão na mesa. Depois a Maria veio conversar com o depoente e disse que ele tinha sido agressivo com ela, mas ela não falou de agressão física. O depoente também não viu ninguém encostando em ninguém. Mais tarde o depoente viu o chefe entrando na sala, mas o depoente não sabe quem chamou ele. O depoente acredita que foi a Maria, mas o depoente pode estar enganado. O depoente foi embora antes de terminar e no outro dia lhe disseram que eles tiveram outra reunião. O depoente não sabe quem participou dessa reunião.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 2 — Transcrição ruim e autocorreções

Objetivo: Testar falsos começos, autocorreções, primeira pessoa, incerteza e fontes.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 89,150s de geração; 91,379s incluindo preparação.

Entrada: 1155 caracteres / 222 palavras. Saída: 1317 caracteres / 236 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-10-02 07:11:39.504 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: negação de percepção omitida ou transferida para o fato
- [2026-10-02 07:12:21.945 -03:00] [WARN] [BLOCK_ACCEPTED_WITH_WARNING] block=1/1; issue=uma negação pode ter sido omitida

Warnings nativos: 4. Log integral: [test-02-log.txt](test-02-log.txt).

### Entrada completa

```text
Então eu cheguei... acho que foi umas três, três e pouco, não sei certinho. Quando eu cheguei o Paulo tava lá, quer dizer, acho que era o Paulo, porque eu vi ele de costas primeiro. Aí depois eu vi que era ele mesmo. Tava falando com a Juliana. Eu não ouvi o começo da conversa. Eu ouvi ela falar “você não pode fazer isso”, mas eu não sei o que ela quis dizer com isso. Depois ele falou alguma coisa, eu não entendi, tinha barulho. Aí eu saí... não, eu acho que antes de sair entrou o Carlos. É, o Carlos entrou antes. Ele perguntou o que tava acontecendo e ninguém respondeu na hora. Depois eu fui pegar café e fiquei fora uns cinco minutos, talvez menos. Quando eu voltei a Juliana tava chorando. Eu não vi ninguém encostar nela. O Carlos me falou depois que o Paulo tinha gritado com ela, mas eu não ouvi esse grito. Outra pessoa, não lembro quem, disse que ele bateu na porta quando saiu. Eu também não vi isso. Acho que ele saiu primeiro e depois a Juliana, mas posso estar confundindo a ordem. Mais tarde falaram que a chefia chamou os dois, mas eu não sei se foi no mesmo dia. Eu não participei dessa conversa e ninguém me contou o que foi falado.
```

### Saída completa

```text
O depoente relatou que então chegou... O depoente acredita que foi umas três, três e pouco, o depoente não sabe certinho. Quando o depoente chegou o Paulo estava lá, quer dizer, acredita que era ele, porque o depoente viu ele de costas primeiro. Depois viu que era mesmo ele. Ele estava falando com a Juliana. O depoente não ouviu o começo da conversa. Ouviu ela dizer “você não pode fazer isso”, mas o depoente não sabe o que ela quis dizer com isso. Depois ele falou alguma coisa, o depoente não entendeu, tinha barulho. O depoente acredita que Carlos entrou antes da saída do depoente. Ele perguntou o que estava acontecendo e ninguém respondeu na hora. Depois o depoente foi pegar café e ficou fora uns cinco minutos, talvez menos. Quando voltou a Juliana já estava chorando. O depoente não viu ninguém encostar nela. O Carlos lhe disse depois que o Paulo tinha gritado com ela, mas o depoente não ouviu esse grito. Outra pessoa, o depoente não lembra quem, disse que ele bateu na porta quando saiu. O depoente também não viu isso. O depoente acredita que ele saiu primeiro e depois a Juliana, mas o depoente pode estar confundindo a ordem. Mais tarde falaram que a chefia chamou os dois, mas o depoente não sabe se foi no mesmo dia. O depoente não participou dessa conversa e ninguém lhe contou o que foi falado.
```

Observações automáticas:

- O validador de produção apontou possível perda de negação; não é conclusão semântica do QA.

## TESTE 3 — Versões conflitantes

Objetivo: Garantir que versões diferentes sejam preservadas sem escolher uma como verdadeira.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 45,222s de geração; 47,505s incluindo preparação.

Entrada: 1201 caracteres / 223 palavras. Saída: 1337 caracteres / 243 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-03-log.txt](test-03-log.txt).

### Entrada completa

```text
Eu cheguei na secretaria por volta das duas e meia e vi a Ana conversando com o Roberto. Eu não ouvi o começo da conversa. Depois a Ana me disse que o Roberto tinha batido a porta quando entrou na sala. Eu não vi isso acontecer. Mais tarde o Carlos falou comigo e disse que, na verdade, a porta já estava fechada e que o Roberto só empurrou para abrir. Eu não sei qual das duas versões está correta.

Depois eu ouvi o Roberto falar mais alto, mas não lembro exatamente o que ele disse. A Ana falou que ele tinha dito que não voltaria para aquela sala. O Carlos, por outro lado, disse que ouviu o Roberto falar que não voltaria para aquela reunião. Eu estava perto, mas tinha outras pessoas falando e não consegui entender a frase inteira.

Em seguida, a Ana saiu da sala. Acho que o Roberto saiu alguns minutos depois, mas posso estar confundindo a ordem. Uma servidora me disse depois que os dois saíram juntos. Eu não vi os dois saindo juntos.

Mais tarde, ouvi dizer que a chefia conversou com o Roberto. A Ana me falou que essa conversa aconteceu no mesmo dia. Já o Carlos disse que acredita que foi no dia seguinte. Eu não participei dessa conversa e não sei quando ela realmente aconteceu.
```

### Saída completa

```text
O depoente relatou que chegou na secretaria por volta das duas e meia e viu a Ana conversando com o Roberto. O depoente não ouviu o começo da conversa. Depois, a Ana lhe disse que o Roberto tinha batido a porta quando entrou na sala. O depoente não viu isso acontecer. Mais tarde, o Carlos falou com o depoente e disse que, na verdade, a porta já estava fechada e que o Roberto só empurrou para abrir. O depoente não sabe qual das duas versões está correta. Depois, o depoente ouviu o Roberto falar mais alto, mas o depoente não lembra exatamente o que ele disse. A Ana falou que ele tinha dito que não voltaria para aquela sala. O Carlos, por outro lado, disse que ouviu o Roberto falar que não voltaria para aquela reunião. O depoente estava perto, mas tinha outras pessoas falando e não conseguiu entender a frase inteira. Em seguida, a Ana saiu da sala. O depoente acredita que o Roberto saiu alguns minutos depois, mas o depoente pode estar confundindo a ordem. Uma servidora lhe disse depois que os dois saíram juntos. O depoente não viu os dois saindo juntos. Mais tarde, o depoente ouviu dizer que a chefia conversou com o Roberto. A Ana lhe falou que essa conversa aconteceu no mesmo dia. Já o Carlos disse que acredita que foi no dia seguinte. O depoente não participou dessa conversa e não sabe quando ela realmente aconteceu.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 4 — Pessoas e pronomes ambíguos

Objetivo: Garantir que o modelo não tente descobrir referentes que o original deixa ambíguos.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 56,858s de geração; 59,157s incluindo preparação.

Entrada: 1379 caracteres / 255 palavras. Saída: 1512 caracteres / 274 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 4. Log integral: [test-04-log.txt](test-04-log.txt).

### Entrada completa

```text
Eu cheguei na sala e estavam lá o Marcos, o Felipe e a Renata. Eu não sei quem chegou primeiro. Quando entrei, o Marcos estava falando com o Felipe e a Renata estava perto da porta. Eu ouvi alguém falar que aquilo já tinha passado do limite, mas não consegui identificar quem falou.

Depois o Felipe saiu da sala e o Marcos continuou falando com a Renata. Ela disse alguma coisa sobre a reunião anterior, mas eu não ouvi direito. Em seguida ele respondeu que não concordava. Eu acho que foi o Marcos que respondeu, porque o Felipe já tinha saído, mas posso estar enganado.

Pouco depois o Felipe voltou. A Renata perguntou para ele se tinha falado com o diretor. Ele respondeu que sim, mas não disse o que foi tratado. Nesse momento o Marcos falou que também queria conversar com ele. Eu não sei se o Marcos estava se referindo ao Felipe ou ao diretor.

Depois os três começaram a falar quase ao mesmo tempo. Eu ouvi a Renata dizer "ele já sabe disso", mas não sei a quem ela estava se referindo. O Felipe respondeu alguma coisa, só que eu não consegui entender.

Mais tarde, o Marcos saiu da sala. A Renata me disse que ele estava irritado. Eu não sei se ela estava falando do Marcos ou do Felipe, porque os dois tinham discutido durante a conversa. Eu não perguntei.

No final, o Felipe e a Renata ficaram na sala. Eu saí antes deles e não sei o que aconteceu depois.
```

### Saída completa

```text
O depoente relatou que chegou na sala e estavam lá o Marcos, o Felipe e a Renata. O depoente não sabe quem chegou primeiro. Quando entrou, o Marcos estava falando com o Felipe e a Renata estava perto da porta. O depoente ouviu alguém falar que aquilo já tinha passado do limite, mas o depoente não conseguiu identificar quem falou. Depois o Felipe saiu da sala e o Marcos continuou falando com a Renata. Ela disse alguma coisa sobre a reunião anterior, mas o depoente não ouviu direito. Em seguida ele respondeu que não concordava. O depoente acredita que foi o Marcos quem respondeu, porque o Felipe já tinha saído, mas o depoente pode estar enganado. Pouco depois o Felipe voltou. A Renata perguntou para ele se tinha falado com o diretor. Ele respondeu que sim, mas não disse o que foi tratado. Nesse momento o Marcos falou que também queria conversar com ele. O depoente não sabe se o Marcos estava se referindo ao Felipe ou ao diretor. Depois os três começaram a falar quase ao mesmo tempo. O depoente ouviu a Renata dizer "ele já sabe disso", mas o depoente não sabe a quem ela estava se referindo. O Felipe respondeu alguma coisa, só que o depoente não conseguiu entender. Mais tarde, o Marcos saiu da sala. A Renata lhe disse que ele estava irritado. O depoente não sabe se ela estava falando do Marcos ou do Felipe, porque os dois tinham discutido durante a conversa. O depoente não perguntou. No final, o Felipe e a Renata ficaram na sala. O depoente saiu antes deles e não sabe o que aconteceu depois.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 5 — Texto longo e contexto

Objetivo: Testar truncamento, contexto, repetição e degradação ao longo de texto maior.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 254,500s de geração; 256,692s incluindo preparação.

Entrada: 6042 caracteres / 1025 palavras. Saída: 6536 caracteres / 1104 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-10-02 07:16:12.523 -03:00] [WARN] [BLOCK_RETRY_START] block=3/6; issue=fidelidade: ação de participante atribuída ao depoente: o depoente respondeu

Warnings nativos: 9. Log integral: [test-05-log.txt](test-05-log.txt).

### Entrada completa

```text
Eu cheguei ao setor por volta das nove e meia da manhã. Não lembro o horário exato, mas acredito que ainda não eram dez horas. Quando entrei, estavam na sala a Patrícia, o Eduardo e o Marcelo. A Patrícia estava sentada na mesa dela e o Eduardo estava em pé perto da porta. O Marcelo estava mexendo no computador. Eu cumprimentei os três e fui para a minha mesa. Naquele momento não percebi nenhuma discussão e não ouvi ninguém falando alto.

Uns dez ou quinze minutos depois, ouvi a Patrícia perguntar ao Eduardo sobre um documento que, segundo ela, deveria ter sido enviado no dia anterior. Eu não sei qual era o documento. O Eduardo respondeu que tinha enviado, mas a Patrícia disse que não tinha recebido. Eu estava trabalhando e não prestei atenção em toda a conversa, então não sei exatamente quais palavras eles usaram.

Depois o Marcelo entrou na conversa e disse que acreditava que o documento tinha sido encaminhado para outro endereço de e-mail. Não sei se ele verificou isso no computador ou se estava apenas lembrando de alguma coisa. A Patrícia perguntou qual endereço tinha sido usado. O Marcelo falou alguma coisa que eu não consegui ouvir direito.

Pouco depois, o Eduardo saiu da sala. Eu não sei se ele saiu para verificar o e-mail ou por outro motivo. A Patrícia continuou conversando com o Marcelo. Ouvi ela dizer que aquilo já tinha acontecido antes, mas não sei a que situação exatamente ela estava se referindo. O Marcelo respondeu que precisava conferir antes de afirmar qualquer coisa.

O Eduardo voltou alguns minutos depois com o celular na mão. Ele mostrou alguma coisa para a Patrícia. Eu não consegui ver a tela. Ele disse que tinha o comprovante do envio. A Patrícia respondeu que o fato de ter enviado não significava que ela tivesse recebido. O tom dos dois ficou um pouco mais alto, mas eu não diria que estavam gritando.

Nesse momento entrou a Fernanda. Ela perguntou o que estava acontecendo. A Patrícia disse que estava tentando localizar um documento. O Eduardo falou alguma coisa logo depois, mas os dois falaram quase ao mesmo tempo e eu não consegui entender tudo. A Fernanda pediu que eles falassem um de cada vez.

Depois ouvi o Eduardo dizer que não aceitava que colocassem a responsabilidade nele. Não sei quem teria colocado a responsabilidade nele, porque não ouvi ninguém dizer isso diretamente. A Patrícia respondeu que não estava culpando ninguém e que só queria saber onde estava o documento.

O Marcelo então disse que poderia verificar o sistema. Ele ficou alguns minutos no computador. Enquanto isso, a Patrícia saiu da sala para atender uma ligação. Não sei quem ligou para ela. O Eduardo ficou conversando com a Fernanda. Eu ouvi a Fernanda dizer para ele ficar tranquilo, mas não ouvi o restante da conversa.

Quando a Patrícia voltou, o Marcelo disse que tinha encontrado um registro. Segundo ele, o documento havia sido enviado, mas eu não entendi se ele disse que tinha sido enviado para o endereço correto ou para o endereço errado. A Patrícia perguntou se era possível reenviar. O Eduardo respondeu que poderia reenviar naquele momento.

Aparentemente o assunto do documento terminou aí. Pelo menos eu não ouvi mais discussão sobre isso naquela hora. Voltei a prestar atenção no meu trabalho.

Mais tarde, perto do horário do almoço, a Fernanda veio falar comigo e disse que a Patrícia tinha ficado chateada com a forma como o Eduardo respondeu. Eu não presenciei nenhuma conversa particular entre a Patrícia e a Fernanda e não sei se a Patrícia realmente disse isso para ela ou se foi uma impressão da própria Fernanda.

Depois do almoço, acho que por volta de uma e meia, voltei para a sala. O Marcelo já estava lá. A Patrícia chegou alguns minutos depois e o Eduardo chegou mais tarde. Não sei dizer quanto tempo depois.

Quando o Eduardo entrou, a Patrícia perguntou se o documento tinha sido reenviado. Ele respondeu que sim. Ela agradeceu. Nesse momento a conversa foi tranquila.

Uns vinte minutos depois, chegou o coordenador. Ele chamou a Patrícia e o Eduardo para conversar na sala dele. Eu não sei quem pediu essa conversa. Acho que a Fernanda pode ter falado com ele, mas isso é apenas uma suposição minha. Eu não vi a Fernanda falando com o coordenador sobre o assunto.

A Patrícia e o Eduardo ficaram na sala do coordenador por aproximadamente meia hora. Não participei dessa reunião e não ouvi o que foi dito. O Marcelo comentou comigo que acreditava que a reunião era sobre o problema do documento, mas ele também disse que não tinha certeza.

Quando os dois voltaram, a Patrícia sentou na mesa dela e ficou trabalhando. O Eduardo saiu do setor por alguns minutos. Eu não percebi nenhuma nova discussão naquele momento.

Mais tarde, o Eduardo comentou comigo que estava cansado daquele tipo de situação. Eu não perguntei exatamente a que situação ele estava se referindo. Ele também disse que tinha explicado tudo ao coordenador. Não contou o que tinha sido explicado.

No final da tarde, a Patrícia conversou comigo rapidamente e disse que esperava que o problema estivesse resolvido. Ela não falou que o Eduardo tinha feito alguma coisa de propósito e também não falou em punição ou denúncia. Pelo que ouvi diretamente dela, apenas disse que esperava que aquilo não acontecesse novamente.

No dia seguinte, o Marcelo me disse que o documento finalmente tinha chegado ao destino correto. Eu não verifiquei isso pessoalmente. Também ouvi de outra pessoa, não lembro quem, que teria ocorrido uma nova conversa entre o coordenador e o Eduardo. Não sei se essa conversa realmente aconteceu.

Alguns dias depois, encontrei a Fernanda no corredor. Ela disse que o ambiente tinha ficado melhor depois daquele episódio. Eu não perguntei o que ela queria dizer exatamente com isso.

Não sei se houve alguma medida administrativa em razão do que aconteceu. Não participei de nenhuma outra reunião sobre o assunto e ninguém me apresentou documento ou decisão relacionada ao caso. Tudo o que sei é o que presenciei na sala e o que algumas pessoas me disseram posteriormente.
```

### Saída completa

```text
O depoente relatou que chegou ao setor por volta das nove e meia da manhã. O depoente não lembra o horário exato, mas o depoente acredita que ainda não eram dez horas. Quando entrou, estavam na sala a Patrícia, o Eduardo e o Marcelo. A Patrícia estava sentada na mesa dela e o Eduardo estava em pé perto da porta. O Marcelo estava mexendo no computador. O depoente cumprimentou os três e foi para sua mesa. Naquele momento não percebeu nenhuma discussão e não ouviu ninguém falando alto. Uns dez ou quinze minutos depois, o depoente ouviu a Patrícia perguntar ao Eduardo sobre um documento que, segundo ela, deveria ter sido enviado no dia anterior. O depoente não sabe qual era o documento. O Eduardo respondeu que tinha enviado, mas a Patrícia disse que não tinha recebido. O depoente estava trabalhando e não prestou atenção em toda a conversa, então o depoente não sabe exatamente quais palavras eles usaram. Depois, o Marcelo entrou na conversa e disse que acreditava que o documento tinha sido encaminhado para outro endereço de e-mail. O depoente não sabe se ele verificou isso no computador ou se estava apenas lembrando de alguma coisa. A Patrícia perguntou qual endereço tinha sido usado. O Marcelo falou alguma coisa que o depoente não conseguiu ouvir direito. Pouco depois, o Eduardo saiu da sala. O depoente não sabe se ele saiu para verificar o e-mail ou por outro motivo. A Patrícia continuou conversando com o Marcelo. O depoente ouviu ela dizer que aquilo já tinha acontecido antes, mas o depoente não sabe a que situação exatamente ela estava se referindo. O Marcelo respondeu que precisava conferir antes de afirmar qualquer coisa. O Eduardo voltou alguns minutos depois com o celular na mão. Ele mostrou alguma coisa para a Patrícia. O depoente não conseguiu ver a tela. Ele disse que tinha o comprovante do envio. A Patrícia respondeu que o fato de ter enviado não significava que ela tivesse recebido. O tom dos dois ficou um pouco mais alto, mas o depoente não diria que estavam gritando. Nesse momento entrou a Fernanda. Ela perguntou o que estava acontecendo. A Patrícia disse que estava tentando localizar um documento. O Eduardo falou alguma coisa logo depois, mas os dois falaram quase ao mesmo tempo e o depoente não conseguiu entender tudo. A Fernanda pediu que eles falassem um de cada vez. Depois o depoente ouviu o Eduardo dizer que não aceitava que colocassem a responsabilidade nele. O depoente não sabe quem teria colocado a responsabilidade nele, porque o depoente não ouviu ninguém dizer isso diretamente. A Patrícia respondeu que não estava culpando ninguém e que só queria saber onde estava o documento. O Marcelo então disse que poderia verificar o sistema. Ele ficou alguns minutos no computador. Enquanto isso, a Patrícia saiu da sala para atender uma ligação. O depoente não sabe quem ligou para ela. O Eduardo ficou conversando com a Fernanda. O depoente ouviu a Fernanda dizer para ele ficar tranquilo, mas o depoente não ouviu o restante da conversa. Quando a Patrícia voltou, o Marcelo disse que tinha encontrado um registro. Segundo ele, o documento havia sido enviado, mas o depoente não entendeu se ele disse que tinha sido enviado para o endereço correto ou para o endereço errado. A Patrícia perguntou se era possível reenviar. O Eduardo respondeu que poderia reenviar naquele momento. Aparentemente o assunto do documento terminou aí. Pelo menos o depoente não ouviu mais discussão sobre isso naquela hora. O depoente voltou a prestar atenção no seu trabalho. Mais tarde, perto do horário do almoço, a Fernanda veio falar com o depoente e disse que a Patrícia tinha ficado chateada com a forma como o Eduardo respondeu. O depoente não presenciou nenhuma conversa particular entre a Patrícia e a Fernanda e não sabe se a Patrícia realmente disse isso para ela ou se foi uma impressão da própria Fernanda. Depois do almoço, o depoente acredita que por volta de uma e meia, retornou à sala. O Marcelo já estava lá. A Patrícia chegou alguns minutos depois e o Eduardo entrou mais tarde. O depoente não sabe dizer quanto tempo depois. Quando o Eduardo entrou, a Patrícia perguntou se o documento tinha sido reenviado. Ele respondeu que sim. Ela agradeceu. Nesse momento a conversa foi tranquila. Uns vinte minutos depois, chegou o coordenador. Ele chamou a Patrícia e o Eduardo para conversar na sala dele. O depoente não sabe quem pediu essa conversa. O depoente acredita que a Fernanda pode ter falado com ele, mas isso é apenas uma suposição sua. O depoente não viu a Fernanda falando com o coordenador sobre o assunto. A Patrícia e o Eduardo ficaram na sala do coordenador por aproximadamente meia hora. O depoente não participou dessa reunião e não ouviu o que foi dito. O Marcelo comentou com o depoente que acreditava que a reunião era sobre o problema do documento, mas ele também disse que não tinha certeza. Quando os dois voltaram, a Patrícia sentou na mesa dela e ficou trabalhando. O Eduardo saiu do setor por alguns minutos. O depoente não percebeu nenhuma nova discussão naquele momento. Mais tarde, o Eduardo comentou com o depoente que estava cansado daquele tipo de situação. O depoente não perguntou exatamente a que situação ele estava se referindo. Ele também disse que tinha explicado tudo ao coordenador. Não contou o que tinha sido explicado. No final da tarde, a Patrícia conversou com o depoente rapidamente e disse que esperava que o problema estivesse resolvido. Ela não falou que o Eduardo tinha feito alguma coisa de propósito e também não falou em punição ou denúncia. Pelo que o depoente ouviu diretamente dela, apenas disse que esperava que aquilo não acontecesse novamente. No dia seguinte, o Marcelo lhe disse que o documento finalmente tinha chegado ao destino correto. O depoente não verificou isso pessoalmente. O depoente também ouviu de outra pessoa, o depoente não lembra quem, que teria ocorrido uma nova conversa entre o coordenador e o Eduardo. O depoente não sabe se essa conversa realmente aconteceu. Alguns dias depois, o depoente encontrou a Fernanda no corredor. Ela disse que o ambiente tinha ficado melhor depois daquele episódio. O depoente não perguntou o que ela queria dizer exatamente com isso. O depoente não sabe se houve alguma medida administrativa em razão do que aconteceu. O depoente não participou de nenhuma outra reunião sobre o assunto e ninguém lhe apresentou documento ou decisão relacionada ao caso. Tudo o que o depoente sabe é o que o depoente presenciou na sala e o que algumas pessoas lhe disseram posteriormente.
```

Observações automáticas:

- Os três marcadores lexicais do encerramento do Teste 5 foram encontrados; isso não garante equivalência semântica.

## TESTE 6 — Estados de conhecimento e incerteza

Objetivo: Testar diferenças entre saber, acreditar, lembrar, supor e ouvir de terceiros.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 25,398s de geração; 27,646s incluindo preparação.

Entrada: 595 caracteres / 106 palavras. Saída: 715 caracteres / 124 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-06-log.txt](test-06-log.txt).

### Entrada completa

```text
Eu sei que a reunião aconteceu na terça-feira porque estava presente. Não lembro o horário exato. Acho que começou depois das duas, mas posso estar enganado. Eu não sei quem marcou a reunião. A Patrícia me disse que foi o diretor, mas eu não confirmei isso com ele. Tenho certeza de que Carlos entrou depois de mim porque eu já estava sentado quando ele chegou. Não tenho certeza se Ana já estava na sala. Acredito que sim, mas não lembro de ter visto ela entrar. Depois me disseram que houve outra reunião na quinta-feira. Eu não participei dessa segunda reunião e não sei quem estava presente.
```

### Saída completa

```text
O depoente relatou que sabe que a reunião aconteceu na terça-feira porque estava presente. O depoente não lembra o horário exato. O depoente acredita que começou depois das duas, mas o depoente pode estar enganado. O depoente não sabe quem marcou a reunião. A Patrícia lhe disse que foi o diretor, mas o depoente não confirmou isso com ele. O depoente tem certeza de que Carlos entrou depois dele porque já estava sentado quando ele chegou. O depoente não tem certeza se Ana já estava na sala. O depoente acredita que sim, mas o depoente não lembra de ter visto ela entrar. Depois lhe disseram que houve outra reunião na quinta-feira. O depoente não participou dessa segunda reunião e não sabe quem estava presente.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 7 — Falas diretas entre aspas

Objetivo: Testar preservação de falas diretas sem converter indevidamente o conteúdo da citação.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 21,071s de geração; 23,293s incluindo preparação.

Entrada: 442 caracteres / 76 palavras. Saída: 514 caracteres / 86 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-07-log.txt](test-07-log.txt).

### Entrada completa

```text
Eu estava perto quando a Daniela falou para o Ricardo: “eu não vou assinar isso agora”. Eu ouvi essa frase claramente. Depois o Ricardo respondeu alguma coisa, mas eu não consegui entender. Mais tarde a Daniela me disse que ele tinha pedido para ela rever o documento. Eu não ouvi esse pedido pessoalmente. Também ouvi o Ricardo dizer “isso precisa ser resolvido hoje”, mas não sei exatamente a quem ele estava se dirigindo quando falou isso.
```

### Saída completa

```text
O depoente relatou que estava perto quando a Daniela falou para o Ricardo: “eu não vou assinar isso agora”. O depoente ouviu essa frase claramente. Depois o Ricardo respondeu alguma coisa, mas o depoente não conseguiu entender. Mais tarde a Daniela lhe disse que ele tinha pedido para ela rever o documento. O depoente não ouviu esse pedido pessoalmente. Também o depoente ouviu o Ricardo dizer “isso precisa ser resolvido hoje”, mas o depoente não sabe exatamente a quem ele estava se dirigindo quando falou isso.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 8 — Nomes e cargos

Objetivo: Evitar troca indevida entre nome, cargo e pessoa.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 21,068s de geração; 23,285s incluindo preparação.

Entrada: 479 caracteres / 87 palavras. Saída: 543 caracteres / 96 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-08-log.txt](test-08-log.txt).

### Entrada completa

```text
Eu vi a professora Helena entrando na sala da direção. Depois o diretor Paulo chegou e conversou com ela. Não ouvi o que eles falaram. Mais tarde uma servidora me disse que a professora tinha procurado o diretor para falar sobre uma turma. Eu não sei se isso é verdade. Depois Paulo saiu da sala e falou com o coordenador Marcos. Não sei se eles conversaram sobre o mesmo assunto. A professora Helena saiu alguns minutos depois. Eu não vi o coordenador entrar na sala da direção.
```

### Saída completa

```text
O depoente relatou que viu a professora Helena entrando na sala da direção. Depois o diretor Paulo chegou e conversou com ela. O depoente não ouviu o que eles falaram. Mais tarde uma servidora lhe disse que a professora tinha procurado o diretor para falar sobre uma turma. O depoente não sabe se isso é verdade. Depois Paulo saiu da sala e falou com o coordenador Marcos. O depoente não sabe se eles conversaram sobre o mesmo assunto. A professora Helena saiu alguns minutos depois. O depoente não viu o coordenador entrar na sala da direção.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 9 — Ambiguidade temporal

Objetivo: Preservar datas, sequência e dúvidas temporais sem inventar cronologia.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 44,857s de geração; 47,156s incluindo preparação.

Entrada: 517 caracteres / 92 palavras. Saída: 607 caracteres / 106 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-10-02 07:20:04.555 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: no dia seguinte, ou talvez dois dias depois, carla me contou que tinha conversado novamente com bruno.

Warnings nativos: 4. Log integral: [test-09-log.txt](test-09-log.txt).

### Entrada completa

```text
Acho que isso aconteceu na quarta-feira, mas pode ter sido na quinta. Sei que foi depois da reunião do conselho, só não lembro se foi no mesmo dia ou no dia seguinte. Eu cheguei antes do almoço. A conversa entre Bruno e Carla aconteceu depois que eu cheguei, mas não sei quanto tempo depois. Mais tarde saí do setor e, quando voltei, eles já não estavam lá. No dia seguinte, ou talvez dois dias depois, Carla me contou que tinha conversado novamente com Bruno. Não sei exatamente quando essa segunda conversa ocorreu.
```

### Saída completa

```text
O depoente relatou que acredita que isso aconteceu na quarta-feira, mas pode ter sido na quinta. O depoente sabe que foi depois da reunião do conselho, o depoente só não lembra se foi no mesmo dia ou no dia seguinte. O depoente chegou antes do almoço. A conversa entre Bruno e Carla aconteceu depois que o depoente chegou, mas o depoente não sabe quanto tempo depois. Mais tarde saiu do setor e, quando voltou, eles já não estavam lá. No dia seguinte, ou talvez dois dias depois, Carla lhe contou que tinha conversado novamente com Bruno. O depoente não sabe exatamente quando essa segunda conversa ocorreu.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 10 — Negação

Objetivo: Testar preservação rigorosa de negações.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 26,595s de geração; 28,879s incluindo preparação.

Entrada: 609 caracteres / 112 palavras. Saída: 705 caracteres / 126 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-10-log.txt](test-10-log.txt).

### Entrada completa

```text
Eu não vi Pedro ameaçar ninguém. Também não ouvi ele dizer que faria alguma coisa contra Maria. Eu vi os dois discutindo, mas não vi contato físico. Maria depois me disse que ficou com medo, mas não falou que Pedro tinha batido nela. Carlos disse que ouviu um barulho, mas não disse que viu Pedro bater na mesa. Eu não sei de onde veio o barulho. Também não sei se o diretor estava na sala naquele momento. Não lembro de ter visto ele entrar. Depois me falaram que houve uma reunião, mas ninguém me disse que Pedro recebeu punição. Eu não participei dessa reunião e não tenho conhecimento do que foi decidido.
```

### Saída completa

```text
O depoente relatou que não viu Pedro ameaçar ninguém. O depoente também não ouviu ele dizer que faria alguma coisa contra Maria. O depoente viu os dois discutindo, mas o depoente não viu contato físico. Maria depois lhe disse que ficou com medo, mas não falou que Pedro tinha batido nela. Carlos disse que ouviu um barulho, mas não disse que viu Pedro bater na mesa. O depoente não sabe de onde veio o barulho. O depoente também não sabe se o diretor estava na sala naquele momento. O depoente não lembra de ter visto ele entrar. Depois lhe disseram que houve uma reunião, mas ninguém lhe disse que Pedro recebeu punição. O depoente não participou dessa reunião e não tem conhecimento do que foi decidido.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## Resumo

- Total de testes previstos: 10
- Testes executados: 10
- Concluídos (GENERATION_OK): 10
- Com warning do validador: 3
- Com primeira pessoa fora de citações (indício): 0
- Com retry: 3
- Com erro: 0
- Com indício de truncamento: 0
- Com duplicação integral: 0
- Com parágrafos repetidos: 0
- Com ContextOverflowException: 0
- Tempo total (s): 648.982
