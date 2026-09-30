param([switch]$Apply)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/prompt-update/dnlib/lib/net6.0/dnlib.dll')
$backup=Join-Path $PSScriptRoot 'backup-before-quantity'
$staged=Join-Path $PSScriptRoot 'staged'
New-Item -ItemType Directory -Path $backup,$staged -Force | Out-Null
$helperModule=[dnlib.DotNet.ModuleDefMD]::Load((Join-Path $PSScriptRoot 'FidelityRepairs.compiled.dll'))
$helperMethod=($helperModule.GetTypes() | Where-Object Name -eq FidelityRepairs).Methods | Where-Object Name -eq Repair
function Copy-MethodBody($source, $destination, $module) {
    $importer = [dnlib.DotNet.Importer]::new($module)
    $body = [dnlib.DotNet.Emit.CilBody]::new()
    $body.InitLocals = $source.Body.InitLocals
    $map = @{}
    foreach ($local in $source.Body.Variables) { $body.Variables.Add([dnlib.DotNet.Emit.Local]::new($importer.Import($local.Type))) }
    foreach ($instruction in $source.Body.Instructions) {
        $copy = [dnlib.DotNet.Emit.Instruction]::new($instruction.OpCode, $null)
        $body.Instructions.Add($copy)
        $map[$instruction] = $copy
    }
    for ($i = 0; $i -lt $source.Body.Instructions.Count; $i++) {
        $operand = $source.Body.Instructions[$i].Operand
        if ($operand -is [dnlib.DotNet.Emit.Instruction]) { $operand = $map[$operand] }
        elseif ($operand -is [dnlib.DotNet.Emit.Local]) { $operand = $body.Variables[$operand.Index] }
        elseif ($operand -is [dnlib.DotNet.Parameter]) { $operand = $destination.Parameters[$operand.Index] }
        elseif ($operand -is [dnlib.DotNet.IMethod]) { $operand = $importer.Import([dnlib.DotNet.IMethod]$operand) }
        elseif ($operand -is [dnlib.DotNet.IField]) { $operand = $importer.Import([dnlib.DotNet.IField]$operand) }
        elseif ($operand -is [dnlib.DotNet.ITypeDefOrRef]) { $operand = $importer.Import([dnlib.DotNet.ITypeDefOrRef]$operand) }
        $body.Instructions[$i].Operand = $operand
    }
    foreach ($handler in $source.Body.ExceptionHandlers) {
        $copy = [dnlib.DotNet.Emit.ExceptionHandler]::new($handler.HandlerType)
        foreach ($boundary in @('TryStart','TryEnd','HandlerStart','HandlerEnd','FilterStart')) {
            if ($null -ne $handler.$boundary) { $copy.$boundary = $map[$handler.$boundary] }
        }
        if ($null -ne $handler.CatchType) { $copy.CatchType = $importer.Import($handler.CatchType) }
        $body.ExceptionHandlers.Add($copy)
    }
    $destination.Body = $body
}



function Unchanged-Snapshot($module) {
 $lines=[Collections.Generic.List[string]]::new()
 foreach($type in $module.GetTypes()) {
  if($type.Name -eq 'FidelityRepairs'){continue}
  foreach($field in $type.Fields){$lines.Add("FIELD $($field.FullName) $($field.Attributes) $($field.Constant)");if($field.HasConstant){$lines.Add([string]$field.Constant.Value)}}
  foreach($method in $type.Methods){
   $lines.Add("METHOD $($method.FullName) $($method.Attributes) $($method.ImplAttributes)")
   if($method.HasBody){foreach($instruction in $method.Body.Instructions){$lines.Add($instruction.ToString())};foreach($local in $method.Body.Variables){$lines.Add("LOCAL $($local.Type)")}}
  }
 }
 return $lines -join "`n"
}
$report=[Collections.Generic.List[string]]::new()
$names=@('DepoimentoLocal.dll','DepoimentoLocal.Vulkan8.dll','DepoimentoLocal.CPU-original.dll')
foreach($name in $names){
 $path=Join-Path $root "engine/$name"
 $saved=Join-Path $backup $name
 if(-not(Test-Path $saved)){Copy-Item -LiteralPath $path -Destination $saved}
 $module=[dnlib.DotNet.ModuleDefMD]::Load($saved)
 try{
  $snapshot=Unchanged-Snapshot $module
  $repair=($module.GetTypes() | Where-Object Name -eq FidelityRepairs).Methods | Where-Object Name -eq Repair
  if($null -eq $repair){throw 'Existing FidelityRepairs missing'}
  Copy-MethodBody $helperMethod $repair $module | Out-Null
  $options=[dnlib.DotNet.Writer.ModuleWriterOptions]::new($module)
  $options.MetadataOptions.Flags=[dnlib.DotNet.Writer.MetadataFlags]::PreserveAll
  $destination=Join-Path $staged $name
  $module.Write($destination,$options)
  $check=[dnlib.DotNet.ModuleDefMD]::Load($destination)
  try{if((Unchanged-Snapshot $check) -cne $snapshot){throw "Unrelated change: $name"}}finally{$check.Dispose()}
  $report.Add("PASS $name : only FidelityRepairs.Repair updated; prompts, validator, generation, parameters and other methods unchanged.")
 }finally{$module.Dispose()}
}
$helperModule.Dispose()
if ($Apply) {
    foreach ($name in $names) { Copy-Item -LiteralPath (Join-Path $staged $name) -Destination (Join-Path $root "engine/$name") -Force }
    $vulkanHash = (Get-FileHash -LiteralPath (Join-Path $staged 'DepoimentoLocal.Vulkan8.dll')).Hash
    $cpuHash = (Get-FileHash -LiteralPath (Join-Path $staged 'DepoimentoLocal.CPU-original.dll')).Hash
    $launchers = @('INICIAR.ps1','INICIAR-ORIGINAL.ps1','INICIAR-CPU.ps1','INICIAR-CPU-ORIGINAL.ps1')
    foreach ($name in $launchers) {
        $path = Join-Path $root $name
        if (-not (Test-Path -LiteralPath (Join-Path $backup $name))) { Copy-Item -LiteralPath $path -Destination $backup }
        $hash = if ($name -like '*CPU*') { $cpuHash } else { $vulkanHash }
        $content = [IO.File]::ReadAllText($path)
        $content = [regex]::Replace($content, "(?m)(\`$expectedHash = ')[A-F0-9]{64}(')", ('${1}' + $hash + '${2}'))
        [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($true))
    }
    $manifestPath = Join-Path $root 'SHA256SUMS.txt'
    if (-not (Test-Path -LiteralPath (Join-Path $backup 'SHA256SUMS.txt'))) { Copy-Item -LiteralPath $manifestPath -Destination $backup }
    $targets = @($names | ForEach-Object { "engine/$_" }) + $launchers
    $lines = foreach ($line in [IO.File]::ReadAllLines($manifestPath)) {
        if ($line -match '^[a-fA-F0-9]{64}  (.+)$' -and $Matches[1] -in $targets) {
            $relative = $Matches[1]
            (Get-FileHash -LiteralPath (Join-Path $root $relative)).Hash.ToLowerInvariant() + '  ' + $relative
        } else { $line }
    }
    [IO.File]::WriteAllLines($manifestPath, $lines, [Text.UTF8Encoding]::new($false))
    $report.Add('APPLIED; launchers and manifest hashes updated.')
}
[IO.File]::WriteAllLines((Join-Path $PSScriptRoot 'verification.txt'), $report, [Text.UTF8Encoding]::new($false))
$report




