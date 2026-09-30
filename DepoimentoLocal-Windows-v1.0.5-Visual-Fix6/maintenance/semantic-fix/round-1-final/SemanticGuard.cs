using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DepoimentoLocal.Windows
{
    // Conservative rejection, never reconstruction of missing facts. These are
    // linguistic checks, not proof of semantic equivalence or a general parser.
    public static class SemanticGuard
    {
        public static string NameKey(string name)
        {
            var key = new System.Text.StringBuilder();
            foreach (char c in name.Normalize(System.Text.NormalizationForm.FormD))
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    key.Append(Char.ToLowerInvariant(c));
            return key.ToString();
        }

        public static string RepairNameSpelling(string original, string output)
        {
            if (String.IsNullOrWhiteSpace(original) || String.IsNullOrWhiteSpace(output)) return output;
            var known = new Dictionary<string,string>();
            var ambiguous = new HashSet<string>();
            // Only orthographic variants of explicitly named participants, never
            // assignment of a name to a pronoun or to the narrator.
            foreach (Match person in Regex.Matches(Unquote(original), @"\b(?i:[oa]|com|para|segundo)\s+(?<p>\p{Lu}[\p{Ll}\p{M}]+)\b"))
            {
                string name = person.Groups["p"].Value;
                string key = NameKey(name);
                if (known.ContainsKey(key) && known[key] != name) ambiguous.Add(key);
                else known[key] = name;
            }
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(output, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = output.Substring(start, quote.Index - start);
                MatchCollection words = Regex.Matches(part, @"\b\p{Lu}[\p{Ll}\p{M}]+\b");
                for (int i = words.Count - 1; i >= 0; i--)
                {
                    Match word = words[i]; string key = NameKey(word.Value);
                    if (known.ContainsKey(key) && !ambiguous.Contains(key) && word.Value != known[key])
                        part = part.Substring(0, word.Index) + known[key] + part.Substring(word.Index + word.Length);
                }
                result.Append(part); result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        public static string RepairFirstPerson(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return text;
            // Explicit unambiguous conjugations only; never infer a conjugation
            // from a suffix. Quoted speech is copied byte for byte.
            string[,] forms = {
                { "entendi", "entendeu" }, { "fiquei", "ficou" },
                { "verifiquei", "verificou" }, { "confirmei", "confirmou" },
                { "observei", "observou" }, { "escutei", "escutou" },
                { "assisti", "assistiu" },
                { "assinei", "assinou" }, { "anotei", "anotou" },
                { "registrei", "registrou" }, { "enviei", "enviou" },
                { "esperei", "esperou" }, { "aguardei", "aguardou" },
                { "prestei", "prestou" }, { "compreendi", "compreendeu" }
            };
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(text, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = text.Substring(start, quote.Index - start);
                part = Regex.Replace(part, @"\b([Oo]) depente\b", "$1 depoente");
                for (int i = 0; i < forms.GetLength(0); i++)
                {
                    // Maintain capitalization without a closure in the embedded IL.
                    part = Regex.Replace(part, @"\b" + forms[i,0] + @"\b", forms[i,1]);
                    string capFrom = Char.ToUpperInvariant(forms[i,0][0]) + forms[i,0].Substring(1);
                    string capTo = Char.ToUpperInvariant(forms[i,1][0]) + forms[i,1].Substring(1);
                    part = Regex.Replace(part, @"\b" + capFrom + @"\b", capTo);
                }
                result.Append(part); result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        public static string Unquote(string text)
        {
            return Regex.Replace(text, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'", " ");
        }

        public static string Normalize(string text)
        {
            return Regex.Replace(text.ToLowerInvariant(), @"\s+", " ").Trim();
        }

        public static HashSet<string> Anchors(string text)
        {
            var result = new HashSet<string>();
            foreach (Match word in Regex.Matches(Normalize(text), @"[\p{L}\p{N}]+"))
            {
                string w = word.Value;
                if (Regex.IsMatch(w, @"^(?:depoente|relatou|informou|disse|depois|antes|quando|estava|estavam|tinha|tinham|havia|haviam|alguma|algum|coisa|porque|então|para|pelo|pela|dele|dela|ele|ela|eles|elas|com|que|uma|umas|esse|essa|isso|aquilo|também|mais|muito|foi|ser|não|sim|lhe|seu|sua|meu|minha|eu)$")) continue;
                if (w.Length >= 4 || Char.IsDigit(w[0])) result.Add(w.Length > 6 ? w.Substring(0, w.Length - 2) : w);
            }
            return result;
        }

        public static string Validate(string original, string output)
        {
            if (String.IsNullOrWhiteSpace(output)) return "fidelidade: saída vazia";
            string source = Normalize(Unquote(original));
            string rendered = Normalize(Unquote(output));
            var participants = new HashSet<string>();
            // Reject an explicit participant substituted for the narrator. Only
            // check participants named in the input and narrator-specific verbs;
            // leave unresolved pronouns unresolved rather than guessing a name.
            string[,] perception = {
                { "vi", "viu" }, { "ouvi", "ouviu" }, { "entendi", "entendeu" },
                { "presenciei", "presenciou" }, { "verifiquei", "verificou" },
                { "sei", "sabe" }, { "lembro", "lembra" }
            };
            foreach (Match person in Regex.Matches(Unquote(original), @"\b(?i:[oa]|com|para|segundo)\s+(?<p>\p{Lu}[\p{Ll}\p{M}]+)\b"))
            {
                string name = Regex.Escape(person.Groups["p"].Value.ToLowerInvariant());
                participants.Add(name);
                for (int i = 0; i < perception.GetLength(0); i++)
                {
                    string assigned = @"\b" + name + @"\s+(?:não\s+)?(?:se\s+)?" + perception[i,1] + @"\b";
                    if (Regex.IsMatch(source, @"\b" + perception[i,0] + @"\b") &&
                        Regex.IsMatch(rendered, assigned) && !Regex.IsMatch(source, assigned))
                        return "fidelidade: participante atribuído como sujeito da percepção ou conhecimento do depoente";
                }
            }
            // In addition to the existing explicit vocabulary, detect first-person
            // forms actually present in this input; never alter a guessed verb.
            foreach (Match verb in Regex.Matches(source, @"\b(?<v>[\p{L}]{3,}ei|entendi|compreendi|assisti|vi|ouvi|sei|sou|estou|tenho|posso|vou|faço|quero|lembro|recordo|acredito|sinto|percebi|consegui|recebi)\b"))
            {
                string v = verb.Groups["v"].Value;
                if (Regex.IsMatch(rendered, @"\b" + Regex.Escape(v) + @"\b")) return "fidelidade: possível primeira pessoa residual: " + v;
            }
            // Quote content is immutable; typography may change.
            foreach (Match quote in Regex.Matches(original, "\"(?<q>[^\"]*)\"|“(?<q>[^”]*)”|«(?<q>[^»]*)»"))
                if (!Normalize(output).Contains(Normalize(quote.Groups["q"].Value))) return "fidelidade: fala citada omitida ou alterada";

            string[,] negativePerception = {
                { @"\bnão\s+(?:vi|viu)\b", @"\bnão\s+viu\b" },
                { @"\bnão\s+(?:ouvi|ouviu)\b", @"\bnão\s+ouviu\b" },
                { @"\bnão\s+(?:presenciei|presenciou)\b", @"\bnão\s+presenciou\b" }
            };
            for (int i = 0; i < negativePerception.GetLength(0); i++)
                if (Regex.Matches(source, negativePerception[i,0]).Count > Regex.Matches(rendered, negativePerception[i,1]).Count)
                    return "fidelidade: negação de percepção omitida ou transferida para o fato";

            string[] srcSentences = Regex.Split(source, @"(?<=[.!?])\s+|[\r\n]+");
            string[] dstSentences = Regex.Split(rendered, @"(?<=[.!?])\s+|[\r\n]+");
            foreach (string sentence in srcSentences)
            {
                var anchors = Anchors(sentence);
                if (anchors.Count < 2) continue;
                string best = ""; double bestScore = -1;
                foreach (string candidate in dstSentences)
                {
                    var found = Anchors(candidate); int count = 0;
                    foreach (string anchor in anchors) if (found.Contains(anchor)) count++;
                    double score = (double)count / anchors.Count;
                    if (score > bestScore) { bestScore = score; best = candidate; }
                }
                if (bestScore < 0.5) return "fidelidade: possível omissão de oração: " + sentence;
                foreach (string name in participants)
                    if (Regex.IsMatch(sentence, @"\b" + name + @"\b") && !Regex.IsMatch(best, @"\b" + name + @"\b"))
                        return "fidelidade: participante explícito omitido ou substituído na oração: " + sentence;
                // Do not count the interjection "não," in an abandoned false
                // start as a factual negation. A lost local negation is critical.
                if (Regex.Matches(sentence, @"\bnão\s+\p{L}").Count >
                    Regex.Matches(best, @"\b(?:não\s+\p{L}|nem\b)").Count)
                    return "fidelidade: possível perda de negação na oração: " + sentence;
                // Compare operators within the matching sentence, so a negation or
                // hearsay elsewhere cannot hide a lost operator here.
                string[,] operators = {
                    { @"\bnão\s+(?:vi|viu|vimos)\b", @"\bnão\s+(?:viu|viram|vimos)\b", "negação de percepção visual" },
                    { @"\bnão\s+(?:ouvi|ouviu|ouvimos)\b", @"\bnão\s+(?:ouviu|ouviram|ouvimos)\b", "negação de percepção auditiva" },
                    { @"\b(?:eu\s+)?vi\b", @"\b(?:viu|visto|presenciou)\b", "percepção visual do depoente" },
                    { @"\bouvi\b", @"\b(?:ouviu|ouvido)\b", "percepção auditiva do depoente" },
                    { @"\bme\s+(?:disseram|falaram|contaram|informaram)\b", @"\b(?:lhe\s+(?:disseram|falaram|contaram|informaram)|(?:disseram|falaram|contaram|informaram)[ -]lhe|foi informad[oa]|ouviu dizer)\b", "fonte indireta" },
                    { @"\b(?:me\s+(?:disse|falou|contou|informou|explicou|relatou)|comigo)\b", @"\b(?:lhe|depoente)\b", "destinatário do relato" },
                    { @"\b(?:disse|falou|contou|relatou|informou)\s+que\s+(?:viu|ouviu)\b", @"\b(?:disse|falou|contou|relatou|informou)\s+(?:que\s+(?:viu|ouviu)|ter\s+(?:visto|ouvido))\b", "cadeia de relato de percepção" },
                    { @"\bnão\s+(?:me\s+)?(?:lembro|recordo)\b", @"\bnão\s+(?:se\s+)?(?:lembra|recorda)\b", "limitação de memória" },
                    { @"\bnão\s+sei\b", @"\bnão\s+sabe\b", "limitação de conhecimento" },
                    { @"\b(?:acho|acredito)\b", @"\b(?:acha|acredita)\b", "crença ou incerteza" },
                    { @"\bposso\s+estar\b", @"\bpode\s+estar\b", "possibilidade de engano" }
                };
                for (int i = 0; i < operators.GetLength(0); i++)
                    if (Regex.IsMatch(sentence, operators[i,0]) && !Regex.IsMatch(best, operators[i,1]))
                        return "fidelidade: perda de " + operators[i,2] + ": " + sentence;
                foreach (Match temporal in Regex.Matches(sentence, @"\b(?:segunda-feira|terça-feira|quarta-feira|quinta-feira|sexta-feira|sábado|domingo|talvez|aproximadamente|aparentemente|certeza|suposição|perto|longe|presente|almoço|manhã|\d+(?::\d+)?)\b"))
                {
                    if (temporal.Value == "aparentemente" && Regex.IsMatch(best, @"\bparece que\b")) continue;
                    if (!best.Contains(temporal.Value)) return "fidelidade: marcador temporal ou de certeza omitido: " + temporal.Value;
                }
                if (Regex.IsMatch(sentence, @"\b(?:uns|umas)\s+(?:\d+|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|quinze|vinte)\b") &&
                    !Regex.IsMatch(best, @"\b(?:uns|umas|cerca|aproximadamente|por volta|mais ou menos|em torno)\b"))
                    return "fidelidade: aproximação de quantidade ou horário omitida";
            }
            return null;
        }
    }
}
