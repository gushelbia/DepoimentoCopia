# QA / regressão — DepoimentoLocal

Atualizado: 2026-09-29T18:58:10.1233594-03:00. Bateria finalizada: True.

Execução real, sequencial, via OriginalAppBridge da interface de produção e botão Reformular do motor. Mesmos binários, prompt, reparos locais, validador e retry do aplicativo; nenhum resultado foi corrigido pelo QA. Cada caso usa uma instância própria. A compilação dos fontes disponíveis (interface e runner) está em build.txt; o fonte/projeto do motor não acompanha a distribuição, portanto ele é reutilizado sem recompilação.

Truncamento, primeira pessoa e ausência do final são indícios conservadores, não avaliações semânticas. Ausência de alerta não significa fidelidade garantida. Duplicação exige repetição integral exata (espaços normalizados); parágrafos repetidos têm pelo menos 60 caracteres. Citações pareadas são excluídas do teste de primeira pessoa. Nuances verbais, fontes e ambiguidades ficam para revisão humana. O motor não expõe motivo de parada por token/EOS neste log; não se infere conclusão normal apenas pela pontuação.

Os logs nativos são separados dos warnings do validador. O log original é compartilhado pelo aplicativo e não inclui PID; não gere textos em outra instância durante a bateria. O runner rejeita atividade concorrente detectável. Tempos de geração excluem carregamento; tempo total inclui carregamento e encerramento das instâncias.

Aplicativo não recompilado nem alterado nesta bateria; somente o executor QA com a interface existente foi compilado. Integridade das DLLs antes/depois: engine-integrity.json. Captura integral conferida pelo número de caracteres publicado em GENERATION_OK. Os trechos citados abaixo não foram abreviados.

## TESTE 1 — Relato simples e direto

Objetivo: Preservar ações observadas, participantes e sequência.

Status técnico: **ERRO**. Geração terminou normalmente: False.

Tempo: 40,262s de geração; 43,337s incluindo preparação.

Entrada: 317 caracteres / 56 palavras. Saída: 368 caracteres / 65 palavras.

Retry: True. Erro: True. Exceção: True. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: Não verificável por GENERATION_OK; saída disponível na tela preservada integralmente.

Warnings do validador:

- [2026-09-29 18:46:50.938 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: a zeladora berenice estava comigo.
- [2026-09-29 18:47:10.721 -03:00] [WARN] [BLOCK_REJECTED] block=1/1; detail=fidelidade: percepção acrescentada ao relato

Warnings nativos: 4. Log integral: [test-01-log.txt](test-01-log.txt).

Exceção registrada:

```text
Geração falhou ou foi cancelada; exceção e stack trace, se presentes, constam no log integral.
[ERROR] [GENERATION_FAILED] sourceChars=317; elapsedMs=40262
System.InvalidOperationException: O bloco 1 ainda precisa de revisão: fidelidade: percepção acrescentada ao relato. O texto gerado foi mantido na tela.
   at DepoimentoLocal.Windows.MainForm.Reformulate_Click(Object sender, EventArgs e)

```

### Entrada completa

```text
Eu abri o almoxarifado às oito horas. A zeladora Berenice estava comigo. Vi Otávio colocar duas caixas lacradas sobre o balcão. Conferi os números das etiquetas e assinei o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. Fechei a porta às oito e vinte. Não abri nenhuma das caixas.
```

### Saída completa

```text
Relatou que o depoente abriu o almoxarifado às oito horas. Viu Berenice com o depoente. Otávio colocou duas caixas lacradas sobre o balcão. O depoente viu conferir os números das etiquetas e assinar o recibo. Berenice levou uma das caixas para a prateleira, e a outra ficou no balcão. O depoente fechou a porta às oito e vinte. O depoente não abriu nenhuma das caixas.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 2 — Coloquial e desorganizado

Objetivo: Preservar aproximações, destinatários e ordem sem polir fatos.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 59,212s de geração; 61,865s incluindo preparação.

Entrada: 533 caracteres / 108 palavras. Saída: 640 caracteres / 122 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-09-29 18:47:43.085 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: marcador temporal ou de certeza omitido: perto

Warnings nativos: 4. Log integral: [test-02-log.txt](test-02-log.txt).

### Entrada completa

```text
Foi assim, eu tava no pátio, perto do bebedouro, isso já depois do sinal, umas dez para as onze, por aí. O Davi veio falar comigo do portão. Antes disso eu tinha visto a Elisa mexendo no cadeado, mas não sei se ela abriu ou só olhou. Aí o Davi falou pra mim que a chave tinha sumido. Eu falei que não tava com a chave. Tinha um ônibus ligado, fazia barulho, então uma parte do que ele disse eu perdi. No fim a Elisa passou com uma chave na mão, só que eu não sei se era aquela. Depois fui atender um telefonema e não acompanhei mais.
```

### Saída completa

```text
Relatou que foi assim, o depoente estava no pátio, perto do bebedouro, isso já depois do sinal, por volta de dez para as onze, por aí. O Davi veio falar com o depoente do portão. Antes disso, o depoente já tinha visto a Elisa mexendo no cadeado, mas o depoente não sabe se ela abriu ou só olhou. Então o Davi falou ao depoente que a chave tinha sumido. O depoente respondeu que não tinha a chave. Havia um ônibus ligado, fazia barulho, então uma parte do que o Davi disse o depoente perdeu. No final, a Elisa passou com uma chave na mão, mas o depoente não sabe se era aquela. Depois, o depoente atendeu um telefonema e não acompanhou mais.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 3 — Autocorreções de transcrição

Objetivo: Distinguir fragmento abandonado, confirmação e dúvida não resolvida.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 22,619s de geração; 25,248s incluindo preparação.

Entrada: 444 caracteres / 76 palavras. Saída: 514 caracteres / 88 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-09-29 18:48:38.085 -03:00] [WARN] [BLOCK_ACCEPTED_WITH_WARNING] block=1/1; issue=uma negação pode ter sido omitida

