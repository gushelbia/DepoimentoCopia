# Rodada 9: bateria realista 2 (pronominais, primeira pessoa, possessivo, destinatário, regência)

Os reparos ficam em `maintenance/semantic-fix/SemanticGuard.cs`. Como nas rodadas anteriores, cada um consulta o original e age só com uma única correspondência. Quando não há certeza, o validador acusa, o motor faz uma nova tentativa e, se a falha continuar, a saída é rejeitada e marcada como incompleta.

| Problema | Reparo (motor) | Acusação (validador) |
|---|---|---|
| **Verbo pronominal do depoente** | `RepairNarratorPronominal`: «lhe arrependo» / «arrependo o depoente» → «se arrepende»; «não lhe sentia» → «não se sentia»; «lhe lembra» → «se lembra»; «ia lhe arrepender» → «ia se arrepender» | `NarratorPronominalIssue` |
| **Primeira pessoa fora das aspas** | `RepairIsolatedOath`: tira o «juro» isolado entre vírgulas («não sabia, juro.» → «não sabia.») | `FirstPersonLeftIssue`: «juro», «nem conheço» (não conta depois de artigo ou preposição: «o trabalho») |
| **Possessivo com outra pessoa na frase** | `RepairNarratorPossessive`: «seu namorado» (com o Bruno na frase) → «o namorado do depoente»; sem mudança quando o nome vem logo depois («sua colega Bianca») | — |
| **Destinatário perdido** | `RepairLostRecipient`: devolve «para ele/ela» quando a frase do original é única («tinham ligado para ela», «o banco disse para ela») | `LostRecipientIssue`: «ligado pra ela» / «disse pra ela» sem o destinatário na saída |
| **Regência com «-aram»** | «eles o mostraram» → «eles lhe mostraram» | — |

O reconhecimento da primeira pessoa (`NarratorPronominal`) considera:
- **as conjugações:** «-o» (tabela de verbos pronominais: lembro, sinto, arrependo…), «-ei/-i», «eu me…»;
- **«-ava/-ia», infinitivo e gerúndio:** o sujeito mais próximo antes do «me» decide;
- **verbo na terceira pessoa:** uma forma como «chamou» ou «disseram» nunca é pronominal do depoente.

Esse reconhecimento corrigiu a regressão da rodada 8 no B09 e o falso alarme do B05.

## Interface

- **Palavras inexistentes** (laranja): `RoleScanner.UnknownWords` usa o corretor ortográfico do próprio Windows em pt-BR (offline, sem download). Marca palavras da saída que não estão no dicionário nem no original, como «estavavam» e «ciento». Falas entre aspas, siglas e palavras com números ficam de fora. Sem o idioma pt-BR instalado, não marca nada.
- **Sugestões dos alertas:** «ia lhe arrepender» → «se arrepender»; «o depoente lhe lembra» → «se lembra»; «eles a mostraram» → «lhe mostraram» (verbos em «-aram»).

## Arquivos

- **Backup do motor e da interface anteriores:** `backup-before/`. O estado anterior das baterias está em `antes/`.
- **Testes:**
  - `Test-PronominalRepairs.ps1`: os reparos e o validador desta rodada.
  - `maintenance/role-alerts/Test-RoleAlerts.ps1`, seção 7: os alertas da interface.
- **Bateria realista 2:** `maintenance/realistic-battery-2/`, com a chave automática `chave-automatica.tsv`.
- **Antes × depois:** `Compare-Round9.ps1` gera `ANTES-DEPOIS.md`.
