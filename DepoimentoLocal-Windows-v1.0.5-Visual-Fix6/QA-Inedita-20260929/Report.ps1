function Get-QaResult($case, $generated) {
    $output = [string]$generated.Output
    $log = [string]$generated.Log
    $modelConfig = [regex]::Match($log, '\[MODEL_CONFIG\] ([^\r\n]+)').Groups[1].Value
    $modelFile = [regex]::Match($log, '\[MODEL_LOAD_START\] ([^\r\n]+)').Groups[1].Value
    $warnings = @($log -split '\r?\n' | Where-Object { $_ -match '\[(BLOCK_ACCEPTED_WITH_WARNING|BLOCK_REJECTED|BLOCK_RETRY_START)\]' })
    $nativeWarnings = @($log -split '\r?\n' | Where-Object { $_ -match '\[NATIVE-WARNING\]' })
    $observations = New-Object 'Collections.Generic.List[string]'
    $empty = [string]::IsNullOrWhiteSpace($output)
    if ($empty) { $observations.Add('Saída vazia.') }
    $normalized = [regex]::Replace($output.Trim(), '\s+', ' ')
    $duplicate = $normalized -cmatch '^(.{80,}?)(?:\s*\1)+$'
    if ($duplicate) { $observations.Add('Duplicação integral exata detectada após normalizar espaços.') }
    $paragraphs = @($output -split '\r?\n' | ForEach-Object { [regex]::Replace($_.Trim(), '\s+', ' ') } | Where-Object { $_.Length -ge 60 })
    $repeated = @($paragraphs | Group-Object -CaseSensitive | Where-Object Count -gt 1)
    foreach ($p in $repeated) { $observations.Add("Parágrafo repetido $($p.Count) vezes: $($p.Name)") }
    # Ignore paired literal quotes; unmatched quotes remain visible to the check.
    $unquoted = [regex]::Replace($output, '(?s)\u201c[^\u201d]*\u201d|"[^"]*"|«[^»]*»|\u2018[^\u2019]*\u2019', ' ')
    $firstPerson = @([regex]::Matches($unquoted, '(?i)\b(eu|nós|comigo|meu|minha|meus|minhas|nosso|nossa|nossos|nossas|ouvi|presenciei|participei|cheguei|vi|verifiquei|perguntei|voltei|saí|cumprimentei|trabalhei|falei|achei|percebi|entendi|consegui)\b|\bnão\s+(sei|lembro|tenho|posso)\b') | ForEach-Object Value | Sort-Object -Unique)
    if ($firstPerson.Count) { $observations.Add('Indício de primeira pessoa fora de citações: ' + ($firstPerson -join ', ') + '. Revisar manualmente.') }
    $negativeWarning = [bool]($warnings -match 'negação|negações')
    if ($negativeWarning) { $observations.Add('O validador de produção apontou possível perda de negação; não é conclusão semântica do QA.') }
    $missingEnd = @()
    $unfinished = -not $empty -and $output.TrimEnd() -notmatch '[.!?…]["\u201d»'')\]]*$'
    $truncation = $unfinished -or [bool]($warnings -match 'trunc|incomplet|cortad') -or $missingEnd.Count -gt 0
    if ($unfinished) { $observations.Add('Possível truncamento: saída sem pontuação de encerramento.') }
    $overflow = $log.Contains('ContextOverflowException') -or ([string]$generated.Exception).Contains('ContextOverflowException')
    $exception = [string]$generated.Exception
    if ($log -match '\[GENERATION_FAILED\]|\[MODEL_LOAD_FAILED\]') {
        $errorLog = $log.Substring([Math]::Max(0, $log.IndexOf('[ERROR]')))
        if (-not $exception.Contains($errorLog)) { $exception += "`n" + $errorLog }
    }
    $hasError = -not $generated.CompletedNormally -or -not [string]::IsNullOrEmpty($exception)
    $loggedChars = [regex]::Match($log, '\[GENERATION_OK\][^\r\n]*outputChars=(\d+)')
    $outputCaptureVerified = if ($loggedChars.Success) { [int]$loggedChars.Groups[1].Value -eq $output.Length } else { $null }
    if ($generated.CompletedNormally -and -not $outputCaptureVerified) {
        $observations.Add('Tamanho capturado diverge do tamanho final registrado no motor, ou registro ausente; verificar captura integral.')
        $hasError = $true
    }
    $status = if ($hasError) {'ERRO'} elseif ($empty -or $duplicate -or $repeated.Count -or $firstPerson.Count -or $truncation -or $warnings.Count) {'CONCLUÍDO COM ALERTAS'} else {'CONCLUÍDO SEM ALERTAS AUTOMÁTICOS'}
    $timing = [regex]::Match($log, '\[GENERATION_(?:OK|FAILED|CANCELLED)\].*?elapsedMs=(\d+)')
    $generationSeconds = if ($timing.Success) {[double]$timing.Groups[1].Value / 1000} else {$generated.GenerationSeconds}
    [pscustomobject][ordered]@{
        number=$case.number; name=$case.name; objective=$case.objective; status=$status
        input=$case.input; output=$output; inputChars=$case.input.Length; outputChars=$output.Length
        inputWords=([regex]::Matches($case.input, '\S+').Count); outputWords=([regex]::Matches($output, '\S+').Count)
        generationSeconds=$generationSeconds; totalSeconds=$generated.TotalSeconds
        modelConfig=$modelConfig; modelFile=$modelFile; outputCaptureVerified=$outputCaptureVerified
        warnings=$warnings; nativeWarnings=$nativeWarnings; retry=$log.Contains('[BLOCK_RETRY_START]')
        exception=$exception; hasException=([bool]($exception -match '\b\w*Exception\b')); hasError=$hasError; contextOverflowException=$overflow; completedNormally=$generated.CompletedNormally
        emptyOutput=$empty; fullDuplication=$duplicate; repeatedParagraphs=($repeated.Count -gt 0)
        firstPerson=$firstPerson; negationWarning=$negativeWarning; truncationSuspected=[bool]$truncation
        missingEndingMarkers=$missingEnd; observations=@($observations)
    }
}

