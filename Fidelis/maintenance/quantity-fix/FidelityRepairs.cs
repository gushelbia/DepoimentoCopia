using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DepoimentoLocal.Windows
{
    public static class FidelityRepairs
    {
        // Conservative repairs anchored in the original. No new inference,
        // actor substitution or reconstruction of unrelated sentences.
        public static string Repair(string original, string output)
        {
            if (String.IsNullOrWhiteSpace(original) || String.IsNullOrWhiteSpace(output)) return output;
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            const string quotedPattern = "\"[^\"]*\"|“[^”]*”|«[^»]*»";
            string source = original;
            string confirmed = @"\b(?:(?:aí|então)\s+)?(?:eu\s+)?saí\s*(?:\.{2,}|…)\s*não\s*[,!.…]*\s*(?:eu\s+)?acho\s+que\s+antes\s+de\s+sair\s+entrou\s+(?:o\s+|a\s+)?(?<person>[\p{L}\p{M}]+(?:\s+[\p{L}\p{M}]+){0,3})\s*\.\s*É\s*,?\s*(?:o\s+|a\s+)?\k<person>\s+entrou\s+antes\s*\.";
            MatchCollection corrections = Regex.Matches(source, confirmed, options);
            if (corrections.Count == 1)
            {
                Match correction = corrections[0];
                bool sourceQuoted = false;
                foreach (Match quote in Regex.Matches(source, quotedPattern))
                    if (correction.Index < quote.Index + quote.Length && correction.Index + correction.Length > quote.Index) sourceQuoted = true;
                string person = correction.Groups["person"].Value;
                string escaped = Regex.Escape(person);
                string rendered = @"\b(?:(?:aí|então)\s+)?(?:saiu\s*(?:\.{2,}|…)\s*(?:não\s*[,!.…]*\s*)?)?(?:acredita\s+que\s+)?(?:antes\s+de\s+sair\s+entrou\s+(?:o\s+|a\s+)?" + escaped + @"|(?:o\s+|a\s+)?" + escaped + @"\s+entrou\s+antes\s+(?:de\s+sair|de\s+sua\s+saída))\s*[.!](?:\s*(?:É\s*,?\s*)?(?:o\s+|a\s+)?" + escaped + @"\s+entrou\s+antes\s*[.!])?";
                MatchCollection candidates = Regex.Matches(output, rendered, options);
                if (!sourceQuoted && candidates.Count == 1)
                {
                    Match candidate = candidates[0];
                    bool outputQuoted = false;
                    foreach (Match quote in Regex.Matches(output, quotedPattern))
                        if (candidate.Index < quote.Index + quote.Length && candidate.Index + candidate.Length > quote.Index) outputQuoted = true;
                    if (!outputQuoted)
                    {
                        string clean = person + " entrou antes de sua saída.";
                        output = output.Substring(0, candidate.Index) + clean + output.Substring(candidate.Index + candidate.Length);
                        source = source.Substring(0, correction.Index) + clean + source.Substring(correction.Index + correction.Length);
                    }
                }
            }

            string[] patterns = {
                @"\bnão\s+(?:(?:me|se)\s+)?(?<verb>sei|sabe|sabia|sabendo|lembro|lembra|lembrava|lembrando|recordo|recorda|recordava|recordando)\b",
                @"\b(?<verb>acho|acha|achava|acredito|acredita|acreditava)\b",
                @"\b(?<verb>posso|pode|podia)\s+estar\s+(?:enganad[oa]|confundindo)\b"
            };
            for (int group = 0; group < patterns.Length; group++)
            {
                var sourceMatches = new List<Match>();
                var outputMatches = new List<Match>();
                for (int side = 0; side < 2; side++)
                {
                    string text = side == 0 ? source : output;
                    MatchCollection quotes = Regex.Matches(text, quotedPattern);
                    foreach (Match match in Regex.Matches(text, patterns[group], options))
                    {
                        bool inside = false;
                        foreach (Match quote in quotes)
                            if (match.Index < quote.Index + quote.Length && match.Index + match.Length > quote.Index) inside = true;
                        if (!inside) (side == 0 ? sourceMatches : outputMatches).Add(match);
                    }
                }
                // Missing/extra statements make alignment ambiguous: leave them
                // to the existing validator rather than inventing a correspondence.
                if (sourceMatches.Count != outputMatches.Count) continue;
                for (int i = sourceMatches.Count - 1; i >= 0; i--)
                {
                    string from = sourceMatches[i].Groups["verb"].Value.ToLowerInvariant();
                    Match match = outputMatches[i];
                    string to = match.Groups["verb"].Value.ToLowerInvariant();
                    string desired = null;
                    if (group == 0)
                    {
                        bool fromKnow = from == "sei" || from.StartsWith("sab", StringComparison.Ordinal);
                        bool toKnow = to == "sei" || to.StartsWith("sab", StringComparison.Ordinal);
                        bool fromRemember = from.StartsWith("lembr", StringComparison.Ordinal);
                        bool toRemember = to.StartsWith("lembr", StringComparison.Ordinal);
                        if (fromKnow != toKnow || fromRemember != toRemember) continue;
                        if (from == "sei" || from == "sabe") desired = "sabe";
                        else if (from == "sabia") desired = "sabia";
                        else if (from == "lembro" || from == "lembra") desired = "lembra";
                        else if (from == "lembrava") desired = "lembrava";
                        else if (from == "recordo" || from == "recorda") desired = "recorda";
                        else if (from == "recordava") desired = "recordava";
                    }
                    else if (group == 1)
                    {
                        if (from == "acho" || from == "acredito") desired = "acredita";
                        else if (from == "achava") desired = "achava";
                        else if (from == "acreditava") desired = "acreditava";
                    }
                    else
                    {
                        if (from == "posso") desired = "pode";
                        else if (from == "podia") desired = "podia";
                    }
                    if (desired == null || desired == to) continue;
                    Group verb = match.Groups["verb"];
                    if (Char.IsUpper(verb.Value[0])) desired = Char.ToUpperInvariant(desired[0]) + desired.Substring(1);
                    output = output.Substring(0, verb.Index) + desired + output.Substring(verb.Index + verb.Length);
                }
            }
            // Restore compound past only when the entire factual sentence still matches.
            // The subject, source, negation and surrounding temporal words must agree;
            // ambiguity or unrelated habitual actions are deliberately left untouched.
            string[,] irregular = {
                { "feito", "fazia|fez", "faziam|fizeram" },
                { "dito", "dizia|disse", "diziam|disseram" },
                { "visto", "via|viu", "viam|viram" },
                { "posto", "punha|pôs", "punham|puseram" },
                { "sido", "era|foi", "eram|foram" },
                { "tido", "tinha|teve", "tinham|tiveram" },
                { "vindo", "vinha|veio", "vinham|vieram" },
                { "escrito", "escrevia|escreveu", "escreviam|escreveram" },
                { "aberto", "abria|abriu", "abriam|abriram" },
                { "coberto", "cobria|cobriu", "cobriam|cobriram" },
                { "morto", "morria|morreu", "morriam|morreram" }
            };
            MatchCollection sourceSentences = Regex.Matches(original, @"[^.!?;\r\n]+[.!?;]?");
            foreach (Match sentence in sourceSentences)
            {
                MatchCollection compound = Regex.Matches(sentence.Value,
                    @"\b(?<aux>tinha|havia|tinham|haviam)\s+(?<part>[\p{L}]+)\b", options);
                foreach (Match action in compound)
                {
                    bool quoted = false;
                    foreach (Match quote in Regex.Matches(original, quotedPattern))
                        if (sentence.Index + action.Index < quote.Index + quote.Length &&
                            sentence.Index + action.Index + action.Length > quote.Index) quoted = true;
                    if (quoted) continue;
                    string participle = action.Groups["part"].Value.ToLowerInvariant();
                    string auxiliary = action.Groups["aux"].Value.ToLowerInvariant();
                    bool plural = auxiliary.EndsWith("m", StringComparison.Ordinal);
                    string alternatives = null;
                    for (int v = 0; v < irregular.GetLength(0); v++)
                        if (participle == irregular[v, 0]) alternatives = irregular[v, plural ? 2 : 1];
                    if (alternatives == null)
                    {
                        if (participle.EndsWith("ado", StringComparison.Ordinal))
                        {
                            string stem = Regex.Escape(participle.Substring(0, participle.Length - 3));
                            alternatives = stem + (plural ? "(?:avam|aram)" : "(?:ava|ou)");
                        }
                        else if (participle.EndsWith("ido", StringComparison.Ordinal))
                        {
                            string stem = Regex.Escape(participle.Substring(0, participle.Length - 3));
                            alternatives = stem + (plural ? "(?:iam|eram|iram)" : "(?:ia|eu|iu)");
                        }
                        else if (participle.EndsWith("ído", StringComparison.Ordinal))
                        {
                            string stem = Regex.Escape(participle.Substring(0, participle.Length - 3));
                            alternatives = stem + (plural ? "(?:íam|íram)" : "(?:ía|iu)");
                        }
                    }
                    if (alternatives == null) continue;
                    // Do not repair a collapsed/missing version of a repeated action.
                    string family = @"\b(?:(?:tinha|havia|tinham|haviam)\s+" + Regex.Escape(participle) + "|" + alternatives + @")\b";
                    if (Regex.Matches(original, family, options).Count != Regex.Matches(output, family, options).Count) continue;
                    string expectedSentence = sentence.Value.Substring(0, action.Index) + " ASPECTMARKER " + sentence.Value.Substring(action.Index + action.Length);
                    int foundStart = -1, foundLength = 0, matches = 0;
                    foreach (Match renderedSentence in Regex.Matches(output, @"[^.!?;\r\n]+[.!?;]?"))
                    {
                        foreach (Match candidate in Regex.Matches(renderedSentence.Value, @"\b(?:" + alternatives + @")\b", options))
                        {
                            bool outputQuoted = false;
                            int candidateStart = renderedSentence.Index + candidate.Index;
                            foreach (Match quote in Regex.Matches(output, quotedPattern))
                                if (candidateStart < quote.Index + quote.Length && candidateStart + candidate.Length > quote.Index) outputQuoted = true;
                            if (outputQuoted) continue;
                            string actualSentence = renderedSentence.Value.Substring(0, candidate.Index) + " ASPECTMARKER " + renderedSentence.Value.Substring(candidate.Index + candidate.Length);
                            string[] comparable = { expectedSentence, actualSentence };
                            for (int side = 0; side < comparable.Length; side++)
                            {
                                string value = comparable[side].ToLowerInvariant();
                                value = Regex.Replace(value, @"\bme\b", "lhe", options);
                                value = Regex.Replace(value, @"\bcomigo\b", "com depoente", options);
                                value = Regex.Replace(value, @"\b(?:eu|o|a|os|as)\b", " ", options);
                                value = Regex.Replace(value, @"[^\p{L}\p{N}]+", " ").Trim();
                                comparable[side] = value;
                            }
                            if (comparable[0] == comparable[1])
                            {
                                matches++; foundStart = candidateStart; foundLength = candidate.Length;
                            }
                        }
                    }
                    if (matches == 1)
                    {
                        string preserved = auxiliary + " " + participle;
                        if (Char.IsUpper(output[foundStart])) preserved = Char.ToUpperInvariant(preserved[0]) + preserved.Substring(1);
                        output = output.Substring(0, foundStart) + preserved + output.Substring(foundStart + foundLength);
                    }
                }
            }
            // Preserve explicit group counts when a pronoun is the only change
            // in an otherwise matching sentence. Never guess an ambiguous referent.
            string cardinal = @"(?:\d+|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|onze|doze|treze|catorze|quatorze|quinze|dezesseis|dezasseis|dezessete|dezassete|dezoito|dezenove|dezanove|vinte|trinta|quarenta|cinquenta|sessenta|setenta|oitenta|noventa|cem|cento|duzentos|duzentas|trezentos|trezentas|quatrocentos|quatrocentas|quinhentos|quinhentas|seiscentos|seiscentas|setecentos|setecentas|oitocentos|oitocentas|novecentos|novecentas|mil)";
            string quantityPattern = @"\b(?:(?:os|as)\s+" + cardinal + @"(?:\s+e\s+(?:" + cardinal + @"|um|uma))*|ambos|ambas)\b";
            foreach (Match sentence in Regex.Matches(original, @"[^.!?;\r\n]+[.!?;]?"))
            {
                MatchCollection quantities = Regex.Matches(sentence.Value, quantityPattern, options);
                if (quantities.Count != 1) continue;
                Match quantity = quantities[0];
                bool quoted = false;
                foreach (Match quote in Regex.Matches(original, quotedPattern))
                    if (sentence.Index + quantity.Index < quote.Index + quote.Length &&
                        sentence.Index + quantity.Index + quantity.Length > quote.Index) quoted = true;
                if (quoted) continue;
                string expected = Regex.Replace(sentence.Value.Substring(0, quantity.Index) + " QUANTITYMARKER " +
                    sentence.Value.Substring(quantity.Index + quantity.Length), @"\s+", " ").Trim();
                int sourceCount = 0;
                foreach (Match other in Regex.Matches(original, @"[^.!?;\r\n]+[.!?;]?"))
                {
                    string comparable = Regex.Replace(Regex.Replace(other.Value, quantityPattern, " QUANTITYMARKER ", options), @"\s+", " ").Trim();
                    if (String.Equals(expected, comparable, StringComparison.OrdinalIgnoreCase)) sourceCount++;
                }
                if (sourceCount != 1) continue;
                int foundStart = -1, foundLength = 0, matches = 0;
                string pronoun = Regex.IsMatch(quantity.Value, @"^(?:as\b|ambas\b)", options) ? "elas" : "eles";
                foreach (Match rendered in Regex.Matches(output, @"[^.!?;\r\n]+[.!?;]?"))
                {
                    foreach (Match candidate in Regex.Matches(rendered.Value, @"\b" + pronoun + @"\b", options))
                    {
                        int start = rendered.Index + candidate.Index;
                        bool outputQuoted = false;
                        foreach (Match quote in Regex.Matches(output, quotedPattern))
                            if (start < quote.Index + quote.Length && start + candidate.Length > quote.Index) outputQuoted = true;
                        if (outputQuoted) continue;
                        string actual = Regex.Replace(rendered.Value.Substring(0, candidate.Index) + " QUANTITYMARKER " +
                            rendered.Value.Substring(candidate.Index + candidate.Length), @"\s+", " ").Trim();
                        if (String.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                        { matches++; foundStart = start; foundLength = candidate.Length; }
                    }
                }
                if (matches == 1)
                {
                    string preserved = quantity.Value;
                    if (Char.IsUpper(output[foundStart])) preserved = Char.ToUpperInvariant(preserved[0]) + preserved.Substring(1);
                    output = output.Substring(0, foundStart) + preserved + output.Substring(foundStart + foundLength);
                }
            }
            return output;
        }
    }
}


