param()
$ErrorActionPreference='Stop'
$results=Join-Path $PSScriptRoot 'Results'
$review=Get-Content (Join-Path $PSScriptRoot 'manual-review.json') -Raw | ConvertFrom-Json
if(@($review).Count -ne 15){throw 'Revisão incompleta'}
if((Get-Content (Join-Path $results 'qa-report.md') -Raw) -notmatch 'Bateria finalizada: True'){throw 'Execução incompleta'}
$rows=@(1..15 | ForEach-Object {Get-Content (Join-Path $results ('test-{0:d2}.json' -f $_)) -Raw | ConvertFrom-Json})
$summary=Get-Content (Join-Path $results 'summary.json') -Raw | ConvertFrom-Json
$lines=[Collections.Generic.List[string]]::new()
$lines.Add('# Relatório integral — bateria inédita do DepoimentoLocal')
$lines.Add('')
$lines.Add('Avaliação em 29/09/2026. Quinze casos novos, uma execução por entrada, com o retry interno normal. Software, prompt, reparos locais e validador congelados. Nenhuma saída foi corrigida. Execução real pelo OriginalAppBridge e botão Reformular; logs e saídas literais preservados em Results/.')
$lines.Add('')
$lines.Add('Os casos são sintéticos e foram preparados antes da execução, pelo mesmo assistente que conhece as correções anteriores. São textos inéditos, mas não constituem uma avaliação cega independente ou amostra estatística. Resultados não devem ser extrapolados como taxa de acerto em depoimentos reais.')
$lines.Add('')
$lines.Add('## Conclusão')
$lines.Add('')
$lines.Add((Get-Content (Join-Path $PSScriptRoot 'conclusion.txt') -Raw).Trim())
$lines.Add('')
$lines.Add('## Métricas técnicas')
$lines.Add('')
$lines.Add('| Métrica automática | Quantidade |')
$lines.Add('|---|---:|')
foreach($property in $summary.PSObject.Properties){$lines.Add('| '+$property.Name+' | '+$property.Value+' |')}
$lines.Add('')
$lines.Add('A detecção automática de primeira pessoa é lexical e incompleta. A contagem manual abaixo prevalece para a avaliação. Erro/exceção de rejeição semântica não significa necessariamente queda do processo. Rascunho visível após rejeição não é resultado aprovado.')
$lines.Add('')
$lines.Add('## Revisão manual — resultados e gravidade')
$lines.Add('')
$lines.Add('| Caso | Conclusão técnica | Avaliação manual | Gravidade | Alteração semântica identificada |')
$lines.Add('|---|---|---|---|---|')
foreach($f in $review){
 $row=$rows[[int]$f.number-1]
 $hash=(Get-FileHash (Join-Path $results ('test-{0:d2}-output.txt' -f [int]$f.number))).Hash
 if($f.outputSha256 -ne $hash){throw "Revisão não corresponde à saída $($f.number)"}
 $lines.Add('| '+$f.number+' | '+$row.status+' | '+$f.verdict+' | '+$f.severity+' | '+$f.semanticChange+' |')
}
$lines.Add('')
$metrics=[ordered]@{
 'Aprovados integralmente na revisão'=@($review | Where-Object verdict -eq 'Aprovado').Count
 'Com alteração semântica identificada, incluindo rascunhos rejeitados'=@($review | Where-Object semanticChange).Count
 'Com primeira pessoa fora de citações na revisão manual'=@($review | Where-Object {$_.manualFirstPerson.Count -gt 0}).Count
 'Com corte/omissão de trecho final na revisão manual'=@($review | Where-Object truncation).Count
 'Com duplicação factual na revisão manual'=@($review | Where-Object duplication).Count
}
foreach($item in $metrics.GetEnumerator()){$lines.Add('- '+$item.Key+': '+$item.Value)}
$lines.Add('')
foreach($field in @('retry','warnings','semanticChange','approved')){
 $ids=switch($field){
 'retry' {@($rows | Where-Object retry | ForEach-Object number)}
 'warnings' {@($rows | Where-Object {$_.warnings.Count -gt 0} | ForEach-Object number)}
 'semanticChange' {@($review | Where-Object semanticChange | ForEach-Object number)}
 'approved' {@($review | Where-Object verdict -eq 'Aprovado' | ForEach-Object number)}
 }
 $label=switch($field){'retry' {'Casos com retry'} 'warnings' {'Casos com warning do validador'} 'semanticChange' {'Casos com alteração semântica'} 'approved' {'Casos aprovados integralmente'}}
 $lines.Add('- '+$label+': '+$(if($ids.Count){$ids -join ', '}else{'nenhum'})+'.')
}
$lines.Add('')
$lines.Add('Gravidade: crítica para inversão de percepção/negação ou falsa atribuição grave; alta para troca de fonte/sujeito, certeza indevida, mudança cronológica ou omissão relevante; moderada para ambiguidade introduzida ou problema estrutural; estilo para redação sem mudança identificada de conteúdo. Falhas operacionais são registradas separadamente. A classificação é preliminar e permite a revisão do usuário.')
$lines.Add('')
$lines.Add('## Evidências integrais por caso')
foreach($f in $review){
 $row=$rows[[int]$f.number-1]; $stem='test-{0:d2}' -f [int]$f.number
 $lines.Add('');$lines.Add('### Caso '+$f.number+' — '+$row.name);$lines.Add('')
 $lines.Add('Objetivo: '+$row.objective);$lines.Add('')
 $lines.Add('**Resultado:** '+$f.verdict+'. **Gravidade:** '+$f.severity+'.');$lines.Add('')
 $lines.Add('**Achados:** '+$f.findings);$lines.Add('')
 $lines.Add('**Demais verificações semânticas:** '+$f.preserved);$lines.Add('')
 $lines.Add('**Redação:** '+$f.style);$lines.Add('')
 $lines.Add('**Primeira pessoa fora de citações — revisão manual:** '+$(if($f.manualFirstPerson.Count){$f.manualFirstPerson -join '; '}else{'nenhuma identificada'})+'.');$lines.Add('')
 $lines.Add('**Registro técnico:** '+$row.status+'; retry='+$row.retry+'; erro='+$row.hasError+'; exceção='+$row.hasException+'; overflow='+$row.contextOverflowException+'; indício automático de truncamento='+$row.truncationSuspected+'; duplicação integral='+$row.fullDuplication+'; parágrafos repetidos='+$row.repeatedParagraphs+'.');$lines.Add('')
 $lines.Add('Tempo de geração: '+$row.generationSeconds+' s. Entrada: '+$row.inputChars+' caracteres. Saída: '+$row.outputChars+' caracteres. Captura conferida por GENERATION_OK: '+$(if($null -eq $row.outputCaptureVerified){'não disponível; rascunho capturado da tela'}else{$row.outputCaptureVerified})+'.');$lines.Add('')
 $lines.Add('**Warnings do validador:**');$lines.Add('')
 if($row.warnings.Count){foreach($w in $row.warnings){$lines.Add('- '+$w)}}else{$lines.Add('Nenhum.')}
 $lines.Add('');$lines.Add('**Warnings nativos:** '+$row.nativeWarnings.Count+'. Constam no [log integral](Results/'+$stem+'-log.txt), preservados separadamente dos warnings do validador.');$lines.Add('')
 if($row.exception){$lines.Add('**Erro/exceção integral registrado:**');$lines.Add('');$lines.Add('```text');$lines.Add($row.exception);$lines.Add('```');$lines.Add('')}
 $lines.Add('**Entrada integral:**');$lines.Add('');$lines.Add('```text');$lines.Add($row.input);$lines.Add('```');$lines.Add('')
 $lines.Add('**Saída integral, sem edição'+$(if(!$row.completedNormally){' — rascunho de geração rejeitada'}else{''})+':**');$lines.Add('');$lines.Add('```text');$lines.Add($row.output);$lines.Add('```');$lines.Add('')
 $lines.Add('SHA-256 da saída: `'+$f.outputSha256+'`. [Saída original](Results/'+$stem+'-output.txt) · [Registro completo](Results/'+$stem+'.json).')
}
$lines.Add('');$lines.Add('## Integridade e limites');$lines.Add('')
$lines.Add((Get-Content (Join-Path $PSScriptRoot 'integrity-final.txt') -Raw).Trim());$lines.Add('')
$lines.Add('A revisão comparou integralmente entradas e saídas quanto a sujeito, fonte, percepção, negação, incerteza, ambiguidade, temporalidade, citações, omissões, acréscimos, nomes e cargos. Nenhum warning foi usado como substituto dessa comparação. O log não expõe a razão de parada EOS/limite de tokens; não se atribui causa de truncamento sem evidência. O aplicativo não foi corrigido durante ou após a avaliação. A etapa está encerrada aguardando nova instrução.')
[IO.File]::WriteAllLines((Join-Path $PSScriptRoot 'RELATORIO-INTEGRAL.md'),$lines,[Text.UTF8Encoding]::new($false))
$metrics | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'manual-metrics.json') -Encoding utf8
