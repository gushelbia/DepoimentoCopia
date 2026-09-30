param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$packageRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $PSScriptRoot 'dnlib/lib/net6.0/dnlib.dll')
$utf8 = [Text.UTF8Encoding]::new($false)
$prompt = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'SystemPrompt.txt')).Trim()
$retry = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'RetryPrompt.txt')).Trim()
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'RetryPrompt.txt'), $retry, $utf8)
$backup = Join-Path $PSScriptRoot 'backup-before-prompt'
$staging = Join-Path $PSScriptRoot 'staged'
New-Item -ItemType Directory -Path $backup,$staging -Force | Out-Null

function Get-LogicSnapshot($module) {
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add("Assembly=$($module.Assembly.FullName); Entry=$($module.EntryPoint); Runtime=$($module.RuntimeVersion)")
    foreach ($type in $module.GetTypes()) {
        $lines.Add("TYPE $($type.FullName) $($type.Attributes) $($type.BaseType)")
        foreach ($field in $type.Fields) {
            $value = if ($field.HasConstant) { [string]$field.Constant.Value } else { '' }
            if ($type.FullName -eq 'DepoimentoLocal.Windows.PromptFactory' -and $field.Name -in @('SystemPrompt','RetryPrompt')) { $value = '<PROMPT>' }
            $lines.Add("FIELD $($field.FullName) $($field.Attributes) $value")
        }
        foreach ($method in $type.Methods) {
            $lines.Add("METHOD $($method.FullName) $($method.Attributes) $($method.ImplAttributes)")
            if (-not $method.HasBody) { continue }
            $lines.Add("BODY $($method.Body.InitLocals) $($method.Body.MaxStack)")
            foreach ($local in $method.Body.Variables) { $lines.Add("LOCAL $($local.Index) $($local.Type)") }
            for ($i = 0; $i -lt $method.Body.Instructions.Count; $i++) {
                $instruction = $method.Body.Instructions[$i]
                if ($type.FullName -eq 'DepoimentoLocal.Windows.PromptFactory' -and $method.Name -in @('Build','BuildRetry') -and $i -eq 0) {
                    if ($instruction.OpCode.Code -ne [dnlib.DotNet.Emit.Code]::Ldstr) { throw 'Unexpected prompt instruction' }
                    $lines.Add('IL_0000: ldstr <PROMPT>')
                } else { $lines.Add($instruction.ToString()) }
            }
            foreach ($handler in $method.Body.ExceptionHandlers) {
                $lines.Add("EH $($handler.HandlerType) $($handler.CatchType) $($handler.TryStart) $($handler.TryEnd) $($handler.HandlerStart) $($handler.HandlerEnd) $($handler.FilterStart)")
            }
        }
    }
    return $lines -join "`n"
}

