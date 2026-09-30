# Teste de interface: layout lado a lado e destaques de conferência.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/review-highlight/Test-HighlightUi.ps1
# Usa o motor real, sem carregar modelo. Guarda e devolve a escolha de tema, a
# área de transferência e a cópia de recuperação do usuário. Grava prints.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'HighlightUiTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
$work = Join-Path $env:TEMP ('DepoimentoLocal-highlight-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
try { $failures = [HighlightUiTest]::Run($root, $work) }
finally { Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue }
if ($failures -ne 0) { exit 1 }
