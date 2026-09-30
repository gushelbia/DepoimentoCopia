# Menus dos campos — 27/09/2026

Alteração somente em ui/ModernShell.cs (SectionCard e renderização dos menus). Os três campos compartilham esse componente: Resposta e Depoimento consolidado são editáveis; Texto reformulado é somente leitura.

ContextMenuStrip contém Recortar/Copiar/Colar/Selecionar tudo. A abertura atualiza disponibilidade pela seleção, ReadOnly e conteúdo textual da área de transferência. Recortar/Colar ficam ocultos no campo somente leitura e os handlers também verificam ReadOnly. Comandos usam os métodos nativos do RichTextBox. ShortcutsEnabled continua true; os menus apenas exibem as combinações, sem capturar atalhos globais. Placeholder e margem do editor recebem o mesmo menu. As cores vêm da paleta atual, incluindo texto desabilitado, e o menu é descartado junto com o componente.

Compilação e teste: Windows PowerShell 5.1 STA, mesmo Add-Type e referências utilizados por ModernUI.ps1. O teste instancia os três componentes reais em uma janela de teste, envia mensagens nativas de botão direito (down/up) e percorre o processamento de teclado Ctrl+C/X/V/A do controle. Não depende de geração nem carrega modelo.

PASS nos seis cenários (três campos x claro/escuro): abertura por botão direito, preservação da seleção, cópia, corte e colagem nos editáveis, substituição da seleção, selecionar tudo, menus ocultos e proteção de escrita no somente leitura, atalhos e estado vazio. results.txt contém os resultados. Estado do teclado da thread e área de transferência são restaurados pelo teste.

As três DLLs do motor foram verificadas por SHA256 contra engine-before.json e permanecem idênticas. Não houve alteração de prompt, modelo, inferência ou layout. O hash da fonte de interface foi atualizado no SHA256SUMS.txt. A interface é compilada na abertura normal do programa; basta reabrir pelo INICIAR.bat.

Reproduzir na raiz do pacote:

powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/context-menu/Test-ContextMenu.ps1
