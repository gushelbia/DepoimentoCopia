$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$engine = Join-Path $root 'engine'
$appDll = Join-Path $engine 'DepoimentoLocal.dll'
$appExe = Join-Path $engine 'DepoimentoLocal.exe'
$vulkanDll = Join-Path $engine 'DepoimentoLocal.Vulkan8.dll'
$vulkanDest = Join-Path $engine 'runtimes\win-x64\native\vulkan'
$cpuDest = Join-Path $engine 'runtimes\win-x64\native\avx2'
$vulkanRequired = @('ggml.dll','ggml-base.dll','ggml-vulkan.dll','llama.dll','mtmd.dll')
$cpuRequired = @('ggml.dll','ggml-base.dll','ggml-cpu.dll','llama.dll','mtmd.dll')
$expectedHash = '4C8AC6BB79F22BD437E3BD6354E1D1449877BA7E3DBCD3F2BD646FB1999EC2F7'
$marker = Join-Path $root 'MODO-ATIVO.txt'

function Fail([string]$msg) {
  Write-Host ''
  Write-Host ('ERRO: ' + $msg) -ForegroundColor Red
  exit 1
}
function Check-File([string]$path, [int64]$minSize = 1024) {
  return (Test-Path $path) -and ((Get-Item $path).Length -ge $minSize)
}

Write-Host '============================================================' -ForegroundColor Cyan
Write-Host ' DEPOIMENTOLOCAL 1.0.5 - VULKAN8' -ForegroundColor Cyan
Write-Host '============================================================' -ForegroundColor Cyan

if (-not (Check-File $appExe 10000)) { Fail 'Executavel principal nao encontrado.' }
if (-not (Check-File $vulkanDll 50000)) { Fail 'DLL Vulkan8 nao encontrada.' }
$vh = (Get-FileHash -Algorithm SHA256 -Path $vulkanDll).Hash.ToUpperInvariant()
if ($vh -ne $expectedHash) { Fail ('DLL Vulkan8 invalida: ' + $vh) }

foreach ($f in $cpuRequired) {
  if (-not (Check-File (Join-Path $cpuDest $f))) { Fail ('Backend CPU AVX2 incompleto: ' + $f) }
}

$vulkanLoader = Join-Path $env:SystemRoot 'System32\vulkan-1.dll'
if (-not (Test-Path $vulkanLoader)) {
  Write-Host '[AVISO] Vulkan do Windows nao encontrado. Abra INICIAR-CPU.bat.' -ForegroundColor Yellow
  exit 1
}

$backendOk = $true
foreach ($f in $vulkanRequired) {
  if (-not (Check-File (Join-Path $vulkanDest $f))) { $backendOk = $false; break }
}

if (-not $backendOk) {
  $tmp = Join-Path $env:TEMP ('DepoimentoLocal-Vulkan-' + [guid]::NewGuid().ToString('N'))
  $pkg = Join-Path $tmp 'backend.nupkg'
  $unpack = Join-Path $tmp 'unpack'
  try {
    New-Item -ItemType Directory -Path $tmp -Force | Out-Null
    New-Item -ItemType Directory -Path $unpack -Force | Out-Null
    $url = 'https://www.nuget.org/api/v2/package/LLamaSharp.Backend.Vulkan.Windows/0.27.0'
    Write-Host '[...] Preparando backend Vulkan oficial LLamaSharp 0.27.0...' -ForegroundColor Cyan
    try {
      Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $pkg
    } catch {
      & curl.exe -L --fail --silent --show-error -o $pkg $url
      if ($LASTEXITCODE -ne 0) { throw 'download do backend Vulkan falhou' }
    }
    if (-not (Check-File $pkg 1000000)) { throw 'pacote Vulkan ausente ou incompleto' }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($pkg, $unpack)
    $src = Join-Path $unpack 'LLamaSharpRuntimes\win-x64\native\vulkan'
    if (-not (Test-Path $src)) { throw 'pasta Vulkan nao encontrada no pacote' }
    New-Item -ItemType Directory -Path $vulkanDest -Force | Out-Null
    foreach ($f in $vulkanRequired) {
      $from = Join-Path $src $f
      if (-not (Check-File $from)) { throw ('arquivo Vulkan invalido: ' + $f) }
      Copy-Item $from (Join-Path $vulkanDest $f) -Force
    }
  } catch {
    if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue }
    Write-Host ('[AVISO] Vulkan nao pode ser preparado: ' + $_.Exception.Message) -ForegroundColor Yellow
    Write-Host 'Abra INICIAR-CPU.bat para usar o programa em CPU.' -ForegroundColor Yellow
    exit 1
  } finally {
    if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue }
  }
}

foreach ($f in $vulkanRequired) {
  if (-not (Check-File (Join-Path $vulkanDest $f))) { Fail ('Backend Vulkan incompleto: ' + $f) }
}

# Sempre restaura a DLL Vulkan8 conhecida e testada antes de abrir.
Copy-Item $vulkanDll $appDll -Force
$activeHash = (Get-FileHash -Algorithm SHA256 -Path $appDll).Hash.ToUpperInvariant()
if ($activeHash -ne $expectedHash) { Fail ('Falha ao ativar a DLL Vulkan8: ' + $activeHash) }

@(
  'DepoimentoLocal 1.0.5',
  'modo=VULKAN8',
  ('data=' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')),
  'gpuLayers=8'
) | Set-Content -Encoding UTF8 $marker

Write-Host '[OK] Vulkan8 confirmado. Abrindo o DepoimentoLocal...' -ForegroundColor Green
& (Join-Path $root 'ModernUI.ps1') -Engine $engine
exit $LASTEXITCODE
