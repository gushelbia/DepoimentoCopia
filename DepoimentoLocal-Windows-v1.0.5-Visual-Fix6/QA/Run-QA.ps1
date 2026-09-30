param([int]$TimeoutSeconds = 1200)
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Execute com powershell.exe (Windows PowerShell 5.1), não pwsh.' }
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'Report.ps1')
$results = Join-Path $PSScriptRoot 'Results'
New-Item -ItemType Directory -Force -Path $results | Out-Null
$mutex = New-Object Threading.Mutex($false, 'Local\DepoimentoLocal-QA')
if (-not $mutex.WaitOne(0)) { $mutex.Dispose(); throw 'Já existe uma bateria QA em execução.' }
try {
    $source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
    $runner = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'QaRunner.cs'))
    Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
    'COMPILE PASS: interface de produção e runner QA' | Tee-Object -FilePath (Join-Path $results 'build.txt')
    [Environment]::CurrentDirectory = Join-Path $root 'engine'
    $fixtures = @(Get-ChildItem (Join-Path $PSScriptRoot 'TestCases') -Filter '*.json' | Sort-Object Name | ForEach-Object {
        $case = Get-Content -Raw -Encoding UTF8 $_.FullName | ConvertFrom-Json
        if ($case.inputFile) { $case.input = [IO.File]::ReadAllText((Join-Path $_.DirectoryName $case.inputFile)).TrimEnd("`r", "`n") }
        if (-not $case.number -or -not $case.name -or -not $case.objective -or [string]::IsNullOrWhiteSpace($case.input)) { throw "Caso inválido: $($_.Name)" }
        $case
    })
    if ($fixtures.Count -ne 10 -or (@($fixtures.number | Sort-Object -Unique).Count -ne 10)) { throw 'São necessários dez casos com números únicos.' }
    $before = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash -Algorithm SHA256)
    $manifest = foreach ($file in @('ui/ModernShell.cs','launcher/DesktopProgram.cs','engine/DepoimentoLocal.dll','engine/DepoimentoLocal.Vulkan8.dll','modelo/qwen2.5-3b-instruct-q4_k_m.gguf')) {
        $full = Join-Path $root $file
        [ordered]@{path=$file;sha256=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash;bytes=(Get-Item -LiteralPath $full).Length}
    }
    $manifest | ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $results 'production-manifest.json')
    $rows = @()
    $suite = [Diagnostics.Stopwatch]::StartNew()
    Write-QaReport -rows @() -results $results -seconds 0 -finished $false
    foreach ($case in $fixtures) {
        Write-Output ("TESTE {0}: {1}" -f $case.number, $case.name)
        try { $generated = [QaRunner]::Run($root, $case.input, $TimeoutSeconds) }
        catch { $generated = [pscustomobject]@{Output='';Log='';Exception=$_.Exception.ToString();GenerationSeconds=0;TotalSeconds=0;CompletedNormally=$false} }
        $row = Get-QaResult $case $generated
        $rows += $row
        $stem = 'test-{0:d2}' -f [int]$case.number
        [IO.File]::WriteAllText((Join-Path $results "$stem-output.txt"), $generated.Output, [Text.Encoding]::UTF8)
        [IO.File]::WriteAllText((Join-Path $results "$stem-log.txt"), $generated.Log, [Text.Encoding]::UTF8)
        $row | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 (Join-Path $results "$stem.json")
        Write-QaReport $rows $results $suite.Elapsed.TotalSeconds $false
        Write-Output ("RESULTADO {0}: {1}; {2:N1}s; warnings={3}; retry={4}" -f $case.number,$row.status,$row.generationSeconds,$row.warnings.Count,$row.retry)
    }
    $after = @(Get-ChildItem (Join-Path $root 'engine') -Filter 'DepoimentoLocal*.dll' | Get-FileHash -Algorithm SHA256)
    $unchanged = -not (Compare-Object ($before | ForEach-Object { $_.Path + ':' + $_.Hash }) ($after | ForEach-Object { $_.Path + ':' + $_.Hash }))
    [ordered]@{unchanged=$unchanged;before=$before;after=$after} | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $results 'engine-integrity.json')
    Write-QaReport $rows $results $suite.Elapsed.TotalSeconds $true
    if (-not $unchanged) { throw 'Binários de produção mudaram durante a execução; consulte engine-integrity.json.' }
    Write-Output (Join-Path $results 'qa-report.md')
} finally { $mutex.ReleaseMutex(); $mutex.Dispose() }
