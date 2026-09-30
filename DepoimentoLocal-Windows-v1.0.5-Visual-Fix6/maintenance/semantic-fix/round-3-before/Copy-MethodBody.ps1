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
        elseif ($operand -is [dnlib.DotNet.MethodDef] -and $methodMap.ContainsKey($operand.FullName)) { $operand = $methodMap[$operand.FullName] }
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




