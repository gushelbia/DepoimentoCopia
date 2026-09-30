using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DepoimentoLocal.Windows
{
    public static class FidelityRepairs
    {
        // Two conservative repairs anchored in the original. No new inference,
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
            return output;
        }
    }
}
