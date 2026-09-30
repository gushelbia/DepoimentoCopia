$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'LaunchProbe.cs') -Raw -Encoding UTF8) -Language CSharp
[LaunchProbe]::Run($root)

