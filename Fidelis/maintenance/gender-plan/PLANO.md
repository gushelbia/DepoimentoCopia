# Plano: gênero do depoente (nada foi alterado)

Comportamento desejado:
- campo «Gênero do depoente» na qualificação: Masculino, Feminino ou Não informado;
- Feminino gera o texto inteiro no feminino;
- Masculino e Não informado ficam como hoje;
- com Não informado, se o original indicar o gênero, o programa sugere preencher o campo, sem decidir sozinho.

As sondagens usaram só frases inventadas (`frases-mulheres.tsv`, F1 a F12). Elas chamam os passos do próprio motor por reflexão, sem alterá-lo (`GenderProbe.cs`, resultados em `sonda-atual.txt` e `sonda-feminino.txt`).

## 1. Como o gênero chegaria ao motor

Hoje a interface só fala com o motor escrevendo nos campos da janela escondida (original, reformulado, consolidado, caminho do modelo) e clicando nos botões. **Não existe canal para outro dado.**

Formas possíveis, só para o gênero:

| Canal | Como funciona | Risco |
|---|---|---|
| **a) Arquivo em `%LOCALAPPDATA%\DepoimentoLocal`** (ex.: `genero.txt` com `F`, `M` ou `N`) | A interface grava o arquivo logo antes de clicar «Reformular». O motor (com patch) lê no início da geração e trata arquivo ausente como `N`. O motor já lê `last-model.txt` dessa pasta, então segue o mesmo padrão. | Baixo a médio. Exige que o motor leia o arquivo. Um arquivo velho pode valer para outra geração, mitigado gravando sempre antes e apagando ao fechar. |
| b) Marcador no texto original (ex.: primeira linha `[GÊNERO: F]`) | O motor retira o marcador antes do prompt. | Alto. Se algum caminho não retirar o marcador, ele vai para o modelo ou aparece no texto. A bateria QA compara o original enviado. |
| c) Mensagem do Windows para a janela do motor | Exige reescrever o tratamento de mensagens da janela do motor (IL). | Alto. É código delicado do motor. |

Na opção 1 abaixo **nenhum canal é necessário**: a conversão acontece na interface, que já tem o campo.

## 2. Onde está «o depoente» e o que precisaria mudar

| Lugar | Ocorrências | O que é | Para gerar no feminino |
|---|---|---|---|
| Prompt principal (`SystemPrompt.txt`, embutido) | 10 | «passando somente **o depoente** para terceira pessoa», «**O depoente** é sempre a pessoa que narra», «NUNCA substitua … por "**o depoente**"» | Variante do prompt por gênero |
| Prompt de nova tentativa | o mesmo texto, com cabeçalho | idem | idem |
| `SemanticGuard` (embutido) | 64 (74 literais no motor) | **29** textos que o código *escreve* («O depoente », «para o depoente», «na presença do depoente»…); **24** padrões que *procura* («\bo depoente\s+…» nos reparos e no validador); **18** formas contraídas | Escrever o sintagma do gênero escolhido e procurar «(o\|a) depoente» |
| Código original do motor (`RepairResidualFirstPerson`) | 5 | «com o depoente», «para o depoente», «do depoente», «pelo depoente», «o depoente » (troca de «eu») | Patch de IL nesses literais |
| `FidelityRepairs` | 1 | «com depoente» | idem |
| Abertura final (`FinalLead`) | 1 | «O depoente relatou que» | «A depoente relatou que» |
| Interface | 20 linhas | alertas de papéis (gênero trocado/presumido), resumo, Word | Alertas cientes do campo |

**Achado importante.** Muitas proteções do validador procuram literalmente «o depoente»: ação atribuída ao depoente, sujeito trocado, destinatário, percepção. Com «a depoente», elas **deixariam de disparar em silêncio** se não fossem todas adaptadas. As proteções para depoentes mulheres ficariam mais fracas que para homens.

## 3. Formas de fazer

### Opção 1 — Converter no fim, na interface (o motor não muda)

- O motor continua gerando no masculino, exatamente como hoje.
- Quando a geração termina e o campo está em **Feminino**, a interface converte o texto recebido, usando o **original** como guia:
  1. o sintagma «o/O/do/ao/no/pelo depoente» passa para «a/A/da/à/na/pela depoente»;
  2. adjetivos e particípios que o original põe no feminino para a narradora («fiquei **nervosa**», «fui **agredida**», «estava **sozinha**») trocam a forma masculina da saída pela feminina;
  3. o pronome «o» que veio de «me» («ele **o** ameaçou» ← «ele **me** ameaçou») passa para «a»;
  4. (a acrescentar) particípios logo depois do sujeito depoente com «foi/ficou/estava» passam para o feminino («foi empurrad**o**» → «empurrad**a**»).
