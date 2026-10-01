# Protótipo (só para o plano): converte para o feminino uma saída gerada no masculino,
# usando o original como guia. Nada disso está no programa.
function Convert-ToFeminine([string]$source, [string]$text) {
    $t = $text
    # 1) O sintagma do depoente e suas contrações.
    $t = $t -creplace '\bO depoente\b','A depoente' -creplace '\bo depoente\b','a depoente'
    $t = $t -creplace '\bdo depoente\b','da depoente' -creplace '\bao depoente\b','à depoente' -creplace '\bno depoente\b','na depoente' -creplace '\bpelo depoente\b','pela depoente'
    # 2) Adjetivos e particípios que o ORIGINAL põe no feminino para a narradora.
    $verbs = 'fiquei|fui|estava|estou|sou|era|cheguei|caí|senti|sinto|permaneci|continuei|saí|acordei|voltei|terminei|estive|fico'
    foreach ($m in [regex]::Matches($source, "(?i)\b(?:eu\s+)?(?:me\s+)?(?:$verbs)\s+(?:muito\s+|bem\s+|tão\s+)?(?<w>\p{L}+a)\b")) {
        $fem = $m.Groups['w'].Value
        if ($fem -match '^(?i)(a|uma|na|da|para|lá|nada|toda|casa|porta|escola|mãe)$') { continue }
        $masc = $fem.Substring(0, $fem.Length - 1) + 'o'
        # não troca quando a palavra masculina se refere a «ele» na saída (outro participante)
        $t = [regex]::Replace($t, "(?i)(?<!\bele\s+\p{L}+\s+)\b$masc\b", $fem)
    }
    # 3) Pronome oblíquo «o» que veio de «me» no original: «ele o ameaçou» → «ele a ameaçou».
    foreach ($m in [regex]::Matches($source, '(?i)\b(?<s>ele|ela|eles|elas)\s+(?:não\s+)?me\s+(?<v>\p{L}{3})')) {
        $t = [regex]::Replace($t, "(?i)\b($($m.Groups['s'].Value))\s+(não\s+)?o\s+($($m.Groups['v'].Value)\p{L}*)", '$1 $2a $3') -replace '  ',' '
    }
    return $t
}