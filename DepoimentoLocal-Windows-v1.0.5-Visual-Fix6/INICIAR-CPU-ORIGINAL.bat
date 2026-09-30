@echo off
setlocal
cd /d "%~dp0"
title DepoimentoLocal 1.0.5 - CPU - interface original
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0INICIAR-CPU-ORIGINAL.ps1"
if errorlevel 1 pause
