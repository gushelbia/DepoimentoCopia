# Campos de qualificação: painel, CPF, limpar, temas e prova de que os dados
# nunca vão ao modelo (carrega o modelo e faz uma geração real).
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/qualification/Test-Qualification.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'QualificationTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
$failures = [QualificationTest]::Run($root)
if ($failures -ne 0) { exit 1 }
