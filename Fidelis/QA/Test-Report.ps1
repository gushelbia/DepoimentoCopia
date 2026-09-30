$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Report.ps1')
$case = [pscustomobject]@{number=7;name='Fixture';objective='Verificar detector';input='Eu ouvi a fala.'}
function Analyze([string]$output, [string]$log='', [bool]$normal=$true) {
    if (-not $log) { $log = '[GENERATION_OK] elapsedMs=1000; outputChars=' + $output.Length }
    Get-QaResult $case ([pscustomobject]@{Output=$output;Log=$log;Exception='';CompletedNormally=$normal;GenerationSeconds=1;TotalSeconds=2})
}
function Assert($condition, $name) { if (-not $condition) { throw "FAIL: $name" }; "PASS: $name" }
Assert ((Analyze '').emptyOutput) 'saída vazia'
$quoted = 'Relatou que ouviu: ' + [char]0x201c + 'eu não vou assinar isso agora' + [char]0x201d + '.'
Assert ((Analyze $quoted).firstPerson.Count -eq 0) 'primeira pessoa em citação ignorada'
Assert ((Analyze 'Eu não sei o que aconteceu.').firstPerson.Count -gt 0) 'primeira pessoa fora de citação detectada'
Assert ((Analyze 'Não verifiquei isso pessoalmente.').firstPerson -contains 'verifiquei') 'primeira pessoa verbal sem pronome explícito'
$paragraph = 'Relatou que não presenciou a conversa entre os servidores e não sabe o que aconteceu na sala.'
Assert ((Analyze ($paragraph + "`n`n" + $paragraph)).fullDuplication) 'duplicação integral detectada'
Assert ((Analyze ($paragraph + "`n" + $paragraph + "`nOutra frase distinta.")).repeatedParagraphs) 'repetição de parágrafo detectada'
Assert (-not (Analyze ($paragraph + ' Depois saiu.')).fullDuplication) 'texto distinto sem duplicação'
Assert (-not (Analyze $paragraph).hasError) 'captura integral normal sem erro técnico'
Assert ((Analyze $paragraph '[GENERATION_OK] outputChars=99999').hasError) 'captura parcial divergente do log rejeitada'
Assert ((Analyze 'Relatou que chegou ao').truncationSuspected) 'final incompleto sinalizado'
Assert ((Analyze '' '[GENERATION_FAILED] ContextOverflowException' $false).contextOverflowException) 'overflow registrado'
Assert ((Analyze '' '[GENERATION_FAILED]' $false).hasError) 'falha não classificada como sucesso'
Assert ((Analyze $paragraph '[BLOCK_RETRY_START] issue=uma negação pode ter sido omitida').retry) 'retry registrado'
Assert ((Analyze $paragraph '[BLOCK_ACCEPTED_WITH_WARNING] issue=uma negação pode ter sido omitida').negationWarning) 'warning de negação do validador'
$case.number = 5
$ending = 'Não sabe se houve alguma medida administrativa. Não participou de nenhuma outra reunião. Tudo o que sabe é o que presenciou na sala e o que algumas pessoas lhe disseram posteriormente.'
Assert ((Analyze $ending).missingEndingMarkers.Count -eq 0) 'três marcadores do final encontrados'
Assert ((Analyze $paragraph).missingEndingMarkers.Count -eq 3) 'ausência dos três marcadores sinalizada'
