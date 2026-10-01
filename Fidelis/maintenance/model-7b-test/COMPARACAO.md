# Qwen2.5 3B (atual) × Qwen2.5 7B — 30/09/2026

## O que foi testado

- **7B:** `Qwen/Qwen2.5-7B-Instruct-GGUF` (repositório oficial da Qwen no Hugging Face, licença Apache 2.0), Q4_K_M em 2 partes. A SHA256 de cada parte confere com a publicada. Está em `D:\Fidelis-Modelos\qwen2.5-7b-instruct-q4_k_m\` e ocupa **4,36 GB** (4.466 MB).
- **Mesmo motor, mesmo prompt, mesmos reparos e validador.** Só o caminho do modelo muda, e só no runner de teste (`Run-Model.ps1`).
- **O programa continua com o 3B.** O `last-model.txt` foi conferido depois da execução. Os hashes das DLLs do motor não mudaram.
- **Máquina:** Ryzen 5 3400G (4 núcleos / 8 threads), Vega 11 integrada (2 GB), 13,9 GB de RAM, 8 camadas na GPU.
- **Dados:**
  - 3B: `maintenance/pronoun-diagnosis/resultados-20-frases.json` e `QA/Results/test-XX.json`;
  - 7B: `frases-7b.json` e `qa-7b.json` nesta pasta.

## Bateria de 20 frases inventadas

Legenda:
- **Sentido:** ✓ correto; ⚠ ambíguo; ✗ errado.
- **Sujeito:** ✗ = «Relatou que o depoente…».
- **Gênero:** ⚠ = masculino presumido; ✗ = errado; ✓ = certo ou neutro.

| # | Gên. | Sentido 3B | Sentido 7B | Suj. 3B | Suj. 7B | Gên. 3B | Gên. 7B | Tempo 3B | Tempo 7B |
|---|---|---|---|---|---|---|---|---|---|
| 1 | ? | ⚠ «ele o atacou» | ⚠ «ele o atacou» (igual) | ✗ | ✗ | ⚠ | ⚠ | 12,2 s | 40,2 s |
| 2 | ? | ✓ | ✓ | ✗ | ✗ | ⚠ | ⚠ | 11,0 s | 36,9 s |
| 3 | ? | ✗ **invertido** (bloqueado) | ✓ «ele bateu o depoente» | ✗ | ✓ | ⚠ | ⚠ | 20,6 s | 29,0 s |
| 4 | ? | ⚠ «ele já estava caído» | ⚠ (igual) | ✗ | ✗ | ⚠ | ⚠ | 11,3 s | 22,0 s |
| 5 | ? | ✗ frase quebrada | ✗ **«me enforcando» → «empurrando o depoente»** (verbo trocado) | ✗ | ✗ | ⚠ | ⚠ | 10,2 s | 21,4 s |
| 6 | ? | ✓ («lhe xingou») | ✓ («lhe xingou») | ✓ | ✓ | ⚠ | ⚠ | 19,9 s | 22,2 s |
| 7 | ? | ✗ **invertido** | ✓ «ele lhe disse» | ✗ | ✓ | ⚠ | ✓ | 11,1 s | 19,8 s |
| 8 | ? | ✓ («lhe segurou») | ✓ «segurou o braço do depoente» | ✓ | ✓ | ⚠ | ⚠ | 11,2 s | 23,9 s |
| 9 | ? | ✓ | ✓ | ✗ | ✗ | ⚠ | ⚠ | 10,7 s | 21,0 s |
| 10 | ? | ✗ **invertido** | ✓ «ela lhe contou» | ✗ | ✓ | ⚠ | ✓ | 10,9 s | 20,9 s |
| 11 | F | ⚠ «ele o chutou» | ⚠ «ele a chutou» com «o depoente» | ✗ | ✗ | ✗ | ✗ (parcial) | 11,0 s | 19,9 s |
| 12 | F | ✓ | ✓ | ✗ | ✗ | ✗ «nervoso» | ✗ «nervoso» | 10,8 s | 20,0 s |
| 13 | M | ✓ | ✗ **«o pai da menina foi buscá-la»** (perdeu que o depoente é o pai) | ✗ | ✓ | ✓ | ✓ | 11,7 s | 19,8 s |
| 14 | ? | ✓ («lhe ameaçou») | ✓ «ameaçou o depoente» | ✓ | ✓ | ⚠ | ⚠ | 22,4 s | 21,9 s |
| 15 | ? | ✓ | ✓ | ✗ | ✗ | ⚠ | ⚠ | 11,5 s | 20,8 s |
| 16 | ? | ✓ | ✓ («lhe cercaram») | ✗ | ✓ | ⚠ | ✓ | 12,1 s | 22,1 s |
| 17 | ? | ✓ | ✓ | ✗ | ✗ | ⚠ | ⚠ | 10,8 s | 20,4 s |
| 18 | ? | ✗ «lhe defendeu» | ✓ «atacou o depoente … se defendeu» | ✓ | ✓ | ⚠ | ⚠ | 20,7 s | 22,2 s |
| 19 | F | ✓ | ✓ | ✗ | ✗ | ✗ «agredido» | ✗ (parcial: «o depoente foi agredida») | 11,4 s | 22,7 s |
| 20 | M | ✓ | ✓ | ✗ | ✗ | ✓ | ✓ | 11,3 s | 20,8 s |

| Total | 3B | 7B |
|---|---|---|
| Erros graves de sentido | **5** (4 inversões + 1 frase quebrada) | **2** (5: verbo trocado; 13: identidade perdida), ambos **novos** |
| Ambíguos | 3 (1, 4, 11) | 3 (1, 4, 11) |
| Sujeito («Relatou que o depoente») | 16 | 11 |
| Gênero: mulheres | 3 de 3 erradas | 3 de 3 erradas (2 parcialmente) |
| Gênero: sem gênero informado | 15 de 15 no masculino | 12 de 15 no masculino |
| Tempo total | 263 s | 468 s (**1,8×**) |

## Bateria QA (10 casos)

| # | Caso | Status 3B | Status 7B | Sentido 3B | Sentido 7B | Suj. 3B | Suj. 7B | Gênero (os dois) | Tempo 3B | Tempo 7B |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Linguagem coloquial | sem alertas | **ERRO** (validador rejeitou) | ✓ | ✓ texto fiel, mas saída marcada **incompleta**; mantém «tava», «pra» | ✗ | ✗ | ✓ (homem explícito) | 63,4 s | 188,4 s |
| 2 | Autocorreções | com alertas | com alertas (negação) | ✓ | ✓ (mantém «tava», «Aí») | ✓* | ✗ | ⚠ | 132,7 s | 107,3 s |
| 3 | Versões conflitantes | sem alertas | sem alertas | ✓ | ✓ | ✗ | ✗ | ⚠ | 74,8 s | 97,8 s |
| 4 | Pronomes ambíguos | sem alertas | sem alertas | ✓ | ✓ | ✗ | ✗ | ✓ (homem) | 74,8 s | 117,7 s |
| 5 | Texto longo | com alertas | com alertas | ✓ | ✗ **«Não contou…» (Eduardo) → «O depoente não soube…»**; erro de grafia «presteu» | ✗ | ✗ | ⚠ | 315,6 s | 546,4 s |
| 6 | Conhecimento e incerteza | sem alertas | sem alertas | ✓ | ✓ | ✗ | ✗ | ✓ (homem) | 36,3 s | 56,4 s |
| 7 | Falas entre aspas | sem alertas | sem alertas | ✓ | ✓ (idêntico) | ✗ | ✗ | ⚠ | 30,4 s | 53,9 s |
| 8 | Nomes e cargos | sem alertas | sem alertas | ✓ | ✓ (idêntico) | ✗ | ✗ | ⚠ | 25,9 s | 45,2 s |
| 9 | Ambiguidade temporal | com alertas | sem alertas | ✓ | ✓ (idêntico) | ✗ | ✗ | ⚠ | 49,2 s | 49,0 s |
| 10 | Negação | sem alertas | sem alertas | ✓ | ✓ (idêntico) | ✗ | ✗ | ⚠ | 33,5 s | 53,8 s |
| | **Total** | 0 erros | 1 ERRO | 0 | 1 | 9 | 10 | igual | **13,9 min** | **21,9 min (1,6×)** |

\* No caso 2, o 3B escreveu «Relatou que então chegou…», sem «o depoente» logo depois.

## Conclusão

- **Nas frases curtas, o 7B acertou quem fez ou disse o quê**, e as 4 inversões do 3B desapareceram. Por outro lado, criou 2 erros graves novos (verbo trocado e identidade perdida) e 1 na bateria QA (sujeito trocado no caso 5). Também teve 1 caso rejeitado pelo validador, que foi calibrado para o 3B.
- **Sujeito e gênero quase não mudam.** Vêm do motor (o «Relatou que» fixo e o «o depoente» do prompt e dos reparos), não do tamanho do modelo.
- **Tempo:** 1,6 a 1,9 vez mais lento. Uma frase curta passa de ~11 s para ~21 s; o texto longo da QA, de ~5,3 min para ~9,1 min.
- **Recomendação:** não vale trocar o modelo agora. Os ganhos reais (inversões) vieram junto com erros novos e com incompatibilidade com o validador. Sujeito e gênero exigem mexer no motor de qualquer forma (soluções C e E). O aviso de papéis (solução A) marca as inversões e ambiguidades dos dois modelos. Os erros novos do 7B (verbo trocado, identidade perdida) não são padrões de papel e **passariam sem marca**.