Warnings nativos: 3. Log integral: [test-03-log.txt](test-03-log.txt).

### Entrada completa

```text
Eu entreguei a pasta na sexta... não, na quinta-feira. Foi quinta, porque sexta eu estava de folga. Entreguei para a recepcionista, quer dizer, para a estagiária que estava cobrindo a recepção. Não sei o nome dela. Eu saí às dez... corrigindo, isso foi quando cheguei; saí perto das onze. Acho que o envelope azul ficou dentro da pasta, mas não tenho certeza. Não estou corrigindo essa dúvida: eu realmente não lembro se coloquei o envelope lá.
```

### Saída completa

```text
Relatou que o depoente entregou a pasta na quinta-feira. Foi quinta, porque sexta o depoente estava de folga. O depoente entregou para a estagiária que estava cobrindo a recepção. O depoente não sabe o nome dela. O depoente saiu às dez... Corrigindo, isso foi quando o depoente chegou; o depoente saiu perto das onze. O depoente acredita que o envelope azul ficou dentro da pasta, mas o depoente não tem certeza. O depoente não está corrigindo essa dúvida: o depoente realmente não lembra se colocou o envelope lá.
```

Observações automáticas:

- O validador de produção apontou possível perda de negação; não é conclusão semântica do QA.

## TESTE 4 — Três participantes do mesmo gênero

Objetivo: Manter sujeito do depoimento e distinguir pronomes definidos de ambíguos.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 53,081s de geração; 55,638s incluindo preparação.

Entrada: 526 caracteres / 96 palavras. Saída: 608 caracteres / 107 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-09-29 18:49:07.080 -03:00] [WARN] [BLOCK_RETRY_START] block=1/1; issue=fidelidade: perda de destinatário do relato: leandro voltou sozinho e me disse que vicente já tinha ido embora.

Warnings nativos: 4. Log integral: [test-04-log.txt](test-04-log.txt).

### Entrada completa

