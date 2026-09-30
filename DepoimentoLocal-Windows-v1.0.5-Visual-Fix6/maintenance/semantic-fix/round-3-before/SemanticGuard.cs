using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DepoimentoLocal.Windows
{
    // Conservative rejection, never reconstruction of missing facts. These are
    // linguistic checks, not proof of semantic equivalence or a general parser.
    public static class SemanticGuard
    {
        public static string[,] NarratorForms()
        {
            return new string[,] {
                {"sei","sabe"}, {"lembro","lembra"}, {"recordo","recorda"},
                {"ouvi","ouviu"}, {"vi","viu"}, {"perguntei","perguntou"},
                {"presenciei","presenciou"}, {"entendi","entendeu"},
                {"verifiquei","verificou"}, {"confirmei","confirmou"},
                {"acho","acredita"}, {"acredito","acredita"}, {"posso","pode"},
                {"tenho","tem"}, {"cheguei","chegou"}, {"entrei","entrou"},
                {"saí","saiu"}, {"voltei","voltou"}, {"fui","foi"}, {"retornei","retornou"}, {"parti","partiu"},
                {"encontrei","encontrou"}, {"participei","participou"},
                {"percebi","percebeu"}, {"consegui","conseguiu"},
                {"cumprimentei","cumprimentou"}, {"prestei","prestou"},
                {"fiquei","ficou"}, {"esperei","esperou"}, {"observei","observou"}
            };
        }

        public static string ReplaceUnique(string text, string pattern, string replacement)
        {
            MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            if (matches.Count != 1) return text;
            Match match = matches[0];
            if (replacement.Length > 0 && match.Length > 0)
                replacement = (Char.IsUpper(match.Value[0]) ? Char.ToUpperInvariant(replacement[0]) : Char.ToLowerInvariant(replacement[0])) + replacement.Substring(1);
            return text.Substring(0, match.Index) + replacement + text.Substring(match.Index + match.Length);
        }

        public static string RepairSourceAnchors(string original, string output)
        {
            output = RepairNameSpelling(original, output);
            if (String.IsNullOrWhiteSpace(original) || String.IsNullOrWhiteSpace(output)) return output;
            // Exact lexical frames only. Never align by sentence number or by a
            // fuzzy similarity score, and never change quoted speech.
            string source = Unquote(original);
            // A restored lexical frame can make its narrator alignment exact.
            // Recheck once against the updated text, still requiring uniqueness.
            for (int pass = 0; pass < 2; pass++)
            {
                var result = new System.Text.StringBuilder(); int start = 0;
                foreach (Match quote in Regex.Matches(output, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
                {
                    string part = output.Substring(start, quote.Index - start);
                    result.Append(RepairReferenceFrames(source, part, Unquote(output)));
                    result.Append(quote.Value); start = quote.Index + quote.Length;
                }
                output = result.ToString();
            }
            return output;
        }

        public static string CorrectionKey(string text)
        {
            text = Normalize(text);
            string[,] forms = NarratorForms();
            for (int i = 0; i < forms.GetLength(0); i++) text = Regex.Replace(text, @"\b" + forms[i,0] + @"\b", forms[i,1]);
            text = Regex.Replace(text, @"\b(?:eu|o depoente|aí|então)\b", " ");
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string ConjugateFrame(string text)
        {
            // Used for equality checks only, never emitted as a generated clause.
            string[,] forms = NarratorForms();
            for (int i = 0; i < forms.GetLength(0); i++)
                text = Regex.Replace(text, @"\b" + forms[i,0] + @"\b", forms[i,1], RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\btava\b", "estava", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\btavam\b", "estavam", RegexOptions.IgnoreCase);
            return text;
        }

        public static string NarratorPrefix(string boundary, string sourcePrefix)
        {
            // An adverb modifying the predicate must not become an exclusive
            // or additive focus on the inserted subject. Keep subject focus only
            // when the source explicitly has that order (e.g. só eu).
            Match focus = Regex.Match(boundary, @"\b(?<word>só|também)\s+$", RegexOptions.IgnoreCase);
            string predicateFocus = "";
            if (focus.Success && Regex.IsMatch(sourcePrefix, @"\b" + Regex.Escape(focus.Groups["word"].Value) + @"\s*$", RegexOptions.IgnoreCase))
            {
                predicateFocus = focus.Groups["word"].Value.ToLowerInvariant() + " ";
                boundary = boundary.Substring(0, focus.Index);
            }
            string subject = boundary.Length == 0 || Regex.IsMatch(boundary, @"[.!?;]\s+$") ? "O depoente " : "o depoente ";
            return boundary + subject + predicateFocus;
        }

        public static HashSet<string> CorrectionWords(string text)
        {
            var words = new HashSet<string>();
            foreach (Match word in Regex.Matches(CorrectionKey(text), @"\p{L}+|\d+"))
                if (!Regex.IsMatch(word.Value, @"^(?:o|a|os|as|um|uma|que|de|do|da)$")) words.Add(word.Value);
            return words;
        }

        public static string RepairReferenceFrames(string source, string text, string fullOutput)
        {
            string[,] forms = NarratorForms();
            foreach (Match report in Regex.Matches(source, @"\b(?<verb>falou|disse|contou|informou|explicou|relatou)\s+(?:pra|para)\s+mim\s+(?<tail>[^,.!?;]+)", RegexOptions.IgnoreCase))
            {
                string tail = report.Groups["tail"].Value.TrimEnd();
                string ambiguous = @"\b" + report.Groups["verb"].Value + @"\s+(?:para|pra)\s+ele\s+" + Regex.Escape(tail) + @"(?=[,.!?;]|$)";
                if (!Regex.IsMatch(source, ambiguous, RegexOptions.IgnoreCase) && Regex.Matches(fullOutput, ambiguous, RegexOptions.IgnoreCase).Count == 1 && Regex.Matches(source, Regex.Escape(report.Value), RegexOptions.IgnoreCase).Count == 1)
                    text = ReplaceUnique(text, ambiguous, report.Groups["verb"].Value + " para o depoente " + tail);
            }
            // Restore an explicit subordinate subject only if deleting it yields
            // one exact output frame and that shortened frame was not in source.
            foreach (Match reference in Regex.Matches(source, @"\b(?<lead>o que|se|quando|porque|que)\s+(?<person>ele|ela|eles|elas)\s+(?<tail>\p{L}+(?:\s+\p{L}+){1,3})\b", RegexOptions.IgnoreCase))
            {
                string missing = reference.Groups["lead"].Value + " " + reference.Groups["tail"].Value;
                if (!Regex.IsMatch(source, @"\b" + Regex.Escape(missing) + @"\b", RegexOptions.IgnoreCase) &&
                    Regex.Matches(source, Regex.Escape(reference.Value), RegexOptions.IgnoreCase).Count == 1 && Regex.Matches(fullOutput, @"\b" + Regex.Escape(missing) + @"\b", RegexOptions.IgnoreCase).Count == 1)
                    text = ReplaceUnique(text, @"\b" + Regex.Escape(missing) + @"\b", reference.Value);
            }
            const string number = @"(?:\d+|uma|um|duas|dois|três|quatro|cinco|seis|sete|oito|nove|dez|onze|doze|quinze|vinte)";
            const string approximation = @"(?:umas?|uns?|cerca de|por volta de|aproximadamente)";
            foreach (Match range in Regex.Matches(source, @"\b(?<approx>" + approximation + @")\s+(?<extent>" + number + @"(?:\s*,\s*|\s+ou\s+|\s+a\s+)(?<last>" + number + @"(?:\s+e\s+(?:pouco|meia))?))(?<tail>[^,.!?;]*)", RegexOptions.IgnoreCase))
            {
                string shortened = @"\b" + approximation + @"\s+" + Regex.Escape(range.Groups["last"].Value + range.Groups["tail"].Value) + @"(?=[,.!?;]|$)";
                if (!Regex.IsMatch(source, shortened, RegexOptions.IgnoreCase) && Regex.Matches(fullOutput, shortened, RegexOptions.IgnoreCase).Count == 1 && Regex.Matches(source, Regex.Escape(range.Value), RegexOptions.IgnoreCase).Count == 1)
                    text = ReplaceUnique(text, shortened, range.Value);
            }
            for (int i = 0; i < forms.GetLength(0); i++)
            {
                string from = forms[i,0], to = forms[i,1];
                // A malformed run-on between a narrator action and an estimate.
                // Only source-provided deictics may be restored, never inferred.
                foreach (Match estimate in Regex.Matches(source, @"\b" + from + @"\s+(?:(?<place>lá|aqui|ali|aí)\s+)?(?:acho|acredito)\s+que\b", RegexOptions.IgnoreCase))
                {
                    string place = estimate.Groups["place"].Value;
                    string pattern = @"\bo depoente\s+" + to + (place.Length > 0 ? @"(?:\s+" + Regex.Escape(place) + ")?" : "") + @"\s+(?:acredita|acha)\s+que\b";
                    if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count == 1 && Regex.Matches(source, Regex.Escape(estimate.Value), RegexOptions.IgnoreCase).Count == 1)
                        text = ReplaceUnique(text, pattern, "o depoente " + to + (place.Length > 0 ? " " + place : "") + "; o depoente acredita que");
                }
                foreach (Match clause in Regex.Matches(source, @"\b(?<neg>não\s+)?" + from + @"\s+(?<tail>[^,.!?;:]+)", RegexOptions.IgnoreCase))
                {
                    string tail = ConjugateFrame(clause.Groups["tail"].Value.TrimEnd());
                    if (tail.Length < 3) continue;
                    // A matching predicate used by another person in the source
                    // makes alignment ambiguous. Leave it for validation/review.
                    if (Regex.Matches(ConjugateFrame(source), @"\b" + to + @"\s+" + Regex.Escape(tail) + @"(?=[,.!?;:]|$)", RegexOptions.IgnoreCase).Count != 1) continue;
                    string boundaryPattern = @"(?<boundary>(?:^|[.!?;]\s+|,\s+|\b(?:mas|porque|quando|pelo que|que)\s+)(?:(?:então|depois|também|só)\s+)?)";
                    string verbPattern = (clause.Groups["neg"].Success ? @"não\s+" : "") + Regex.Escape(to);
                    // Unlike the stand-alone pronoun repair, this step has the
                    // source: the same unique first-person predicate and complete
                    // complement must establish that depoente is the subject.
                    string invertedBody = verbPattern + @"\s+o depoente\s+" + Regex.Escape(tail) + @"(?=[,.!?;:]|$)";
                    MatchCollection inversions = Regex.Matches(text, boundaryPattern + @"(?<verb>" + verbPattern + @")\s+o depoente\s+(?<tail>" + Regex.Escape(tail) + @")(?=[,.!?;:]|$)", RegexOptions.IgnoreCase);
                    if (inversions.Count == 1 && Regex.Matches(fullOutput, @"\b" + invertedBody, RegexOptions.IgnoreCase).Count == 1)
                    {
                        Match inversion = inversions[0]; string prefix = inversion.Groups["boundary"].Value;
                        string fixedOrder = NarratorPrefix(prefix, source.Substring(0, clause.Index)) + inversion.Groups["verb"].Value.ToLowerInvariant() + " " + inversion.Groups["tail"].Value;
                        text = text.Substring(0, inversion.Index) + fixedOrder + text.Substring(inversion.Index + inversion.Length);
                    }
                    string body = clause.Groups["neg"].Success ? @"não\s+" : "";
                    body += Regex.Escape(to) + @"\s+" + Regex.Escape(tail) + @"(?=[,.!?;:]|$)";
                    if (Regex.Matches(fullOutput, @"\b" + body, RegexOptions.IgnoreCase).Count != 1) continue;
                    string pattern = boundaryPattern + @"(?<body>" + body + ")";
                    MatchCollection candidates = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                    if (candidates.Count != 1) continue;
                    Match candidate = candidates[0];
                    string boundary = candidate.Groups["boundary"].Value;
                    string replacement = NarratorPrefix(boundary, source.Substring(0, clause.Index)) + candidate.Groups["body"].Value.ToLowerInvariant().Substring(0, 1) + candidate.Groups["body"].Value.Substring(1);
                    text = text.Substring(0, candidate.Index) + replacement + text.Substring(candidate.Index + candidate.Length);
                }
            }
            // Only a source-marked abandoned fragment followed by an explicit
            // correction AND a redundant confirmation licenses consolidation.
            // Match the abandoned predicate exactly; retain the rendered correction
            // and its uncertainty. No participant names or events are hard-coded.
            foreach (Match correction in Regex.Matches(source, @"(?<abandoned>[^.!?;]+)(?:\.{2,}|…)\s*não\s*,\s*(?<corrected>[^.!?;]+)[.!]\s*(?:é|sim)\s*,\s*(?<confirmed>[^.!?;]+)[.!]", RegexOptions.IgnoreCase))
            {
                var corrected = CorrectionWords(correction.Groups["corrected"].Value);
                var confirmed = CorrectionWords(correction.Groups["confirmed"].Value);
                if (confirmed.Count < 2 || !confirmed.IsSubsetOf(corrected)) continue;
                if (Regex.IsMatch(correction.Groups["corrected"].Value, @"\bnão\b", RegexOptions.IgnoreCase)) continue;
                string pattern = @"(?<abandoned>[^.!?;]+)(?:\.{2,}|…)\s*não\s*,\s*(?<corrected>[^.!?;]+)[.!](?<confirmation>\s*(?:é|sim)\s*,\s*(?<confirmed>[^.!?;]+)[.!])?";
                MatchCollection candidates = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match candidate in candidates)
                {
                    if (CorrectionKey(candidate.Groups["abandoned"].Value) != CorrectionKey(correction.Groups["abandoned"].Value)) continue;
                    var candidateAnchors = CorrectionWords(candidate.Groups["corrected"].Value);
                    if (!confirmed.IsSubsetOf(candidateAnchors)) continue;
                    bool sourceUncertain = Regex.IsMatch(correction.Groups["corrected"].Value, @"\b(?:acho|acredito)\b", RegexOptions.IgnoreCase);
                    bool outputUncertain = Regex.IsMatch(candidate.Groups["corrected"].Value, @"\b(?:acha|acredita)\b", RegexOptions.IgnoreCase);
                    if (sourceUncertain != outputUncertain) continue;
                    if (candidate.Groups["confirmation"].Success && !CorrectionWords(candidate.Groups["confirmed"].Value).SetEquals(confirmed)) continue;
                    if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                    string replacement = candidate.Groups["corrected"].Value.Trim();
                    if (Regex.IsMatch(correction.Groups["corrected"].Value.Trim(), @"^(?:eu\s+)?(?:acho|acredito)\s+que\b", RegexOptions.IgnoreCase) &&
                        Regex.IsMatch(replacement, @"^(?:acha|acredita)\s+que\b", RegexOptions.IgnoreCase))
                        replacement = "o depoente " + Char.ToLowerInvariant(replacement[0]) + replacement.Substring(1);
                    // A temporal nominalization may introduce sua although the
                    // abandoned first-person action identifies its owner. Restore
                    // only that owner, and only for the same source temporal link.
                    string[,] nominalizations = {
                        {"saí","sair","saída","da"}, {"entrei","entrar","entrada","da"},
                        {"cheguei","chegar","chegada","da"}, {"voltei","voltar","volta","da"},
                        {"retornei","retornar","retorno","do"}, {"parti","partir","partida","da"}
                    };
                    for (int n = 0; n < nominalizations.GetLength(0); n++)
                    {
                        if (!Regex.IsMatch(correction.Groups["abandoned"].Value, @"\b" + nominalizations[n,0] + @"\s*$", RegexOptions.IgnoreCase)) continue;
                        foreach (string relation in new string[] {"antes", "depois"})
                        {
                            if (!Regex.IsMatch(correction.Groups["corrected"].Value, @"\b" + relation + @"\s+de\s+" + nominalizations[n,1] + @"\b", RegexOptions.IgnoreCase)) continue;
                            replacement = ReplaceUnique(replacement, @"\b" + relation + @"\s+(?:de|da|do)\s+(?:sua|seu)\s+" + nominalizations[n,2] + @"\b", relation + " " + nominalizations[n,3] + " " + nominalizations[n,2] + " do depoente");
                        }
                    }
                    replacement = Char.ToUpperInvariant(replacement[0]) + replacement.Substring(1) + ".";
                    // Preserve the inter-sentence space consumed by abandoned.
                    if (Char.IsWhiteSpace(candidate.Value[0])) replacement = " " + replacement;
                    text = text.Substring(0, candidate.Index) + replacement + text.Substring(candidate.Index + candidate.Length);
                    break;
                }
            }
            text = Regex.Replace(text, @"(^|[.!?]\s+)Depoente\s+", "$1O depoente ");
            return text;
        }

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
                // An explicit post-verbal eu is a subject, unlike o depoente,
                // which could be the object of viu/ouviu. Move only eu; never
                // reinterpret 'a médica viu o depoente sair' as narrator action.
                MatchCollection inverted = Regex.Matches(part, @"\b(?<verb>vi|ouvi|entendi|perguntei|presenciei|verifiquei|confirmei|observei|escutei)\s+eu\b", RegexOptions.IgnoreCase);
                for (int i = inverted.Count - 1; i >= 0; i--)
                {
                    Match match = inverted[i];
                    string subject = Char.IsUpper(match.Value[0]) ? "Eu " : "eu ";
                    part = part.Substring(0, match.Index) + subject + match.Groups["verb"].Value.ToLowerInvariant() + part.Substring(match.Index + match.Length);
                }
                part = Regex.Replace(part, @"\b(?:para|pra)\s+mim\b", "para o depoente", RegexOptions.IgnoreCase);
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
                    { @"\b(?:disse|falou|contou|informou|explicou|relatou)\s+(?:\p{L}+\s+){0,2}(?:para|pra)\s+mim\b", @"\b(?:para o depoente|ao depoente|lhe\s+(?:disse|falou|contou|informou|explicou|relatou)|(?:disse|falou|contou|informou|explicou|relatou)-lhe)\b", "destinatário explícito" },
                    { @"\b(?:disse|falou|contou|relatou|informou)\s+que\s+(?:viu|ouviu)\b", @"\b(?:disse|falou|contou|relatou|informou)\s+(?:que\s+(?:viu|ouviu)|ter\s+(?:visto|ouvido))\b", "cadeia de relato de percepção" },
                    { @"\bnão\s+(?:me\s+)?(?:lembro|recordo)\b", @"\bnão\s+(?:se\s+)?(?:lembra|recorda)\b", "limitação de memória" },
                    { @"\bnão\s+sei\b", @"\bnão\s+sabe\b", "limitação de conhecimento" },
                    { @"\b(?:acho|acredito)\b", @"\b(?:acha|acredita)\b", "crença ou incerteza" },
                    { @"\bposso\s+estar\b", @"\bpode\s+estar\b", "possibilidade de engano" }
                };
                for (int i = 0; i < operators.GetLength(0); i++)
                    if (Regex.IsMatch(sentence, operators[i,0]) && !Regex.IsMatch(best, operators[i,1]))
                        return "fidelidade: perda de " + operators[i,2] + ": " + sentence;
                // Detect deletion of an explicit subordinate subject only when
                // the surrounding words still match. No referent is inferred.
                foreach (Match reference in Regex.Matches(sentence, @"\b(?<lead>o que|se|quando|porque|que)\s+(?<person>ele|ela|eles|elas)\s+(?<tail>\p{L}+(?:\s+\p{L}+){1,3})\b"))
                {
                    string missing = reference.Groups["lead"].Value + " " + reference.Groups["tail"].Value;
                    if (best.Contains(missing) && !best.Contains(reference.Value))
                        return "fidelidade: referente explícito omitido na oração: " + sentence;
                }
                const string number = @"(?:\d+|uma|um|duas|dois|três|quatro|cinco|seis|sete|oito|nove|dez|onze|doze|quinze|vinte)";
                foreach (Match range in Regex.Matches(sentence, @"\b(?:umas?|uns?|cerca de|por volta de)\s+(?<extent>" + number + @"(?:\s*,\s*|\s+ou\s+|\s+a\s+)" + number + @"(?:\s+e\s+(?:pouco|meia))?)\b"))
                    if (!best.Contains(range.Groups["extent"].Value))
                        return "fidelidade: expressão aproximada reduzida: " + sentence;
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
