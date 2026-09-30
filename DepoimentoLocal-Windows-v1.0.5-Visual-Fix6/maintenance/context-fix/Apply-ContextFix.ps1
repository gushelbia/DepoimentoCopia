param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/prompt-update/dnlib/lib/net6.0/dnlib.dll')
$backup = Join-Path $PSScriptRoot 'backup-before-context-fix'
$staged = Join-Path $PSScriptRoot 'staged'
New-Item -ItemType Directory -Path $backup,$staged -Force | Out-Null
$retry = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'RetryPrompt.txt')).Trim()
$helperModule = [dnlib.DotNet.ModuleDefMD]::Load((Join-Path $PSScriptRoot 'ContextBudget.compiled.dll'))
$llamaModule = [dnlib.DotNet.ModuleDefMD]::Load((Join-Path $root 'engine/LLamaSharp.dll'))
$helperMethod = ($helperModule.GetTypes() | Where-Object Name -eq 'ContextBudget').Methods | Where-Object Name -eq 'Validate'
$overflowMethod = ($llamaModule.GetTypes() | Where-Object FullName -eq 'LLama.Common.InferenceParams').Methods | Where-Object Name -eq 'set_OverflowStrategy'
$throwValue = (($llamaModule.GetTypes() | Where-Object FullName -eq 'LLama.Common.ContextOverflowStrategy').Fields | Where-Object Name -eq 'ThrowException').Constant.Value
if ($throwValue -ne 0) { throw 'Unexpected overflow enum value' }

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
    if ($source.Body.ExceptionHandlers.Count -ne 0) { throw 'Unexpected helper exception regions' }
    $destination.Body = $body
}

function Unchanged-Snapshot($module) {
    $lines = [Collections.Generic.List[string]]::new()
    foreach ($type in $module.GetTypes()) {
        if ($type.FullName -eq 'DepoimentoLocal.Windows.ContextBudget') { continue }
        foreach ($field in $type.Fields) {
            if ($type.Name -eq 'PromptFactory' -and $field.Name -eq 'RetryPrompt') { continue }
            $lines.Add("FIELD $($field.FullName) $($field.Attributes) $($field.Constant)")
            if ($field.HasConstant) { $lines.Add([string]$field.Constant.Value) }
        }
        foreach ($method in $type.Methods) {
            if (($type.FullName -match '^DepoimentoLocal.Windows.LlmService/<(LoadAsync|GenerateAsync)>' -and $method.Name -eq 'MoveNext') -or ($type.Name -eq 'PromptFactory' -and $method.Name -eq 'BuildRetry')) { continue }
            $lines.Add("METHOD $($method.FullName) $($method.Attributes) $($method.ImplAttributes)")
            if ($method.HasBody) {
                foreach ($instruction in $method.Body.Instructions) { $lines.Add($instruction.ToString()) }
                foreach ($local in $method.Body.Variables) { $lines.Add("LOCAL $($local.Type)") }
            }
        }
    }
    return $lines -join "`n"
}

