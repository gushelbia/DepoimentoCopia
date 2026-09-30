# Offline regression: applies the production repairs and validation of the
# deployed engine to outputs already reviewed, without running the model.
param([string]$OutFile)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$asm=[Reflection.Assembly]::LoadFrom((Join-Path $root 'engine/DepoimentoLocal.dll'))
$type=$asm.GetType('DepoimentoLocal.Windows.TextProcessing')
$flags=[Reflection.BindingFlags]'Public,NonPublic,Static'
function Invoke-Text($name,[object[]]$arguments){$type.GetMethod($name,$flags).Invoke($null,$arguments)}
$suites=@(
    @{name='Bateria inédita (saídas finais da rodada 3)'; cases=(Join-Path $root 'QA-Inedita-20260929/TestCases'); outputs=(Join-Path $PSScriptRoot 'round-4-before/QA-Inedita-Results')},
    @{name='Bateria anterior (saídas finais da rodada 3)'; cases=(Join-Path $root 'QA/TestCases'); outputs=(Join-Path $root 'QA/Results')}
)
$lines=[Collections.Generic.List[string]]::new()
foreach($suite in $suites){
    $lines.Add("## $($suite.name)")
    foreach($f in Get-ChildItem $suite.cases -Filter *.json | Sort-Object Name){
        $case=Get-Content -Raw -Encoding UTF8 $f.FullName | ConvertFrom-Json
        if($case.inputFile){$case.input=[IO.File]::ReadAllText((Join-Path $f.DirectoryName $case.inputFile)).TrimEnd("`r","`n")}
        $n='{0:d2}' -f [int]$case.number
        $path=Join-Path $suite.outputs "test-$n-output.txt"
        $saved=[IO.File]::ReadAllText($path).TrimStart([char]0xFEFF)
        $replayed=Invoke-Text RepairSemanticAnchors @($case.input,(Invoke-Text RepairResidualFirstPerson @($saved)))
        $issue=Invoke-Text Validate @($case.input,$replayed)
        $state=if($replayed -ceq $saved){'INALTERADA'}else{'ALTERADA'}
        $lines.Add("- caso $n : $state ; validação=" + $(if($issue){$issue}else{'ok'}))
        if($replayed -cne $saved){
            $a=$saved -split '(?<=[.!?])\s+'; $b=$replayed -split '(?<=[.!?])\s+'
            foreach($s in $b){ if($a -cnotcontains $s){ $lines.Add("    + $s") } }
            foreach($s in $a){ if($b -cnotcontains $s){ $lines.Add("    - $s") } }
        }
    }
}
if($OutFile){$lines | Set-Content -Encoding UTF8 $OutFile}
$lines