```text
Eu estava com Leandro e Natan quando Vicente entrou na oficina. Leandro pediu a Natan que guardasse a ferramenta dele. Eu não sei de quem era a ferramenta. Vicente me perguntou se eu tinha visto a nota fiscal. Respondi que não. Natan disse a Leandro que ele precisava voltar no sábado. Não ficou claro para mim qual dos dois precisava voltar. Depois Vicente saiu, e eu fiquei com Natan. Eu não perguntei a Natan sobre a nota. Leandro voltou sozinho e me disse que Vicente já tinha ido embora. Eu não vi Vicente sair do prédio.
```

### Saída completa

```text
Relatou que o depoente estava com Leandro e Natan quando Vicente entrou na oficina. Leandro pediu a Natan que guardasse a ferramenta dele. O depoente não sabe de quem era a ferramenta. Vicente lhe perguntou se o depoente tinha visto a nota fiscal. O depoente respondeu que não. Natan disse a Leandro que ele precisava voltar no sábado. Não ficou claro para o depoente qual dos dois precisava voltar. Depois Vicente saiu, e o depoente ficou com Natan. O depoente não perguntou a Natan sobre a nota. Leandro voltou sozinho e lhe disse que Vicente já tinha ido embora. O depoente não viu Vicente sair do prédio.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 5 — Percepção e relato em cadeia

Objetivo: Preservar cada camada de fonte e distinguir leitura, audição e observação.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 23,364s de geração; 26,082s incluindo preparação.

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

## TESTE 6 — Negação da percepção

Objetivo: Não transformar não ver/não ouvir em negativa do acontecimento.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 22,382s de geração; 25,159s incluindo preparação.

Entrada: 408 caracteres / 75 palavras. Saída: 492 caracteres / 88 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-06-log.txt](test-06-log.txt).

### Entrada completa

```text
Eu não vi Ícaro retirar dinheiro da gaveta. Isso não significa que eu saiba se ele retirou ou não. Vi Ícaro perto do balcão, com uma sacola fechada. Não ouvi ele pedir autorização à gerente. Também não posso afirmar que ele não pediu, porque fiquei fora da loja por alguns minutos. A gerente Yara me disse que faltavam cinquenta reais, mas eu não conferi o caixa e não sei se essa diferença já existia antes.
```

### Saída completa

```text
Relatou que o depoente não viu Ícaro retirar dinheiro da gaveta. Isso não significa que o depoente saiba se ele retirou ou não. O depoente viu Ícaro perto do balcão, com uma sacola fechada. O depoente não ouviu ele pedir autorização à gerente. O depoente também não pode afirmar que ele não pediu, porque o depoente ficou fora da loja por alguns minutos. A gerente Yara lhe disse que faltavam cinquenta reais, mas o depoente não conferiu o caixa e não sabe se essa diferença já existia antes.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 7 — Negações encadeadas

Objetivo: Preservar escopo de negativas e ausência de confirmação ou desmentido.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 21,790s de geração; 24,396s incluindo preparação.

Entrada: 418 caracteres / 74 palavras. Saída: 478 caracteres / 84 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-07-log.txt](test-07-log.txt).

### Entrada completa

```text
Eu não disse que Celso não entregou o crachá. Disse que não vi a entrega. Não é verdade que ninguém me avisou da vistoria: recebi um aviso, mas não pelo telefone. Não lembro quem deixou o bilhete. A supervisora Lídia não negou que o armário estivesse aberto; ela disse que não sabia. Eu não confirmei nem desmenti a versão dela. Nunca entrei naquele depósito sem acompanhamento e não autorizei ninguém a usar meu nome.
```

### Saída completa

