$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'Test3Probe.cs') -Raw -Encoding UTF8) -Language CSharp
[Test3Probe]::Run($root)
