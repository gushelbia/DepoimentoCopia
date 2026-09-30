# Atualização exclusiva dos prompts

Nota posterior: a correção de contexto está documentada em
`../context-fix/README.md`. Ela mantém o prompt principal e usa um retry compacto.
As validações linguísticas históricas abaixo não são testes dessa correção.

Os prompts de primeira tentativa (`PromptFactory.Build` / `SystemPrompt`) e
de correção (`PromptFactory.BuildRetry` / `RetryPrompt`) foram atualizados nas
três DLLs da distribuição: ativa, Vulkan8 e CPU-original.

`SystemPrompt.txt` e `RetryPrompt.txt` são cópias legíveis dos textos embutidos.
O programa não lê esses arquivos durante a execução. A atualização já está
aplicada; basta abrir normalmente pelo inicializador habitual.

Incluem terceira pessoa mesmo sem pronome, preservação de palavras e sentido,
consolidação de autocorreções explícitas, preservação de incertezas e memória,
origem das informações de terceiros e revisão silenciosa dos cinco pontos.
A resposta solicitada contém somente o depoimento reformulado.

O reforço posterior sobre origem da informação acrescenta a proibição explícita
de trocar autor, testemunha, informante e depoente, de transformar relato indireto
em confissão/admissão/observação direta, e de alterar a cadeia de origem. Inclui
os três exemplos obrigatórios (Carlos/Paulo, Maria/João e diretor), a análise do
caso do grito e quatro perguntas de revisão silenciosa para cada informação.
Foi aplicado e verificado nas versões ativa, CPU e Vulkan. Os textos anteriores
a esse reforço estão em `SystemPrompt-before-origin-rules.txt` e
`RetryPrompt-before-origin-rules.txt`.

## Escopo verificado

- Nenhuma alteração no arquivo GGUF, na interface ou nos parâmetros de geração.
- Instruções dos métodos, variáveis locais, tratamento de exceções e demais
  constantes comparados com as DLLs originais: somente os dois literais de
  prompt e suas duas constantes foram alterados.
- Os quatro inicializadores receberam somente o novo hash esperado da DLL.
  O manifesto SHA256SUMS foi atualizado para os arquivos afetados.
- Originais preservados em `backup-before-prompt`.
- `verification.txt` registra a comparação estrutural.

## Validação real e limitações

`Test-Prompt.ps1`, executado pelo Windows PowerShell 5.1 em STA, compila a
interface existente e executa quatro exemplos no modelo local configurado,
usando os mesmos controles e motor do programa. `examples-results.txt` registra
integralmente a execução da versão ANTERIOR ao reforço sobre origem da informação.
O reforço atual teve validação estrutural dos prompts embutidos e da preservação
da lógica, da interface e dos hashes; não foi feita nova avaliação de geração.

Os testes NÃO passaram integralmente nos critérios linguísticos:

- Caso 1: terceira pessoa e incertezas preservadas, porém ainda houve
  substituições desnecessárias de palavras.
- Caso 2: a saída de "Quer dizer, acho que era o Paulo" ficou incorreta
  ("ialhe era o Paulo"), sem preservar adequadamente a incerteza.
- Caso 3: a autocorreção ainda produziu expressão inexistente ("ousentiu").
- Caso 4: preservou a origem dos relatos, o autor da ação, "encostar" e a
  negativa de percepção direta.

Portanto, os requisitos estão explicitamente no prompt, mas esta alteração
sozinha ainda não demonstrou eliminar os erros de saída. Nenhum pós-processamento,
validador, executor, limite de contexto ou parâmetro foi modificado para contornar
essas falhas. Os registros não constituem aprovação integral da qualidade das
reformulações.