$report = [Collections.Generic.List[string]]::new()
$names = @('DepoimentoLocal.dll','DepoimentoLocal.Vulkan8.dll','DepoimentoLocal.CPU-original.dll')
foreach ($name in $names) {
    $path = Join-Path $root "engine/$name"
    $saved = Join-Path $backup $name
    if (-not (Test-Path -LiteralPath $saved)) { Copy-Item -LiteralPath $path -Destination $saved }
    $module = [dnlib.DotNet.ModuleDefMD]::Load($saved)
    try {
        $snapshot = Unchanged-Snapshot $module
        $importer = [dnlib.DotNet.Importer]::new($module)
        $service = $module.GetTypes() | Where-Object FullName -eq 'DepoimentoLocal.Windows.LlmService'
        $load = ($service.NestedTypes | Where-Object Name -like '<LoadAsync>*').Methods | Where-Object Name -eq 'MoveNext'
        $generateType = $service.NestedTypes | Where-Object Name -like '<GenerateAsync>*'
        $generate = $generateType.Methods | Where-Object Name -eq 'MoveNext'
        $loadChanges = 0
        foreach ($instruction in $load.Body.Instructions) {
            if ($instruction.IsLdcI4() -and $instruction.GetLdcI4Value() -eq 2048) { $instruction.OpCode = [dnlib.DotNet.Emit.OpCodes]::Ldc_I4; $instruction.Operand = [int]4096; $loadChanges++ }
            if ($instruction.Operand -is [string] -and $instruction.Operand.StartsWith('context=2048;')) { $instruction.Operand = $instruction.Operand.Replace('context=2048;', 'context=4096;'); $loadChanges++ }
        }
        if ($loadChanges -ne 2) { throw "Unexpected context configuration: $name" }
        $factory = $module.GetTypes() | Where-Object Name -eq 'PromptFactory'
        ($factory.Methods | Where-Object Name -eq 'BuildRetry').Body.Instructions[0].Operand = $retry
        ($factory.Fields | Where-Object Name -eq 'RetryPrompt').Constant.Value = $retry

        $guardType = [dnlib.DotNet.TypeDefUser]::new('DepoimentoLocal.Windows', 'ContextBudget', $module.CorLibTypes.Object.TypeDefOrRef)
        $guardType.Attributes = [dnlib.DotNet.TypeAttributes]'Public, Abstract, Sealed, BeforeFieldInit'
        $module.Types.Add($guardType)
        $guard = [dnlib.DotNet.MethodDefUser]::new('Validate', $importer.Import($helperMethod.MethodSig), $helperMethod.ImplAttributes, $helperMethod.Attributes)
        $guardType.Methods.Add($guard)
        Copy-MethodBody $helperMethod $guard $module

        $instructions = $generate.Body.Instructions
        $anchor = @($instructions | Where-Object { $_.OpCode.Code -eq [dnlib.DotNet.Emit.Code]::Newobj -and $_.Operand.FullName -eq 'System.Void LLama.Common.InferenceParams::.ctor()' })
        if ($anchor.Count -ne 1) { throw 'Unexpected inference parameter setup' }
        $anchor = $anchor[0]
        $weights = $service.Fields | Where-Object Name -eq '_weights'
        $promptField = $generateType.Fields | Where-Object Name -eq 'prompt'
        $maxField = $generateType.Fields | Where-Object Name -eq 'maxTokens'
        $log = ($module.GetTypes() | Where-Object Name -eq 'AppLog').Methods | Where-Object Name -eq 'Info'
        $added = @(
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldstr, 'CONTEXT_BUDGET'),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldloc_1),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldfld, $weights),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldfld, $promptField),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldfld, $maxField),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldc_I4, [int]4096),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call, $guard),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call, $log)
        )
        foreach ($instruction in $instructions) { if ([object]::ReferenceEquals($instruction.Operand, $anchor)) { $instruction.Operand = $added[0] } }
        $index = $instructions.IndexOf($anchor)
        foreach ($instruction in $added) { $instructions.Insert($index++, $instruction) }
        $index = $instructions.IndexOf($anchor) + 1
        $instructions.Insert($index++, [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Dup))
        $instructions.Insert($index++, [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldc_I4_0))
        $instructions.Insert($index++, [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Callvirt, $importer.Import($overflowMethod)))
        $generate.Body.SimplifyBranches()
        $generate.Body.OptimizeBranches()
        $options = [dnlib.DotNet.Writer.ModuleWriterOptions]::new($module)
        $options.MetadataOptions.Flags = [dnlib.DotNet.Writer.MetadataFlags]::PreserveAll
        $destination = Join-Path $staged $name
        $module.Write($destination, $options)
        $check = [dnlib.DotNet.ModuleDefMD]::Load($destination)
        try { if ((Unchanged-Snapshot $check) -cne $snapshot) { throw "Unrelated logic changed: $name" } } finally { $check.Dispose() }
        $report.Add("PASS $name : context=4096; compiled token-budget guard embedded; explicit ThrowException; concise retry. Validator, first prompt, GPU layers, temperature and UI unchanged.")
    } finally { $module.Dispose() }
}
$helperModule.Dispose()
$llamaModule.Dispose()
if ($Apply) {
    foreach ($name in $names) { Copy-Item -LiteralPath (Join-Path $staged $name) -Destination (Join-Path $root "engine/$name") -Force }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'RetryPrompt.txt') -Destination (Join-Path $root 'maintenance/prompt-update/RetryPrompt.txt') -Force
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
