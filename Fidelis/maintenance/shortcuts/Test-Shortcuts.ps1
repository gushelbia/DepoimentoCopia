# Atalhos de teclado e bloqueio da formatação nativa do RichTextBox.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/shortcuts/Test-Shortcuts.ps1
# Usa o motor real e carrega o modelo (Ctrl+Enter e Esc). Teclas simuladas só na
# thread do teste. Dados fictícios; arquivos numa pasta temporária apagada no fim.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$kit = Get-Content (Join-Path $root 'maintenance/dialogs/DialogKit.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'ShortcutTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $kit + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
$work = Join-Path $env:TEMP ('DepoimentoLocal-atalhos-' + [guid]::NewGuid().ToString('N'))
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
try { $failures = [ShortcutTest]::Run($root, $work) }
finally { Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue }
if ($failures -ne 0) { exit 1 }