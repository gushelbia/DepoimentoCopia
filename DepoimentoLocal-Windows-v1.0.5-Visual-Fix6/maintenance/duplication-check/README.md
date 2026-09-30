# Verificação de duplicação — 27/09/2026

Compilação de diagnóstico: Build-Check.ps1 compila os fontes atuais da interface e do inicializador, sem alterar os fontes, e cria DepoimentoLocal.DuplicationCheck.exe. Usa o mesmo motor, modelo e parâmetros. O executável normal não é substituído.

Inspeção estática:
- ModernShell.cs registra Reformulate_Click uma vez; há proteção contra reentrada e botão desabilitado durante geração.
- OriginalAppBridge.Click envia WM_COMMAND OU BM_CLICK, sem disparar ambos.
- SyncTimer_Tick usa atribuição Editor.Text, não AppendText. A atribuição final repete uma substituição, não concatenação.
- Ctrl+C é nativo do RichTextBox. Menu usa ShortcutKeyDisplayString, sem registrar segundo atalho. Copiar do menu chama Editor.Copy uma vez.
- IL do motor: registro único de Reformulate_Click; limpa _reformulated no início; callbacks de streaming atualizam progresso/status; saída aceita usa set_Text(String.Join(...)). AppendText ocorre no consolidado ao adicionar trecho.
- Add_Click acrescenta o trecho ao consolidado a cada acionamento. Copy_Click copia o consolidado inteiro. Isso é diferente de Ctrl+C com foco no texto reformulado.

Evidência na janela que já estava aberta:
- Texto reformulado: uma cópia, 1.352 caracteres.
- Consolidado: duas cópias integrais do mesmo texto.
- Status: Depoimento consolidado copiado.
- Ctrl+A, Ctrl+C uma vez no reformulado, Ctrl+V uma vez em nova aba vazia do Bloco de Notas: 1.352 caracteres, sem duplicação.
- Aba anterior do Bloco de Notas: 2.706 caracteres, compatíveis com 2 x 1.352 + separador de 2 caracteres.
- Isso localiza a duplicação observada no consolidado, mas não comprova quais ações anteriores a produziram.

Teste na compilação nova:
- Carregamento real do modelo Vulkan, 8 camadas GPU, 4 threads CPU.
- Uma geração com relato da bicicleta: uma cópia de 130 caracteres na saída.
- Ctrl+A: seleção igual ao resultado completo.
- Ctrl+C uma vez; nova aba vazia do Bloco de Notas; Ctrl+V uma vez: texto visualmente idêntico, 130 caracteres.

Resultado: duplicação por Ctrl+C/Ctrl+V não reproduzida. Não foi aplicada deduplicação automática nem alterado prompt, modelo, regras, parâmetros ou código de produção.

Segunda geração na mesma instância: relato diferente (mercado), saída única e sem o texto da bicicleta. Substituição confirmada. A qualidade linguística da resposta não foi avaliada nem alterada neste teste.
