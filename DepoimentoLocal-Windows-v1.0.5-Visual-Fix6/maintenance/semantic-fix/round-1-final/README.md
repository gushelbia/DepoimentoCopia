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

## Reprodução

Na raiz da distribuição, em PowerShell 7:

```powershell
./maintenance/context-fix/Compile-CSharp.ps1 -Source ./maintenance/semantic-fix/SemanticGuard.cs -Output ./maintenance/semantic-fix/SemanticGuard.compiled.dll
./maintenance/semantic-fix/Test-SemanticGuard.ps1
./maintenance/semantic-fix/Apply-SemanticFix.ps1 -Apply
./maintenance/semantic-fix/Test-Embedded.ps1
./QA/Build-Application.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File ./QA/Run-QA.ps1
```

Não gerar em outra instância durante a bateria: o log original é compartilhado e não tem PID. O runner precisa acessar AppData. Cada caso usa uma instância própria. Não se alteram DLLs durante uma bateria.

## Limites

Na bateria final de 29/09/2026, os dez casos concluíram tecnicamente; a revisão semântica considerou sete fiéis nos pontos avaliados e deixou os casos 1, 2 e 5 sem aprovação integral. `comparison.md` apresenta o antes/depois completo e `semantic-review.json` vincula cada avaliação ao hash da saída. No caso 5, as trocas nominais graves e a primeira pessoa foram corrigidas, mas persistem sujeitos elípticos e a construção inadequada “Ouviu o depoente ela dizer”. O reparo de pronome preserva identidade, porém ainda não trata a ordem de sujeito pós-verbal. Nos casos 1 e 2, referentes ficaram menos claros; no caso 2 também houve redução da expressão de horário. Esta versão ainda não satisfaz integralmente a exigência de fidelidade rigorosa em todos os dez casos.

O validador é heurístico, não um analisador completo de português nem uma prova de equivalência semântica. Pode exigir revisão para uma paráfrase legítima e não detecta todas as trocas de pronomes ou papéis. Bloqueia perdas detectadas após um retry; o texto parcial continua visível conforme o comportamento anterior. Uma rejeição é uma proteção, não um teste semanticamente aprovado. A revisão humana comparativa continua necessária. O log nativo não expõe motivo EOS/limite de tokens.

As cópias em `backup/` são anteriores a esta intervenção. Os arquivos de manutenção antigos são histórico, não a implementação ativa; reaplicar patches antigos pode desfazer esta correção.
