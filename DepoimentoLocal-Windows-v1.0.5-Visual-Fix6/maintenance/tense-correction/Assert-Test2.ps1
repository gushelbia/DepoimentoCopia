$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$work=Join-Path $root 'maintenance/context-fix'
$output=Get-Content (Join-Path $work 'automatic-retry-output.txt') -Raw
$log=Get-Content (Join-Path $work 'automatic-retry-log.txt') -Raw
$report=[Collections.Generic.List[string]]::new()
$checks=[ordered]@{
 'Validador aceitou a geração'=$log.Contains('[GENERATION_OK]')
 'Sem estouro de contexto'= -not $log.Contains('ContextOverflowException')
 'Presente: significado da fala'=$output.Contains('não sabe o que ela quis dizer')
 'Presente: mesmo dia'=$output.Contains('não sabe se foi no mesmo dia')
 'Sem não sabia indevido'= -not ($output -match '\bnão sabia\b')
 'Autocorreção consolidada'=$output.Contains('Carlos entrou antes de sua saída.')
 'Sem falso começo saiu'= -not ($output -match 'saiu\s*(\.{2,}|…)')
 'Fonte Carlos e ação Paulo'=$output.Contains('Carlos lhe disse depois que Paulo gritou com ela, mas não ouviu esse grito.')
 'Limitação de memória preservada'=($output -match 'não (se )?lembra quem')
 'Incerteza de ordem preservada'=$output.Contains('pode estar confundindo a ordem')
 'Contexto 4096'=$log.Contains('context=4096')
}
foreach($item in $checks.GetEnumerator()){
 $report.Add("$(if($item.Value){'PASS'}else{'FAIL'}) $($item.Key)")
}
$report | Set-Content (Join-Path $PSScriptRoot 'teste2-results.txt')
$report
if($checks.Values -contains $false){throw 'Teste 2 falhou'}
foreach($name in @('automatic-retry-log.txt','automatic-retry-output.txt','automatic-retry-results.txt')){Copy-Item -LiteralPath (Join-Path $work $name) -Destination $PSScriptRoot -Force}
