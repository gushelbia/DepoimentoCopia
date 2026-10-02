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
# Round 7: a role swap that could not be repaired is critical (retry, then rejection and incomplete mark).
$issue=Invoke-Text Validate @('Eu empurrei ele porque ele estava me enforcando.','O depoente relatou que empurrou ele porque o depoente estava enforcando.')
Assert ([bool]$issue -and $issue.Contains('papéis trocados') -and (Invoke-Text IsCriticalIssue @($issue))) 'embedded: unrepaired swapped gerund is critical'
$fixed=Invoke-Text RepairSemanticAnchors @('Eu empurrei ele porque ele estava me enforcando.','Relatou que o depoente empurrou ele porque ele estava o depoente enforcando.')
Assert ($fixed -ceq 'O depoente relatou que empurrou ele porque ele estava enforcando o depoente.') 'embedded: broken gerund repaired from the source'
$fixed=Invoke-Text RepairSemanticAnchors @('Meu marido foi preso e eu fiquei sozinha.','Relatou que foi preso e o depoente ficou sozinho.')
Assert ($fixed -ceq 'O depoente relatou que o marido do depoente foi preso e o depoente ficou sozinho.') 'embedded: deleted subject restored'
Assert (!(Invoke-Text Validate @('Eu vi a médica chegar ao laboratório.','O depoente viu a médica chegar ao laboratório.'))) 'faithful conversion accepted'
# Round 3, through the production order: residual repair, anchored repair, validation.
function Invoke-Pipeline($source,$draft){ Invoke-Text RepairSemanticAnchors @($source,(Invoke-Text RepairResidualFirstPerson @($draft))) }
$fixed=Invoke-Pipeline 'Fechei o cofre. Depois ela me mostrou o recibo.' 'Fechei o cofre. Depois ela mostrou o recibo.'
Assert ($fixed -ceq 'O depoente fechou o cofre. Depois ela lhe mostrou o recibo.') 'copied first person and recipient repaired in production'
Assert (!(Invoke-Text Validate @('Fechei o cofre. Depois ela me mostrou o recibo.',$fixed))) 'repaired draft accepted in production'
$fixed=Invoke-Pipeline 'O porteiro Joaquim me contou que a vizinha lhe pediu a chave.' 'O porteiro Joaquim lhe contou que a vizinha lhe pediu a chave.'
Assert ($fixed -ceq 'O porteiro Joaquim contou ao depoente que a vizinha lhe pediu a chave.') 'two recipients kept distinct in production'
$fixed=Invoke-Pipeline 'Heitor me perguntou se eu tinha a senha. Respondi que não.' 'Heitor lhe perguntou se o depoente tinha a senha. Respondeu que não.'
Assert ($fixed -ceq 'Heitor lhe perguntou se o depoente tinha a senha. O depoente respondeu que não.') 'elliptical narrator made explicit in production'
$issue=Invoke-Text Validate @('Conferi as notas e assinei o livro.','O depoente viu conferir as notas e assinar o livro.')
Assert ([bool]$issue -and (Invoke-Text IsCriticalIssue @($issue))) 'narrator action turned into perception requires retry'
$issue=Invoke-Text Validate @('A Clarice me contou que o portão estava aberto.','A Clarice contou que o portão estava aberto.')
Assert ([bool]$issue -and (Invoke-Text IsCriticalIssue @($issue))) 'lost recipient requires retry'
Assert (!(Invoke-Text Validate @('Eu falei que não tava com o crachá.','O depoente respondeu que não estava com o crachá.'))) 'speech paraphrase not rejected as omission'
# Round 4: incomplete-output marker embedded in production and wired into Reformular.
$guard=$asm.GetType('DepoimentoLocal.Windows.SemanticGuard')
$markFn={param($t,$r) $guard.GetMethod('MarkIncomplete',$flags).Invoke($null,@($t,$r))}
$marked=& $markFn 'Primeiro bloco aceito. Segundo bloco rejeitado.' 'O bloco 2 ainda precisa de revisão'
Assert ($marked.StartsWith('[SAÍDA INCOMPLETA') -and $marked.Contains('Motivo: O bloco 2') -and $marked.TrimEnd().EndsWith('reformulada por completo]')) 'production marker has header, reason and footer'
Assert ((& $markFn $marked 'x') -ceq $marked) 'production marker does not duplicate'
Assert ((& $markFn '' 'x') -eq '') 'production marker keeps empty screen empty'
$handlerIl=[Reflection.Assembly]::LoadFrom((Join-Path $root 'maintenance/prompt-update/dnlib/lib/net6.0/dnlib.dll')) | Out-Null
$module=[dnlib.DotNet.ModuleDefMD]::Load((Join-Path $root 'engine/DepoimentoLocal.dll'))
try {
    $move=($module.GetTypes() | Where-Object FullName -eq 'DepoimentoLocal.Windows.MainForm/<Reformulate_Click>d__30').Methods | Where-Object Name -eq MoveNext
    $il=$move.Body.Instructions; $wired=0
    foreach($h in $move.Body.ExceptionHandlers){
        if($h.HandlerType -ne 'Catch'){continue}
        $at=$il.IndexOf($h.HandlerStart)
        $window=@($il[($at+1)..($at+9)] | ForEach-Object { [string]$_.Operand })
        if(($window -match 'MarkIncomplete').Count -eq 1 -and ($window -match '_reformulated').Count -eq 2){$wired++}
    }
    Assert ($wired -eq 2) 'rejection/error and cancellation handlers mark the on-screen text'
    Assert (@($il | Where-Object { [string]$_.Operand -like '*mantido na tela*' }).Count -eq 0) 'rejection message no longer says the text was kept as is'
    # Round 5: a user cancellation is recognized right after each generation.
    $checked=0
    for($i=0;$i -lt $il.Count-4;$i++){
        if([string]$il[$i].Operand -like '*TaskAwaiter*System.String*::GetResult()*' -and
           [string]$il[$i+2].OpCode -eq 'ldarg.0' -and [string]$il[$i+3].Operand -like '*<token>5__3' -and
           [string]$il[$i+4].Operand -like '*CancellationToken::ThrowIfCancellationRequested()'){$checked++}
    }
    Assert ($checked -eq 2) 'cancellation checked after first pass and after retry'
} finally { $module.Dispose() }
$issue=Invoke-Text Validate @('Eu cheguei cedo ao prédio.','')
Assert (($issue -eq 'fidelidade: saída vazia') -and (Invoke-Text IsCriticalIssue @($issue))) 'empty output without cancellation is still rejected'
$factory=$asm.GetType('DepoimentoLocal.Windows.PromptFactory')
$prompt=$factory.GetMethod('Build',$flags).Invoke($null,@('Eu vi a médica.','Relatou que '))
Assert ($prompt.EndsWith("<|im_start|>assistant`n")) 'no assistant prefill that drops opening clauses'
$retry=$factory.GetMethod('BuildRetry',$flags).Invoke($null,@('ORIGINAL_SENTINEL','DRAFT_SENTINEL','ISSUE_SENTINEL','Relatou que '))
Assert ($retry.Contains('ORIGINAL_SENTINEL') -and $retry.Contains('ISSUE_SENTINEL') -and !$retry.Contains('DRAFT_SENTINEL')) 'retry uses original and diagnostic, never incomplete draft'
Assert (!($asm.GetReferencedAssemblies().Name -contains 'SemanticGuard.compiled')) 'helper embedded without deployment assembly dependency'
$results | Set-Content (Join-Path $PSScriptRoot 'embedded-tests.txt')
$results
