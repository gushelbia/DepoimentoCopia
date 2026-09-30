# QA / regressão — DepoimentoLocal

Atualizado: 2026-09-29T20:51:02.5883410-03:00. Bateria finalizada: True.

Execução real, sequencial, via OriginalAppBridge da interface de produção e botão Reformular do motor. Mesmos binários, prompt, reparos locais, validador e retry do aplicativo; nenhum resultado foi corrigido pelo QA. Cada caso usa uma instância própria. A compilação dos fontes disponíveis (interface e runner) está em build.txt; o fonte/projeto do motor não acompanha a distribuição, portanto ele é reutilizado sem recompilação.

Truncamento, primeira pessoa e ausência do final são indícios conservadores, não avaliações semânticas. Ausência de alerta não significa fidelidade garantida. Duplicação exige repetição integral exata (espaços normalizados); parágrafos repetidos têm pelo menos 60 caracteres. Citações pareadas são excluídas do teste de primeira pessoa. Nuances verbais, fontes e ambiguidades ficam para revisão humana. O motor não expõe motivo de parada por token/EOS neste log; não se infere conclusão normal apenas pela pontuação.

Os logs nativos são separados dos warnings do validador. O log original é compartilhado pelo aplicativo e não inclui PID; não gere textos em outra instância durante a bateria. O runner rejeita atividade concorrente detectável. Tempos de geração excluem carregamento; tempo total inclui carregamento e encerramento das instâncias.

Aplicativo não recompilado nem alterado nesta bateria; somente o executor QA com a interface existente foi compilado. Integridade das DLLs antes/depois: engine-integrity.json. Captura integral conferida pelo número de caracteres publicado em GENERATION_OK. Os trechos citados abaixo não foram abreviados.

## TESTE 1 — Relato simples e direto

Objetivo: Preservar ações observadas, participantes e sequência.

Status técnico: **ERRO**. Geração terminou normalmente: False.

Tempo: 40,047s de geração; 42,893s incluindo preparação.

Entrada: 317 caracteres / 56 palavras. Saída: 728 caracteres / 119 palavras.

Retry: True. Erro: True. Exceção: True. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: Não verificável por GENERATION_OK; saída disponível na tela preservada integralmente.

Warnings do validador:

