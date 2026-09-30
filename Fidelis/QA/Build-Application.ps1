# Compile all available application sources into QA/Build, never replace the shipped app.
# Run with PowerShell 7 (bundled Roslyn), using the distribution's .NET 8 references.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$engine = Join-Path $root 'engine'
$build = Join-Path $PSScriptRoot 'Build'
New-Item -ItemType Directory -Force -Path $build | Out-Null
$references = [Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($file in Get-ChildItem -LiteralPath $engine -Filter '*.dll' -File) {
    if ($file.Name -like 'DepoimentoLocal*' -or $file.Name -like 'ContextRegression*' -or $file.Name -eq 'BudgetProbe.dll' -or $file.Name -like 'PresentationFramework*') { continue }
    try {
        [void][Reflection.AssemblyName]::GetAssemblyName($file.FullName)
        $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($file.FullName))
    } catch [System.BadImageFormatException] { }
}
$trees = [Microsoft.CodeAnalysis.SyntaxTree[]]@(
    [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText((Join-Path $root 'launcher/DesktopProgram.cs'))),
    [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs')))
)
$options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new([Microsoft.CodeAnalysis.OutputKind]::WindowsApplication).WithOptimizationLevel([Microsoft.CodeAnalysis.OptimizationLevel]::Release).WithPlatform([Microsoft.CodeAnalysis.Platform]::X64)
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create('DepoimentoLocal.Desktop.QACompile', $trees, $references, $options)
$stream = [IO.File]::Create((Join-Path $build 'DepoimentoLocal.Desktop.QACompile.dll'))
try { $result = $compilation.Emit($stream) } finally { $stream.Dispose() }
if (-not $result.Success) { $result.Diagnostics | ForEach-Object ToString; throw 'Falha na compilação da aplicação.' }
'COMPILE PASS: DesktopProgram.cs + ModernShell.cs; Release x64; referências .NET 8 locais. Motor binário preservado (fontes ausentes).' | Tee-Object -FilePath (Join-Path $PSScriptRoot 'Results/application-build.txt')
