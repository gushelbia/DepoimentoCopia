# Testes do detector de itens de conferência (sem janela, sem motor).
#   powershell.exe -NoProfile -ExecutionPolicy Bypass -File maintenance/review-highlight/Test-ReviewScanner.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source = Get-Content (Join-Path $root 'ui/ModernShell.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition $source -Language CSharp -ReferencedAssemblies @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
$results = [Collections.Generic.List[string]]::new()
$failures = 0
function Check($ok, $name, $detail) {
    if (-not $ok) { $script:failures++ }
    $line = ($(if ($ok) { 'PASS ' } else { 'FAIL ' })) + $name + $(if ($detail) { ' | ' + $detail } else { '' })
    $results.Add($line); Write-Output $line
}
function Items($text) { , [ReviewScanner]::Find($text) }
function Describe($list) { $parts = [Collections.Generic.List[string]]::new(); foreach ($i in $list) { $parts.Add($i.Kind + ':' + $i.Text) }; , $parts }
function Expect($name, $text, [string[]]$expected) {
    $list = [ReviewScanner]::Find($text)
    $got = Describe $list
    if ($null -eq $expected) { $expected = @() }
    $same = $got.Count -eq $expected.Count
    for ($k = 0; $same -and $k -lt $got.Count; $k++) { if ($got[$k] -cne $expected[$k]) { $same = $false } }
    Check $same $name ("encontrados=" + $got.Count + $(if ($got.Count) { ': ' + ($got -join ' ; ') } else { '' }))
}

# Formatos reconhecidos
Expect 'horários numéricos' 'Cheguei às 14h, saí às 14h30 e voltei às 16:45.' @('horário:14h','horário:14h30','horário:16:45')
Expect 'horário com min e horas' 'Às 7h45min ou 8 horas.' @('horário:7h45min','horário:8 horas')
Expect 'horários por extenso após «às»' 'Abri às oito horas e fechei às oito e vinte.' @('horário:oito horas','horário:oito e vinte')
Expect 'intervalo por extenso' 'Cheguei entre sete e meia e oito horas, não às oito e meia.' @('horário:sete e meia','horário:oito horas','horário:oito e meia')
Expect 'meio-dia (com «perto do», marcado inteiro)' 'Perto do meio-dia, saí.' @('horário:Perto do meio-dia')
Expect 'meio-dia exato' 'Saí ao meio-dia.' @('horário:meio-dia')
Expect 'datas numéricas' 'Em 12/08/2026, 12/08, 12-08-2026 e 12.08.26.' @('data:12/08/2026','data:12/08','data:12-08-2026','data:12.08.26')
Expect 'datas por extenso' 'Foi em 12 de agosto de 2026, no dia 13 ou em 1º de maio.' @('data:12 de agosto de 2026','data:dia 13','data:1º de maio')
Expect 'CPF formatado e após a palavra CPF' 'CPF 123.456.789-09; outro CPF: 98765432100.' @('CPF:123.456.789-09','CPF:98765432100')
Expect 'placas antiga e Mercosul' 'Placas ABC-1234, ABC1234, ABC 1234, BRA2E19 e BRA-2E19.' @('placa:ABC-1234','placa:ABC1234','placa:ABC 1234','placa:BRA2E19','placa:BRA-2E19')
Expect 'valores em reais' 'Paguei R$ 1.234,56, R$50, 50 reais, cinquenta reais, mil e duzentos reais e dez reais e cinquenta centavos.' @('valor:R$ 1.234,56','valor:R$50','valor:50 reais','valor:cinquenta reais','valor:mil e duzentos reais','valor:dez reais e cinquenta centavos')
Expect 'telefones' 'Ligue (11) 91234-5678, 91234-5678, 3456-7890 ou +55 11 91234-5678.' @('telefone:(11) 91234-5678','telefone:91234-5678','telefone:3456-7890','telefone:+55 11 91234-5678')

# Falsos positivos que não podem ser destacados
Expect 'pronome «as duas» não é horário' 'Vi as duas assinarem campos diferentes.' @()
Expect 'minúsculas e ano não são placa' 'Pela lei 8666, no dia de 2024.' @()
Expect 'quantidades e medidas não são data' 'Dois ou três dias, uns 2-3 dias, 1.5 km.' @()
Expect 'intervalo de anos não é telefone' 'Entre 2025-2026 houve mudanças.' @()
Expect 'nomes próprios ficam de fora' 'O porteiro Amaro e a síndica Ruth conversaram.' @()
Expect '«às duas caixas» não é horário' 'Entregou às duas caixas que chegaram.' @()

