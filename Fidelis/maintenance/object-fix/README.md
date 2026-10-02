# Rodada 8: bateria realista (depoente como objeto, valores, repetição, regência)

Os reparos ficam em `maintenance/semantic-fix/SemanticGuard.cs` e funcionam como os anteriores: consultam o original e agem só com uma única correspondência. Sem certeza, o validador acusa, o motor faz uma nova tentativa e, se a falha continuar, a saída é rejeitada e marcada como incompleta.

| Problema | Reparo (motor) | Acusação (validador) |
|---|---|---|
| **Depoente apagado como objeto** | `RepairNarratorObject`: «e ameaçou.» → «e ameaçou o depoente.»; «aceitou como orientando» → «aceitou o depoente como orientando»; «perguntou as horas» → «lhe perguntou as horas» | `NarratorObjectIssue`: «me V» do original sem referência ao depoente na saída (ação ausente, virou «se», ou «lhe» com verbo de ação) |
| **Depoente trocado em «se»** | «veio se xingando» → «veio xingando o depoente» | idem |
| **Possessivo do depoente** | `RepairNarratorOwner`: «Meu orientador» → «o orientador do depoente»; «puxou da minha mão» → «puxou da mão do depoente» (não «a mão») | `NarratorOwnerIssue` |
| **Valores** | `RepairNumberWords`: «cem e cinquenta» → «cento e cinquenta» (quando o original diz «cento e cinquenta») | — |
| **Repetição** | `RepairRepeatedSubject`: só o padrão exato «foi X que X [verbo]» → «foi X que [verbo]» («foi o depoente que achou») | `RepeatedNarratorIssue`: qualquer outra repetição («o depoente que o depoente») |
| **Regência** | «lhe chamou», «ia lhe reprovar», «lhe humilhou» → «chamou/reprovar/humilhou o depoente»; «o pediu» → «lhe pediu»; «e lhe atacou» → «e ele atacou o depoente» (sujeito do original) | — |

**Verbos de regência dupla** («levar», «trazer», «cobrar», «jogar», «atender», «puxar», «servir») nunca trocam «o» por «lhe» nem o contrário.

**Na interface:**
- O alerta de «lhe» com verbo de ação pega também depois de vírgula.
- «ela o pediu» aparece como «regência», com a sugestão «lhe pediu».
- O destaque reconhece valores por extenso em contexto de dinheiro, mesmo sem «reais» («tinha me dado cento e cinquenta»), e marca «cem e cinquenta» como divergente.

## Arquivos

- **Backup do motor anterior:** `backup-before/` (com a interface anterior). O estado anterior das baterias está em `antes/`.
- **Teste de unidade:** `Test-ObjectRepairs.ps1`. Os alertas da interface estão em `maintenance/role-alerts/Test-RoleAlerts.ps1`, seção 6.
- **Teste de regressão (bateria realista):** `maintenance/realistic-battery/`.
  - `bateria-realista.txt`: os casos, com a chave de conferência.
  - `chave-automatica.tsv`: o que cada saída deve e não deve conter.
  - `Run-RealisticBattery.ps1`: roda os casos e confere a chave e, se existir, o `aprovados.json`.
- **Antes × depois:** `Compare-Round8.ps1` gera `ANTES-DEPOIS.md`.
