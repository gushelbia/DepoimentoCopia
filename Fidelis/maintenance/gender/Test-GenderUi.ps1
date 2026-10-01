# Gênero do depoente na janela real, com o modelo: conversão na tela, no
# consolidado, no rascunho, na recuperação e no Word; Masculino e Não informado
# idênticos ao motor; sugestão só com pista clara, sem trocar o campo sozinha.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/gender/Test-GenderUi.ps1
# Frases inventadas; arquivos numa pasta temporária apagada no fim. A cópia de
# recuperação real do usuário é guardada e devolvida.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
$kit = Get-Content (Join-Path $root 'maintenance/dialogs/DialogKit.cs') -Raw -Encoding UTF8
$test = Get-Content (Join-Path $PSScriptRoot 'GenderUiTest.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition ($source + "`n" + $kit + "`n" + $test) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
Write-Output 'COMPILE PASS (.NET Framework compatibility)'
$work = Join-Path $env:TEMP ('DepoimentoLocal-genero-' + [guid]::NewGuid().ToString('N'))
[Environment]::CurrentDirectory = (Join-Path $root 'engine')
Set-Location -LiteralPath (Join-Path $root 'engine')
try { $failures = [GenderUiTest]::Run($root, $work) }
finally { Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue }
if ($failures -ne 0) { exit 1 }