# Equivalências
function Same($a, $b) { $x = [ReviewScanner]::Find($a); $y = [ReviewScanner]::Find($b); if ($x.Count -ne 1 -or $y.Count -ne 1) { throw "esperado 1 item em «$a» e «$b»" }; [ReviewScanner]::Equivalent($x[0], $y[0]) }
Check ((Same 'às 14h' 'às 14h00') -and (Same 'às 14h' 'às 14:00') -and (Same 'às 14h00' 'às 14:00')) '14h = 14h00 = 14:00' ''
Check (-not (Same 'às 14h' 'às 15h')) '14h diferente de 15h' ''
Check (-not (Same 'às oito horas' 'às 8h') -and -not (Same 'às oito horas' 'às 20:00')) '«oito horas» sem período (8h ou 20h) não é equivalente exato de 8h nem de 20h' ''
Check (-not (Same 'às oito da manhã' 'às 20:00')) '«oito da manhã» diferente de 20:00' ''
Check ((Same 'R$ 50,00' 'cinquenta reais') -and (Same 'R$ 50,00' '50 reais')) 'R$ 50,00 = cinquenta reais = 50 reais' ''
Check ((Same '12/08' '12 de agosto') -and (Same '12/08/2026' '12 de agosto')) '12/08 = 12 de agosto = 12/08/2026' ''
Check (-not (Same '12/08/2026' '12/08/2025')) 'anos diferentes não equivalem' ''
Check (Same '(11) 91234-5678' '91234-5678') 'telefone com e sem DDD' ''
Check ((Same 'ABC-1234' 'ABC1234') -and -not (Same 'ABC-1234' 'ABC-1243')) 'placa com e sem hífen; dígitos trocados não equivalem' ''

# Comparação e resumo
$orig = 'Cheguei às 14h do dia 12/08. Paguei R$ 50,00. A placa era ABC-1234.'
$ref = 'O depoente chegou às 14:00 do dia 12 de agosto. Pagou cinquenta reais. A placa era ABC-1243.'
$r = [ReviewScanner]::Compare($orig, $ref)
$unmatched = @($r.Reformulated | Where-Object Unmatched | ForEach-Object Text) -join ','
Check ($r.Reformulated.Count -eq 4 -and $r.UnmatchedInReformulated -eq 1 -and $unmatched -eq 'ABC-1243') 'só o item alterado fica sem correspondência' $unmatched
Check (($r.MissingFromReformulated | ForEach-Object Text) -join ',' -eq 'ABC-1234') 'item do original que sumiu é listado' ''
$summary = [ReviewScanner]::Summary($r)
# The sample uses «o depoente» without saying the gender: one role alert (see maintenance/role-alerts).
Check ($summary -eq '4 itens para conferir, 1 não encontrado no original; sumiu do reformulado: ABC-1234; 1 aviso informativo.') 'resumo' $summary
Check ([ReviewScanner]::Summary([ReviewScanner]::Compare('Sem itens aqui.', 'Nada a conferir.')) -eq '') 'sem itens: resumo vazio' ''
$r2 = [ReviewScanner]::Compare('Cheguei às 9h.', '')
Check ($r2.Original.Count -eq 1 -and -not $r2.Original[0].Unmatched) 'sem reformulado: original destacado sem comparação' ''

