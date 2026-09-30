param([switch]$Apply)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path (Join-Path $root 'maintenance/prompt-update/dnlib/lib/net6.0/dnlib.dll')
. (Join-Path $PSScriptRoot 'Copy-MethodBody.ps1')
$backup=Join-Path $PSScriptRoot 'backup'
$staged=Join-Path $PSScriptRoot 'staged'
New-Item -ItemType Directory -Force $backup,$staged | Out-Null
$helper=[dnlib.DotNet.ModuleDefMD]::Load((Join-Path $PSScriptRoot 'SemanticGuard.compiled.dll'))
$prompt=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'SystemPrompt.txt')).Trim()
$names=@('DepoimentoLocal.dll','DepoimentoLocal.Vulkan8.dll','DepoimentoLocal.CPU-original.dll')
$report=[Collections.Generic.List[string]]::new()
foreach($name in $names) {
    $path=Join-Path $root "engine/$name"
    $saved=Join-Path $backup $name
    if(-not(Test-Path $saved)){Copy-Item -LiteralPath $path -Destination $saved}
    $allowed=@((Get-FileHash -LiteralPath $saved).Hash)
    $previous=Join-Path $staged $name
    if(Test-Path $previous){$allowed+=(Get-FileHash -LiteralPath $previous).Hash}
    if((Get-FileHash -LiteralPath $path).Hash -notin $allowed){throw "Motor alterado depois deste patch; não sobrescrever: $path"}
    # Repeatable from this change's snapshot, never from older maintenance backups.
    $m=[dnlib.DotNet.ModuleDefMD]::Load($saved)
    try {
        $before=@{}
        foreach($t in $m.GetTypes()){foreach($method in $t.Methods){if($method.HasBody){$before[$method.FullName]=($method.Body.Instructions | ForEach-Object ToString) -join "`n"}}}
        $importer=[dnlib.DotNet.Importer]::new($m)
        $type=[dnlib.DotNet.TypeDefUser]::new('DepoimentoLocal.Windows','SemanticGuard',$m.CorLibTypes.Object.TypeDefOrRef)
        $type.Attributes=[dnlib.DotNet.TypeAttributes]'Public, Abstract, Sealed, BeforeFieldInit'
        $m.Types.Add($type)
        $methodMap=@{}
        $helperType=$helper.GetTypes() | Where-Object Name -eq SemanticGuard
        foreach($method in $helperType.Methods){
            $copy=[dnlib.DotNet.MethodDefUser]::new($method.Name,$importer.Import($method.MethodSig),$method.ImplAttributes,$method.Attributes)
            $type.Methods.Add($copy); $methodMap[$method.FullName]=$copy
        }
        foreach($method in $helperType.Methods){Copy-MethodBody $method $methodMap[$method.FullName] $m | Out-Null}
        $text=$m.GetTypes() | Where-Object Name -eq TextProcessing
        $factory=$m.GetTypes() | Where-Object Name -eq PromptFactory
        $pronoun=($m.GetTypes() | Where-Object FullName -eq 'DepoimentoLocal.Windows.TextProcessing/<>c__DisplayClass5_0').Methods | Where-Object Name -eq '<RepairResidualFirstPerson>b__0'
        $pronounChanges=0
        for($i=0;$i -lt $pronoun.Body.Instructions.Count-1;$i++){
            if($pronoun.Body.Instructions[$i].Operand -ceq '\beu\s+'){
                if($pronoun.Body.Instructions[$i+1].OpCode.Code -ne [dnlib.DotNet.Emit.Code]::Ldstr -or $pronoun.Body.Instructions[$i+1].Operand -cne ''){throw 'Unexpected narrator repair layout'}
                $pronoun.Body.Instructions[$i+1].Operand='o depoente '
                $pronounChanges++
            }
        }
        if($pronounChanges -ne 1){throw 'Narrator pronoun repair not found exactly once'}
        $residual=$text.Methods | Where-Object Name -eq RepairResidualFirstPerson
        $newRepair=$type.Methods | Where-Object Name -eq RepairFirstPerson
        $residual.Body.Instructions.Insert(0,[dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0))
        $residual.Body.Instructions.Insert(1,[dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call,$newRepair))
        $residual.Body.Instructions.Insert(2,[dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Starg,$residual.Parameters[0]))
        foreach($method in $factory.Methods){
            if($method.Name -eq 'Build'){$method.Body.Instructions[0].Operand=$prompt}
            if($method.Name -eq 'BuildRetry'){
                $method.Body.Instructions[0].Operand="Reescreva novamente a TRANSCRIÇÃO ORIGINAL inteira. Corrija o problema apontado conferindo todas as orações. Não use um rascunho como fonte. Não omita o início nem o final.`n`n"+$prompt
                foreach($ins in $method.Body.Instructions){
                    if($ins.OpCode.Code -eq [dnlib.DotNet.Emit.Code]::Ldarg_1){$ins.OpCode=[dnlib.DotNet.Emit.OpCodes]::Ldstr; $ins.Operand=''}
                    if($ins.Operand -is [string] -and $ins.Operand.Contains('RASCUNHO A CORRIGIR')){$ins.Operand="`n`nReescreva desde a primeira frase, preservando cada oração."}
                }
            }
            # Assistant prefill 'Relatou que' biases the first sentence toward a
            # factual summary. The existing display lead remains applied afterwards.
            if($method.Name -eq 'ChatMl'){
                foreach($ins in $method.Body.Instructions){if($ins.OpCode.Code -eq [dnlib.DotNet.Emit.Code]::Ldarg_2){$ins.OpCode=[dnlib.DotNet.Emit.OpCodes]::Ldstr; $ins.Operand=''}}
            }
        }
        foreach($field in $factory.Fields){
            if($field.Name -eq 'SystemPrompt'){$field.Constant.Value=$prompt}
            if($field.Name -eq 'RetryPrompt'){$field.Constant.Value=($factory.Methods | Where-Object Name -eq BuildRetry).Body.Instructions[0].Operand}
        }
        $validate=$text.Methods | Where-Object Name -eq Validate
        $guard=$type.Methods | Where-Object Name -eq Validate
        $start=$validate.Body.Instructions[0]
        $added=@(
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_1),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call,$guard),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Dup),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Brfalse,$start),
            [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ret)
        )
        # On the null path consume the duplicate before the old validator starts.
        $pop=[dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Pop)
        $added[4].Operand=$pop
        $index=0; foreach($ins in $added){$validate.Body.Instructions.Insert($index++,$ins)}
        $validate.Body.Instructions.Insert($index,$pop)
        $critical=$text.Methods | Where-Object Name -eq IsCriticalIssue
        $existingEquality=($critical.Body.Instructions | Where-Object { $_.Operand -is [dnlib.DotNet.IMethod] -and $_.Operand.Name -eq 'op_Equality' } | Select-Object -First 1).Operand
        $critical.Body=[dnlib.DotNet.Emit.CilBody]::new()
        # Existing raw negation-count warning may legitimately flag removed false
        # starts. All semantic/operator/first-person failures require retry/review.
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0))
        $nullRet=[dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldc_I4_0)
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Brfalse,$nullRet))
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0))
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldstr,'uma negação pode ter sido omitida'))
        $eq=($validate.Body.Instructions | Where-Object { $_.Operand -is [dnlib.DotNet.IMethod] -and $_.Operand.Name -eq 'op_Equality' } | Select-Object -First 1).Operand
        if($null -eq $eq){$eq=$existingEquality}
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call,$eq))
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldc_I4_0))
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ceq))
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ret))
        $critical.Body.Instructions.Add($nullRet)
        $critical.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ret))
        $repair=$text.Methods | Where-Object Name -eq RepairSemanticAnchors
        $repair.Body=[dnlib.DotNet.Emit.CilBody]::new()
        $repair.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0))
        $repair.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_1))
        $anchors=$type.Methods | Where-Object Name -eq RepairSourceAnchors
        $repair.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Call,$anchors))
        $repair.Body.Instructions.Add([dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ret))
        foreach($method in @($validate,$critical,$repair,$residual)+@($factory.Methods)+@($type.Methods)){if($method.HasBody){$method.Body.SimplifyBranches(); $method.Body.OptimizeBranches()}}
        $options=[dnlib.DotNet.Writer.ModuleWriterOptions]::new($m)
        $options.MetadataOptions.Flags=[dnlib.DotNet.Writer.MetadataFlags]::PreserveAll
        $destination=Join-Path $staged $name
        $m.Write($destination,$options)
        $check=[dnlib.DotNet.ModuleDefMD]::Load($destination)
        try {
            foreach($t in $check.GetTypes()){foreach($method in $t.Methods){
                if(!$before.ContainsKey($method.FullName)){continue}
                if(($t.Name -eq 'PromptFactory' -and $method.Name -in @('Build','BuildRetry','ChatMl')) -or ($t.Name -eq 'TextProcessing' -and $method.Name -in @('Validate','IsCriticalIssue','RepairSemanticAnchors','RepairResidualFirstPerson'))){continue}
                if($t.FullName -eq 'DepoimentoLocal.Windows.TextProcessing/<>c__DisplayClass5_0' -and $method.Name -eq '<RepairResidualFirstPerson>b__0'){continue}
                if($method.HasBody -and (($method.Body.Instructions | ForEach-Object ToString) -join "`n") -cne $before[$method.FullName]){throw "Unrelated method changed: $($method.FullName)"}
            }}
        } finally {$check.Dispose()}
        $report.Add("PASS $name : only prompts, assistant prefill, semantic guard, supplemental conjugations, retry classification and unsafe positional repair changed.")
    } finally {$m.Dispose()}
}
$helper.Dispose()
if($Apply){
    foreach($name in $names){Copy-Item -LiteralPath (Join-Path $staged $name) -Destination (Join-Path $root "engine/$name") -Force}
    $launchers=@('INICIAR.ps1','INICIAR-ORIGINAL.ps1','INICIAR-CPU.ps1','INICIAR-CPU-ORIGINAL.ps1')
    foreach($name in $launchers){
        $path=Join-Path $root $name
        if(!(Test-Path (Join-Path $backup $name))){Copy-Item -LiteralPath $path -Destination $backup}
        $dll=if($name -like '*CPU*'){'DepoimentoLocal.CPU-original.dll'}else{'DepoimentoLocal.Vulkan8.dll'}
        $hash=(Get-FileHash (Join-Path $root "engine/$dll")).Hash
        $content=[IO.File]::ReadAllText($path)
        $content=[regex]::Replace($content,"(?m)(\`$expectedHash = ')[A-F0-9]{64}(')",('${1}'+$hash+'${2}'))
        [IO.File]::WriteAllText($path,$content,[Text.UTF8Encoding]::new($true))
    }
    $manifest=Join-Path $root 'SHA256SUMS.txt'
    if(!(Test-Path (Join-Path $backup 'SHA256SUMS.txt'))){Copy-Item -LiteralPath $manifest -Destination $backup}
    $targets=@($names | ForEach-Object {"engine/$_"})+$launchers
    $lines=foreach($line in [IO.File]::ReadAllLines($manifest)){
        if($line -match '^[a-fA-F0-9]{64}  (.+)$' -and $Matches[1] -in $targets){$rel=$Matches[1]; (Get-FileHash (Join-Path $root $rel)).Hash.ToLowerInvariant()+'  '+$rel}else{$line}
    }
    [IO.File]::WriteAllLines($manifest,$lines,[Text.UTF8Encoding]::new($false))
}
$report | Set-Content (Join-Path $PSScriptRoot 'verification.txt')
$report