function Write-QaReport($rows, $results, $seconds, $finished) {
    $text = New-Object Text.StringBuilder
    [void]$text.AppendLine('# QA / regressão — DepoimentoLocal')
    [void]$text.AppendLine('')
    [void]$text.AppendLine("Atualizado: $(Get-Date -Format o). Bateria finalizada: $finished.")
    [void]$text.AppendLine('')
    [void]$text.AppendLine('Execução real, sequencial, via OriginalAppBridge da interface de produção e botão Reformular do motor. Mesmos binários, prompt, reparos locais, validador e retry do aplicativo; nenhum resultado foi corrigido pelo QA. Cada caso usa uma instância própria. A compilação dos fontes disponíveis (interface e runner) está em build.txt; o fonte/projeto do motor não acompanha a distribuição, portanto ele é reutilizado sem recompilação.')
    [void]$text.AppendLine('')
    [void]$text.AppendLine('Truncamento, primeira pessoa e ausência do final são indícios conservadores, não avaliações semânticas. Ausência de alerta não significa fidelidade garantida. Duplicação exige repetição integral exata (espaços normalizados); parágrafos repetidos têm pelo menos 60 caracteres. Citações pareadas são excluídas do teste de primeira pessoa. Nuances verbais, fontes e ambiguidades ficam para revisão humana. O motor não expõe motivo de parada por token/EOS neste log; não se infere conclusão normal apenas pela pontuação.')
    [void]$text.AppendLine('')
    [void]$text.AppendLine('Os logs nativos são separados dos warnings do validador. O log original é compartilhado pelo aplicativo e não inclui PID; não gere textos em outra instância durante a bateria. O runner rejeita atividade concorrente detectável. Tempos de geração excluem carregamento; tempo total inclui carregamento e encerramento das instâncias.')
    [void]$text.AppendLine('')
    [void]$text.AppendLine('Aplicativo não recompilado nem alterado nesta bateria; somente o executor QA com a interface existente foi compilado. Integridade das DLLs antes/depois: engine-integrity.json. Captura integral conferida pelo número de caracteres publicado em GENERATION_OK. Os trechos citados abaixo não foram abreviados.')
    foreach ($row in $rows) {
        [void]$text.AppendLine("`n## TESTE $($row.number) — $($row.name)`n")
        [void]$text.AppendLine("Objetivo: $($row.objective)`n")
        [void]$text.AppendLine("Status técnico: **$($row.status)**. Geração terminou normalmente: $($row.completedNormally).`n")
        [void]$text.AppendLine(('Tempo: {0:N3}s de geração; {1:N3}s incluindo preparação.' -f $row.generationSeconds, $row.totalSeconds))
        [void]$text.AppendLine("`nEntrada: $($row.inputChars) caracteres / $($row.inputWords) palavras. Saída: $($row.outputChars) caracteres / $($row.outputWords) palavras.`n")
        [void]$text.AppendLine("Retry: $($row.retry). Erro: $($row.hasError). Exceção: $($row.hasException). ContextOverflowException: $($row.contextOverflowException).`n")
        [void]$text.AppendLine("Modelo: $($row.modelFile). Configuração registrada: $($row.modelConfig).`n")
        $capture = if ($null -eq $row.outputCaptureVerified) { 'Não verificável por GENERATION_OK; saída disponível na tela preservada integralmente.' } else { [string]$row.outputCaptureVerified }
        [void]$text.AppendLine("Tamanho da saída confere com o log do motor: $capture`n")
        [void]$text.AppendLine('Warnings do validador:')
        [void]$text.AppendLine('')
        if ($row.warnings.Count) { foreach ($warning in $row.warnings) { [void]$text.AppendLine("- $warning") } } else { [void]$text.AppendLine('Nenhum registrado.') }
        [void]$text.AppendLine("`nWarnings nativos: $($row.nativeWarnings.Count). Log integral: [test-$('{0:d2}' -f [int]$row.number)-log.txt](test-$('{0:d2}' -f [int]$row.number)-log.txt).`n")
        if ($row.exception) { [void]$text.AppendLine("Exceção registrada:`n`n``````text`n$($row.exception)`n```````n") }
        [void]$text.AppendLine("### Entrada completa`n`n``````text`n$($row.input)`n```````n")
        [void]$text.AppendLine("### Saída completa`n`n``````text`n$($row.output)`n```````n")
        [void]$text.AppendLine("Observações automáticas:`n")
        if ($row.observations.Count) { foreach ($observation in $row.observations) { [void]$text.AppendLine("- $observation") } } else { [void]$text.AppendLine('Nenhum indício adicional detectado. Revisão humana pendente.') }
    }
    [void]$text.AppendLine("`n## Resumo`n")
    $summary = [ordered]@{
        'Total de testes previstos'=15; 'Testes executados'=@($rows).Count
        'Concluídos (GENERATION_OK)'=@($rows | Where-Object completedNormally).Count
        'Com warning do validador'=@($rows | Where-Object {$_.warnings.Count -gt 0}).Count
        'Com primeira pessoa fora de citações (indício)'=@($rows | Where-Object {$_.firstPerson.Count -gt 0}).Count
        'Com retry'=@($rows | Where-Object retry).Count
        'Com erro'=@($rows | Where-Object hasError).Count
        'Com indício de truncamento'=@($rows | Where-Object truncationSuspected).Count
        'Com duplicação integral'=@($rows | Where-Object fullDuplication).Count
        'Com parágrafos repetidos'=@($rows | Where-Object repeatedParagraphs).Count
        'Com ContextOverflowException'=@($rows | Where-Object contextOverflowException).Count
        'Tempo total (s)'=[Math]::Round($seconds,3)
    }
    foreach ($item in $summary.GetEnumerator()) { [void]$text.AppendLine("- $($item.Key): $($item.Value)") }
    [IO.File]::WriteAllText((Join-Path $results 'qa-report.md'), $text.ToString(), [Text.Encoding]::UTF8)
    $summary | ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $results 'summary.json')
}
