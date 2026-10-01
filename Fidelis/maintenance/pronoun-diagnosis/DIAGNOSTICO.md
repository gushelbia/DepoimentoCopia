# Diagnóstico: pronomes, sujeito e gênero (30/09/2026)

Nada foi alterado no motor nem na interface. Os hashes das DLLs do motor foram conferidos antes e depois de cada execução.

A bateria tem 20 frases **inventadas** (`frases.json`). Elas rodaram pelo mesmo caminho do botão «Reformular» (`QA/QaRunner.cs`), com o modelo atual (Qwen2.5 3B, temperatura 0).

- `resultados-20-frases.json` guarda as 20 saídas.
- `resultados-com-log.json` guarda a reexecução dos casos 1, 3, 5, 7, 10 e 18, com as linhas de reparo do log. As saídas foram idênticas às da primeira execução, porque o processo é determinístico.

## 1. Onde ficam as instruções e o que dizem

O prompt principal está embutido nas três DLLs do motor. A cópia legível e idêntica ao que está no motor é `maintenance/semantic-fix/SystemPrompt.txt`. Sobre pessoa, sujeito, pronomes e gênero, ele diz:

- «Use "o depoente" para deixar explícito quem viu, ouviu, sabe…»: **manda usar o masculino**.
- Na mesma linha: «Preserve o gênero das pessoas e não deduza o gênero do depoente a partir dos nomes citados». Isso contradiz a instrução anterior, e o modelo segue «o depoente».
- «Não substitua nomes por pronomes nem **resolva pronomes ambíguos**»: incentiva manter «ele o atacou».
- Há regra para o destinatário: «me disse» → «lhe disse». **Não há regra** para objeto direto («me atacou»), reflexivo («me defendi») nem voz passiva.

Há mais três fontes do problema no código:

- **«Relatou que » é uma string fixa no código do motor**, não texto do modelo. No 1º bloco, o motor a coloca antes da saída (`ApplyLead`) e põe em minúscula a letra seguinte. Quando a saída começa com «O depoente…», resulta «Relatou que o depoente…»: o verbo «relatou» fica sem sujeito e a frase parece falar de outra pessoa.
- **`SemanticGuard`**, a camada de correção embutida no motor, insere «o depoente» (masculino) em 7 pontos do código ao reparar a primeira pessoa. Todas as verificações procuram só «o depoente».
- **O validador** só detecta a ação de um participante atribuída ao depoente para verbos terminados em -ou/-eu/-iu. «Disse», por exemplo, fica de fora. Também não detecta o sujeito trocado em verbos de fala («Ele me disse» → «o depoente lhe disse»), reflexivo virando objeto, nem ambiguidade. Não confere gênero.

## 2. Bateria (20 frases inventadas)

Legenda:
- **Gênero esperado:** ? = não informado; F = mulher; M = homem.
- **Sentido:** ✓ correto; ⚠ ambíguo; ✗ errado.
- **Sujeito:** ✗ = «Relatou que o depoente…», verbo sem sujeito que parece falar de outra pessoa.
- **Gênero:** ⚠ = masculino presumido; ✗ = errado.

