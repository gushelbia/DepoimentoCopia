# Gênero do depoente (opção 1 do plano: conversão na interface)

O motor continua gerando no masculino («O depoente relatou que…»). Com o campo **Gênero do depoente = Feminino**, a interface converte o texto recebido para o feminino, usando o original como guia (`GenderConverter` em `ui/ModernShell.cs`). Com **Masculino** ou **Não informado**, nada é convertido: a tela mostra exatamente o texto do motor.

## O que converte

1. «o/O/do/no/ao/pelo depoente» → «a/A/da/na/à/pela depoente».
2. Palavras que o original põe no feminino para a narradora («fiquei **nervosa**», «eu estava **sozinha**»), só na oração da depoente, sem outro sujeito no meio.
   - «estava», «era» e «andava» só contam com «eu» antes, porque também são terceira pessoa («minha vizinha estava assustada» não é a depoente).
3. Caso F9: um particípio logo depois de «a depoente» + «foi/ficou/estava…» vai para o feminino («foi empurrad**a**»).
   - Exceção: se o original usa essa palavra para outra pessoa («**Ele** estava nervoso»), ela fica como está e aparece um aviso informativo.
4. «ele **o** ameaçou» → «ele **a** ameaçou», quando o original tem «ele **me** ameaçou».

As falas entre aspas não mudam. Converter duas vezes dá o mesmo resultado.

## Na dúvida, não converte e mostra um aviso informativo (no painel de revisão, sem pintar o texto de laranja)

- forma masculina que pode ser da depoente, mas com outro sujeito perto;
- palavra que no original é de outra pessoa;
- «dele» quando o original tem «de mim».

## Onde vale o texto convertido

Na tela, no consolidado («Adicionar»), no rascunho, na recuperação e no Word.

Ao trocar o campo:
- o reformulado é reconvertido, se não foi editado à mão;
- o consolidado nunca é reescrito.

## Sugestão «Usar Feminino/Masculino»

- Aparece só com o campo em Não informado e com uma pista clara no original («fiquei nervosa», «sou a mãe», «posso estar enganado»).
- Só muda o campo quando o usuário clica.

## Testes (frases inventadas)

| Script | O que confere |
|---|---|
| `Test-GenderConverter.ps1` | Conversões, termos de outras pessoas, aspas, idempotência, avisos, alertas com «a depoente», sugestão, identidade das 20 frases |
| `Test-GenderUi.ps1` (STA, modelo real) | Campo, sugestão, tela, consolidado, recuperação, rascunho, Word, troca de gênero, Masculino e Não informado idênticos ao motor |
| `Run-GenderBattery.ps1` (STA, modelo real) | `frases-genero.tsv`: mulheres (F), homens (M) e sem gênero (N), e mais a identidade dos 30 textos aprovados (20 frases + 10 QA) |
| `Print-Gender.ps1` | Prints da sugestão e do texto convertido |

## Pendências guardadas na bateria (para a próxima correção)

São inversões do próprio modelo, que acontecem também com homens:

- **`pendente-papel`**:
  - «Ele disse que **eu era culpada**» → «disse que ele era culpado»;
  - «O Carlos entrou **depois de mim**» → «entrou depois do Carlos».
- **`pendente-sujeito`**: o modelo apaga o sujeito de outra pessoa, e o fato passa para o depoente.
  - «**Ele** estava nervoso» → «relatou que estava nervoso»;
  - «**Meu marido** foi preso» → «relatou que foi preso».
  - Nenhum alerta pega esse caso hoje.
