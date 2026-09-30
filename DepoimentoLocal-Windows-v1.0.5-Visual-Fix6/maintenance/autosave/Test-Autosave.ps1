# Teste de interface das proteções do depoimento consolidado.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/autosave/Test-Autosave.ps1
# Usa o motor real (sem carregar modelo). A cópia de recuperação real do usuário,
# se existir, é guardada antes e devolvida no fim.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'AutosaveTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
$work = Join-Path $env:TEMP ('DepoimentoLocal-autosave-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
try { $failures = [AutosaveTest]::Run($root, $work) }
finally { Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue }
if ($failures -ne 0) { exit 1 }
