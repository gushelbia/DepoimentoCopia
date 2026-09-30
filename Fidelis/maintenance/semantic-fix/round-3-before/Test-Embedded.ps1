$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$asm=[Reflection.Assembly]::LoadFrom((Join-Path $root 'engine/DepoimentoLocal.dll'))
$type=$asm.GetType('DepoimentoLocal.Windows.TextProcessing')
$flags=[Reflection.BindingFlags]'Public,NonPublic,Static'
$results=[Collections.Generic.List[string]]::new()
function Invoke-Text($name,[object[]]$arguments){$type.GetMethod($name,$flags).Invoke($null,$arguments)}
function Assert($condition,$name){if(!$condition){throw "FAIL $name"}; $results.Add("PASS $name")}
$issue=Invoke-Text Validate @('Eu não vi Renata abrir a janela.','Renata não abriu a janela.')
Assert ([bool]$issue) 'embedded validation detects perception scope'
Assert (Invoke-Text IsCriticalIssue @($issue)) 'embedded scope failure requires retry'
$issue=Invoke-Text Validate @('Me contaram que o depósito abriu ontem.','Informou que o depósito abriu ontem.')
Assert ([bool]$issue) 'embedded validation detects lost hearsay'
Assert (Invoke-Text IsCriticalIssue @($issue)) 'embedded hearsay failure requires retry'
$issue=Invoke-Text Validate @('A Isabela saiu para atender a ligação.','O depoente saiu para atender a ligação.')
Assert ([bool]$issue) 'embedded validation detects participant replaced by narrator'
Assert (Invoke-Text IsCriticalIssue @($issue)) 'embedded actor substitution requires retry'
$fixed=Invoke-Text RepairResidualFirstPerson @('Não entendi e fiquei fora. Não verifiquei. “Eu fiquei.”')
Assert ($fixed -ceq 'Não entendeu e ficou fora. Não verificou. “Eu fiquei.”') 'embedded conjugation repair preserves quotation'
$fixed=Invoke-Text RepairResidualFirstPerson @('Eu não entendi. “Eu fiquei.”')
Assert ($fixed -ceq 'O depoente não entendeu. “Eu fiquei.”') 'explicit narrator is preserved rather than deleted'
$fixed=Invoke-Text RepairResidualFirstPerson @('Ouvi eu a técnica responder. Ela falou para mim. “Ouvi eu.”')
Assert ($fixed -ceq 'O depoente ouviu a técnica responder. Ela falou para o depoente. “Ouvi eu.”') 'post-verbal subject and explicit recipient in production repair'
$fixed=Invoke-Text RepairSemanticAnchors @('Eu não sei o que ele pretende fazer depois.','Não sabe o que pretende fazer depois.')
Assert ($fixed -ceq 'O depoente não sabe o que ele pretende fazer depois.') 'source-anchored subject and referent repair in production'
$fixed=Invoke-Text RepairSemanticAnchors @('Ouvi a médica pedir silêncio.','Ouviu o depoente a médica pedir silêncio.')
Assert ($fixed -ceq 'O depoente ouviu a médica pedir silêncio.') 'source-proven post-verbal narrator in production'
$fixed=Invoke-Text RepairSemanticAnchors @('Eu cheguei... não, acho que antes de chegar telefonou a médica. É, a médica telefonou antes.','Chegou... Não, acredita que a médica telefonou antes de sua chegada. Sim, a médica telefonou antes.')
Assert ($fixed -ceq 'O depoente acredita que a médica telefonou antes da chegada do depoente.') 'correction retains uncertainty and explicit temporal owner in production'
$fixed=Invoke-Text RepairSemanticAnchors @('Só não lembro a data.','Só não lembra a data.')
Assert ($fixed -ceq 'O depoente só não lembra a data.') 'predicate focus is not transferred to narrator'
$draft='Não sabe da chave. Não lembra da porta.'
$fixed=Invoke-Text RepairSemanticAnchors @('Não lembro da chave. Não sei da porta.',$draft)
Assert ($fixed -ceq $draft) 'positional cross-category repair is disabled'
Assert (!(Invoke-Text IsCriticalIssue @('uma negação pode ter sido omitida'))) 'legacy count-only warning remains advisory'
Assert (!(Invoke-Text Validate @('Eu vi a médica chegar ao laboratório.','O depoente viu a médica chegar ao laboratório.'))) 'faithful conversion accepted'
$factory=$asm.GetType('DepoimentoLocal.Windows.PromptFactory')
$prompt=$factory.GetMethod('Build',$flags).Invoke($null,@('Eu vi a médica.','Relatou que '))
Assert ($prompt.EndsWith("<|im_start|>assistant`n")) 'no assistant prefill that drops opening clauses'
$retry=$factory.GetMethod('BuildRetry',$flags).Invoke($null,@('ORIGINAL_SENTINEL','DRAFT_SENTINEL','ISSUE_SENTINEL','Relatou que '))
Assert ($retry.Contains('ORIGINAL_SENTINEL') -and $retry.Contains('ISSUE_SENTINEL') -and !$retry.Contains('DRAFT_SENTINEL')) 'retry uses original and diagnostic, never incomplete draft'
Assert (!($asm.GetReferencedAssemblies().Name -contains 'SemanticGuard.compiled')) 'helper embedded without deployment assembly dependency'
$results | Set-Content (Join-Path $PSScriptRoot 'embedded-tests.txt')
$results