```text
Relatou que o depoente não disse que Celso não entregou o crachá. Disse que o depoente não viu a entrega. Não é verdade que ninguém lhe avisou da vistoria: o depoente recebeu um aviso, mas não pelo telefone. O depoente não lembra quem deixou o bilhete. A supervisora Lídia não negou que o armário estivesse aberto; ela disse que não sabia. O depoente não confirmou nem desmentiu a versão dela. Nunca entrou no depósito sem acompanhamento e não autorizou ninguém a usar seu nome.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 8 — Datas e ordem incertas

Objetivo: Preservar alternativas de data, intervalos e limites de lembrança.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 28,206s de geração; 30,764s incluindo preparação.

Entrada: 505 caracteres / 96 palavras. Saída: 594 caracteres / 111 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-08-log.txt](test-08-log.txt).

### Entrada completa

```text
Acho que visitei o arquivo em 12 de agosto, talvez tenha sido no dia 13. Sei que foi antes do feriado municipal de 15 de agosto. Cheguei entre sete e meia e oito horas, não às oito e meia. O caminhão apareceu depois da minha chegada, mas antes de abrirem o portão lateral. Não lembro se fiquei até nove horas ou até nove e quinze. Dois ou três dias depois, recebi uma ligação de Tainá sobre as caixas. Ela disse que a retirada tinha sido cancelada na véspera da ligação. Não sei a data desse cancelamento.
```

### Saída completa

```text
Relatou que o depoente acredita que visitou o arquivo em 12 de agosto, talvez tenha sido no dia 13. O depoente sabe que foi antes do feriado municipal de 15 de agosto. O depoente chegou entre sete e meia e oito horas, não às oito e meia. O caminhão apareceu depois da chegada do depoente, mas antes de abrirem o portão lateral. O depoente não lembra se ficou até nove horas ou até nove e quinze. Dois ou três dias depois, o depoente recebeu uma ligação de Tainá sobre as caixas. Ela disse que a retirada tinha sido cancelada na véspera da ligação. O depoente não sabe a data desse cancelamento.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 9 — Versões incompatíveis

Objetivo: Preservar conflito, atribuições e certeza de cada participante.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 24,246s de geração; 26,933s incluindo preparação.

Entrada: 507 caracteres / 85 palavras. Saída: 548 caracteres / 91 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-09-log.txt](test-09-log.txt).

### Entrada completa

```text
Na segunda-feira, Hugo me disse que a janela havia sido fechada antes da chuva. Na terça-feira, Iracema afirmou que encontrou a mesma janela aberta durante a chuva. Eu não estava no prédio naquele período. Hugo depois explicou que fechou a janela da copa, não a do corredor. Iracema disse que estava falando da janela da copa. Não sei qual versão está correta. Um segurança comentou que talvez alguém tivesse aberto a janela novamente, mas deixou claro que era uma hipótese. Eu não presenciei essa abertura.
```

### Saída completa

```text
Relatou que na segunda-feira, Hugo lhe disse que a janela havia sido fechada antes da chuva. Na terça-feira, Iracema afirmou que encontrou a mesma janela aberta durante a chuva. O depoente não estava no prédio naquele período. Hugo depois explicou que fechou a janela da copa, não a do corredor. Iracema disse que estava falando da janela da copa. O depoente não sabe qual versão está correta. Um segurança comentou que talvez alguém tivesse aberto a janela novamente, mas deixou claro que era uma hipótese. O depoente não presenciou essa abertura.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 10 — Citações e primeira pessoa legítima

Objetivo: Preservar palavras entre aspas e identidade de quem as pronunciou.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 23,571s de geração; 26,245s incluindo preparação.

Entrada: 396 caracteres / 69 palavras. Saída: 466 caracteres / 79 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-10-log.txt](test-10-log.txt).

### Entrada completa

```text
Ouvi Mirela dizer a Jonas: “Eu não prometi entregar hoje; prometi tentar”. Jonas respondeu: “Você me disse amanhã, não hoje”. Eu ouvi as duas frases. Não sei a que dia Jonas se referia com “amanhã”. Depois alguém atrás de mim falou “nós já avisamos”, mas não identifiquei a voz. Mirela me disse mais tarde que não estava falando do contrato. Eu não perguntei de qual documento ela estava falando.
```

### Saída completa

```text
Relatou que o depoente ouviu Mirela dizer a Jonas: “Eu não prometi entregar hoje; prometi tentar”. Jonas respondeu: “Você me disse amanhã, não hoje”. O depoente ouviu as duas frases. O depoente não sabe a que dia Jonas se referia com “amanhã”. Depois alguém atrás do depoente falou: “nós já avisamos”, mas o depoente não identificou a voz. Mirela lhe disse mais tarde que não estava falando do contrato. O depoente não perguntou de qual documento ela estava falando.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 11 — Nomes e funções semelhantes

