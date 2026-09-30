# Correção de fidelidade semântica

O projeto do motor não está nesta distribuição. Os fontes disponíveis da interface/launcher são compilados normalmente; `SemanticGuard.cs` é compilado com Roslyn e referências .NET 8 locais e incorporado nas três DLLs do motor com dnlib, seguindo o mecanismo de manutenção já existente. Não é uma recompilação integral do motor a partir de fontes.

`diagnosis.md` documenta o fluxo anterior e os problemas dos dez casos. `baseline/` preserva os resultados anteriores; `intermediate/`, `intermediate-2/`, `intermediate-3/` e `intermediate-4/` preservam rodadas de desenvolvimento. `QA/Results/` contém a última bateria real pelo OriginalAppBridge de produção. `inputs-verification.txt` comprova que as entradas coincidem com o anexo. Nenhuma entrada de QA foi editada.

## Alterações

- `SystemPrompt.txt`: transformação oração a oração, identidade do depoente, cadeia de fontes, destinatário, negações de percepção, incertezas, tempos e citações. Sem nomes ou frases específicos da bateria.
- `SemanticGuard.cs`: verificação conservadora de cobertura por oração, operadores de percepção e fonte, trocas entre participante e depoente nas duas direções, marcadores temporais/de certeza e primeira pessoa presente na entrada. Repara conjugações explicitamente enumeradas e a acentuação de nomes conhecidos na entrada, fora de citações. A grafia só é restaurada quando existe uma única variante na fonte; variantes distintas não são fundidas. Não reconstrói fatos nem resolve ambiguidades.
- `Apply-SemanticFix.ps1` e `Copy-MethodBody.ps1`: incorporação reproduzível, cópia de segurança e conferência dos métodos não envolvidos. Removem o prefixo do contexto do assistente; mantêm a introdução visual posterior. Retry usa original e diagnóstico, sem o rascunho incompleto. Desativam a troca posicional de saber/lembrar. O reparo de primeira pessoa passa a preservar o pronome explícito como “o depoente”, em vez de simplesmente apagar “eu”, fora de citações.
- `engine/DepoimentoLocal.dll`, `engine/DepoimentoLocal.Vulkan8.dll`, `engine/DepoimentoLocal.CPU-original.dll`: comportamento acima aplicado às três variantes. Modelo, parâmetros, divisão em blocos, inferência, interface e reparos anteriores de aspecto/quantidade preservados.
- `INICIAR.ps1`, `INICIAR-ORIGINAL.ps1`, `INICIAR-CPU.ps1`, `INICIAR-CPU-ORIGINAL.ps1`, `SHA256SUMS.txt`: somente hashes correspondentes atualizados.
- `Test-SemanticGuard.ps1`, `Test-Embedded.ps1`: testes positivos e negativos com outros nomes/cenários e verificação do comportamento realmente incorporado no motor.

### Segunda rodada conservadora

`round-1-final/Results` preserva a execução usada como referência pelo usuário. Nesta rodada, o prompt permaneceu idêntico a essa versão. `round-2-notes.md` detalha as causas e decisões; `round-2-source-hashes.json` registra os hashes antes/depois.

- `SemanticGuard.cs`: reparos locais condicionados à correspondência única com a entrada para destinatário, referente subordinado, horário aproximado, sujeito do depoente, ordem sujeito/verbo, locativo e autocorreção confirmada. Preserva o foco de “só” e “também” ao inserir o sujeito. A validação ganhou verificações de destinatário, referente e redução de intervalo. Não usa nomes nem frases fixas da bateria.
- `Apply-SemanticFix.ps1`: liga o reparo de âncoras à nova rotina e incorpora o helper nas três variantes do motor. `Copy-MethodBody.ps1` mantém o mecanismo existente.
- `Test-ReferenceRepairs.ps1` (novo), `Test-SemanticGuard.ps1` e `Test-Embedded.ps1`: 92 verificações auxiliares, incluindo idempotência, citações, ambiguidades, foco de advérbios e sujeito que é objeto de outro participante.
- `Review-Results.ps1`: aceita a execução anterior explicitamente e exige que cada revisão corresponda ao hash da saída real. `round-2-review.json`, `round-2-comparison.md` e `round-2-outputs.md` documentam a avaliação e as saídas.
- As três DLLs e `SemanticGuard.compiled.dll` refletem o helper compilado. Os quatro inicializadores e `SHA256SUMS.txt` tiveram somente hashes atualizados. Interface/launcher e runner foram compilados com os fontes disponíveis, sem alteração de seu comportamento.

