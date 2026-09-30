param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Output,
    [switch]$Console
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$engine = Join-Path $root 'engine'
$tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText((Resolve-Path $Source)))
$references = [Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($file in Get-ChildItem -LiteralPath $engine -Filter '*.dll' -File) {
    if ($file.Name -like 'DepoimentoLocal*' -or $file.Name -like 'ContextRegression*') { continue }
    try {
        [void][Reflection.AssemblyName]::GetAssemblyName($file.FullName)
        $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($file.FullName))
    } catch [System.BadImageFormatException] { }
}
$kind = if ($Console) { [Microsoft.CodeAnalysis.OutputKind]::ConsoleApplication } else { [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary }
$options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new($kind).WithOptimizationLevel([Microsoft.CodeAnalysis.OptimizationLevel]::Release)
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create([IO.Path]::GetFileNameWithoutExtension($Output), [Microsoft.CodeAnalysis.SyntaxTree[]]@($tree), $references, $options)
$stream = [IO.File]::Create([IO.Path]::GetFullPath($Output))
try { $result = $compilation.Emit($stream) } finally { $stream.Dispose() }
if (-not $result.Success) { $result.Diagnostics | ForEach-Object { Write-Output $_.ToString() }; throw 'C# compilation failed' }
Write-Output "COMPILE PASS: $Output (bundled .NET 8 references)"
