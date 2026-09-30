# Inicialização gráfica Windows x64

O projeto DepoimentoLocal.Desktop.csproj usa OutputType=WinExe, UseWindowsForms=true, RuntimeIdentifier=win-x64, SelfContained=true, PublishSingleFile=false, PublishTrimmed=false e runtime8.0.31. DesktopProgram abre o motor gráfico e a mesma ModernShell.cs, sem BAT, PowerShell, console, download ou alteração das configurações de inferência.

## Compilação entregue

Não há SDK .NET instalado nesta máquina nem projeto-fonte do motor original. Build-Launcher.ps1 usa o compilador Roslyn disponível no PowerShell7 do ambiente de desenvolvimento e as referências .NET8 já distribuídas em engine. Compila a interface existente junto com DesktopProgram como WindowsApplication x64. O apphost gráfico x64 existente é copiado, com apenas o slot padrão de caminho gerenciado ajustado para engine/DepoimentoLocal.Desktop.dll. Os arquivos runtimeconfig/deps correspondentes ficam com o runtime em engine. O motor não é recompilado nem modificado.

A entrada normal é o Fidelis.exe da raiz (antes DepoimentoLocal.exe; o motor em engine mantém seus nomes). O EXE interno de engine continua sendo o motor e não é a entrada para o usuário.

## Publicação com SDK .NET8

Em uma máquina de desenvolvimento com SDK e os pacotes de runtime8.0.31 disponíveis, execute:

    dotnet publish launcher/DepoimentoLocal.Desktop.csproj -c Release -o launcher/bin/publish

O resultado contém o apphost gráfico DepoimentoLocal.Desktop.exe, DLL, deps, runtimeconfig e runtime Windows x64. Para atualizar a estrutura deste pacote, copie o conteúdo publicado para engine (preservando o motor DepoimentoLocal.* e runtimes existentes). Então execute no PowerShell7:

    ./launcher/Build-Launcher.ps1 -PublishedHost ./launcher/bin/publish/DepoimentoLocal.Desktop.exe

Este comando vincula o apphost da publicação à DLL em engine e grava o Fidelis.exe da raiz. Inclua modelo/ e ui/ ao distribuir a pasta completa. A publicação SDK está configurada, mas não foi executada nesta máquina sem SDK; a compilação entregue e o teste direto foram feitos pelo fluxo local descrito acima.

## Caminhos e modelo

A raiz é derivada de AppContext.BaseDirectory; não depende da pasta corrente do Explorer/atalho. A interface e o motor usam engine como diretório de trabalho. A seleção prioriza uma cópia portátil do modelo lembrado em modelo/, depois o arquivo lembrado existente, o Qwen atual empacotado e um único GGUF disponível. Ambiguidade mantém o seletor existente, sem trocar silenciosamente para outro modelo.

O GGUF copiado de Downloads foi comparado por SHA256: 626B4A6678B86442240E33DF819E00132D3BA7DDDFE1CDC4FBB18E0A9615C62D. Para contornar a limitação do backend nativo com ancestrais acentuados, passa-se o caminho relativo do GGUF a partir de engine. ModernUI.ps1 também usa esse diretório para manter compatibilidade com caminhos lembrados ao abrir pelos scripts antigos.

## Teste realizado

maintenance/launcher/Test-Launcher.ps1 iniciou diretamente DepoimentoLocal.exe a partir de outro diretório, com DOTNET_ROOT e DOTNET_ROOT_X64 inválidos e descoberta global desativada. Confirmou interface visível, modelo localizado em modelo/, coreclr carregado da pasta engine, carregamento real do GGUF pelo botão da interface e encerramento normal. launch-results.txt registra os resultados. Nenhum script de inicialização foi chamado pelo EXE. A ausência de console é garantida pelo subsistema PE Windows GUI dos dois executáveis, CreateNoWindow e ausência de processo shell no fluxo de inicialização.

Os hashes de ModernShell.cs e das DLLs ativa/Vulkan8 foram comparados antes/depois: idênticos. O teste registrou context4096, batch512, ubatch256, threads4, gpuLayers8, mmaptrue e temperature0, com MODEL_LOAD_OK.
