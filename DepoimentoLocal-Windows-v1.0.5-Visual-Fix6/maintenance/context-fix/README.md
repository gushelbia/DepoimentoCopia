# Correção de overflow no retry — versão interna 0.1.9.0

Aplicada nas DLLs ativa, Vulkan8 e CPU da distribuição.

- ContextSize: 2048 → 4096; log MODEL_CONFIG atualizado.
- Prompt principal preservado integralmente. Retry separado e compacto, com o
  problema detectado, original completo, rascunho completo e regras de fidelidade.
- Estratégia explicitamente ThrowException. Nenhum truncamento automático.
- ContextBudget.cs foi compilado com Roslyn contra as referências .NET 8 do
  programa, e seu método foi incorporado nas DLLs. Antes de cada geração,
  conta os tokens com o tokenizador do modelo, reserva MaxTokens + 32 tokens e
  rejeita uma solicitação que não cabe, informando que nada foi cortado.
- O LLamaSharp instalado já cria um LLamaContext novo por InferAsync, descarta
  o anterior e reinicia a amostragem. Esse fluxo foi preservado e testado.
- Validação automática, temperature=0, gpuLayers=8 na versão Vulkan, fallback
  CPU com gpuLayers=0, modelo GGUF e interface preservados.
- Hashes dos inicializadores e manifesto atualizados. O atualizador de prompts
  passou a usar a DLL atual como base para não desfazer esta correção futuramente.

## Compilação e verificação

Não há solução/projeto-fonte completo do motor nesta distribuição. O novo
método C# foi compilado para o runtime existente; as DLLs foram reconstruídas
com dnlib. O script compara os demais métodos/campos com o backup, incluindo
todo o validador, e interrompe se houver mudanças fora do escopo.

`verification.txt` registra a verificação estrutural. Backups anteriores à
correção estão em `backup-before-context-fix`.

## Testes executados

`regression-results.txt` registra testes REAIS com o modelo local, usando um
texto sintético identificado como tal:

- Primeira geração, retry e repetição do retry sem overflow.
- Contextos distintos, descartados ao concluir cada chamada.
- Resultado idêntico ao repetir o mesmo retry determinístico.
- Validador ainda detecta a troca de "não sabe" por "não se lembra".
- Entrada de 9214 tokens incluindo reservas rejeitada antes da inferência,
  sem truncamento; nova chamada após essa rejeição funciona.
- O modelo ainda respondeu incorretamente ao caso semântico sintético; isso
  permaneceu detectado pelo validador. Ausência de overflow não implica aprovação
  semântica do resultado.

`budget-probe.txt` mede outro caso SINTÉTICO com os tamanhos do log (original
1155 caracteres, rascunho 1076, reserva de resposta 465 tokens):

| Chamada | Prompt completo | Resposta + segurança | Total | Folga em 4096 |
| --- | ---: | ---: | ---: | ---: |
| Primeira | 1530 | 465 + 32 | 2027 | 2069 |
| Retry | 1192 | 465 + 32 | 1689 | 2407 |

4096 é suficiente para esses testes, portanto não foi adotado 8192. Os mesmos
números de caracteres não garantem a mesma tokenização em outros textos; a
checagem usa tokens reais em cada execução.

## Teste exato concluído em 27/09/2026

O usuário forneceu o original, salvo em `exact-original.txt`, sem alteração
do conteúdo. Os espaços externos foram tratados com Trim, como no programa:
1155 caracteres e MaxTokens=465, iguais aos da ocorrência no log anterior.

O teste direto dos métodos reais registrou contexto novo e descarte ao terminar
cada geração. O teste separado pelo botão real da interface compilou o fonte
existente e executou o fluxo automático completo, sem invocar o retry manualmente.

| Chamada real | Prompt completo | Resposta + segurança | Total | Folga em 4096 |
| --- | ---: | ---: | ---: | ---: |
| Primeira | 1522 | 465 + 32 | 2019 | 2077 |
| Retry | 1145 | 465 + 32 | 1642 | 2454 |

O fluxo registrou BLOCK_RETRY_START, BLOCK_RETRY_END e BLOCK_REJECTED. O retry
terminou em aproximadamente 44 segundos sem ContextOverflowException. 4096 é
suficiente para esse caso exato; não é necessário aumentar para 8192.

O resultado semântico NÃO foi aprovado: persistiu o erro de sentido em
"não sei"/"não sabe", detectado e rejeitado pelo validador existente. A correção
de capacidade não elimina esse erro linguístico. A primeira geração e o retry
produziram 1076 caracteres brutos, e o programa manteve o texto para revisão.

Evidências: `exact-text-results.txt`, `automatic-retry-results.txt`,
`automatic-retry-log.txt` e `automatic-retry-output.txt`.

Para repetir o teste direto, a partir da pasta engine:

```powershell
dotnet exec --runtimeconfig DepoimentoLocal.runtimeconfig.json --depsfile DepoimentoLocal.deps.json ContextRegressionExact.dll ..\maintenance\context-fix ..\maintenance\context-fix\exact-original.txt
```

Para repetir pela interface, execute `Test-AutomaticRetry.ps1` pelo Windows
PowerShell 5.1 em STA. O teste encerra somente a instância que abriu, após salvar
a saída e o log, inclusive se o validador rejeitar o resultado.

Nota posterior (27/09/2026): o novo teste após a correção de tempo verbal/autocorreção foi aceito na primeira geração, com aviso não bloqueante de negação. Os arquivos automatic-retry-* nesta pasta foram atualizados por essa execução. O resultado semântico anterior permanece em exact-text-results.txt; a nova evidência completa está em ../tense-correction/.