Objetivo: Não fundir nomes próximos nem trocar cargos, objetos ou responsabilidade.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 23,270s de geração; 25,864s incluindo preparação.

Entrada: 489 caracteres / 79 palavras. Saída: 558 caracteres / 90 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-11-log.txt](test-11-log.txt).

### Entrada completa

```text
Eu trabalho como auxiliar de biblioteca. A bibliotecária Lívia recebeu o formulário da assistente Lídia. Vi as duas assinarem campos diferentes. O professor Ari levou o formulário para o secretário Arlindo. Não vi a entrega ao secretário; Ari me contou isso depois. A diretora Célia estava no corredor, mas não participou da conversa que ouvi. O vice-diretor Célio telefonou para mim à tarde e perguntou pelo formulário. Eu não sou secretário e não tinha autorização para aprovar o pedido.
```

### Saída completa

```text
Relatou que o depoente trabalha como auxiliar de biblioteca. A bibliotecária Lívia recebeu o formulário da assistente Lídia. O depoente viu as duas assinarem campos diferentes. O professor Ari levou o formulário para o secretário Arlindo. O depoente não viu a entrega ao secretário; Ari lhe disse isso depois. A diretora Célia estava no corredor, mas não participou da conversa que o depoente ouviu. O vice-diretor Célio telefonou para o depoente à tarde e perguntou pelo formulário. O depoente não é secretário e não tinha autorização para aprovar o pedido.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 12 — Relato longo e mudanças de assunto

Objetivo: Verificar cobertura dos blocos, identidade do depoente, fontes, temporalidade e final.

Status técnico: **CONCLUÍDO COM ALERTAS**. Geração terminou normalmente: True.

Tempo: 245,385s de geração; 248,012s incluindo preparação.

Entrada: 5184 caracteres / 909 palavras. Saída: 5850 caracteres / 1019 palavras.

Retry: True. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

- [2026-09-29 18:54:59.748 -03:00] [WARN] [BLOCK_RETRY_START] block=3/5; issue=fidelidade: percepção acrescentada ao relato

Warnings nativos: 8. Log integral: [test-12-log.txt](test-12-log.txt).

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
Relatou que o depoente trabalha na recepção de um centro cultural. Na manhã de 18 de maio, o depoente chegou por volta das sete e quarenta. O relógio da parede estava parado, por isso o depoente consultou seu celular. A produtora Solange estava na entrada, o técnico Bento estava no palco e a auxiliar Neide arrumava as cadeiras. O depoente cumprimentou os três e deixou sua mochila sob o balcão. O depoente não percebeu discussão quando o depoente entrou. Solange lhe perguntou se a lista dos convidados já tinha chegado. Respondeu que ainda não tinha recebido nenhuma lista. Ela disse que o coordenador Gaspar tinha enviado uma versão na noite anterior. O depoente não viu esse envio e não sabe para qual endereço teria sido encaminhado. O depoente consultou somente a caixa de entrada da recepção, não a pasta de mensagens indesejadas. Neide ouviu a conversa de que o depoente participava, mas não respondeu. Alguns minutos depois, Bento veio até o balcão com um cabo na mão. Disse que uma das caixas de som estava falhando. O depoente ouviu um chiado quando ele fez um teste, mas o depoente não sabe avaliar se o problema era no cabo ou na caixa. Solange pediu que ele aguardasse a chegada do eletricista. O depoente não ouviu ninguém dizer que o equipamento estava queimado. Enquanto isso, entrou a professora Vilma com duas sacolas. O depoente pediu que o depoente guardasse os materiais da oficina. O depoente colocou as sacolas no armário atrás da recepção, sem abrir nenhuma delas. Vilma disse que voltaria antes das dez. O depoente não sabe o que havia dentro. Mais tarde Neide comentou que uma das sacolas parecia conter tintas, mas ela também não abriu a sacola na presença do depoente. Perto das oito e meia, o depoente atendeu uma ligação. A pessoa se apresentou como representante da empresa de transporte, mas o depoente não lembra o nome. Informou que o veículo do grupo estava atrasado. O depoente anotou a informação e passou para Solange. Não foi informado a causa do atraso nem quanto tempo levaria. O depoente não afirmou que o grupo tinha cancelado a visita. Saíu para beber água. Quando voltou, Gaspar estava ao lado de Solange. O depoente não viu a chegada dele. Os dois olharam uma folha, mas o depoente estava longe e não conseguiu ler. Gaspar lhe pediu que ele imprimisse uma lista nova. O depoente recebeu um arquivo no computador e imprimiu três páginas. O depoente não comparou essa lista com qualquer versão anterior. O depoente entregou as folhas a Solange, não a Gaspar. Depois ouviu Neide perguntando a Bento sobre uma extensão elétrica. Ele disse que tinha deixado a extensão perto da porta de serviço. O depoente não viu onde ele a colocou. Neide saiu em direção à porta e voltou sem nada nas mãos. O depoente não sabe se não encontrou a extensão ou se foi fazer outra coisa. Não perguntou. O eletricista chegou, o depoente acredita que pouco depois das nove. O depoente não sabe seu nome. O depoente viu ele conversar com Bento e desligar uma tomada. O depoente não acompanhou o restante do serviço porque o depoente começou a atender os visitantes. Mais tarde Bento lhe disse que o som estava funcionando. O depoente ouviu música no salão, mas o depoente não sabe se saía da mesma caixa que tinha apresentado chiado. Por volta de nove e meia, Vilma voltou ao balcão. O depoente entregou a ela as duas sacolas. Ela conferiu algo dentro de uma delas e disse que estava faltando um pincel. O depoente não sabe se esse pincel estava na sacola quando ela chegou. Vilma não disse que alguém tinha retirado o objeto. O depoente não viu ninguém mexer nas sacolas enquanto estavam no armário, mas houve um período em que o depoente ficou longe do balcão. Antes do almoço, Solange lhe contou que a empresa havia enviado outro veículo. O depoente não falou novamente com a empresa. Um grupo chegou pouco depois, acompanhado de uma monitora. O depoente não tem certeza de que era o mesmo grupo da ligação. A monitora entregou um papel a Gaspar. O depoente viu a entrega, mas o depoente não leu o papel. O depoente saiu para almoçar às doze e dez e voltou por volta de uma hora. Na volta, Neide disse que tinha havido uma conversa entre Vilma e Gaspar sobre materiais. O depoente não participou dessa conversa e não sabe se trataram do pincel. Neide disse que ouviu apenas o final. Não lhe contou as palavras usadas por nenhum dos dois. Durante a tarde, o depoente ajudou a separar crachás para a atividade seguinte. Bento perguntou se o depoente tinha visto uma chave pequena. O depoente respondeu que não sabia de qual chave ele estava falando. Ele mostrou uma fotografia, mas o depoente não reconheceu a chave. Não disse que ela nunca tinha passado pela recepção. Solange comentou que talvez estivesse com o pessoal da limpeza; isso foi apresentado como possibilidade. No fim do expediente, Gaspar agradeceu à equipe e disse que faria uma avaliação do evento em outro dia. Não marcou uma data na sua presença. O depoente foi embora por volta das cinco e vinte. Quando saiu, Solange e Bento ainda estavam no salão. O depoente não sabe quem fechou o prédio naquela noite. Na manhã seguinte, o depoente recebeu uma mensagem de Vilma dizendo que tinha encontrado o pincel em casa. Foi ela quem informou isso; o depoente não viu o objeto. O depoente não recebeu notícia de que alguém tivesse sido responsabilizado pelo desaparecimento. Na semana seguinte, o depoente ouviu de um funcionário cujo nome não lembra que a avaliação havia sido adiada. O depoente não sabe se essa informação estava correta. O depoente não participou de reunião de avaliação e não teve acesso a atas. O depoente não pode afirmar que o problema do som, o atraso do transporte e a falta do pincel tivessem relação entre si. Seu relato reúne o que o depoente observou, as informações que lhe passaram e os pontos que o depoente não conseguiu confirmar.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 13 — Correção explícita do próprio depoente

Objetivo: Aplicar retratação efetiva sem manter alegação abandonada ou inventar certeza.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 23,309s de geração; 25,937s incluindo preparação.

Entrada: 458 caracteres / 83 palavras. Saída: 517 caracteres / 93 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-13-log.txt](test-13-log.txt).

### Entrada completa

```text
Eu disse há pouco que Rubens me entregou a chave, mas preciso corrigir isso: quem me entregou foi o porteiro Amaro. Rubens estava ao lado dele. Tenho certeza de quem fez a entrega porque assinei o livro diante de Amaro. Sobre o horário, mantenho a dúvida: pode ter sido às seis ou às seis e meia. Também preciso corrigir a palavra lacrado; o pacote estava fechado com fita, mas eu não verifiquei se havia lacre. Não quero afirmar que o pacote estava lacrado.
```

### Saída completa

```text
Relatou que o depoente disse há pouco que Rubens lhe entregou a chave, mas precisa corrigir isso: quem lhe entregou foi o porteiro Amaro. Rubens estava ao lado dele. O depoente tem certeza de quem fez a entrega porque o depoente assinou o livro diante de Amaro. Sobre o horário, mantém a dúvida: pode ter sido às seis ou às seis e meia. Também precisa corrigir a palavra "lacrado"; o pacote estava fechado com fita, mas o depoente não verificou se havia lacre. O depoente não quer afirmar que o pacote estava lacrado.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 14 — Detalhes laterais relevantes ou não

