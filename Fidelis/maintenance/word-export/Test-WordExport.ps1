# Word formatado: formatação, cabeçalho de qualificação, assinaturas e botões.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/word-export/Test-WordExport.ps1
# Os .docx de teste vão para %TEMP%\DepoimentoLocal-teste-word (dados fictícios).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$kit = Get-Content (Join-Path $root 'maintenance/dialogs/DialogKit.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'WordExportTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $kit + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Xml.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
$out = Join-Path $env:TEMP 'DepoimentoLocal-teste-word'
$failures = [WordExportTest]::Run($root, $out)
if ($failures -ne 0) { exit 1 }