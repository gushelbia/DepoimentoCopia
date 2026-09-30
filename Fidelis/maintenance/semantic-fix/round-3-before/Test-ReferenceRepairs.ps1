$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'SemanticGuard.compiled.dll')
$results=[Collections.Generic.List[string]]::new()
function Check($name,$source,$draft,$expected){
    $actual=[DepoimentoLocal.Windows.SemanticGuard]::RepairSourceAnchors($source,$draft)
    if($actual -cne $expected){throw "$name`nExpected: $expected`nActual: $actual"}
    if([DepoimentoLocal.Windows.SemanticGuard]::RepairSourceAnchors($source,$actual) -cne $actual){throw "Not idempotent: $name"}
    $results.Add("PASS $name")
}
Check 'unique implicit narrator predicate' 'Eu não sei qual chave foi usada.' 'Não sabe qual chave foi usada.' 'O depoente não sabe qual chave foi usada.'
Check 'same predicate with two actors is ambiguous' 'A técnica não sabe qual chave foi usada. Eu não sei qual chave foi usada.' 'A técnica não sabe qual chave foi usada. Não sabe qual chave foi usada.' 'A técnica não sabe qual chave foi usada. Não sabe qual chave foi usada.'
Check 'named subject never replaced' 'Eu não sei qual chave foi usada.' 'A técnica não sabe qual chave foi usada.' 'A técnica não sabe qual chave foi usada.'
Check 'adverb after explicit narrator does not duplicate it' 'Eu também não vi o carro passar.' 'O depoente também não viu o carro passar.' 'O depoente também não viu o carro passar.'
Check 'adverb after named subject does not replace it' 'Eu também não vi o carro passar.' 'A técnica também não viu o carro passar.' 'A técnica também não viu o carro passar.'
Check 'coordinated narrator verbs' 'Depois eu fui buscar água e fiquei na cozinha.' 'Depois foi buscar água e ficou na cozinha.' 'Depois o depoente foi buscar água e ficou na cozinha.'
Check 'only modifies predicate not narrator' 'Sei onde foi, só não lembro a data.' 'Sabe onde foi, só não lembra a data.' 'O depoente sabe onde foi, o depoente só não lembra a data.'
Check 'also modifies predicate not narrator' 'Também não sei qual chave foi usada.' 'Também não sabe qual chave foi usada.' 'O depoente também não sabe qual chave foi usada.'
Check 'source explicit exclusive subject focus retained' 'Só eu não sei qual chave foi usada.' 'Só não sabe qual chave foi usada.' 'Só o depoente não sabe qual chave foi usada.'
Check 'source explicit additive subject focus retained' 'Também eu não sei qual chave foi usada.' 'Também não sabe qual chave foi usada.' 'Também o depoente não sabe qual chave foi usada.'
Check 'post-verbal narrator with predicate focus' 'Também ouvi a médica pedir silêncio.' 'Também ouviu o depoente a médica pedir silêncio.' 'O depoente também ouviu a médica pedir silêncio.'
Check 'source-proven post-verbal narrator' 'Ouvi a médica pedir silêncio.' 'Ouviu o depoente a médica pedir silêncio.' 'O depoente ouviu a médica pedir silêncio.'
Check 'source-proven post-verbal visual narrator' 'Eu não vi a funcionária retirar a bolsa.' 'Não viu o depoente a funcionária retirar a bolsa.' 'O depoente não viu a funcionária retirar a bolsa.'
Check 'post-verbal object of third party remains object' 'A médica ouviu o depoente pedir silêncio.' 'A médica ouviu o depoente pedir silêncio.' 'A médica ouviu o depoente pedir silêncio.'
Check 'same perception in source by multiple people is ambiguous' 'Eu ouvi a médica pedir silêncio. Ele ouviu a médica pedir silêncio.' 'Ouviu o depoente a médica pedir silêncio. Ele ouviu a médica pedir silêncio.' 'Ouviu o depoente a médica pedir silêncio. Ele ouviu a médica pedir silêncio.'
Check 'knowledge categories never interchanged' 'Eu não sei qual chave foi usada.' 'Não lembra qual chave foi usada.' 'Não lembra qual chave foi usada.'
Check 'quotation untouched' 'Eu não sei qual chave foi usada.' '“Não sabe qual chave foi usada.”' '“Não sabe qual chave foi usada.”'
Check 'same output frame across quotation segments is ambiguous' 'Eu não sei qual chave foi usada.' 'Não sabe qual chave foi usada. “pausa” Não sabe qual chave foi usada.' 'Não sabe qual chave foi usada. “pausa” Não sabe qual chave foi usada.'
Check 'explicit recipient from unique source frame' 'A Isabela falou para mim que faltava a assinatura.' 'A Isabela falou para ele que faltava a assinatura.' 'A Isabela falou para o depoente que faltava a assinatura.'
Check 'two source recipients are not merged' 'A Isabela falou para mim que faltava a assinatura. A técnica falou para ele que faltava a assinatura.' 'A Isabela falou para ele que faltava a assinatura. A técnica falou para ele que faltava a assinatura.' 'A Isabela falou para ele que faltava a assinatura. A técnica falou para ele que faltava a assinatura.'
Check 'restore explicit subordinate pronoun' 'Não sei o que ele pretende fazer depois.' 'Não sabe o que pretende fazer depois.' 'O depoente não sabe o que ele pretende fazer depois.'
Check 'two source subordinate frames are ambiguous' 'Não sei o que ele pretende fazer depois. Não sei o que pretende fazer depois.' 'Não sabe o que pretende fazer depois.' 'O depoente não sabe o que pretende fazer depois.'
Check 'restore complete approximate alternatives' 'Eu esperei uns oito ou nove minutos.' 'Esperou uns nove minutos.' 'O depoente esperou uns oito ou nove minutos.'
Check 'restore repeated approximate quantity' 'Eu esperei uns seis, seis e pouco minutos.' 'Esperou uns seis e pouco minutos.' 'O depoente esperou uns seis, seis e pouco minutos.'
Check 'another source interval prevents restoration' 'Eu esperei uns oito ou nove minutos. Ela esperou uns nove minutos.' 'Esperou uns nove minutos.' 'Esperou uns nove minutos.'
Check 'run-on estimate retains place and uncertainty' 'Eu voltei ali acho que era cedo.' 'O depoente voltou acredita que era cedo.' 'O depoente voltou ali; o depoente acredita que era cedo.'
Check 'explicit correction with repeated confirmation' 'Eu entrei... não, acho que a médica telefonou antes. É, a médica telefonou antes.' 'Entrou... Não, acredita que a médica telefonou antes. Sim, a médica telefonou antes.' 'O depoente acredita que a médica telefonou antes.'
Check 'temporal owner in explicit correction' 'Eu cheguei... não, acho que antes de chegar telefonou a médica. É, a médica telefonou antes.' 'Chegou... Não, acredita que a médica telefonou antes de sua chegada. Sim, a médica telefonou antes.' 'O depoente acredita que a médica telefonou antes da chegada do depoente.'
Check 'masculine temporal nominalization' 'Eu retornei... não, acho que depois de retornar telefonou a médica. É, a médica telefonou depois.' 'Retornou... Não, acredita que a médica telefonou depois do seu retorno. Sim, a médica telefonou depois.' 'O depoente acredita que a médica telefonou depois do retorno do depoente.'
Check 'another temporal owner is not reassigned' 'Eu cheguei... não, acho que antes de chegar telefonou a médica. É, a médica telefonou antes.' 'Chegou... Não, acredita que a médica telefonou antes da chegada da técnica. Sim, a médica telefonou antes.' 'O depoente acredita que a médica telefonou antes da chegada da técnica.'
Check 'no confirmation means no consolidation' 'Eu entrei... não, acho que a médica telefonou antes.' 'Entrou... Não, acredita que a médica telefonou antes.' 'Entrou... Não, o depoente acredita que a médica telefonou antes.'
Check 'different temporal confirmation is not redundant' 'Eu entrei... não, acho que a médica telefonou antes. É, a médica telefonou depois.' 'Entrou... Não, acredita que a médica telefonou antes. Sim, a médica telefonou depois.' 'Entrou... Não, o depoente acredita que a médica telefonou antes. Sim, a médica telefonou depois.'
$results | Set-Content (Join-Path $PSScriptRoot 'reference-tests.txt')
$results