Objetivo: Não apagar conteúdo sem autorização, mesmo quando parece irrelevante.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 27,217s de geração; 29,991s incluindo preparação.

Entrada: 500 caracteres / 91 palavras. Saída: 643 caracteres / 114 palavras.

Retry: False. Erro: False. Exceção: False. ContextOverflowException: False.

Modelo: file=qwen2.5-3b-instruct-q4_k_m.gguf; bytes=2104932768. Configuração registrada: context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0.

Tamanho da saída confere com o log do motor: True

Warnings do validador:

Nenhum registrado.

Warnings nativos: 3. Log integral: [test-14-log.txt](test-14-log.txt).

### Entrada completa

```text
Eu fui ao balcão pedir uma segunda via do recibo. Estava chovendo, e meu guarda-chuva amarelo ficou pingando perto da porta. Não acho que a cor do guarda-chuva tenha relação com o atendimento, mas lembro desse detalhe. A atendente Dora pediu que eu esperasse. Enquanto esperava, li um cartaz sobre vacinação e tomei água. Vi Dora entregar um papel a um homem de boné; não sei se era o meu recibo. Depois ela me entregou uma via com meu nome, que conferi. Saí sem reclamar e sem conversar com o homem.
```

### Saída completa

```text
Relatou que o depoente foi ao balcão pedir uma segunda via do recibo. Estava chovendo, e o guarda-chuva amarelo do depoente ficou pingando perto da porta. O depoente não acredita que a cor do guarda-chuva tenha relação com o atendimento, mas o depoente lembra desse detalhe. A atendente Dora pediu que o depoente esperasse. Enquanto esperava, o depoente leu um cartaz sobre vacinação e tomou água. O depoente viu Dora entregar um papel a um homem de boné; o depoente não sabe se era o recibo do depoente. Depois ela lhe entregou uma via com o nome do depoente, que o depoente conferiu. O depoente saiu sem reclamar e sem conversar com o homem.
```

Observações automáticas:

Nenhum indício adicional detectado. Revisão humana pendente.

## TESTE 15 — Ambiguidade real a preservar

Objetivo: Não resolver possuidor, destinatário, pessoa ou local por inferência.

Status técnico: **CONCLUÍDO SEM ALERTAS AUTOMÁTICOS**. Geração terminou normalmente: True.

Tempo: 23,383s de geração; 26,094s incluindo preparação.

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
- Testes executados: 15
- Concluídos (GENERATION_OK): 14
- Com warning do validador: 5
- Com primeira pessoa fora de citações (indício): 0
- Com retry: 4
- Com erro: 1
- Com indício de truncamento: 0
- Com duplicação integral: 0
- Com parágrafos repetidos: 0
- Com ContextOverflowException: 0
- Tempo total (s): 702.116
