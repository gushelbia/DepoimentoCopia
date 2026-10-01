# Alertas de papéis e pronomes (laranja, «conferir»)

Mudança só na interface (`ui/ModernShell.cs`, classe `RoleScanner`). O motor, o prompt e o texto gerado não mudam. A marcação é só visual, como os destaques de datas e horários, e nada chega ao texto copiado, ao .txt ou ao Word.

Ela vem do diagnóstico em `maintenance/pronoun-diagnosis/DIAGNOSTICO.md`: o modelo pequeno troca quem fez ou disse o quê ao passar o «eu» para «o depoente».

## O que é marcado no texto reformulado

| Alerta | Exemplo (bateria) | Por quê |
|---|---|---|
| Quem fez ou disse | «o depoente **lhe disse**» para «Ele **me** disse» | A ação era dirigida ao depoente e virou uma ação dele sobre outra pessoa |
| Reflexivo | «o depoente só **lhe defendeu**» para «eu só **me defendi**» | «me defendi» é «se defendeu» |
| Frase quebrada | «ele estava **o depoente enforcando**» | «o depoente» no meio da locução verbal |
| «lhe» com verbo de ação | «ela **lhe atacou**», «Marcos **lhe segurou**» | Com verbo de ação direta, o «me» do original vira «lhe»: erro de regência e ambíguo |
| Pronome ambíguo | «ele **o atacou**», «quando o depoente chegou, **ele já estava** caído» | «o» ou «ele» pode ser lido como outra pessoa ou como o próprio depoente |
| Sujeito (aviso, sem cor) | «**Relatou que o depoente**…» | «Relatou» fica sem sujeito e parece falar de outra pessoa (a expressão vem do motor) |
| Gênero trocado | «o depoente … **nervoso**» para «fiquei **nervosa**» | O original indica mulher |
| Gênero presumido (aviso, sem cor) | «**o depoente**» quando o original não diz o gênero | «o depoente» supõe homem |

**Cores.** Pintam o texto de laranja: quem fez ou disse (incluindo o reflexivo), frase quebrada, «lhe» com verbo de ação, pronome ambíguo e gênero trocado (o original indica o gênero e o texto contradiz, como «nervosa» → «nervoso»). Sujeito («Relatou que o depoente») e gênero presumido são **avisos informativos**: não pintam o texto e aparecem só na contagem e na lista, em seção própria. Assim o laranja fica reservado ao que muda quem fez o quê.

Alertas que se sobrepõem viram uma única marca, com o motivo mais grave primeiro; os avisos informativos são agrupados à parte. A barra de status mostra «N alertas de papéis e pronomes; M avisos informativos». Clicar nela lista primeiro os alertas, depois os avisos, cada um com o motivo e a forma clara sugerida.

Alguns casos não são marcados:
- falas entre aspas;
- «lhe» com verbos que pedem destinatário («disse», «deu», «contou», «apresentou»…);
- «lhe» quando o original não tem «me».

Com «estava» e «era», que também servem para «ele/ela», o gênero só conta quando há «eu» explícito. Assim, «a porta estava fechada» não indica uma depoente mulher.

## Teste

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/role-alerts/Test-RoleAlerts.ps1

O teste cobre:
- as 20 frases inventadas da bateria de diagnóstico, com as saídas reais do 3B e os alertas esperados em cada uma;
- a exigência de que os 8 casos com erro de sentido tenham alerta de sentido;
- versões corrigidas sem falso alarme, fala entre aspas e textos vazios;
- as 10 saídas aprovadas da bateria QA, que não podem receber alerta de sentido (inversão, reflexivo, frase quebrada ou «lhe»);
- o resumo, a lista e a cor laranja.

`maintenance/review-highlight/Test-HighlightUi.ps1` confere na janela real que o alerta é pintado em laranja no reformulado e aparece na lista ao clicar.

## Limites

São padrões, não uma análise completa do português. Uma troca de papéis com outra forma pode passar sem marca, e um alerta pode não ser erro. A leitura atenta continua necessária.
