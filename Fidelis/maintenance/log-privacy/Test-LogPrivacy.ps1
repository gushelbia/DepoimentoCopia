# Uso normal não grava texto no log: gerações reais (motor e modelo) com frases
# inventadas que têm palavras-sentinela; o trecho do log desta execução não pode
# conter nenhuma palavra da entrada nem da saída. Uma geração em modo diagnóstico
# confirma que o texto completo só aparece quando o modo está ligado.
#   powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File maintenance/log-privacy/Test-LogPrivacy.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = [IO.File]::ReadAllText((Join-Path $root 'ui/ModernShell.cs'))
$runner = [IO.File]::ReadAllText((Join-Path $root 'QA/QaRunner.cs'))
Add-Type -TypeDefinition ($source + "`n" + $runner) -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
[Environment]::CurrentDirectory = Join-Path $root 'engine'
$failures = 0; $results = @()
function Check($ok, $name, $detail) { if (-not $ok) { $script:failures++ }; $line = ($(if ($ok) {'PASS '} else {'FAIL '}) + $name + $(if ($detail) {' | ' + $detail} else {''})); $script:results += $line; Write-Output $line }
# Words that the technical log uses by itself (event names, error categories, native messages).
$technical = @('depoente','fidelidade','sujeito','outra','pessoa','apagado','papéis','trocados','trocado','pronome','ambíguo','frase','quebrada','primeira','residual','possível','omissão','oração','texto','omitido','técnico','diagnóstico','modelo','bloco','revisão','saída','ainda','contém','verbos','negação','omitida','alerta','alertas','problema','precisa','reformulação','interrompida','seguintes','foram','processados','trecho','marcado','incompleto','concluído','geração','cancelada','preparando','corrigindo','terceira','carregado','pronto','reformular','localmente','relatou')
$cases = @(
    @{ text = 'Minha vizinha Gertrudes estava assustada e bateu na minha porta.'; sentinel = 'Gertrudes' },
    @{ text = 'Eu empurrei o Valdomiro porque ele estava me enforcando.'; sentinel = 'Valdomiro' },
    @{ text = 'Eu fiquei na Xavantina até tarde e ele me atacou na saída.'; sentinel = 'Xavantina' }
)
foreach ($c in $cases) {
    $g = [QaRunner]::Run($root, $c.text, 600, $false)
    $log = $g.Log
    Check ($g.CompletedNormally -and $g.Output.Length -gt 0) ("uso normal, geração concluída: " + $c.sentinel) $g.Exception
    Check ($log -match '\[GENERATION_START\]' -and $log -match '\[BLOCK_START\]' -and $log -match '\[GENERATION_(OK|FAILED)\]' -and $log -match 'elapsedMs=' -and $log -match 'outputChars=') ("log técnico continua útil (eventos, tempos, tamanhos): " + $c.sentinel) ''
    Check ($log -match 'log=técnico') ("log registra o modo técnico: " + $c.sentinel) ''
    Check ($log -notmatch [regex]::Escape($c.sentinel)) ("sentinela «" + $c.sentinel + "» ausente do log") ''
    Check ($log -notmatch '«|»|“|”') ("nenhum trecho entre aspas no log: " + $c.sentinel) ''
    $leaks = @()
    foreach ($w in [regex]::Matches($c.text + ' ' + $g.Output, '\p{L}{5,}')) {
        $word = $w.Value.ToLowerInvariant()
        if ($technical -contains $word) { continue }
        if ($log -match ('(?i)\b' + [regex]::Escape($w.Value) + '\b')) { $leaks += $w.Value }
    }
    Check ($leaks.Count -eq 0) ("nenhuma palavra da entrada ou da saída no log: " + $c.sentinel) (($leaks | Sort-Object -Unique) -join ', ')
    $repairs = [regex]::Matches($log, 'repairs=([a-z,-]+)') | ForEach-Object { $_.Groups[1].Value }
    $results += ('     reparos registrados: ' + ($(if ($repairs) { ($repairs -join ' | ') } else { '(nenhum)' })))
}
$g = [QaRunner]::Run($root, $cases[1].text, 600, $true)
Check ($g.Log -match 'log=diagnóstico') 'modo diagnóstico (testes): registrado no log' ''
Check ($g.Output.Length -gt 0) 'modo diagnóstico: geração concluída' $g.Exception
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'privacy-results.txt')
$results | Where-Object { $_ -like '     *' }
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
