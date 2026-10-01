# Print da janela com os alertas de papéis (frases inventadas da bateria).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -TypeDefinition ((Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8) + "`n" + (Get-Content (Join-Path $PSScriptRoot 'RolePrint.cs') -Raw -Encoding UTF8)) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = (Join-Path $root 'engine'); Set-Location -LiteralPath (Join-Path $root 'engine')
[RolePrint]::Run($root, (Join-Path $PSScriptRoot 'print-alertas.png'))