- [2026-09-29 20:44:49.469 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: a zeladora berenice estava comigo.
- [2026-09-29 20:45:08.882 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: percepção acrescentada ao relato

Warnings nativos: 4. Log integral: [test-01-log.txt](test-01-log.txt).

Exceção registrada:

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=317; elapsedMs=40047
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: percepção acrescentada ao relato. A reformulação foi interrompida: os blocos seguintes não foram processados e o trecho na tela foi marcado como incompleto.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

### Entrada completa

```text
Eu abri o almoxarifado às oito horas. A zeladora Berenice estava comigo. Vi Otávio colocar duas caixas lacradas sobre o balcão. Conferi os números das etiquetas e assinei o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. Fechei a porta às oito e vinte. Não abri nenhuma das caixas.
```

### Saída completa

```text
[SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO]
Motivo: O bloco 1 ainda precisa de revisão: fidelidade: percepção acrescentada ao relato. A reformulação foi interrompida: os blocos seguintes não foram processados e o trecho na tela foi marcado como incompleto.

Relatou que o depoente abriu o almoxarifado às oito horas. Viu Berenice com o depoente. Otávio colocou duas caixas lacradas sobre o balcão. O depoente viu conferir os números das etiquetas e assinar o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. O depoente fechou a porta às oito e vinte. O depoente não abriu nenhuma das caixas.

[FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]
```

Observações automáticas:

- Possível truncamento: saída sem pontuação de encerramento.

## TESTE 5 — Percepção e relato em cadeia

Objetivo: Preservar cada camada de fonte e distinguir leitura, audição e observação.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 21,804s de geração; 24,421s incluindo preparação.

Entrada: 426 caracteres / 80 palavras. Saída: 523 caracteres / 95 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-05-log.txt](test-05-log.txt).

### Entrada completa

```text
Vi um copo quebrado ao lado da impressora. Não vi quem o derrubou. A técnica Sílvia me contou que o motorista Raul lhe disse que alguém esbarrou na mesa. Eu não ouvi Raul falar isso. Mais tarde li uma mensagem de Noemi dizendo que ela tinha ouvido a queda, mas não tinha visto o copo cair. Sílvia acrescentou que achava que o copo já estava trincado. Eu não examinei os cacos. Sei apenas que havia vidro no chão quando entrei.
```

### Saída completa

```text
Relatou que o depoente viu um copo quebrado ao lado da impressora. O depoente não viu quem o derrubou. A técnica Sílvia contou ao depoente que o motorista Raul lhe disse que alguém esbarrou na mesa. O depoente não ouviu Raul falar isso. Mais tarde o depoente leu uma mensagem de Noemi dizendo que ela tinha ouvido a queda, mas não tinha visto o copo cair. Sílvia acrescentou que achava que o copo já estava trincado. O depoente não examinou os cacos. O depoente sabe apenas que havia vidro no chão quando o depoente entrou.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 12 — Relato longo e mudanças de assunto

Objetivo: Verificar cobertura dos blocos, identidade do depoente, fontes, temporalidade e final.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 297,119s de geração; 299,821s incluindo preparação.

Entrada: 5184 caracteres / 909 palavras. Saída: 5911 caracteres / 1031 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-09-29 20:47:10.355 -03:00] [WARN] [BLOCK_RETRY_START] block=2/5; issue=fidelidade: ação de participante atribuída ao depoente: o depoente pediu
- [2026-09-29 20:48:38.883 -03:00] [WARN] [BLOCK_RETRY_START] block=3/5; issue=fidelidade: percepção acrescentada ao relato

Warnings nativos: 9. Log integral: [test-12-log.txt](test-12-log.txt).

### Entrada completa

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

### Saída completa

```text
Relatou que o depoente trabalha na recepção de um centro cultural. Na manhã de 18 de maio, o depoente chegou por volta das sete e quarenta. O relógio da parede estava parado, por isso o depoente consultou seu celular. A produtora Solange estava na entrada, o técnico Bento estava no palco e a auxiliar Neide arrumava as cadeiras. O depoente cumprimentou os três e deixou sua mochila sob o balcão. O depoente não percebeu discussão quando o depoente entrou. Solange lhe perguntou se a lista dos convidados já tinha chegado. O depoente respondeu que ainda não tinha recebido nenhuma lista. Ela disse que o coordenador Gaspar tinha enviado uma versão na noite anterior. O depoente não viu esse envio e não sabe para qual endereço teria sido encaminhado. O depoente consultou somente a caixa de entrada da recepção, não a pasta de mensagens indesejadas. Neide ouviu a conversa de que o depoente participava, mas não respondeu. Alguns minutos depois, Bento veio até o balcão com um cabo na mão. Disse que uma das caixas de som estava falhando. O depoente ouviu um chiado quando ele fez um teste, mas o depoente não sabe avaliar se o problema era no cabo ou na caixa. Solange pediu que ele aguardasse a chegada do eletricista. O depoente não ouviu ninguém dizer que o equipamento estava queimado. Enquanto isso, entrou a professora Vilma com duas sacolas. Ela pediu que o depoente guardasse os materiais da oficina. O depoente colocou as sacolas no armário atrás da recepção, sem abrir nenhuma delas. Vilma disse que voltaria antes das dez. O depoente não sabe o que havia dentro. Mais tarde Neide comentou que uma das sacolas parecia conter tintas, mas ela também não abriu a sacola na presença do depoente. Perto das oito e meia, o depoente atendeu uma ligação. A pessoa se apresentou como representante da empresa de transporte, mas o depoente não lembra o nome. Informou que o veículo do grupo estava atrasado. O depoente anotou a informação e a passou para Solange. Não lhe disseram a causa do atraso nem quanto tempo levaria. O depoente não afirmou que o grupo tinha cancelado a visita. O depoente saiu para beber água. Quando voltou, Gaspar estava ao lado de Solange. O depoente não viu a chegada dele. Os dois olharam uma folha, mas o depoente estava longe e não conseguiu ler. Gaspar lhe pediu que imprimisse uma lista nova. O depoente recebeu um arquivo no computador e o depoente imprimiu três páginas. O depoente não comparou essa lista com qualquer versão anterior. O depoente entregou as folhas a Solange, não a Gaspar. Depois o depoente ouviu Neide perguntar a Bento sobre uma extensão elétrica. Ele disse que tinha deixado a extensão perto da porta de serviço. O depoente não viu onde ele a colocou. Neide saiu em direção à porta e voltou sem nada nas mãos. O depoente não sabe se não encontrou a extensão ou se foi fazer outra coisa. O depoente não perguntou. O eletricista chegou, o depoente acredita que pouco depois das nove. O depoente não sabe seu nome. O depoente viu ele conversar com Bento e desligar uma tomada. O depoente não acompanhou o restante do serviço porque o depoente começou a atender os visitantes. Mais tarde Bento lhe disse que o som estava funcionando. O depoente ouviu música no salão, mas o depoente não sabe se saía da mesma caixa que tinha apresentado chiado. Por volta de nove e meia, Vilma voltou ao balcão. O depoente entregou a ela as duas sacolas. Ela conferiu algo dentro de uma delas e disse que estava faltando um pincel. O depoente não sabe se esse pincel estava na sacola quando ela chegou. Vilma não disse que alguém tinha retirado o objeto. O depoente não viu ninguém mexer nas sacolas enquanto estavam no armário, mas houve um período em que o depoente ficou longe do balcão. Antes do almoço, Solange lhe contou que a empresa havia enviado outro veículo. O depoente não falou novamente com a empresa. Um grupo chegou pouco depois, acompanhado de uma monitora. O depoente não tem certeza de que era o mesmo grupo da ligação. A monitora entregou um papel a Gaspar. O depoente viu a entrega, mas o depoente não leu o papel. O depoente saiu para almoçar às doze e dez e voltou por volta de uma hora. Na volta, Neide disse que tinha havido uma conversa entre Vilma e Gaspar sobre materiais. O depoente não participou dessa conversa e não sabe se trataram do pincel. Neide disse que ouviu apenas o final. Não lhe contou as palavras usadas por nenhum dos dois. Durante a tarde, o depoente ajudou a separar crachás para a atividade seguinte. Bento perguntou se o depoente tinha visto uma chave pequena. O depoente respondeu que não sabia de qual chave ele estava falando. Ele mostrou uma fotografia, mas o depoente não reconheceu a chave. Não disse que ela nunca tinha passado pela recepção. Solange comentou que talvez estivesse com o pessoal da limpeza; isso foi apresentado como possibilidade. No fim do expediente, Gaspar agradeceu à equipe e disse que faria uma avaliação do evento em outro dia. Não marcou uma data na presença do depoente. O depoente foi embora por volta das cinco e vinte. Quando saiu, Solange e Bento ainda estavam no salão. O depoente não sabe quem fechou o prédio naquela noite. Na manhã seguinte, o depoente recebeu uma mensagem de Vilma dizendo que tinha encontrado o pincel em casa. Foi ela quem informou isso; o depoente não viu o objeto. O depoente não recebeu notícia de que alguém tivesse sido responsabilizado pelo desaparecimento. Na semana seguinte, o depoente ouviu de um funcionário cujo nome não lembra que a avaliação havia sido adiada. O depoente não sabe se essa informação estava correta. O depoente não participou de reunião de avaliação e não teve acesso a atas. O depoente não pode afirmar que o problema do som, o atraso do transporte e a falta do pincel tivessem relação entre si. O relato do depoente reúne o que o depoente observou, as informações que lhe passaram e os pontos que o depoente não conseguiu confirmar.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 15 — Ambiguidade real a preservar

Objetivo: Não resolver possuidor, destinatário, pessoa ou local por inferência.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 25,918s de geração; 28,515s incluindo preparação.

Entrada: 477 caracteres / 93 palavras. Saída: 584 caracteres / 110 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-15-log.txt](test-15-log.txt).

