# Rodada conservadora — referências, autocorreções e sujeito

A execução anterior de 29/09/2026 está preservada em `round-1-final/Results`, junto dos fontes e motores correspondentes. As entradas permanecem as mesmas. Não foi criada bateria inédita.

## Correção ativa

O prompt final é idêntico ao da execução anterior. O reforço de prompt experimentado em `round-2-attempt-1` não resolveu a redução do horário e produziu problemas de redação nos casos 4 e 6; foi descartado. A correção ativa concentra-se no reparo local e na validação, preservando a geração que já funcionava.

- Destinatário: o validador anterior não cobria `falou para/pra mim`. A checagem foi ampliada. Quando verbo e complemento têm correspondência única com a fonte, o reparo pode restaurar `para o depoente` no lugar de `para ele`. Se houver dois destinatários possíveis na fonte, não altera.
- Referente subordinado: restaura um pronome explicitamente presente na entrada apenas quando sua exclusão produz exatamente um trecho da saída e o trecho sem pronome não existe também na fonte.
- Expressão aproximada: restaura o intervalo/alternativa inteiro a partir da entrada quando há uma única correspondência exata com o último limite e o restante da oração. Não escolhe um horário nem aumenta a certeza. A validação rejeita a redução não reparada.
- Sujeito: explicita o depoente em predicados derivados de primeira pessoa com correspondência lexical única. Normaliza somente conjugações conhecidas e `tava/tavam` para comparar; não usa distância semântica ou posição de frases. Não substitui sujeito nomeado e não duplica sujeito antes de advérbios. Correspondências repetidas ou ambíguas permanecem intocadas.
- Foco de advérbios: ao inserir o sujeito, mantém `só`/`também` junto do predicado quando assim aparecem na fonte. Não converte `só não lembro` em `só o depoente não lembra`. Quando a própria entrada diz `só eu`, conserva o foco exclusivo sobre o sujeito. Essa distinção foi verificada tanto no helper quanto na DLL incorporada.
- Ordem e pontuação: sem consultar a fonte, reposiciona somente `eu` explícito depois de verbos de percepção. Para mover `o depoente` pós-verbal, exige que o mesmo predicado e o complemento completo correspondam uma única vez à primeira pessoa na entrada; não move esse termo quando ele é objeto da ação de terceiro ou quando há ambiguidade. Se a fonte contém uma ação seguida de estimativa sem pontuação, separa as orações e preserva o locativo que consta na entrada.
- Autocorreção: consolida somente fragmento abandonado com negação corretiva e confirmação redundante explícitas na fonte. Confere o predicado abandonado, as palavras da confirmação (incluindo antes/depois) e a presença de incerteza. Preserva a correção; não remove versões conflitantes ou uma confirmação com informação diferente. Explicita o sujeito da crença e o dono de uma nominalização temporal somente quando o fragmento abandonado identifica aquela ação em primeira pessoa e a correção conserva a mesma relação antes/depois.

Todos os reparos protegem citações. O fluxo continua sendo geração, reparos locais, validação e no máximo um retry; não há novo modelo, chamada externa ou reconstrução por inferência.

## Arquivos

Alterados nesta rodada: `SemanticGuard.cs`, `Apply-SemanticFix.ps1`, `Test-SemanticGuard.ps1`, `Test-Embedded.ps1` e `Review-Results.ps1`. Acrescentado `Test-ReferenceRepairs.ps1`. O script de comparação aceita uma execução anterior explícita, para comparar com a rodada aprovada pelo usuário e não só com o relatório original.

As três DLLs foram atualizadas pelo mesmo mecanismo dnlib; quatro inicializadores e o manifesto receberam apenas hashes. Modelo, parâmetros, divisão em blocos, UI e proteções semânticas anteriores foram preservados. `SystemPrompt.txt` foi restaurado byte a byte, comprovado em `round-2-source-hashes.json`.

Compilação: fontes disponíveis do launcher/interface, executor de QA e helper .NET 8. O projeto completo do motor não acompanha a distribuição; não se afirma recompilação integral de seus fontes.

## Verificação

`guard-tests.txt`, `reference-tests.txt` e `embedded-tests.txt` contêm testes positivos/negativos com outros nomes, ações e números, incluindo idempotência, citações, ambiguidade, sujeito como objeto e confirmação temporal divergente. A última bateria real está em `QA/Results`. O comparativo final deve usar `round-1-final/Results` como base.

Os reparos não constituem um analisador completo de português. Exigem correspondência conservadora e podem deixar sem reparo uma paráfrase legítima; a validação permanece heurística. A revisão semântica de todas as dez saídas decide a aprovação desta execução. A próxima etapa depende da leitura do usuário, sem início de bateria inédita nesta rodada.
