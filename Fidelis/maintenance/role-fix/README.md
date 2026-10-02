# Rodada 7: inversões de papel (modelo 3B mantido)

## Etapa 1: alerta na interface

Alerta laranja «sujeito de outra pessoa apagado» (`RoleScanner.DeletedSubjects` em `ui/ModernShell.cs`).

**Quando aparece:**
- no original, outra pessoa é sujeito de um predicado: «Meu marido foi preso», «O porteiro ficou calado»;
- no texto gerado, o mesmo predicado vem logo depois de «o/a depoente» ou de «relatou que».

**Quando não aparece:** se o depoente faz o mesmo verbo no original, o caso é duvidoso e o alerta não é dado.

**Testes:** casos em `maintenance/role-alerts/Test-RoleAlerts.ps1`, seção 5.

## Etapa 2: motor (`maintenance/semantic-fix/SemanticGuard.cs`)

Os reparos funcionam como a correção da primeira pessoa: consultam o original e agem só quando há uma única correspondência possível. Sem certeza, nada é corrigido; o validador acusa, o motor faz uma nova tentativa e, se a falha continuar, a saída é rejeitada e marcada como incompleta.

| Problema | Reparo | O validador acusa quando não há reparo |
|---|---|---|
| Sujeito de outra pessoa apagado | `RepairDeletedSubject`: «relatou que foi preso» → «relatou que o marido do depoente foi preso»; mantém a concordância do original («ela estava nervosa») | `DeletedSubjectIssue` |
| Papéis trocados | `RepairSwappedRelative`: «entrou depois do Carlos» → «o Carlos entrou depois do depoente»; «ficou lhe encarando» → «ficou encarando o depoente». `RepairNarratorPronoun` aceita a troca de gênero da palavra seguinte: «ele era culpado» → «o depoente era culpado» | `SwappedRoleIssue` |
| Pronome ambíguo | `RepairAmbiguousObject`: «ele o atacou» → «o depoente foi atacado por ele». Só com particípio regular; «bater», «ver» e verbos de fala ficam como estão | `AmbiguousObjectIssue` |
| Frase quebrada / gerúndio | `RepairGerundObject`: «ele estava o depoente enforcando» e «o depoente estava enforcando» → «ele estava enforcando o depoente» | `GerundObjectIssue` e `BrokenPhraseIssue` |

## Arquivos

- **Backup do motor anterior:** `backup-before/`, com as 3 DLLs, o SemanticGuard (fonte e compilado), o script de aplicação, os inicializadores e o SHA256SUMS.
- **Estado anterior:** `antes/`, com as 20 frases, a bateria de gênero e as etapas da sonda nas 54 frases.
- **Testes:**
  - `Test-RoleRepairs.ps1`: reparos e validador, só com frases inventadas;
  - `maintenance/semantic-fix/Test-Embedded.ps1`: 3 checagens no motor instalado.
- **Antes × depois:** `Compare-Round7.ps1` gera `ANTES-DEPOIS.md` e marca INESPERADO qualquer mudança fora dos casos destes quatro problemas.
