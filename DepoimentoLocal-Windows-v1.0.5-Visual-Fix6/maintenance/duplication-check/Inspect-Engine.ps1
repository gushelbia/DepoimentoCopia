$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/prompt-update/dnlib/lib/net6.0/dnlib.dll')
$m=[dnlib.DotNet.ModuleDefMD]::Load((Join-Path $root 'engine/DepoimentoLocal.dll'))
$lines=foreach($t in $m.GetTypes()) { foreach($method in $t.Methods) { if($method.HasBody -and ($method.Body.Instructions | Where-Object { $_.Operand -and $_.Operand.ToString() -match 'AppendText|set_Text|add_Click' })) { $method.FullName; $method.Body.Instructions | ForEach-Object { $_.ToString() } } } }
$lines | Set-Content (Join-Path $PSScriptRoot 'engine-ui-il.txt')
