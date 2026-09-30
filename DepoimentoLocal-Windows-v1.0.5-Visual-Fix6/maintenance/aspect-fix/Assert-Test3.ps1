$ErrorActionPreference='Stop'
$output=Get-Content (Join-Path $PSScriptRoot 'teste3-output.txt') -Raw
$log=Get-Content (Join-Path $PSScriptRoot 'teste3-log.txt') -Raw
$checks=[ordered]@{
 'Ação concluída preservada'=$output.Contains('Ana lhe disse que Roberto tinha batido a porta quando entrou na sala.')
 'Sem batia indevido'= -not ($output -match '\bbatia\b')
 'Anterioridade de dito'=$output.Contains('Ana falou que ele tinha dito que não voltaria para aquela sala.')
 'Versão de Carlos sobre a porta'=$output.Contains('Carlos falou com ele e disse que, na verdade, a porta já estava fechada e que Roberto só empurrou para abrir.')
 'Nenhuma versão escolhida'=$output.Contains('Não sabe qual das duas versões está correta.')
 'Versão sala / versão reunião'=$output.Contains('Carlos, por outro lado, disse que ouviu Roberto falar que não voltaria para aquela reunião.')
 'Limitação de memória'=$output.Contains('não se lembra exatamente o que ele disse')
 'Incerteza na ordem'=$output.Contains('Acredita que Roberto saiu alguns minutos depois, mas pode estar confundindo a ordem.')
 'Relato da servidora e negativa pessoal'=$output.Contains('Uma servidora lhe disse depois que os dois saíram juntos. Não viu os dois saindo juntos.')
 'Ana mesmo dia / Carlos dia seguinte'=$output.Contains('Ana lhe falou que essa conversa aconteceu no mesmo dia. Carlos, por outro lado, disse que acredita que foi no dia seguinte.')
 'Desconhecimento e não participação'=$output.Contains('Não participou dessa conversa e não sabe quando ela realmente aconteceu.')
 'Terceira pessoa'= -not ($output -match '\b(eu|me|comigo|cheguei|vi|ouvi|sei|lembro|acho|posso|consegui|participei)\b')
 'Validação aceita'=$log.Contains('[GENERATION_OK]')
 'Sem retry'= -not $log.Contains('[BLOCK_RETRY_START]')
 'Sem overflow'= -not $log.Contains('ContextOverflowException')
 'Parâmetros preservados'=$log.Contains('context=4096; batch=512; ubatch=256; threads=4; gpuLayers=8; mmap=true; temperature=0')
}
$report=foreach($check in $checks.GetEnumerator()){"$(if($check.Value){'PASS'}else{'FAIL'}) $($check.Key)"}
$report | Set-Content (Join-Path $PSScriptRoot 'teste3-fidelity-results.txt')
$report
if($checks.Values -contains $false){throw 'Teste 3: falha de fidelidade'}
Add-Type -Path (Join-Path $PSScriptRoot 'FidelityRepairs.compiled.dll')
$source=Get-Content (Join-Path $PSScriptRoot 'teste3-original.txt') -Raw
$draft=$output.Replace('Roberto tinha batido a porta','Roberto batia a porta')
$actual=[DepoimentoLocal.Windows.FidelityRepairs]::Repair($source,$draft)
if($actual -cne $output){throw 'A correção não se limitou ao trecho esperado'}
$timer=[Diagnostics.Stopwatch]::StartNew()
for($i=0;$i -lt 100;$i++){[void][DepoimentoLocal.Windows.FidelityRepairs]::Repair($source,$draft)}
$timer.Stop()
("PASS correção local isolada: apenas batia -> tinha batido; média de 100 execuções: {0:N3} ms" -f ($timer.Elapsed.TotalMilliseconds/100)) | Tee-Object -FilePath (Join-Path $PSScriptRoot 'local-performance.txt')