| # | Gên. | Entrada | Saída | Sentido | Sujeito | Gênero |
|---|---|---|---|---|---|---|
| 1 | ? | Eu estava correndo na praia quando ele me atacou por trás. | Relatou que o depoente estava correndo na praia quando ele o atacou por trás. | ⚠ ambíguo («ele o atacou») | ✗ | ⚠ |
| 2 | ? | Eu estava saindo do mercado quando ela me empurrou contra a parede. | Relatou que o depoente estava saindo do mercado quando ela o empurrou contra a parede. | ✓ | ✗ | ⚠ |
| 3 | ? | Ele me bateu com um pedaço de pau e depois fugiu. | [SAÍDA INCOMPLETA…] Relatou que o depoente lhe bateu com um pedaço de pau e depois fugiu. | ✗ **invertido** (o depoente bateu); o validador **bloqueou** | ✗ | ⚠ |
| 4 | ? | Quando eu cheguei, ele já estava caído no chão e eu chamei a ambulância. | Relatou que quando o depoente chegou, ele já estava caído no chão e o depoente chamou a ambulância. | ⚠ ambíguo («ele» pode ser lido como o depoente) | ✗ | ⚠ |
| 5 | ? | Eu empurrei ele porque ele estava me enforcando. | Relatou que o depoente empurrou ele porque ele estava o depoente enforcando. | ✗ frase quebrada | ✗ | ⚠ |
| 6 | ? | Ela me xingou e eu respondi que ia chamar a polícia. | Relatou que ela lhe xingou e o depoente respondeu que ia chamar a polícia. | ✓ (regência «lhe xingou») | ✓ | ⚠ |
| 7 | ? | Ele me disse que tinha visto ela sair com o carro. | Relatou que o depoente lhe disse que tinha visto ela sair com o carro. | ✗ **invertido**: agora é o depoente quem fala; passou sem alerta | ✗ | ⚠ |
| 8 | ? | O Marcos me segurou pelo braço enquanto o outro rapaz pegava minha bolsa. | Relatou que o Marcos lhe segurou pelo braço enquanto o outro rapaz pegava a bolsa do depoente. | ✓ | ✓ | ⚠ |
| 9 | ? | Eu vi quando ele deu um soco nela. | Relatou que o depoente viu quando ele deu um soco nela. | ✓ | ✗ | ⚠ |
| 10 | ? | Ela me contou que ele tinha batido nela na noite anterior. | Relatou que o depoente lhe disse que ele tinha batido nela na noite anterior. | ✗ **invertido**: agora é o depoente quem conta; passou sem alerta | ✗ | ⚠ |
| 11 | F | Eu estava grávida de cinco meses quando ele me chutou. | Relatou que o depoente estava grávida de cinco meses quando ele o chutou. | ⚠ ambíguo («ele o chutou») | ✗ | ✗ «o depoente estava grávida» |
| 12 | F | Eu fiquei nervosa e liguei para o meu irmão. | Relatou que o depoente ficou nervoso e ligou para o seu irmão. | ✓ | ✗ | ✗ «nervoso» |
| 13 | M | Eu sou o pai da menina e fui buscá-la na escola quando ele apareceu. | Relatou que o depoente é o pai da menina e saiu para buscá-la na escola quando ele apareceu. | ✓ (leve: «fui» → «saiu para») | ✗ | ✓ |
| 14 | ? | Ele me ameaçou com uma faca e eu corri para dentro de casa. | Relatou que ele lhe ameaçou com uma faca e o depoente correu para dentro de casa. | ✓ | ✓ | ⚠ |
| 15 | ? | Eu não vi quem me acertou, só senti a pancada na cabeça. | Relatou que o depoente não viu quem o acertou, o depoente só sentiu a pancada na cabeça. | ✓ | ✗ | ⚠ |
| 16 | ? | Eles me cercaram na saída do bar e um deles me deu uma rasteira. | Relatou que o depoente foi cercado na saída do bar e um deles lhe deu uma rasteira. | ✓ | ✗ | ⚠ «cercado» |
| 17 | ? | Eu defendi ela quando ele tentou agarrá-la. | Relatou que o depoente defendeu ela quando ele tentou agarrá-la. | ✓ | ✗ | ⚠ |
| 18 | ? | Ela me atacou primeiro e eu só me defendi. | Relatou que ela lhe atacou primeiro e o depoente só lhe defendeu. | ✗ «me defendi» → «lhe defendeu» (defendeu a ela, não a si); passou sem alerta | ✓ | ⚠ |
| 19 | F | Fui agredida pelo meu ex-companheiro na frente dos meus filhos. | Relatou que o depoente foi agredido pelo seu ex-companheiro na frente dos seus filhos. | ✓ | ✗ | ✗ «agredido» |
| 20 | M | Eu estava sozinho em casa quando ouvi ele arrombar a porta. | Relatou que o depoente estava sozinho em casa quando o depoente ouviu ele arrombar a porta. | ✓ | ✗ | ✓ |

### Resumo

- **Sentido:** 8 de 20 com problema.
  - 4 inversões graves de quem fez ou disse (3, 7, 10, 18). O validador pegou só o caso 3; os casos 7, 10 e 18 passaram sem nenhum alerta.
  - 1 frase quebrada (5).
  - 3 ambíguas (1, 4, 11).
- **Sujeito:** 16 de 20 começam com «Relatou que o depoente…».
- **Gênero:**
  - as 3 mulheres saíram no masculino («o depoente estava grávida», «nervoso», «agredido»);
  - os 15 casos sem gênero informado foram escritos no masculino (correção: antes constava 16 por erro de contagem);
  - os 2 homens saíram corretos.

