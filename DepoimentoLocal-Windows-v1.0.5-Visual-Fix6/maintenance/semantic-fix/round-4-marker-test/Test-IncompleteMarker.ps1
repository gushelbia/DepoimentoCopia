# Isolated test of the incomplete-output marker. Runs with Windows PowerShell 5.1 (STA):
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File Test-IncompleteMarker.ps1
# Uses a temporary copy of the engine outside the distribution. Only that copy
# receives a test sentinel: Validate rejects a block whose source contains
# SENTINELA-REJEICAO. Production DLLs are hash-checked before and after.
$ErrorActionPreference='Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Execute com powershell.exe -STA.' }
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$tmp=Join-Path $env:TEMP 'DepoimentoLocal-marker-test'
$production=@(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
if(Test-Path "$tmp\modelo"){ cmd /c rmdir "$tmp\modelo" | Out-Null }
if(Test-Path $tmp){ Remove-Item -Recurse -Force $tmp }
New-Item -ItemType Directory $tmp | Out-Null
Copy-Item -Recurse (Join-Path $root 'engine') (Join-Path $tmp 'engine')
cmd /c mklink /J "$tmp\modelo" "$(Join-Path $root 'modelo')" | Out-Null

Add-Type -Path (Join-Path $root 'maintenance/prompt-update/dnlib/lib/net45/dnlib.dll')
$dll=Join-Path $tmp 'engine/DepoimentoLocal.dll'
$m=[dnlib.DotNet.ModuleDefMD]::Load([IO.File]::ReadAllBytes($dll))
$validate=($m.GetTypes() | Where-Object Name -eq TextProcessing).Methods | Where-Object Name -eq Validate
$contains=$null
foreach($t in $m.GetTypes()){foreach($x in $t.Methods){if($x.HasBody){foreach($i in $x.Body.Instructions){if($i.Operand -is [dnlib.DotNet.IMethod] -and $i.Operand.FullName -eq 'System.Boolean System.String::Contains(System.String)'){$contains=$i.Operand}}}}}
if($null -eq $contains){throw 'String.Contains não encontrado'}
$first=$validate.Body.Instructions[0]
$added=@(
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldarg_0),
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldstr,'SENTINELA-REJEICAO'),
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Callvirt,$contains),
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Brfalse,$first),
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ldstr,'fidelidade: rejeição simulada para teste do marcador'),
  [dnlib.DotNet.Emit.Instruction]::new([dnlib.DotNet.Emit.OpCodes]::Ret))
$k=0; foreach($i in $added){$validate.Body.Instructions.Insert($k++,$i)}
$validate.Body.SimplifyBranches(); $validate.Body.OptimizeBranches()
$options=[dnlib.DotNet.Writer.ModuleWriterOptions]::new($m); $options.MetadataOptions.Flags=[dnlib.DotNet.Writer.MetadataFlags]::PreserveAll
$m.Write($dll,$options); $m.Dispose()

# About 2,300 characters: the engine splits at ~1,180, so this is 2 blocks.
# The sentinel sits in the last sentence, which always falls in block 2.
$p1='Eu trabalho como porteiro de um condomínio residencial há quatro anos. Na terça de manhã, cheguei por volta das seis e cinquenta e abri a guarita, como faço todos os dias. A síndica Ruth passou pela portaria logo depois e me pediu que eu anotasse a entrega de um armário de madeira. Eu anotei no caderno de ocorrências, na página do dia. Não vi quem trouxe o armário, porque fui ao depósito buscar as chaves reservas da garagem. Quando voltei, o armário já estava no hall, encostado na parede ao lado do elevador. O zelador Anselmo disse que o caminhão tinha ido embora poucos minutos antes. Eu não conferi a placa do caminhão e não sei de qual empresa ele era. Também não assinei nenhum recibo de entrega, e ninguém me entregou nota fiscal. Anotei apenas o horário aproximado, perto das sete e dez, porque não olhei o relógio no momento exato. Depois disso fiquei na guarita atendendo os moradores que saíam para trabalhar e recebendo as correspondências do dia.'
$p2='Mais tarde, a moradora do apartamento doze desceu e perguntou se alguém tinha deixado uma encomenda para ela. Respondi que só tinha chegado o armário. Ela olhou o armário e disse que não era dela. Eu não abri o armário e não mexi nas portas dele. Liguei para a síndica, mas ela não atendeu. Deixei um recado na caixa postal e voltei para a guarita. Perto do meio-dia, Anselmo levou o armário para a garagem com a ajuda de um morador que eu não conheço. Não sei quem autorizou essa mudança nem se a síndica foi avisada. No fim da tarde, recebi uma mensagem da síndica dizendo que o armário era de um apartamento do terceiro andar. Não confirmei essa informação com ninguém.'
$reject=$p1+"`r`n`r`n"+$p2.Replace('Não confirmei essa informação','SENTINELA-REJEICAO. Não confirmei essa informação')
$cancel=$p1+"`r`n`r`n"+$p2
if($cancel.Length -le 1200){throw 'Texto de teste curto demais para dois blocos'}
$source=[IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$testLines=[IO.File]::ReadAllLines((Join-Path $PSScriptRoot 'MarkerTest.cs'))
$shellUsings=@($source -split "`r?`n" | Where-Object { $_ -match '^using ' } | ForEach-Object { $_.Trim() })
$usings=($testLines | Where-Object { $_ -match '^using ' -and $shellUsings -notcontains $_.Trim() }) -join "`n"
$body=($testLines | Where-Object { $_ -notmatch '^using ' }) -join "`n"
Add-Type -TypeDefinition ($usings+"`n"+$source+"`n"+$body) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory=Join-Path $tmp 'engine'
$png=Join-Path $PSScriptRoot 'marker-screen.png'
$results=[MarkerTest]::Run($tmp,$reject,$cancel,$png)
$after=@(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash)
$same=-not (Compare-Object ($production | ForEach-Object {$_.Path+$_.Hash}) ($after | ForEach-Object {$_.Path+$_.Hash}))
$results.Add($(if($same){'PASS DLLs de produção inalteradas (sentinela só na cópia temporária)'}else{'FAIL DLLs de produção mudaram'}))
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'marker-results.txt')
$results
# Cleanup: remove the junction first (never the real model), then the copy.
cmd /c rmdir "$tmp\modelo" | Out-Null
Get-Process DepoimentoLocal -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$tmp*" } | ForEach-Object { $_.Kill(); $_.WaitForExit(10000) }
try { Start-Sleep -Seconds 2; Remove-Item -Recurse -Force $tmp -ErrorAction Stop } catch { Write-Output "AVISO: cópia temporária não removida: $tmp" }
