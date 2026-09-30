$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'FidelityRepairs.compiled.dll')
$results=[Collections.Generic.List[string]]::new()
function Check($name,$source,$draft,$expected) {
 $actual=[DepoimentoLocal.Windows.FidelityRepairs]::Repair($source,$draft)
 if($actual -cne $expected){throw "$name : $actual"}
 $results.Add("PASS $name")
}
Check 'Teste 3 batido' 'Depois a Ana me disse que o Roberto tinha batido a porta quando entrou na sala.' 'Depois Ana lhe disse que Roberto batia a porta quando entrou na sala.' 'Depois Ana lhe disse que Roberto tinha batido a porta quando entrou na sala.'
Check 'feito' 'Ana me disse que Roberto tinha feito o trabalho antes da reunião.' 'Ana lhe disse que Roberto fazia o trabalho antes da reunião.' 'Ana lhe disse que Roberto tinha feito o trabalho antes da reunião.'
Check 'havia dito' 'Ana me disse que Roberto havia dito a verdade.' 'Ana lhe disse que Roberto dizia a verdade.' 'Ana lhe disse que Roberto havia dito a verdade.'
Check 'saído' 'Roberto tinha saído antes da reunião.' 'Roberto saía antes da reunião.' 'Roberto tinha saído antes da reunião.'
Check 'anterioridade do perfeito' 'Roberto tinha batido a porta.' 'Roberto bateu a porta.' 'Roberto tinha batido a porta.'
Check 'plural' 'Os servidores tinham saído antes da reunião.' 'Os servidores saíam antes da reunião.' 'Os servidores tinham saído antes da reunião.'
Check 'regular' 'Roberto havia conversado com Ana.' 'Roberto conversava com Ana.' 'Roberto havia conversado com Ana.'
Check 'habitual legítimo' 'Roberto batia a porta quando entrava na sala.' 'Roberto batia a porta quando entrava na sala.' 'Roberto batia a porta quando entrava na sala.'
Check 'outra fonte' 'Ana me disse que Roberto tinha batido a porta.' 'Carlos lhe disse que Roberto batia a porta.' 'Carlos lhe disse que Roberto batia a porta.'
Check 'outro ator' 'Ana me disse que Roberto tinha batido a porta.' 'Ana lhe disse que Carlos batia a porta.' 'Ana lhe disse que Carlos batia a porta.'
Check 'negação diferente' 'Roberto não tinha batido a porta.' 'Roberto batia a porta.' 'Roberto batia a porta.'
Check 'incerteza diferente' 'Talvez Roberto tinha batido a porta.' 'Roberto batia a porta.' 'Roberto batia a porta.'
Check 'ação habitual distinta' 'Roberto tinha batido a porta ontem. Carlos batia a porta sempre.' 'Roberto batia a porta ontem. Carlos batia a porta sempre.' 'Roberto tinha batido a porta ontem. Carlos batia a porta sempre.'
Check 'citação literal' 'Ana disse “Roberto tinha batido a porta”.' 'Ana disse “Roberto batia a porta”.' 'Ana disse “Roberto batia a porta”.'
Check 'ambiguidade de saída' 'Roberto tinha batido a porta.' 'Roberto batia a porta. Roberto batia a porta.' 'Roberto batia a porta. Roberto batia a porta.'
Check 'tinha existencial' 'Eu estava perto, mas tinha outras pessoas falando.' 'Estava perto, mas tinha outras pessoas falando.' 'Estava perto, mas tinha outras pessoas falando.'
Check 'versão colapsada ambígua' 'Roberto tinha batido a porta. Roberto batia a porta.' 'Roberto batia a porta.' 'Roberto batia a porta.'
$results | Set-Content (Join-Path $PSScriptRoot 'aspect-unit-results.txt')
$results

