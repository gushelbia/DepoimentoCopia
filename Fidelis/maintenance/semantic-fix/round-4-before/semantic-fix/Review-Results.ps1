# Combines human semantic findings with immutable outputs from the real runner.
# Never modifies fixtures or generated text; run after the suite finishes.
param([string]$PreviousRun='baseline', [string]$ReviewFile='semantic-review.json', [string]$OutputName='comparison.md')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$current=Join-Path $root 'QA/Results'
$findings=Get-Content (Join-Path $PSScriptRoot $ReviewFile) -Raw | ConvertFrom-Json
if(@($findings).Count -ne 10 -or @($findings.number | Sort-Object -Unique).Count -ne 10){throw 'Revisão deve abranger exatamente os dez casos.'}
if((Get-Content (Join-Path $current 'qa-report.md') -Raw) -notmatch 'Bateria finalizada: True'){throw 'A bateria ainda não terminou.'}
$lines=[Collections.Generic.List[string]]::new()
$lines.Add('# Comparação semântica e técnica — DepoimentoLocal')
$lines.Add('')
$lines.Add('Entradas idênticas ao anexo, sem alterações. Saídas obtidas pelo OriginalAppBridge de produção; nenhuma saída foi editada. Execução anterior preservada em `'+$PreviousRun+'`. A aprovação semântica abaixo é uma revisão manual desta execução, não uma garantia geral do modelo ou do validador.')
$lines.Add('')
$lines.Add('| Teste | Antes: problema relevante | Depois: revisão semântica | Estado técnico / retry |')
$lines.Add('|---|---|---|---|')
foreach($f in $findings){
    $row=Get-Content (Join-Path $current ('test-{0:d2}.json' -f [int]$f.number)) -Raw | ConvertFrom-Json
    $actualHash=(Get-FileHash (Join-Path $current ('test-{0:d2}-output.txt' -f [int]$f.number))).Hash
    if(!$f.outputSha256 -or $f.outputSha256 -ne $actualHash){throw "A revisão semântica não corresponde à saída atual do caso $($f.number)."}
    $lines.Add('| '+$f.number+' | '+$f.before+' | **'+$f.verdict+'** — '+$f.after+' | '+$row.status+' / '+$row.retry+' |')
}
$lines.Add('')
$lines.Add('## Métricas da mesma bateria')
$lines.Add('')
$before=Get-Content (Join-Path $PSScriptRoot "$PreviousRun/summary.json") -Raw | ConvertFrom-Json
$after=Get-Content (Join-Path $current 'summary.json') -Raw | ConvertFrom-Json
$lines.Add('| Métrica | Antes | Depois |')
$lines.Add('|---|---:|---:|')
foreach($property in $before.PSObject.Properties){
    $lines.Add('| '+$property.Name+' | '+$property.Value+' | '+$after.($property.Name)+' |')
}
$lines.Add('')
$lines.Add('Warnings incluem tentativas rejeitadas e recuperadas no retry. Ausência de warning não equivale a aprovação semântica. Rejeição final significa que a tarefa de reformulação daquele caso não foi concluída, mesmo quando a proteção evitou aceitar um erro.')
$lines.Add('')
$lines.Add('## Evidências por caso')
foreach($f in $findings){
    $stem='test-{0:d2}' -f [int]$f.number
    $old=Get-Content (Join-Path $PSScriptRoot "$PreviousRun/$stem.json") -Raw | ConvertFrom-Json
    $row=Get-Content (Join-Path $current "$stem.json") -Raw | ConvertFrom-Json
    if($old.input -cne $row.input){throw "Entrada alterada no caso $($f.number)"}
    $lines.Add('')
    $lines.Add('### Teste '+$f.number)
    $lines.Add('')
    $lines.Add($f.details)
    $lines.Add('')
    $lines.Add('Antes: '+$old.outputChars+' caracteres; depois: '+$row.outputChars+' caracteres. Logs e saídas integrais em `'+$PreviousRun+'/'+$stem+'*` e `../../QA/Results/'+$stem+'*`.')
}
$lines.Add('')
$lines.Add('## Compilação, limites e arquivos')
$lines.Add('')
$lines.Add('Compilados: launcher/interface (Release x64), interface + runner e SemanticGuard.cs com referências .NET 8. O projeto/fonte completo do motor não existe nesta distribuição; as correções compiladas foram incorporadas nas três DLLs usando a infraestrutura dnlib existente. Conferência de IL preserva todos os métodos fora da lista explícita de alterações. A integridade dos binários durante a bateria está em QA/Results/engine-integrity.json.')
$lines.Add('')
$lines.Add('Consulte README.md para os arquivos modificados e os comandos de reprodução, diagnosis.md para causas e limites da evidência, guard-tests.txt e embedded-tests.txt para verificações adicionais. Os testes anteriores de aspecto, quantidade e reparos passaram. O validador continua heurístico: aceitações técnicas não dispensam revisão humana, e paráfrases legítimas podem provocar rejeições conservadoras.')
[IO.File]::WriteAllLines((Join-Path $PSScriptRoot $OutputName),$lines,[Text.UTF8Encoding]::new($false))
