# Preservação de aspecto verbal — Teste 3 (27/09/2026)

Alteração limitada à rotina local FidelityRepairs.Repair, já chamada antes da validação na primeira geração e no retry. Os prompts e todas as demais rotinas do motor permaneceram iguais. Não foram alterados modelo, parâmetros, interface, executável de entrada ou número de chamadas à IA.

A nova etapa restaura a construção com tinha/havia + particípio quando a saída a simplificou para imperfeito ou perfeito simples e há correspondência inequívoca com a frase original. Abrange os quatro exemplos solicitados, formas regulares e alguns particípios irregulares comuns, inclusive plural. A comparação exige o restante da frase equivalente, tolerando pontuação, artigos e conversão me/lhe. Fonte, sujeito, negação e marcadores temporais não são descartados na comparação. Contagens divergentes da mesma ação, candidatos repetidos ou contextos diferentes impedem a substituição. Citações literais são excluídas. Não é um analisador semântico geral: os casos sem correspondência segura permanecem sob as regras e validação existentes.

## Compilação e aplicação

FidelityRepairs.cs contém também as correções locais anteriores, preservadas. Compile-CSharp.ps1 compilou o helper com referências .NET8 do pacote. Apply-AspectFix.ps1 incorporou somente esse método nas DLLs ativa, Vulkan8 e CPU. A comparação dos métodos/constantes restantes passou nas três DLLs (verification.txt). Launchers legados e manifesto SHA256 foram atualizados. O launcher gráfico já existente usa as DLLs atualizadas, sem precisar ser recompilado.

## Teste real

Test-Test3.ps1 abriu diretamente DepoimentoLocal.exe, carregou o GGUF local e inseriu o texto integral fornecido pelo usuário: 1195 caracteres, quatro parágrafos, sem abreviações, ajustes ou texto adicional. O teste conferiu o texto inserido; fixture.json registra seu hash. A geração terminou em 49,903 segundos, aceita pelo validador, sem retry e sem overflow. O log registrou context4096, batch512, ubatch256, threads4, gpuLayers8, mmaptrue e temperature0.

A saída contém “Ana lhe disse que Roberto tinha batido a porta quando entrou na sala.” e “Ana falou que ele tinha dito que não voltaria para aquela sala.” Preservou as versões divergentes sobre a porta, sala/reunião, ordem da saída e mesmo dia/dia seguinte, bem como fontes, incertezas, limitações de percepção e terceira pessoa. As 16 verificações do resultado estão em teste3-fidelity-results.txt. A saída completa está em teste3-output.txt e o log em teste3-log.txt.

Também passaram 17 casos locais de aspecto verbal (incluindo falsas correspondências que devem ficar intactas) e as 12 verificações anteriores de presente/passado, autocorreção e fonte. Na saída real, uma regressão simulada de “tinha batido” para “batia” foi reparada sem alterar qualquer outro trecho. O processamento local completo levou média de 0,452 ms em 100 execuções; não há inferência adicional.

Para repetir: compile o helper usando maintenance/context-fix/Compile-CSharp.ps1; execute Test-Aspect.ps1 e Test-PreviousRepairs.ps1; aplique Apply-AspectFix.ps1 -Apply com o programa fechado; execute Test-Test3.ps1 no Windows PowerShell5.1 STA e Assert-Test3.ps1 no PowerShell7. Os arquivos de backup preservam o estado anterior a esta correção.
