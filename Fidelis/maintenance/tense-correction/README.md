# Correção de tempo verbal e autocorreção — 27/09/2026

Correção compilada em C# com referências do .NET 8 distribuído com o programa e incorporada nas três DLLs (ativa, Vulkan8 e CPU). Como esta distribuição não contém a solução/projeto completo do motor, a compilação foi do helper e a integração foi realizada com dnlib. O script Apply-FidelityFix.ps1 guarda as DLLs anteriores e confere que os demais métodos e constantes permanecem iguais.

## Escopo

FidelityRepairs.Repair compara o original com a saída antes da validação, tanto na primeira resposta quanto no retry. Mantém presente/passado dos estados de conhecimento, memória, opinião e possibilidade. Só alinha grupos com igual número de ocorrências; evita citações e categorias distintas. Consolida o padrão de autocorreção confirmada de saída/entrada usado no Teste 2, com nome extraído do original (não fixado em Carlos). Não é um analisador semântico geral para todas as formas possíveis de autocorreção.

A tentativa de resolver apenas com reforço do prompt não funcionou no teste real. Os prompts anteriores foram restaurados, preservando as regras já existentes. Não houve mudança de modelo, interface, temperature=0, GPU8, contexto4096, orçamento de tokens, estratégia de overflow ou validador. A correção local não acrescenta inferências.

## Verificação

- 12 verificações locais passaram: presente, passado explícito, citações, correspondência ambígua, categorias distintas, confirmação, nomes distintos, fonte de terceiro e saída anterior do Teste 2.
- Execução real pela interface com o mesmo original de 1155 caracteres e modelo Qwen2.5-3B existente.
- GENERATION_OK em 52,061 segundos, sem retry e sem ContextOverflowException.
- Resultado: “não sabe o que ela quis dizer”, “não sabe se foi no mesmo dia” e “Carlos entrou antes de sua saída.”
- Fonte preservada: “Carlos lhe disse depois que Paulo gritou com ela, mas não ouviu esse grito.”
- O validador manteve um aviso não bloqueante: “uma negação pode ter sido omitida”. O texto foi aceito com esse aviso; não se afirma ausência de todos os possíveis problemas linguísticos.
- Prompt1522 + resposta465 + reserva32 = 2019 tokens; margem2077 no contexto4096.
- Interface ModernShell.cs com hash inalterado F2172D047DC5ADE4403FD666AC939FCCA618E8118322CBB37D691047082209F9.

Evidências: unit-results.txt, teste2-results.txt, automatic-retry-output.txt e automatic-retry-log.txt. verification.txt registra comparação das DLLs. Os launchers e SHA256SUMS.txt foram atualizados para as DLLs aplicadas.

## Reproduzir

Na raiz do pacote, compile com maintenance/context-fix/Compile-CSharp.ps1 usando Source maintenance/tense-correction/FidelityRepairs.cs e Output maintenance/tense-correction/FidelityRepairs.compiled.dll. Execute Test-FidelityRepairs.ps1 e Apply-FidelityFix.ps1 -Apply. Execute maintenance/context-fix/Test-AutomaticRetry.ps1 pelo Windows PowerShell 5.1 em STA e então maintenance/tense-correction/Assert-Test2.ps1 pelo PowerShell 7.