As tentativas intermediárias desta rodada estão em `round-2-attempt-1/`, `round-2-attempt-2/` e `round-2-attempt-3/`. Elas não são o resultado final. A ampliação experimental de prompt foi descartada; uma alteração indevida do foco de “só” identificada na revisão manual também foi corrigida antes da execução final.

### Terceira rodada — achados da bateria inédita

`round-3-notes.md` documenta a correção dos achados de `QA-Inedita-20260929/RELATORIO-INTEGRAL.md`: primeira pessoa residual (formas em -ei/-i/irregulares presentes na entrada), destinatário do relato, dois «lhe» na mesma cadeia, sujeito elíptico do depoente, ação do depoente convertida em percepção, ação de participante atribuída ao depoente, autocorreções paralelas e rejeição lexical excessiva. O prompt não mudou. O estado anterior está em `round-3-before/`. Na execução final, 14/15 casos inéditos concluíram (antes 10/15) e a bateria anterior de 10 casos não regrediu. Os casos inéditos orientaram as regras, portanto não medem mais generalização.

## Reprodução

Na raiz da distribuição, em PowerShell 7:

```powershell
./maintenance/context-fix/Compile-CSharp.ps1 -Source ./maintenance/semantic-fix/SemanticGuard.cs -Output ./maintenance/semantic-fix/SemanticGuard.compiled.dll
./maintenance/semantic-fix/Test-SemanticGuard.ps1
./maintenance/semantic-fix/Test-ReferenceRepairs.ps1
./maintenance/semantic-fix/Apply-SemanticFix.ps1 -Apply
./maintenance/semantic-fix/Test-Embedded.ps1
./QA/Build-Application.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File ./QA/Run-QA.ps1
```

Não gerar em outra instância durante a bateria: o log original é compartilhado e não tem PID. O runner precisa acessar AppData. Cada caso usa uma instância própria. Não se alteram DLLs durante uma bateria.

## Limites

Na execução final da segunda rodada, em 29/09/2026, das 17:01 às 17:13, os dez casos concluíram tecnicamente. A revisão das dez entradas e saídas não identificou alteração de fonte, sujeito, percepção, negação, incerteza, temporalidade ou referente, nem omissão relevante, truncamento ou duplicação. Os casos 1, 2 e 5 tiveram os problemas apontados corrigidos e os outros sete mantiveram as proteções anteriores. `round-2-comparison.md` apresenta o antes/depois contra `round-1-final/Results`; `round-2-review.json` vincula cada avaliação ao hash da saída; `round-2-outputs.md` reúne as dez entradas e saídas integrais para conferência do usuário. `comparison.md` e `semantic-review.json` são o histórico da primeira rodada.

Permanecem três casos com retry (2, 5 e 9), como na rodada anterior. No caso 2, a saída aceita ainda tem alerta legado de contagem de negações: a consolidação retirou o “não” corretivo, enquanto a revisão confirmou a preservação das negativas factuais. Persistem redação coloquial, repetição estilística de “o depoente” e elipses cujo sujeito se recupera pelo contexto. A pausa inicial do caso 2 foi mantida porque a chegada não foi retratada. A aprovação se refere à fidelidade das saídas observadas, não a uma revisão estilística integral ou a uma garantia para entradas inéditas. Foram aprovadas 92 verificações auxiliares; a integridade das DLLs durante a bateria e a identidade das entradas foram conferidas. Não foi iniciada uma bateria inédita.

O validador é heurístico, não um analisador completo de português nem uma prova de equivalência semântica. Pode exigir revisão para uma paráfrase legítima e não detecta todas as trocas de pronomes ou papéis. Bloqueia perdas detectadas após um retry; o texto parcial continua visível conforme o comportamento anterior. Uma rejeição é uma proteção, não um teste semanticamente aprovado. A revisão humana comparativa continua necessária. O log nativo não expõe motivo EOS/limite de tokens.

As cópias em `backup/` são anteriores a esta intervenção. Os arquivos de manutenção antigos são histórico, não a implementação ativa; reaplicar patches antigos pode desfazer esta correção.
