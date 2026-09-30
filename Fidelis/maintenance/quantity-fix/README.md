# Preservação de quantidades explícitas

A correção em FidelityRepairs.Repair restaura grupos quantitativos explícitos (os três, as duas, os 12, os vinte e três, ambos/ambas) substituídos por eles/elas quando o restante da frase coincide e há correspondência única entre original e saída. Não altera citações, gênero divergente, negação, outros eventos ou correspondências ambíguas. Não faz substituição global de pronomes.

Compilação: usar ../context-fix/Compile-CSharp.ps1 com FidelityRepairs.cs e saída FidelityRepairs.compiled.dll; Apply-QuantityFix.ps1 -Apply atualiza apenas o método Repair nas três variantes do motor e os hashes de inicialização. Há cópias anteriores em backup-before-quantity. O executável gráfico existente carrega as DLLs atualizadas após reiniciar.

Verificação estrutural em verification.txt: prompts, geração, modelo/parâmetros, validação e demais métodos preservados. Nenhuma alteração na interface.

Testes: Test-Quantity.ps1 (16 verificações), Test-Aspect.ps1 (17 verificações) e Test-PreviousRepairs.ps1 (12 verificações) aprovados. A comparação do Teste 4 completo exige igualdade literal com a saída anterior trocando apenas a frase pedida. Aplicar a correção novamente não muda o resultado.

Teste 4: original capturado integralmente da seleção do campo aberto, incluindo 11 linhas e 1.369 caracteres; saída anterior salva em teste4-before.txt. Esperado em teste4-expected.txt. Reteste real realizado pelo executável normal, com o mesmo modelo local Vulkan/8 camadas/4 threads; ver teste4-results.txt e teste4-output.txt.
