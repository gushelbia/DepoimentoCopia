$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'SemanticGuard.compiled.dll')
$results=[Collections.Generic.List[string]]::new()
function Check($name,$source,$output,$reject){
    $issue=[DepoimentoLocal.Windows.SemanticGuard]::Validate($source,$output)
    if([bool]$issue -ne $reject){throw "$name : unexpected result '$issue'"}
    $results.Add("PASS $name")
}
Check 'negative perception cannot become denial of event' 'Eu não vi Luísa retirar a pasta.' 'Luísa não retirou a pasta.' $true
Check 'preserve negative perception with different participants' 'Eu não vi Luísa retirar a pasta.' 'O depoente não viu Luísa retirar a pasta.' $false
Check 'negative hearing cannot become denial of speech' 'Não ouvi o técnico anunciar o fechamento.' 'O técnico não anunciou o fechamento.' $true
Check 'anonymous source lost' 'Me informaram que abriram o depósito ontem.' 'Informou que abriram o depósito ontem.' $true
Check 'anonymous source preserved' 'Me informaram que abriram o depósito ontem.' 'Lhe informaram que abriram o depósito ontem.' $false
Check 'missing opening sentence' 'Talvez o ônibus tenha partido no domingo. Eu esperei na estação.' 'O depoente esperou na estação.' $true
Check 'certainty marker lost' 'Talvez o ônibus tenha partido no domingo.' 'O ônibus partiu no domingo.' $true
Check 'first person outside old vocabulary' 'Eu confirmei o recebimento da encomenda.' 'Confirmei o recebimento da encomenda.' $true
Check 'quotation first person unchanged' 'Eu ouvi: “eu confirmei a entrega”.' 'O depoente ouviu: “eu confirmei a entrega”.' $false
Check 'quotation content changed' 'Eu ouvi: “eu confirmei a entrega”.' 'O depoente ouviu: “ele confirmou a entrega”.' $true
Check 'omitted visual witness' 'Eu vi a engenheira entrar no laboratório.' 'A engenheira entrou no laboratório.' $true
Check 'factual third person unchanged' 'A engenheira entrou no laboratório.' 'A engenheira entrou no laboratório.' $false
Check 'narrator replaced with participant' 'A Letícia saiu. Eu ouvi a enfermeira chamar.' 'A Letícia saiu. Letícia ouviu a enfermeira chamar.' $true
Check 'narrator remains explicit' 'A Letícia saiu. Eu ouvi a enfermeira chamar.' 'A Letícia saiu. O depoente ouviu a enfermeira chamar.' $false
Check 'participant really perceived' 'A Letícia ouviu a enfermeira chamar. Eu ouvi um ruído.' 'A Letícia ouviu a enfermeira chamar. O depoente ouviu um ruído.' $false
Check 'witness proximity omitted' 'Eu estava perto quando a gerente falou com o visitante.' 'A gerente falou com o visitante.' $true
Check 'shared first and third person conjugation' 'Eu aguardava o transporte. Eu disse que soube do atraso.' 'O depoente aguardava o transporte. O depoente disse que soube do atraso.' $false
Check 'short negative perception omitted' 'A janela caiu. Eu não vi isso.' 'A janela caiu.' $true
Check 'event negation omitted' 'A funcionária não assinou o formulário.' 'A funcionária assinou o formulário.' $true
Check 'approximate duration converted to exact' 'Esperei uns vinte minutos na recepção.' 'Esperou vinte minutos na recepção.' $true
Check 'approximation rephrased' 'Esperei uns vinte minutos na recepção.' 'Esperou aproximadamente vinte minutos na recepção.' $false
Check 'nested source collapsed' 'A médica disse que ouviu a gerente pedir silêncio.' 'A médica ouviu que a gerente pediu silêncio.' $true
Check 'nested source preserved' 'A médica disse que ouviu a gerente pedir silêncio.' 'A médica disse que ouviu a gerente pedir silêncio.' $false
Check 'named participant replaced by narrator' 'A Beatriz respondeu que precisava do protocolo.' 'O depoente respondeu que precisava do protocolo.' $true
Check 'named participant preserved' 'A Beatriz respondeu que precisava do protocolo.' 'A Beatriz respondeu que precisava do protocolo.' $false
Check 'participant action and narrator uncertainty' 'Enquanto isso, a Beatriz saiu para telefonar. Eu não sei para quem.' 'Enquanto isso, o depoente saiu para telefonar. Não sabe para quem.' $true
Check 'apparent uncertainty paraphrase' 'Aparentemente a inspeção terminou naquele momento.' 'Parece que a inspeção terminou naquele momento.' $false
Check 'apparent uncertainty removed' 'Aparentemente a inspeção terminou naquele momento.' 'A inspeção terminou naquele momento.' $true
$source='A Sílvia telefonou. O José chegou.'
$draft='A Silvia telefonou. O Jose chegou. “Silvia falou.”'
$expected='A Sílvia telefonou. O José chegou. “Silvia falou.”'
if([DepoimentoLocal.Windows.SemanticGuard]::RepairNameSpelling($source,$draft) -cne $expected){throw 'Name spelling/quotation repair'}
$results.Add('PASS source-anchored name accents and quotes')
$source='O Andre saiu. O André ficou.'
$draft='Andre saiu. André ficou.'
if([DepoimentoLocal.Windows.SemanticGuard]::RepairNameSpelling($source,$draft) -cne $draft){throw 'Ambiguous names changed'}
$results.Add('PASS distinct source spellings not conflated')
if([DepoimentoLocal.Windows.SemanticGuard]::RepairNameSpelling('A Sílvia saiu.','O depoente saiu.') -cne 'O depoente saiu.'){throw 'Spelling repair assigned actor'}
$results.Add('PASS spelling repair never assigns actor')
if(![string]::IsNullOrEmpty([DepoimentoLocal.Windows.SemanticGuard]::RepairFirstPerson($null))){throw 'empty repair changed'}
$quoted='Eu entendi. “Eu fiquei e verifiquei”. Fiquei fora e confirmei depois.'
$expected='Eu entendeu. “Eu fiquei e verifiquei”. Ficou fora e confirmou depois.'
if([DepoimentoLocal.Windows.SemanticGuard]::RepairFirstPerson($quoted) -cne $expected){throw 'Quotation changed by verb repair'}
$results.Add('PASS supplemental conjugation repair preserves quotes')
Check 'explicit recipient cannot become third-party pronoun' 'A recepcionista falou para mim que faltava uma assinatura.' 'A recepcionista falou para ele que faltava uma assinatura.' $true
Check 'explicit recipient with pronoun elsewhere is still checked' 'Quando voltei, a recepcionista falou pra mim que faltava uma assinatura.' 'Quando o depoente voltou, a recepcionista falou para ele que faltava uma assinatura.' $true
Check 'explicit recipient preserved' 'A recepcionista falou para mim que faltava uma assinatura.' 'A recepcionista falou para o depoente que faltava uma assinatura.' $false
Check 'recipient clitic paraphrase' 'A recepcionista falou para mim que faltava uma assinatura.' 'A recepcionista falou-lhe que faltava uma assinatura.' $false
Check 'subordinate subject lost' 'Eu não sei o que ele pretende fazer depois.' 'O depoente não sabe o que pretende fazer depois.' $true
Check 'subordinate subject retained' 'Eu não sei o que ele pretende fazer depois.' 'O depoente não sabe o que ele pretende fazer depois.' $false
Check 'approximate interval shortened' 'Eu esperei uns seis, seis e pouco minutos.' 'O depoente esperou uns seis e pouco minutos.' $true
Check 'approximate alternatives retained' 'Eu esperei uns oito ou nove minutos.' 'O depoente esperou aproximadamente oito ou nove minutos.' $false
$draft='Ouvi eu o porteiro explicar. A médica ouviu o depoente pedir ajuda. “Ouvi eu a resposta.”'
$expected='Eu ouvi o porteiro explicar. A médica ouviu o depoente pedir ajuda. “Ouvi eu a resposta.”'
if([DepoimentoLocal.Windows.SemanticGuard]::RepairFirstPerson($draft) -cne $expected){throw 'Post-verbal subject/object/quote repair'}
$results.Add('PASS post-verbal eu reordered without changing object or quotes')
$results | Set-Content (Join-Path $PSScriptRoot 'guard-tests.txt')
$results
