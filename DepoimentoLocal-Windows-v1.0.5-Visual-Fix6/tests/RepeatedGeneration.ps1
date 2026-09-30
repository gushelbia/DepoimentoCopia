$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'ui/ModernShell.cs')
$test = Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'RepeatedGeneration.cs')
# Add test-only imports before the production source for the .NET Framework compiler.
$code = "using System.Reflection;`n" + $source + ($test -replace '(?m)^using System\.(Reflection|Threading);\r?\n', '')
Add-Type -TypeDefinition $code -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS'
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
[RepeatedGeneration]::Run($root)

