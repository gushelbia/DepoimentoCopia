$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'FidelityRepairs.compiled.dll')
$results=[Collections.Generic.List[string]]::new()
function Check($name,$source,$draft,$expected) {
 $actual=[DepoimentoLocal.Windows.FidelityRepairs]::Repair($source,$draft)
 if($actual -cne $expected){throw "$name : $actual"}
 $results.Add("PASS $name")
}
Check 'presente' 'Não sei. Não lembro. Acho. Posso estar enganado.' 'Não sabia. Não lembrava. Acreditava. Podia estar enganado.' 'Não sabe. Não lembra. Acredita. Pode estar enganado.'
Check 'passado explícito' 'Não sabia. Não lembrava. Achava. Podia estar confundindo.' 'Não sabe. Não lembra. Acredita. Pode estar confundindo.' 'Não sabia. Não lembrava. Achava. Podia estar confundindo.'
Check 'citação preservada' 'Carlos disse “não sei”. Não sei.' 'Carlos disse “não sei”. Não sabia.' 'Carlos disse “não sei”. Não sabe.'
Check 'contagem ambígua' 'Não sei. Não sei.' 'Não sabia.' 'Não sabia.'
Check 'categoria diferente' 'Não lembro.' 'Não sabia.' 'Não sabia.'
$s='Aí eu saí... não, eu acho que antes de sair entrou o Carlos. É, o Carlos entrou antes.'
Check 'correção confirmada' $s 'Aí saiu... Acredita que antes de sair entrou o Carlos.' 'Carlos entrou antes de sua saída.'
Check 'confirmação repetida' $s 'Então saiu... Não, acredita que antes de sair entrou o Carlos. É, Carlos entrou antes.' 'Carlos entrou antes de sua saída.'
Check 'sujeito diferente' $s 'Aí saiu... Acredita que antes de sair entrou o João.' 'Aí saiu... Acredita que antes de sair entrou o João.'
Check 'sem confirmação' 'Eu saí... não, acho que Carlos entrou antes.' 'Aí saiu... Acredita que antes de sair entrou o Carlos.' 'Aí saiu... Acredita que antes de sair entrou o Carlos.'
Check 'outro nome' ($s.Replace('Carlos','Maria').Replace('o Maria','a Maria')) 'Aí saiu... Acredita que antes de sair entrou a Maria.' 'Maria entrou antes de sua saída.'
Check 'fonte e ação preservadas' 'Carlos me falou que Paulo gritou, mas eu não sei quando.' 'Carlos lhe disse que Paulo gritou, mas não sabia quando.' 'Carlos lhe disse que Paulo gritou, mas não sabe quando.'
$original=[IO.File]::ReadAllText((Join-Path $PSScriptRoot '../context-fix/exact-original.txt')).Trim()
$line=Get-Content (Join-Path $PSScriptRoot '../context-fix/exact-text-results.txt') | Where-Object { $_.StartsWith('FIRST OUTPUT=') }
$baseline=$line.Substring(13)
$fixed=[DepoimentoLocal.Windows.FidelityRepairs]::Repair($original,$baseline)
if(($fixed -match 'não sabia|saiu\.\.\.') -or -not $fixed.Contains('Carlos entrou antes de sua saída.') -or ([regex]::Matches($fixed,'não sabe').Count -ne 3)){throw $fixed}
$results.Add('PASS Teste 2 baseline: três estados presentes e autocorreção')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'repaired-baseline.txt'),$fixed)
$results | Set-Content (Join-Path $PSScriptRoot 'unit-results.txt')
$results
