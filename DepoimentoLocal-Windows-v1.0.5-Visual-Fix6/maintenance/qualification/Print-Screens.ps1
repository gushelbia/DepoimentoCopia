# Prints da janela para aprovação (painel de qualificação fechado e aberto,
# temas claro e escuro, janela estreita). Dados fictícios. A janela aparece na
# tela por alguns segundos.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/qualification/Print-Screens.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'PrintScreens.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
[PrintScreens]::Run($root, $PSScriptRoot)