$names = @('DepoimentoLocal.dll','DepoimentoLocal.Vulkan8.dll','DepoimentoLocal.CPU-original.dll')
$report = [Collections.Generic.List[string]]::new()
foreach ($name in $names) {
    $source = Join-Path $packageRoot "engine/$name"
    $saved = Join-Path $backup $name
    if (-not (Test-Path -LiteralPath $saved)) { Copy-Item -LiteralPath $source -Destination $saved }
    # Preserve later execution fixes when updating prompts again.
    $module = [dnlib.DotNet.ModuleDefMD]::Load($source)
    try {
        $before = Get-LogicSnapshot $module
        $factory = @($module.GetTypes() | Where-Object FullName -eq 'DepoimentoLocal.Windows.PromptFactory')
        if ($factory.Count -ne 1) { throw "Unexpected PromptFactory in $name" }
        $changedMethods = 0
        $changedFields = 0
        foreach ($method in $factory[0].Methods) {
            if ($method.Name -notin @('Build','BuildRetry')) { continue }
            $instruction = $method.Body.Instructions[0]
            if ($instruction.OpCode.Code -ne [dnlib.DotNet.Emit.Code]::Ldstr) { throw 'Unexpected prompt layout' }
            [IO.File]::WriteAllText((Join-Path $backup "$name.$($method.Name).txt"), [string]$instruction.Operand, $utf8)
            $instruction.Operand = if ($method.Name -eq 'Build') { $prompt } else { $retry }
            $changedMethods++
        }
        foreach ($field in $factory[0].Fields) {
            if ($field.Name -notin @('SystemPrompt','RetryPrompt')) { continue }
            $field.Constant.Value = if ($field.Name -eq 'SystemPrompt') { $prompt } else { $retry }
            $changedFields++
        }
        if ($changedMethods -ne 2 -or $changedFields -ne 2) { throw 'Unexpected prompt counts' }
        $options = [dnlib.DotNet.Writer.ModuleWriterOptions]::new($module)
        $options.MetadataOptions.Flags = [dnlib.DotNet.Writer.MetadataFlags]::PreserveAll
        foreach ($type in $module.GetTypes()) {
            foreach ($method in $type.Methods) { if ($method.HasBody) { $method.Body.KeepOldMaxStack = $true } }
        }
        $destination = Join-Path $staging $name
        $module.Write($destination, $options)
        $verified = [dnlib.DotNet.ModuleDefMD]::Load($destination)
        try {
            if ((Get-LogicSnapshot $verified) -cne $before) { throw "Non-prompt logic changed in $name" }
            $vf = $verified.GetTypes() | Where-Object FullName -eq 'DepoimentoLocal.Windows.PromptFactory'
            foreach ($method in $vf.Methods) {
                if ($method.Name -in @('Build','BuildRetry')) {
                    $expected = if ($method.Name -eq 'Build') { $prompt } else { $retry }
                    if ($method.Body.Instructions[0].Operand -cne $expected) { throw 'Prompt verification failed' }
                }
            }
        } finally { $verified.Dispose() }
        $report.Add("PASS $name : only 2 prompt literals and 2 prompt constants updated; all method instructions, locals, exception handlers and other field constants preserved.")
    } finally { $module.Dispose() }
}

if ($Apply) {
    foreach ($name in $names) { Copy-Item -LiteralPath (Join-Path $staging $name) -Destination (Join-Path $packageRoot "engine/$name") -Force }
    $vulkanHash = (Get-FileHash -LiteralPath (Join-Path $staging 'DepoimentoLocal.Vulkan8.dll')).Hash
    $cpuHash = (Get-FileHash -LiteralPath (Join-Path $staging 'DepoimentoLocal.CPU-original.dll')).Hash
    foreach ($launcher in @('INICIAR.ps1','INICIAR-ORIGINAL.ps1','INICIAR-CPU.ps1','INICIAR-CPU-ORIGINAL.ps1')) {
        $path = Join-Path $packageRoot $launcher
        $saved = Join-Path $backup $launcher
        if (-not (Test-Path -LiteralPath $saved)) { Copy-Item -LiteralPath $path -Destination $saved }
        $text = [IO.File]::ReadAllText($saved)
        $hash = if ($launcher -like '*CPU*') { $cpuHash } else { $vulkanHash }
        $updated = [regex]::Replace($text, "(?m)(\`$expectedHash = ')[A-F0-9]{64}(')", ('${1}' + $hash + '${2}'))
        if ($updated -eq $text) { throw "Hash not updated: $launcher" }
        [IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($true))
        $report.Add("PASS $launcher : only expectedHash updated.")
    }
    $manifestPath = Join-Path $packageRoot 'SHA256SUMS.txt'
    if (-not (Test-Path -LiteralPath (Join-Path $backup 'SHA256SUMS.txt'))) { Copy-Item -LiteralPath $manifestPath -Destination $backup }
    $manifest = [IO.File]::ReadAllLines($manifestPath)
    $manifest = foreach ($line in $manifest) {
        if ($line -match '^([a-fA-F0-9]{64})  (.+)$') {
            $relative = $Matches[2]
            if ($relative -in @('engine/DepoimentoLocal.dll','engine/DepoimentoLocal.Vulkan8.dll','engine/DepoimentoLocal.CPU-original.dll','INICIAR.ps1','INICIAR-ORIGINAL.ps1','INICIAR-CPU.ps1','INICIAR-CPU-ORIGINAL.ps1')) {
                $hash = (Get-FileHash -LiteralPath (Join-Path $packageRoot $relative)).Hash.ToLowerInvariant()
                "$hash  $relative"
            } else { $line }
        } else { $line }
    }
    [IO.File]::WriteAllLines($manifestPath, $manifest, $utf8)
    $report.Add('APPLIED to the existing package. UI source, model and execution logic unchanged.')
}
[IO.File]::WriteAllLines((Join-Path $PSScriptRoot 'verification.txt'), $report, $utf8)
$report
