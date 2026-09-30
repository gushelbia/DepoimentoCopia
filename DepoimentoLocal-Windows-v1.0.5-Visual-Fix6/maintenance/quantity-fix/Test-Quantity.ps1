$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'FidelityRepairs.compiled.dll')
$r=[Collections.Generic.List[string]]::new()
function Check($name,$s,$o,$e){$a=[DepoimentoLocal.Windows.FidelityRepairs]::Repair($s,$o);if($a -cne $e){throw "$name : $a"};$r.Add("PASS $name")}
Check 'tres' 'Depois os três começaram a falar quase ao mesmo tempo.' 'Depois eles começaram a falar quase ao mesmo tempo.' 'Depois os três começaram a falar quase ao mesmo tempo.'
Check 'feminino' 'As duas chegaram juntas.' 'Elas chegaram juntas.' 'As duas chegaram juntas.'
Check 'digitos' 'Os 12 saíram juntos.' 'Eles saíram juntos.' 'Os 12 saíram juntos.'
Check 'composto' 'Os vinte e três chegaram.' 'Eles chegaram.' 'Os vinte e três chegaram.'
Check 'ambos' 'Depois ambos saíram.' 'Depois eles saíram.' 'Depois ambos saíram.'
Check 'sem quantidade' 'Depois eles saíram.' 'Depois eles saíram.' 'Depois eles saíram.'
Check 'ja preservado' 'Os três saíram.' 'Os três saíram.' 'Os três saíram.'
Check 'negacao' 'Os três não saíram.' 'Eles saíram.' 'Eles saíram.'
Check 'outro evento' 'Os três saíram ontem.' 'Eles saíram hoje.' 'Eles saíram hoje.'
Check 'citacao fonte' 'Disse “os três saíram”.' 'Disse “eles saíram”.' 'Disse “eles saíram”.'
Check 'saida ambigua' 'Os três saíram.' 'Eles saíram. Eles saíram.' 'Eles saíram. Eles saíram.'
Check 'fonte ambigua' 'Os três saíram. Os quatro saíram.' 'Eles saíram.' 'Eles saíram.'
Check 'genero diferente' 'As três saíram.' 'Eles saíram.' 'Eles saíram.'
Check 'quantidade objeto' 'Viu os três.' 'Eles saíram.' 'Eles saíram.'
$s=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'teste4-original.txt'))
$o=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'teste4-before.txt'))
$e=$o.Replace('Depois eles começaram a falar quase ao mesmo tempo.','Depois os três começaram a falar quase ao mesmo tempo.')
Check 'Teste 4 completo: somente eles -> os tres' $s $o $e
Check 'Teste 4 idempotencia' $s $e $e
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'teste4-expected.txt'),$e)
$r | Set-Content (Join-Path $PSScriptRoot 'quantity-unit-results.txt')
$r