- O ponto de entrada é limpo: o momento em que a interface recebe o texto final do motor. Depois disso o reformulado da tela é independente, e «Adicionar», copiar e o Word usam o texto convertido.
- **Protótipo nas 12 frases** (`Convert-ToFeminine.ps1`): **10 de 12 corretas.** A F9 («foi empurrado») passa a ser corrigida pela regra 4. As F5 e F7 são inversões do modelo, não erros de gênero (ver abaixo).
- **Risco: baixo.**
  - Masculino e Não informado **não mudam por construção**, porque o conversor só roda com Feminino.
  - A bateria QA e o motor ficam idênticos.
  - Nenhum dado vai ao modelo.
- **Limite:** só converte o que tem pista. Uma palavra masculina que o modelo inventou sem correspondente no original, fora do padrão «foi/ficou/estava + particípio», pode escapar. O alerta laranja de gênero trocado continua conferindo.

### Opção 2 — Converter no fim, no motor

- O mesmo conversor, dentro do motor, no fim do reparo final, onde já está `FinalLead`.
- Recebe o gênero pelo canal (a), o arquivo.
- **Ganho sobre a opção 1:** o texto na janela do motor também fica no feminino, e a validação final vê o texto convertido.
- **Risco: médio.**
  - Exige patch no motor e o canal de arquivo, que pode ficar desatualizado.
  - Para os homens o resultado seria idêntico, conferível pela bateria QA.
  - A qualidade é a mesma da opção 1.

### Opção 3 — Motor inteiro ciente do gênero (prompt feminino + reparos e validador adaptados)

- O prompt muda conforme o gênero.
- `SemanticGuard`, validador, abertura e os 6 literais originais passam a usar o sintagma do gênero escolhido.
- **Sondagem com prompt feminino** (sem adaptar o resto): o modelo acerta o feminino **sozinho em 8 de 12**, inclusive casos sem pista no original, e até corrigiu a F7. Mas o motor atual estraga o resultado: «**O** depoente relatou que **a** depoente…» e «quando **o** depoente ouviu», «**o** depoente era culpada». Por isso todas as peças teriam de ser adaptadas juntas.
- **Risco: alto.**
  - Mexe em cerca de 75 pontos do motor e no prompt.
  - Se faltar adaptar algum padrão do validador, a proteção some em silêncio para as mulheres.
  - O texto aprovado dos homens só fica idêntico se o caminho masculino continuar byte a byte igual, o que é verificável mas trabalhoso.
- É o caminho de maior qualidade.

### Recomendação

Começar pela **opção 1**:
- não toca no motor nem nos textos aprovados;
- resolve os casos com pista no original, que são a maioria, e a regra 4 cobre a passiva;
- pode ser testada só com a interface.

Se, depois de usar, sobrarem erros de gênero sem pista, a opção 3 fica como próximo passo, com o protótipo da opção 1 servindo de referência de qualidade.

### Sugestão de preenchimento (vale para todas as opções)

- Só na interface.
- Com Não informado, se o original indicar o gênero («fiquei nervosa», «sou a mãe», «posso estar enganado»), o painel mostra: «O original indica depoente mulher («nervosa»). Usar Feminino?», com um botão. Não muda sozinho.
- Reaproveita a detecção que os alertas de gênero já fazem.
- O aviso informativo de gênero presumido some quando o campo está preenchido.

## 4. Como testar

- **Bateria nova de gênero**, só com frases inventadas:
  - as 12 frases de mulheres (F1–F12);
  - as mesmas 12 com depoente homem (M1–M12);
  - algumas sem marca de gênero.
- **Feminino:**
  - nenhuma forma masculina referida à depoente;
  - nenhum alerta laranja de gênero trocado;
  - fatos iguais aos da versão masculina (só o gênero muda).
- **Masculino e Não informado:**
  - as 20 frases da bateria atual e os 10 casos da QA saem **idênticos** ao texto aprovado hoje (verificação automática por comparação exata);
  - na opção 1 isso é garantido por construção, e o teste confirma.
- **Testes de unidade do conversor:**
  - contrações («à depoente», «pela depoente»);
  - falas entre aspas preservadas;
  - nomes próprios intactos;
  - palavras femininas que não se referem à depoente («a porta estava fechada») não mexem em nada;
  - aplicar duas vezes não muda (idempotência).
- **Sugestão de preenchimento:**
  - aparece só com Não informado e pista clara;
  - nunca troca o campo sozinha.
- **Interface:**
  - o campo novo salva no rascunho, na recuperação e no Word;
  - o painel continua cabendo nas larguras testadas.

## Achado à parte: inversões que nada pega

Nas sondagens, duas frases saíram com **papéis trocados pelo próprio modelo**, independentes do gênero:
- **F5:** «Ele disse que eu era culpada» → «O depoente disse que ele era culpado»;
- **F7:** «O Carlos entrou depois de mim» → «O depoente entrou depois do Carlos».

Nem o reparo da rodada 6 nem os alertas pegam esses dois formatos, porque não há «me» envolvido. Vale incluí-los nos testes e tratá-los como tarefa separada, antes ou depois do gênero.
