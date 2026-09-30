param([string]$PublishedHost)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$engine=Join-Path $root 'engine'
$desktop='DepoimentoLocal.Desktop'
if(-not $PublishedHost){
 $references=[Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
 foreach($file in Get-ChildItem -LiteralPath $engine -Filter '*.dll' -File){
  if($file.Name -like 'DepoimentoLocal*' -or $file.Name -like 'ContextRegression*' -or $file.Name -eq 'BudgetProbe.dll' -or $file.Name -like 'PresentationFramework*'){continue}
  try{[void][Reflection.AssemblyName]::GetAssemblyName($file.FullName);$references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($file.FullName))}catch [System.BadImageFormatException]{}
 }
 $trees=[Microsoft.CodeAnalysis.SyntaxTree[]]@(
  [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'DesktopProgram.cs'))),
  [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs')))
 )
 $options=[Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new([Microsoft.CodeAnalysis.OutputKind]::WindowsApplication).WithOptimizationLevel([Microsoft.CodeAnalysis.OptimizationLevel]::Release).WithPlatform([Microsoft.CodeAnalysis.Platform]::X64)
 $compilation=[Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create($desktop,$trees,$references,$options)
 $stream=[IO.File]::Create((Join-Path $engine "$desktop.dll"))
 try{$result=$compilation.Emit($stream)}finally{$stream.Dispose()}
 if(-not $result.Success){$result.Diagnostics | ForEach-Object {$_.ToString()};throw 'Compilation failed'}
 Copy-Item (Join-Path $engine 'DepoimentoLocal.runtimeconfig.json') (Join-Path $engine "$desktop.runtimeconfig.json") -Force
 $deps=[IO.File]::ReadAllText((Join-Path $engine 'DepoimentoLocal.deps.json')).Replace('DepoimentoLocal/0.1.9',"$desktop/0.0.0").Replace('"DepoimentoLocal.dll"',('"'+$desktop+'.dll"'))
 [IO.File]::WriteAllText((Join-Path $engine "$desktop.deps.json"),$deps)
 $hostPath=Join-Path $engine 'DepoimentoLocal.exe'
 $oldName='DepoimentoLocal.dll'
}else{
 $hostPath=(Resolve-Path $PublishedHost).Path
 $oldName="$desktop.dll"
}
# Standard .NET apphost has a 1024-byte embedded managed path slot. Bind it to engine/.
$bytes=[IO.File]::ReadAllBytes($hostPath)
$ascii=[Text.Encoding]::UTF8.GetString($bytes)
# Search bytes directly because UTF8 decoding of PE bytes changes string offsets.
$needle=[Text.Encoding]::UTF8.GetBytes($oldName+[char]0)
$positions=[Collections.Generic.List[int]]::new()
for($i=0;$i -le $bytes.Length-$needle.Length;$i++){
 if($bytes[$i] -ne $needle[0]){continue};$match=$true
 for($j=1;$j -lt $needle.Length;$j++){if($bytes[$i+$j] -ne $needle[$j]){$match=$false;break}}
 if($match){$positions.Add($i)}
}
if($positions.Count -ne 1){throw 'Unexpected apphost managed-path slot'}
$position=$positions[0]
$newPath=[Text.Encoding]::UTF8.GetBytes("engine/$desktop.dll")
for($i=$needle.Length;$i -le $newPath.Length;$i++){if($bytes[$position+$i] -ne 0){throw 'Apphost path padding is not empty'}}
[Array]::Clear($bytes,$position,$newPath.Length+1)
[Array]::Copy($newPath,0,$bytes,$position,$newPath.Length)
# Require x64 native Windows GUI subsystem (2), never console subsystem (3).
$pe=[BitConverter]::ToInt32($bytes,0x3c)
if([BitConverter]::ToUInt16($bytes,$pe+4) -ne 0x8664){throw 'Expected x64 apphost'}
$subsystem=$pe+24+68
if([BitConverter]::ToUInt16($bytes,$subsystem) -ne 2){throw 'Expected Windows GUI apphost'}
# The program's entry point is Fidelis.exe (renamed from DepoimentoLocal.exe; the engine keeps its names).
[IO.File]::WriteAllBytes((Join-Path $root 'Fidelis.exe'),$bytes)
$oldEntry=Join-Path $root 'DepoimentoLocal.exe'
if(Test-Path -LiteralPath $oldEntry){Remove-Item -LiteralPath $oldEntry}
'COMPILE PASS: Windows GUI x64; self-contained .NET 8 in engine; root apphost Fidelis.exe bound to desktop assembly.'

