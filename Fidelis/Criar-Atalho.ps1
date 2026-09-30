# Cria (ou atualiza) o atalho «Fidelis» na Área de Trabalho, apontando para o
# Fidelis.exe desta pasta. Rode de novo se mover ou renomear a pasta.
#   Dois cliques em Criar-Atalho.bat, ou:
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File Criar-Atalho.ps1 [-MenuIniciar] [-Destino pasta]
param([switch]$MenuIniciar, [string]$Destino)
$ErrorActionPreference = 'Stop'
$pasta = $PSScriptRoot
$exe = Join-Path $pasta 'Fidelis.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Fidelis.exe não encontrado em $pasta" }
$icone = Join-Path $pasta 'ui\Fidelis.ico'
if (-not (Test-Path -LiteralPath $icone)) { $icone = Join-Path $pasta 'ui\DepoimentoLocal.ico' }
$destinos = @([Environment]::GetFolderPath('Desktop'))
if ($Destino) { $destinos = @($Destino) }
elseif ($MenuIniciar) { $destinos += (Join-Path ([Environment]::GetFolderPath('Programs')) '') }
$shell = New-Object -ComObject WScript.Shell
foreach ($destino in $destinos) {
    $caminho = Join-Path $destino 'Fidelis.lnk'
    $atalho = $shell.CreateShortcut($caminho)
    $atalho.TargetPath = $exe
    $atalho.WorkingDirectory = $pasta
    $atalho.Description = 'Fidelis'
    if (Test-Path -LiteralPath $icone) { $atalho.IconLocation = $icone + ',0' }
    $atalho.Save()
    Write-Output ('Atalho criado: ' + $caminho + ' -> ' + $exe)
}