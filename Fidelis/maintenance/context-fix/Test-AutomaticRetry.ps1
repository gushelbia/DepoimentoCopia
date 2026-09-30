$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'ui/ModernShell.cs')
$test = Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'AutomaticRetryTest.cs')
Add-Type -TypeDefinition ($source + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS'
[AutomaticRetryTest]::Run($root)