### Entrada completa

```text
Eu estava na sala quando Inês disse a Vera que sua irmã esperava no carro. Não sei de quem era a irmã nem qual carro era. Depois ela pediu que eu levasse a bolsa para lá. Não consegui identificar se quem fez o pedido foi Inês ou Vera, porque eu estava de costas. Não levei a bolsa. Ouvi uma delas dizer que a outra já sabia do problema. Não sei qual problema. Mais tarde Vera me disse que ela tinha resolvido aquilo. Não perguntei quem era “ela” nem o que tinha sido resolvido.
```

### Saída completa

```text
Relatou que o depoente estava na sala quando Inês disse a Vera que sua irmã esperava no carro. O depoente não sabe de quem era a irmã nem qual carro era. Depois ela pediu que o depoente levasse a bolsa para lá. O depoente não conseguiu identificar se quem fez o pedido foi Inês ou Vera, porque o depoente estava de costas. O depoente não levou a bolsa. O depoente ouviu uma delas dizer que a outra já sabia do problema. O depoente não sabe qual problema. Mais tarde Vera lhe disse que ela tinha resolvido aquilo. O depoente não perguntou quem era “ela” nem o que tinha sido resolvido.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## Resumo

- Total de testes previstos: 15
- Testes executados: 4
- Concluídos (GENERATION_OK): 3
- Com warning do validador: 2
- Com primeira pessoa fora de citações (indício): 0
- Com retry: 2
- Com erro: 1
- Com indício de truncamento: 1
- Com duplicação integral: 0
- Com parágrafos repetidos: 0
- Com ContextOverflowException: 0
- Tempo total (s): 396.045