### Origem de cada erro (log dos reparos)

- **Casos 1, 5 e 7:** nenhum reparo. O texto saiu assim **do modelo**; o motor só acrescentou «Relatou que ».
- **Casos 3 e 18:** o modelo deixou «me». O reparo de primeira pessoa gerou «o depoente lhe bateu» / «lhe atacou». O validador acusou e houve nova tentativa:
  - no caso 3, a saída foi bloqueada e marcada como incompleta;
  - no caso 18, a 2ª tentativa passou com «lhe defendeu».
- **Caso 10:** o reparo de primeira pessoa foi seguido de «ok», e a inversão passou.
- **Conferência:** rodei o validador fora do programa (`SemanticGuard.Validate`, o mesmo código embutido) nas 20 saídas. Ele só acusa o caso 3.

## 3. Soluções propostas e riscos

| Solução | O que resolve | Risco / custo |
|---|---|---|
| **A. Alerta na interface** (como os destaques atuais) para padrões de papel: «X me disse» → «o depoente lhe disse», «me defendi» → «lhe defendeu», «ele o [verbo]» ambíguo, «o depoente» no meio de uma locução verbal, «Relatou que o depoente» | Casos 1, 4, 5, 7, 10, 11 e 18 ficam marcados em laranja «conferir» | **Baixo.** Não mexe no motor nem no texto gerado. Não corrige nada, só avisa. Pode dar falso alarme, que é só visual. |
| **B. Ampliar o validador do motor** (`SemanticGuard`) com as mesmas verificações de A | Faz o motor tentar de novo ou bloquear em vez de entregar o erro | **Médio.** Mexe no motor (patch com dnlib, já usado antes). Mais tentativas deixam a geração mais lenta; um falso positivo bloqueia uma saída boa. Exige rodar de novo as baterias QA e semântica. |
| **C. Trocar o «Relatou que » fixo** (por exemplo, por «O depoente relatou que» ou nenhum), ou mudar a regra de maiúscula | Os 16 casos de sujeito | **Médio.** É uma string no código do motor, mas muda **todas** as saídas: a bateria QA deixa de ser idêntica e precisa ser reaprovada. |
| **D. Ajustar o prompt:** objeto direto («me atacou» → «atacou o depoente» ou «o depoente foi atacado por ele»), reflexivo («me defendi» → «se defendeu»), nunca trocar quem fala | Pode reduzir 1, 5, 7, 10, 11 e 18 | **Médio a alto e incerto.** O modelo 3B segue o prompt de forma irregular: o histórico em `prompt-update` mostra que regras novas não eliminaram erros. Muda todas as saídas e o prompt fica maior. |
| **E. Gênero pelo campo de qualificação** (campo novo «Gênero do depoente»; o dado não sai do computador) | Os 3 erros das mulheres | **Alto se for feito no motor.** O prompt manda «o depoente», e o `SemanticGuard` insere e procura «o depoente» em dezenas de expressões, que teriam de aceitar «a depoente». A interface não tem como enviar o gênero ao motor sem mudar o motor. Uma alternativa só na interface trocaria «o depoente» por «a depoente» depois da geração, mas não corrigiria «nervoso» ou «agredido»: resolveria pela metade. |
| **F. Modelo maior** (Qwen2.5 7B Q4_K_M, 4,7 GB) | Provavelmente menos erros de pronome, sem garantia | **Baixo para o código, alto para o tempo.** A máquina (Ryzen 5 3400G, 4 núcleos; GPU integrada Vega 11 com 2 GB; 13,9 GB de RAM) roda o 3B a cerca de 19 caracteres/s. Estimativa para o 7B: cerca de 2,3 a 2,7 vezes mais lento. Uma frase curta passaria de cerca de 11 s para cerca de 25 a 30 s; um bloco de 1.200 caracteres, de cerca de 1 min para cerca de 2,5 a 3 min. Cabe na RAM (cerca de 5,5 GB). O prompt e o `SemanticGuard` foram calibrados para o 3B, e todas as bases de comparação mudam. Dá para testar antes com esta bateria, sem trocar nada no programa. |

**Ordem sugerida:**
1. A (sem risco, protege já).
2. Testar F com esta bateria, só medindo.
3. Com o resultado, decidir entre B + C e a troca de modelo.
4. E por último, porque é a mais invasiva.
