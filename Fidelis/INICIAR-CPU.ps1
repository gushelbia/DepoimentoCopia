$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$engine = Join-Path $root 'engine'
$appExe = Join-Path $engine 'DepoimentoLocal.exe'
$appDll = Join-Path $engine 'DepoimentoLocal.dll'
$cpuDll = Join-Path $engine 'DepoimentoLocal.CPU-original.dll'
$expectedHash = '08A788B10D55E3B3AC02E78A4444DA891E3B4C95F9535078E139F332C3E8D670'
$marker = Join-Path $root 'MODO-ATIVO.txt'
if (-not (Test-Path $appExe) -or -not (Test-Path $cpuDll)) { Write-Host 'Arquivos do programa nao encontrados.' -ForegroundColor Red; exit 1 }
$h = (Get-FileHash -Algorithm SHA256 -Path $cpuDll).Hash.ToUpperInvariant()
if ($h -ne $expectedHash) { Write-Host ('DLL CPU invalida: ' + $h) -ForegroundColor Red; exit 1 }
Copy-Item $cpuDll $appDll -Force
@('DepoimentoLocal 1.0.5','modo=CPU',('data=' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')),'gpuLayers=0') | Set-Content -Encoding UTF8 $marker
Write-Host '[OK] Abrindo o DepoimentoLocal em CPU.' -ForegroundColor Green
& (Join-Path $root 'ModernUI.ps1') -Engine $engine
exit $LASTEXITCODE
