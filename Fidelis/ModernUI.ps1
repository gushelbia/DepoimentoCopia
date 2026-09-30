param(
    [Parameter(Mandatory=$true)][string]$Engine
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$source = Join-Path $root 'ui\ModernShell.cs'
$appExe = Join-Path $Engine 'DepoimentoLocal.exe'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

try {
    if (-not (Test-Path $source)) { throw "Arquivo da interface moderna não encontrado: $source" }
    if (-not (Test-Path $appExe)) { throw "Executável do motor não encontrado: $appExe" }

    $backendDescription = ''
    $marker = Join-Path $root 'MODO-ATIVO.txt'
    if (Test-Path $marker) {
        try {
            $markerLines = Get-Content -Path $marker -ErrorAction Stop
            $modeLine = $markerLines | Where-Object { $_ -match '^modo=' } | Select-Object -First 1
            $gpuLine = $markerLines | Where-Object { $_ -match '^gpuLayers=' } | Select-Object -First 1
            $mode = if ($modeLine) { ($modeLine -replace '^modo=', '').Trim() } else { '' }
            $gpuLayers = 0
            if ($gpuLine) { [void][int]::TryParse((($gpuLine -replace '^gpuLayers=', '').Trim()), [ref]$gpuLayers) }
            if ($mode -match 'VULKAN') {
                if ($gpuLayers -gt 0) { $backendDescription = "Vulkan • $gpuLayers camadas GPU" }
                else { $backendDescription = 'Vulkan' }
            } elseif ($mode -eq 'CPU') {
                $backendDescription = 'CPU'
            }
        } catch { }
    }

    $code = Get-Content -Raw -Encoding UTF8 -Path $source
    Add-Type -TypeDefinition $code -Language CSharp -ReferencedAssemblies @(
        'System.dll',
        'System.Core.dll',
        'System.Windows.Forms.dll',
        'System.Drawing.dll',
        'System.IO.Compression.dll',
        'System.IO.Compression.FileSystem.dll'
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $appExe
    $psi.WorkingDirectory = $Engine
    $psi.UseShellExecute = $true
    $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Minimized
    $process = [System.Diagnostics.Process]::Start($psi)
    if ($null -eq $process) { throw 'Não foi possível iniciar o motor do Fidelis.' }

    # Compatibilidade com o caminho relativo do modelo gravado pelo executável.
    Set-Location -LiteralPath $Engine
    $icon = Join-Path $root 'ui\Fidelis.ico'
if (-not (Test-Path -LiteralPath $icon)) { $icon = Join-Path $root 'ui\DepoimentoLocal.ico' }
    [ModernShell]::Run($process, $icon, $backendDescription)
}
catch {
    try {
        if ($null -ne $process -and -not $process.HasExited) { $process.Kill() }
    } catch { }
    try {
        [System.Windows.Forms.MessageBox]::Show(
            $_.Exception.Message,
            'Fidelis — erro ao abrir interface',
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Error
        ) | Out-Null
    } catch { }
    exit 1
}
exit 0
