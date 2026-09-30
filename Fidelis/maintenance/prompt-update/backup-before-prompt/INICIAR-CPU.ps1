$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$engine = Join-Path $root 'engine'
$appExe = Join-Path $engine 'DepoimentoLocal.exe'
$appDll = Join-Path $engine 'DepoimentoLocal.dll'
$cpuDll = Join-Path $engine 'DepoimentoLocal.CPU-original.dll'
$expectedHash = '9A50741B48E9FDCA7382029F3C1A0983EE419679EDD216DE491A66EA83283A2E'
$marker = Join-Path $root 'MODO-ATIVO.txt'
if (-not (Test-Path $appExe) -or -not (Test-Path $cpuDll)) { Write-Host 'Arquivos do programa nao encontrados.' -ForegroundColor Red; exit 1 }
$h = (Get-FileHash -Algorithm SHA256 -Path $cpuDll).Hash.ToUpperInvariant()
if ($h -ne $expectedHash) { Write-Host ('DLL CPU invalida: ' + $h) -ForegroundColor Red; exit 1 }
Copy-Item $cpuDll $appDll -Force
@('DepoimentoLocal 1.0.5','modo=CPU',('data=' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')),'gpuLayers=0') | Set-Content -Encoding UTF8 $marker
Write-Host '[OK] Abrindo o DepoimentoLocal em CPU.' -ForegroundColor Green
& (Join-Path $root 'ModernUI.ps1') -Engine $engine
exit $LASTEXITCODE
