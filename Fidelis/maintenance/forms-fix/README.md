# Rodada 10: bateria realista 3 (formas de primeira pessoa, aproximação, plural, marcadores, nome)

Os reparos ficam em `maintenance/semantic-fix/SemanticGuard.cs`. Como nas rodadas anteriores, cada um consulta o original e age só com uma única correspondência. Sem certeza, o validador acusa, o motor faz uma nova tentativa e, se a falha continuar, a saída é rejeitada e marcada como incompleta.

| Problema | Reparo (motor) | Acusação (validador) |
|---|---|---|
| **«comigo», «mim», «foi eu», «foi meu»** | `RepairOtherFirstPerson`: «brigado com ele» → «brigado com o depoente»; «depois dele» → «depois do depoente»; «acreditou nele» → «no depoente»; «tinha sido ele» → «tinha sido o depoente»; «nem foi seu,» → «nem foi do depoente,» | `OtherFirstPersonIssue` |
| **Aproximação perdida** | `RepairLostApproximation`: «às sete e meia» → «por volta das sete e meia»; «vinte minutos» → «cerca de vinte minutos» (para «umas», «uns», «por volta de», «cerca de», «lá pelas», «aproximadamente») | `LostApproximationIssue` |
| **Primeira pessoa do plural** (ver abaixo) | `RepairFirstPersonPlural`: «os quatro», «o depoente e a Olívia» ou «o grupo»; «nos ameaçou» → «ameaçou o grupo»; «era nossa» → «era do grupo»; «nós estávamos» → «o grupo estava»; «, eu,» numa enumeração → «, o depoente,» | o validador anterior já acusa «nós», «nossa» e afins |
| **Marcadores da fala** | `RepairSpeechMarkers`: «olha», «tipo», «né», «aí» e «sei lá» isolados saem; «tava» → «estava»; «sei lá se foi ele» → «não sabe se foi ele» | `StripSpeechMarkers`: marcadores isolados não contam como omissão de oração |
| **Nome próprio na abertura** | `RepairNamesAndOpening`: nome do original em minúscula volta à maiúscula; «Eu, Marta, sou…» → «O depoente, Marta, relatou que é…» | — |
| **Primeira pessoa singular restante** | `RepairFirstPersonLeft`: «nem conheço» → «nem conhece», quando o sujeito da oração é o depoente | `FirstPersonLeftIssue` (rodada 9) |

**Na interface:**
- O particípio («tivesse me explicado») deixou de ser tratado como verbo pronominal do depoente, o que tirou o falso alarme do C01.
- «aproximação perdida» aparece em laranja quando o original é aproximado e a saída ficou exata, com a sugestão «por volta das …».

## Primeira pessoa do plural: forma escolhida

O reparo combina três formas, sempre a partir do original:

1. **Número explícito** na mesma frase ou na anterior («a gente estava em quatro», «nós três») → **«os quatro»**, com o verbo no plural. O «em quatro» redundante sai.
2. **Um único membro nomeado com segurança** na mesma frase (antes de «a gente») ou na anterior → **«o depoente e a Olívia»**, com o verbo no plural. Conta como seguro:
   - sujeito no início da frase («A Olívia veio…»);
   - «eu e o Otávio» / «eu e o meu irmão, o Otávio»;
   - «com a Lia», quando o depoente aparece como «eu».

   Não contam:
   - nomes que aparecem depois de «a gente» («falar com a Noêmia»);
   - dois nomes coordenados («o Caio e a Rita»).
3. **Nos outros casos, ou na dúvida** → **«o grupo»**.

| Original | Saída |
|---|---|
| «A gente estava em quatro no carro, eu, a Débora, a Fernanda e o Gilson» (C02) | os quatro estavam no carro, o depoente, a Débora, a Fernanda e o Gilson |
| «Um carro preto bateu atrás da gente» (C02, frase seguinte) | bateu atrás dos quatro |
| «a culpa era nossa», «Ele nos ameaçou…» (C02, frases mais distantes) | do grupo; ameaçou o grupo |
| «A Olívia veio tirar satisfação comigo… Ela acreditou em mim e a gente foi junto…» (C07) | o depoente e a Olívia foram juntos (na tela, com Feminino: «a depoente e a Olívia foram juntas») |
| «Eu e o meu irmão, o Otávio… Na saída três caras pararam a gente» (B02) | pararam o depoente e o Otávio |
| «Eu fui com ela na delegacia… e a gente fez o boletim» (B08) | o grupo fez o boletim (membro sem nome) |

**Na interface (campo Feminino):**
- «a depoente e a Olívia foram juntos» → «juntas», quando a outra pessoa também é mulher.
- B01: «a Kátia, que também é bolsista, o informou» (de «me contou») → «a informou». A troca vale para o «o» vindo de «me» depois de um nome. Não vale se houver termo masculino no meio.

## Arquivos

- **Backup do motor e da interface anteriores:** `backup-before/`. O estado anterior das baterias está em `antes/`.
- **Testes:**
  - `Test-FormsRepairs.ps1`: os reparos desta rodada.
  - `maintenance/role-alerts/Test-RoleAlerts.ps1`, seção 8: os alertas da interface.
- **Bateria realista 3:** `maintenance/realistic-battery-3/`, com a chave automática.
- **Antes × depois:** `Compare-Round10.ps1` gera `ANTES-DEPOIS.md`.
