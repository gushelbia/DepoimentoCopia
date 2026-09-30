# Bateria inédita — avaliação sem alteração do aplicativo

Quinze casos sintéticos novos, escritos e congelados antes da execução. Não são depoimentos reais nem constituem amostra estatística representativa. Esta avaliação verifica generalização para textos diferentes da bateria anterior, mas não é um teste cego independente: os casos foram preparados pelo mesmo assistente que conhece as correções.

## Método

- Uma execução de cada entrada, com o retry interno normal do aplicativo. Não repetir casos para escolher a melhor saída.
- Mesmo `QaRunner.cs` da bateria anterior, byte a byte: abre o motor real, conecta pelo OriginalAppBridge e aciona Carregar modelo/Reformular. Não simula a geração.
- Nova cópia de `Run-QA.ps1`, com contagem de quinze casos. Nova cópia de `Report.ps1`, com contagem de quinze e sem os três marcadores específicos do antigo Teste 5, que não pertencem a esta bateria. Demais verificações automáticas preservadas.
- Nenhuma alteração em código, prompt, validador, retry, reparos, binários ou configuração do aplicativo. Apenas o executor de QA junto à interface existente é compilado em memória para controlar o programa.
- `application-before.json` e `inputs-before.json` registram hashes anteriores; a verificação final deve confirmar identidade. O modelo e os binários usados constam de `Results/production-manifest.json`.
- Entradas em `TestCases/`; saída literal, log integral e registro estruturado de cada execução em `Results/test-NN*`. Nenhuma saída é corrigida manualmente.

## Critérios de revisão semântica

Comparação integral de cada entrada/saída quanto a sujeito, fonte e cadeia de relato, percepção versus fato, negações, incertezas e grau de certeza, ambiguidades, datas e sequência, citações, omissões, acréscimos, nomes e funções. Primeira pessoa dentro de citação literal não é falha. Um resultado sem warning não está automaticamente aprovado.

Gravidade adotada para achados (classificação preliminar, sujeita à avaliação do usuário):

- **Crítica:** inversão de negação ou transformação de limitação perceptiva em inexistência/existência de fato; atribuição falsa de conduta grave.
- **Alta:** troca de sujeito/fonte, perda de camada de relato, certeza indevida, resolução inventada de ambiguidade, mudança de data/ordem ou omissão relevante.
- **Moderada:** ambiguidade nova ou perda de detalhe que prejudique a interpretação, sem inversão inequívoca do núcleo factual; problema estrutural que exija intervenção humana.
- **Estilo:** construção pouco natural ou repetição verbal sem mudança identificada de conteúdo. Não confundir repetição estilística com duplicação de fatos.
- **Falha operacional:** geração rejeitada, vazia, parcial ou interrompida. Uma rejeição pode representar uma proteção correta, mas não conclui a tarefa solicitada. Se há saída parcial, ela também é revisada e identificada como parcial.

Casos só são aprovados quando a geração está completa e a revisão não identifica alteração semântica. A ausência de erro identificado é limitada a estas saídas; não garante fidelidade de entradas futuras.

## Cobertura

Os casos 1 a 15 correspondem aos quinze tópicos solicitados, incluindo relato longo de 5.184 caracteres (caso 12). Todos têm palavras, participantes e episódios diferentes da bateria anterior. O caso 14 inclui detalhes aparentemente laterais: a tarefa não autoriza apagá-los.

## Reprodução

Na raiz da distribuição, sem outra geração concorrente:

```powershell
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File ./QA-Inedita-20260929/Run-QA.ps1
```

O comando sobrescreve apenas os resultados desta pasta; preserve a execução anterior antes de uma nova rodada. A execução atual não deve ser repetida ou usada para ajustes do software nesta etapa.
