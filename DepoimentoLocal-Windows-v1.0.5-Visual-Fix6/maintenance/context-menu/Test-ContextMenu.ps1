$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source=Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$test=Get-Content (Join-Path $PSScriptRoot 'ContextMenuTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source+"`n"+$test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS'
[ContextMenuTest]::Run($PSScriptRoot)