# Horários aproximados, relativos e ambíguos: expressão inteira, nunca "bate".
function One($text) { $l = [ReviewScanner]::Find($text); if ($l.Count -ne 1) { return $null }; $l[0] }
function ExpectApprox($name, $text, $whole) {
    $i = One $text
    Check ($null -ne $i -and $i.Text -eq $whole -and $i.Approximate -and [ReviewScanner]::Uncertain($i)) $name $(if ($i) { "«" + $i.Text + "» aproximado=" + $i.Approximate } else { 'itens<>1' })
}
ExpectApprox 'relativo «dez para as onze» marcado inteiro' 'Foi umas dez para as onze, por aí.' 'umas dez para as onze, por aí'
ExpectApprox '«meio-dia e pouco» marcado inteiro' 'Saí meio-dia e pouco.' 'meio-dia e pouco'
ExpectApprox '«umas duas e pouco» marcado inteiro' 'Acho que era umas duas e pouco, não lembro.' 'umas duas e pouco'
ExpectApprox '«depois das duas» é relativo' 'Começou depois das duas, mas não sei.' 'depois das duas'
ExpectApprox '«por volta das 14h30» é aproximado' 'Cheguei por volta das 14h30.' 'por volta das 14h30'
ExpectApprox '«pouco antes das nove» é relativo' 'Entrei pouco antes das nove.' 'pouco antes das nove'
$i = One 'Cheguei às duas e meia.'
Check ($null -ne $i -and $i.Text -eq 'duas e meia' -and $i.AmbiguousHalfDay -and [ReviewScanner]::Uncertain($i)) '«duas e meia» sem período é ambíguo (2h30 ou 14h30)' $(if ($i) { $i.Text } else { '' })
$i = One 'Cheguei às 8h.'
Check ($null -ne $i -and -not [ReviewScanner]::Uncertain($i)) '«8h» numérico é exato (relógio de 24 horas)' ''
function State($item) { if ($item.Unmatched) { 'sem correspondência' } elseif ($item.Alert) { 'alerta' } else { 'bate' } }
function ExpectState($name, $orig, $ref, $expectedRef, $expectedOrig) {
    $r = [ReviewScanner]::Compare($orig, $ref)
    $got = (State $r.Reformulated[0]) + ' / ' + (State $r.Original[0])
    Check ($r.Reformulated.Count -eq 1 -and $r.Original.Count -eq 1 -and $got -eq ($expectedRef + ' / ' + $expectedOrig)) $name ("reformulado / original = " + $got)
}
ExpectState 'relativo vs exato: nunca «bate»' 'Foi umas dez para as onze.' 'Foi às 10h50.' 'alerta' 'alerta'
ExpectState 'relativo vs outro horário distante: sem correspondência' 'Foi umas dez para as onze.' 'Foi às 15h.' 'sem correspondência' 'sem correspondência'
ExpectState '«meio-dia e pouco» vs «ao meio-dia»: alerta' 'Saí meio-dia e pouco.' 'Saiu ao meio-dia.' 'alerta' 'alerta'
ExpectState '«umas duas e pouco» vs «14h»: alerta' 'Era umas duas e pouco.' 'Eram 14h.' 'alerta' 'alerta'
ExpectState '«duas e meia» ambíguo vs «14h30»: alerta' 'Cheguei às duas e meia.' 'Chegou às 14h30.' 'alerta' 'alerta'
ExpectState 'mesma expressão ambígua dos dois lados: ainda alerta' 'Cheguei às duas e meia.' 'Chegou às duas e meia.' 'alerta' 'alerta'
ExpectState 'aproximado mantido igual: ainda alerta' 'Cheguei por volta das 14h30.' 'Chegou por volta das 14:30.' 'alerta' 'alerta'
ExpectState 'exato vs exato continua «bate»' 'Cheguei às 14h.' 'Chegou às 14:00.' 'bate' 'bate'
$r = [ReviewScanner]::Compare('Cheguei às 14h30. Placa ABC-1234. Saí às 15h15.', 'Chegou às 14:30. Placa ABC-1243. Saiu às 15h.')
Check ([ReviewScanner]::Summary($r) -eq '3 itens para conferir, 2 não encontrados no original; sumiram do reformulado: ABC-1234, 15h15.') 'resumo no plural («sumiram»)' ([ReviewScanner]::Summary($r))
Check ([ReviewScanner]::ShortSummary($r) -eq '3 itens para conferir, 2 não encontrados no original; 2 sumiram do reformulado. Clique para ver a lista.') 'resumo curto sem nomes' ([ReviewScanner]::ShortSummary($r))
$d = [ReviewScanner]::Details($r)
Check ($d.Contains('Não encontrados no original') -and $d.Contains('ABC-1243') -and $d.Contains('Sumiram do reformulado') -and $d.Contains('ABC-1234') -and $d.Contains('Conferidos nos dois textos') -and $d.Contains('14:30')) 'lista completa ao clicar' ''
$results | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'scanner-results.txt')
Write-Output ($(if ($failures -eq 0) { 'RESULT PASS' } else { "RESULT FAIL ($failures)" }))
if ($failures -ne 0) { exit 1 }
