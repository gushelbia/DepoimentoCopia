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

        // First-person forms whose third person cannot be derived from a suffix.
        // Only words that are verbs in every reading; no noun homographs.
        public static string[,] BareFirstPerson()
        {
            return new string[,] {
                {"vi","viu"}, {"li","leu"}, {"fui","foi"}, {"tive","teve"}, {"estive","esteve"},
                {"pude","pôde"}, {"fiz","fez"}, {"vim","veio"}, {"dei","deu"},
                {"sei","sabe"}, {"sou","é"}, {"estou","está"}, {"tenho","tem"}, {"posso","pode"},
                {"vou","vai"}, {"faço","faz"}, {"quero","quer"}, {"vejo","vê"}, {"ouço","ouve"},
                {"lembro","lembra"}, {"recordo","recorda"}, {"acho","acredita"}, {"acredito","acredita"},
                {"abri","abriu"}, {"ouvi","ouviu"}, {"saí","saiu"}, {"caí","caiu"}, {"recebi","recebeu"},
                {"bebi","bebeu"}, {"percebi","percebeu"}, {"escrevi","escreveu"}, {"corri","correu"},
                {"subi","subiu"}, {"pedi","pediu"}, {"decidi","decidiu"}, {"dormi","dormiu"},
                {"senti","sentiu"}, {"vesti","vestiu"}, {"desci","desceu"}, {"conheci","conheceu"},
                {"esqueci","esqueceu"}, {"respondi","respondeu"}, {"entendi","entendeu"},
                {"compreendi","compreendeu"}, {"atendi","atendeu"}, {"prometi","prometeu"},
                {"perdi","perdeu"}, {"mexi","mexeu"}, {"escolhi","escolheu"}, {"cumpri","cumpriu"},
                {"insisti","insistiu"}, {"desisti","desistiu"}, {"assisti","assistiu"},
                {"consegui","conseguiu"}, {"segui","seguiu"}, {"fugi","fugiu"}, {"parti","partiu"},
                {"cobri","cobriu"}, {"descobri","descobriu"}, {"sorri","sorriu"}, {"servi","serviu"},
                {"preferi","preferiu"}, {"permiti","permitiu"}, {"admiti","admitiu"}, {"repeti","repetiu"},
                {"vendi","vendeu"}, {"devolvi","devolveu"}, {"resolvi","resolveu"}, {"acendi","acendeu"},
                {"prendi","prendeu"}, {"aprendi","aprendeu"}, {"estendi","estendeu"}, {"defendi","defendeu"},
                {"escondi","escondeu"}, {"corrigi","corrigiu"}, {"exigi","exigiu"}, {"dirigi","dirigiu"},
                {"reagi","reagiu"}, {"agi","agiu"}, {"interrompi","interrompeu"}, {"sofri","sofreu"},
                {"ofereci","ofereceu"}, {"agradeci","agradeceu"}, {"apareci","apareceu"},
                {"permaneci","permaneceu"}, {"reconheci","reconheceu"}, {"mordi","mordeu"},
                {"varri","varreu"}, {"emiti","emitiu"}, {"imprimi","imprimiu"}, {"distribuí","distribuiu"},
                {"incluí","incluiu"}, {"concluí","concluiu"}, {"construí","construiu"}, {"destruí","destruiu"},
                {"atribuí","atribuiu"}, {"contribuí","contribuiu"}, {"possuí","possuiu"}, {"substituí","substituiu"}
            };
        }

        // Present forms that are also nouns or adjectives. Converted only right
        // after an explicit narrator subject, where they can only be verbs.
        public static string[,] SubjectOnlyFirstPerson()
        {
            return new string[,] {
                {"trabalho","trabalha"}, {"moro","mora"}, {"estudo","estuda"}, {"preciso","precisa"},
                {"conheço","conhece"}, {"mantenho","mantém"}, {"prefiro","prefere"}, {"costumo","costuma"},
                {"confirmo","confirma"}, {"afirmo","afirma"}, {"nego","nega"}, {"garanto","garante"},
                {"reconheço","reconhece"}, {"uso","usa"}, {"fico","fica"}, {"chego","chega"},
                {"saio","sai"}, {"volto","volta"}, {"entro","entra"}, {"atendo","atende"}, {"cuido","cuida"}
            };
        }

        public static string Lookup(string[,] table, string word)
        {
            string w = word.ToLowerInvariant();
            for (int i = 0; i < table.GetLength(0); i++) if (table[i,0] == w) return table[i,1];
            return null;
        }

        // Third person of an unambiguous first-person form, or null. The -ei
        // preterite of -ar verbs is regular; future and noun homographs excluded.
        public static string ThirdPerson(string word)
        {
            string w = word.ToLowerInvariant();
            string known = Lookup(BareFirstPerson(), w);
            if (known != null) return known;
            if (w.Length < 5 || !w.EndsWith("ei") || Regex.IsMatch(w, @"[áàâãéêíóôõúç]")) return null;
            if (Regex.IsMatch(w, @"^(?:farei|direi|trarei|irei|serei|estarei|terei|poderei|saberei|verei|darei|haverei|sensei)$")) return null;
            string stem = w.Substring(0, w.Length - 2);
            if (stem.EndsWith("qu")) return stem.Substring(0, stem.Length - 2) + "cou";
            if (stem.EndsWith("gu")) return stem.Substring(0, stem.Length - 1) + "ou";
            if (stem.EndsWith("c")) return stem.Substring(0, stem.Length - 1) + "çou";
            return stem + "ou";
        }

        public static bool IsSentenceEnd(char c)
        {
            return c == '.' || c == '!' || c == '?';
        }

        public static int SentenceStart(string text, int index)
        {
            for (int i = Math.Min(index, text.Length) - 1; i >= 0; i--) if (IsSentenceEnd(text[i])) return i + 1;
            return 0;
        }

        public static string SentenceAt(string text, int index)
        {
            int start = SentenceStart(text, index), end = index;
            while (end < text.Length && !IsSentenceEnd(text[end])) end++;
            return text.Substring(start, end - start);
        }

        public static string MatchCase(string model, string word)
        {
            if (model.Length == 0 || word.Length == 0) return word;
            return (Char.IsUpper(model[0]) ? Char.ToUpperInvariant(word[0]) : Char.ToLowerInvariant(word[0])) + word.Substring(1);
        }

        // Narrator verb pairs actually present in this source (outside quotes),
        // merged with the fixed narrator vocabulary. Never guessed from output.
        public static string[,] SourceNarratorForms(string source)
        {
            string[,] fixedForms = NarratorForms();
            var from = new List<string>(); var to = new List<string>();
            for (int i = 0; i < fixedForms.GetLength(0); i++) { from.Add(fixedForms[i,0]); to.Add(fixedForms[i,1]); }
            foreach (Match word in Regex.Matches(Unquote(source), @"\b\p{L}+\b"))
            {
                string w = word.Value.ToLowerInvariant();
                if (from.Contains(w)) continue;
                string third = ThirdPerson(w);
                if (third == null)
                {
                    // Subject-only presents qualify when the source marks the narrator.
                    third = Lookup(SubjectOnlyFirstPerson(), w);
                    if (third == null || !Regex.IsMatch(source, @"\beu\s+(?:não\s+)?" + Regex.Escape(w) + @"\b", RegexOptions.IgnoreCase)) continue;
                }
                if (third == w) continue;
                from.Add(w); to.Add(third);
            }
            var result = new string[from.Count, 2];
            for (int i = 0; i < from.Count; i++) { result[i,0] = from[i]; result[i,1] = to[i]; }
            return result;
        }

        public static string ConjugateWith(string text, string[,] forms)
        {
            // Used for equality checks only, never emitted as a generated clause.
            for (int i = 0; i < forms.GetLength(0); i++)
                text = Regex.Replace(text, @"\b" + forms[i,0] + @"\b", forms[i,1], RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\btava\b", "estava", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\btavam\b", "estavam", RegexOptions.IgnoreCase);
            return text;
        }

        // Converts first-person verbs the model copied verbatim. A narrator
        // subject is inserted only at a clause boundary, never after a named
        // subject; otherwise the verb alone is conjugated. Quotes are excluded
        // by the caller.
        public static string RepairNarratorVerbs(string source, string text)
        {
            string plain = Unquote(source);
            // An explicit narrator subject makes any first-person form a verb.
            MatchCollection subjectForms = Regex.Matches(text, @"\b[Oo] depoente\s+(?:(?:não|nunca|também|só|ainda|já)\s+)*(?:(?:se|lhe)\s+)?(?<w>\p{L}+)\b");
            for (int i = subjectForms.Count - 1; i >= 0; i--)
            {
                Group w = subjectForms[i].Groups["w"];
                // Only an act the source attributes to the narrator. A form absent
                // from the source ("o depoente pedi" for "ela pediu") is a model
                // attribution error: leave it visible to validation.
                if (!Regex.IsMatch(plain, @"\b" + Regex.Escape(w.Value) + @"\b", RegexOptions.IgnoreCase)) continue;
                string third = ThirdPerson(w.Value);
                if (third == null) third = Lookup(SubjectOnlyFirstPerson(), w.Value);
                if (third == null || third == w.Value.ToLowerInvariant()) continue;
                text = text.Substring(0, w.Index) + third + text.Substring(w.Index + w.Length);
            }
            text = Regex.Replace(text, @"\b([Oo] depoente\s+(?:não\s+)?)me\s+(lembra|recorda)\b", "$1se $2");
            var seen = new HashSet<string>();
            foreach (Match word in Regex.Matches(plain, @"\b\p{L}+\b"))
            {
                string w = word.Value.ToLowerInvariant();
                if (!seen.Add(w)) continue;
                string third = ThirdPerson(w);
                if (third == null || third == w) continue;
                string pattern = @"(?<pre>^|[.!?;:]\s+|,\s+|\b(?:e|mas|porque|pois|quando|que|então|enquanto|onde)\s+)(?<adv>(?:(?:mais tarde|depois|antes|então|aí|ainda|já|logo|em seguida|enquanto isso|nesse momento|no fim|no final|por isso|na volta)\s*,?\s+)?)(?<focus>(?:só|também)\s+)?(?<neg>(?:não|nunca)\s+)?(?<clit>(?:me|se|lhe)\s+)?(?<w>" + Regex.Escape(w) + @")\b";
                MatchCollection clauses = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                for (int i = clauses.Count - 1; i >= 0; i--)
                {
                    Match m = clauses[i];
                    string pre = m.Groups["pre"].Value, adv = m.Groups["adv"].Value;
                    string clitic = m.Groups["clit"].Value;
                    if (Regex.IsMatch(clitic, @"^me\s", RegexOptions.IgnoreCase)) clitic = "se ";
                    bool sentenceStart = adv.Length == 0 && (pre.Length == 0 || Regex.IsMatch(pre, @"[.!?]\s+$"));
                    bool insert = true;
                    if (Regex.IsMatch(pre, @"^(?:e|mas)\s+$", RegexOptions.IgnoreCase))
                    {
                        // Coordinated clause of a sentence already led by the narrator.
                        int start = SentenceStart(text, m.Index);
                        if (Regex.IsMatch(text.Substring(start, m.Index - start), @"^\s*(?:relatou que\s+)?o depoente\b", RegexOptions.IgnoreCase)) insert = false;
                    }
                    string rest = m.Groups["focus"].Value + m.Groups["neg"].Value + clitic;
                    string replacement;
                    if (insert)
                        replacement = pre + adv + (sentenceStart ? "O depoente " : "o depoente ") + rest.ToLowerInvariant() + third;
                    else if (rest.Length > 0)
                        replacement = pre + adv + MatchCase(rest, rest) + third;
                    else
                        replacement = pre + adv + MatchCase(m.Groups["w"].Value, third);
                    text = text.Substring(0, m.Index) + replacement + text.Substring(m.Index + m.Length);
                }
                // Any remaining occurrence has an explicit subject already.
                MatchCollection rest2 = Regex.Matches(text, @"\b" + Regex.Escape(w) + @"\b", RegexOptions.IgnoreCase);
                for (int i = rest2.Count - 1; i >= 0; i--)
                    text = text.Substring(0, rest2[i].Index) + MatchCase(rest2[i].Value, third) + text.Substring(rest2[i].Index + rest2[i].Length);
            }
            return text;
        }

        public static string SpeechVerbs()
        {
            return "disse|contou|falou|informou|explicou|relatou|perguntou|pediu|entregou|deu|mostrou|mandou|enviou|passou|avisou|respondeu|garantiu|confirmou|comentou|repetiu|devolveu";
        }

        // "X me disse" -> "X lhe disse". When the same source sentence has
        // another lhe, use "ao depoente" so two recipients stay distinct.
        public static string RepairRecipients(string source, string text, string fullOutput)
        {
            string plain = Unquote(source);
            string verbs = SpeechVerbs();
            foreach (Match report in Regex.Matches(plain, @"\b(?<subj>\p{Lu}[\p{Ll}\p{M}]+|[Ee]les?|[Ee]las?)\s+(?<neg>não\s+)?me\s+(?<v>" + verbs + @")\b"))
            {
                string subj = report.Groups["subj"].Value, v = report.Groups["v"].Value;
                if (Regex.Matches(plain, Regex.Escape(report.Value)).Count != 1) continue;
                if (Regex.IsMatch(plain, @"\b" + Regex.Escape(subj) + @"\s+(?:não\s+)?" + v + @"\b", RegexOptions.IgnoreCase)) continue;
                string sentence = SentenceAt(plain, report.Index);
                bool otherRecipient = Regex.Matches(sentence, @"\blhe\b", RegexOptions.IgnoreCase).Count > 0;
                string pattern = @"\b" + Regex.Escape(subj) + @"\s+(?<neg>não\s+)?(?:(?<dep>o depoente)\s+|(?<lhe>lhe)\s+)?" + v + @"\b(?!-lhe|\s+(?:\p{L}+\s+){0,3}?(?:ao|para o) depoente\b)";
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match m = found[0];
                if (m.Groups["lhe"].Success && !otherRecipient) continue;
                string head = m.Value.Substring(0, subj.Length) + " " + m.Groups["neg"].Value;
                string replacement = otherRecipient ? head + v + " ao depoente" : head + "lhe " + v;
                text = text.Substring(0, m.Index) + replacement + text.Substring(m.Index + m.Length);
            }
            return text;
        }

        // Makes the narrator explicit where a converted verb opens a sentence or
        // a relative clause, only when that verb+next word is the narrator's
        // alone in the source and occurs once in the output.
        public static string RepairEllipticalNarrator(string source, string text, string fullOutput, string[,] forms)
        {
            string plain = Unquote(source);
            string conjugated = ConjugateWith(plain, forms);
            for (int i = 0; i < forms.GetLength(0); i++)
            {
                string from = forms[i,0], to = forms[i,1];
                // Two following words anchor the frame; one word ("da", "que")
                // would align a different predicate by position.
                foreach (Match clause in Regex.Matches(plain, @"(?:^|[.!?;:]\s+)(?<neg>(?:não|nunca)\s+)?" + from + @"\s+(?<w1>\p{L}+)\s+(?<w2>\p{L}+)", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(clause.Groups["w1"].Value, "^eu$", RegexOptions.IgnoreCase)) continue;
                    string next = Regex.Escape(clause.Groups["w1"].Value) + @"\s+" + Regex.Escape(clause.Groups["w2"].Value) + @"\b";
                    if (Regex.Matches(conjugated, @"\b" + to + @"\s+" + next, RegexOptions.IgnoreCase).Count != 1) continue;
                    if (Regex.Matches(fullOutput, @"\b" + to + @"\s+" + next, RegexOptions.IgnoreCase).Count != 1) continue;
                    string neg = clause.Groups["neg"].Success ? @"(?:não|nunca)\s+" : "";
                    MatchCollection found = Regex.Matches(text, @"(?<b>^|[.!?;:]\s+)(?<neg>" + neg + @")(?<v>" + to + @")(?<next>\s+" + next + ")", RegexOptions.IgnoreCase);
                    if (found.Count != 1) continue;
                    Match m = found[0]; string b = m.Groups["b"].Value;
                    string subject = b.Length == 0 || Regex.IsMatch(b, @"[.!?]\s+$") ? "O depoente " : "o depoente ";
                    string replacement = b + subject + m.Groups["neg"].Value.ToLowerInvariant() + to + m.Groups["next"].Value;
                    text = text.Substring(0, m.Index) + replacement + text.Substring(m.Index + m.Length);
                }
                // "Respondi que eu ainda...": the narrator is the subject of both
                // verbs. "Respondeu que o depoente..." would give the main verb
                // another possible subject; move the narrator to the main verb.
                foreach (Match clause in Regex.Matches(plain, @"(?:^|[.!?;:]\s+)" + from + @"\s+que\s+eu\s+(?<w>\p{L}+)", RegexOptions.IgnoreCase))
                {
                    string w = Regex.Escape(ConjugateWith(clause.Groups["w"].Value, forms));
                    MatchCollection found = Regex.Matches(text, @"(?<b>^|[.!?;:]\s+)" + to + @"\s+que\s+o depoente\s+(?<w>" + w + @")\b", RegexOptions.IgnoreCase);
                    if (found.Count != 1 || Regex.Matches(fullOutput, @"\b" + to + @"\s+que\s+o depoente\s+" + w + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                    Match m = found[0]; string b = m.Groups["b"].Value;
                    string subject = b.Length == 0 || Regex.IsMatch(b, @"[.!?]\s+$") ? "O depoente " : "o depoente ";
                    text = text.Substring(0, m.Index) + b + subject + to + " que " + m.Groups["w"].Value + text.Substring(m.Index + m.Length);
                }
                foreach (Match clause in Regex.Matches(plain, @"\b(?<left>\p{L}+\s+(?:que|quando|onde|enquanto))\s+(?<neg>não\s+)?" + from + @"\b", RegexOptions.IgnoreCase))
                {
                    string left = clause.Groups["left"].Value, neg = clause.Groups["neg"].Value;
                    // "acho que saí": the main verb already has the narrator as subject.
                    if (IsNarratorComplement(left, forms)) continue;
                    string body = Regex.Escape(left) + @"\s+" + Regex.Escape(neg) + to + @"\b";
                    if (Regex.Matches(conjugated, @"\b" + body, RegexOptions.IgnoreCase).Count != 1) continue;
                    if (Regex.Matches(fullOutput, @"\b" + body, RegexOptions.IgnoreCase).Count != 1) continue;
                    MatchCollection found = Regex.Matches(text, @"\b(?<left>" + Regex.Escape(left) + @")\s+(?<rest>" + Regex.Escape(neg) + to + @")\b", RegexOptions.IgnoreCase);
                    if (found.Count != 1) continue;
                    Match m = found[0];
                    text = text.Substring(0, m.Index) + m.Groups["left"].Value + " o depoente " + m.Groups["rest"].Value + text.Substring(m.Index + m.Length);
                }
            }
            return text;
        }

        public static bool IsNarratorVerb(string phrase, string[,] forms)
        {
            string first = Regex.Match(phrase, @"^\p{L}+").Value.ToLowerInvariant();
            for (int i = 0; i < forms.GetLength(0); i++) if (forms[i,0] == first || forms[i,1] == first) return true;
            return false;
        }

        // "acredita que", "sabe que": a que-complement of a narrator verb.
        // Temporal/relative links (quando, onde, enquanto) never qualify.
        public static bool IsNarratorComplement(string left, string[,] forms)
        {
            return Regex.IsMatch(left, @"^\p{L}+\s+que$", RegexOptions.IgnoreCase) && IsNarratorVerb(left, forms);
        }

        // The model sometimes renders the narrator's "eu" as ele/ela, which
        // assigns a gender and competes with other participants. Restore only
        // an exact, unique source frame.
        public static string RepairNarratorPronoun(string source, string text, string fullOutput, string[,] forms)
        {
            string plain = Unquote(source);
            string conjugated = ConjugateWith(plain, forms);
            foreach (Match clause in Regex.Matches(plain, @"\beu\s+(?<neg>não\s+)?(?<v>\p{L}+)\s+(?<next>\p{L}+)", RegexOptions.IgnoreCase))
            {
                string to = ConjugateWith(clause.Groups["v"].Value.ToLowerInvariant(), forms);
                string body = Regex.Escape(clause.Groups["neg"].Value) + to + @"\s+" + Regex.Escape(clause.Groups["next"].Value) + @"\b";
                if (Regex.IsMatch(conjugated, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase)) continue;
                if (Regex.Matches(fullOutput, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, @"\b(?<pro>[Ee]le|[Ee]la)(?<rest>\s+" + body + ")", RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match m = found[0];
                text = text.Substring(0, m.Index) + MatchCase(m.Groups["pro"].Value, "o depoente") + m.Groups["rest"].Value + text.Substring(m.Index + m.Length);
            }
            return text;
        }

        // Explicit self-corrections whose abandoned and corrected parts share the
        // same leading preposition/article. The corrected part is kept verbatim.
        public static string RepairParallelCorrections(string source, string text)
        {
            string plain = Unquote(source);
            const string lead = @"(?:para|pra|a|ao|à|o|com|de|do|da|em|no|na|nos|nas|às|as|os|pela|pelo)";
            const string marker = @"(?:quer dizer|ou melhor|digo|aliás|não|corrigindo)";
            foreach (Match correction in Regex.Matches(plain, @"\b(?<old>(?<prep>" + lead + @")\s+[^,.;:!?…]{1,60}?)(?:\s*,|\s*(?:\.{2,}|…))\s*" + marker + @"\s*,\s*(?<new>\k<prep>\s+[^,.;:!?…]+)", RegexOptions.IgnoreCase))
            {
                string oldPart = correction.Groups["old"].Value.Trim();
                if (Regex.IsMatch(correction.Groups["new"].Value, @"\bnão\b", RegexOptions.IgnoreCase)) continue;
                string pattern = Regex.Escape(oldPart) + @"(?:\s*,|\s*(?:\.{2,}|…))\s*" + marker + @"\s*,\s*(?=" + Regex.Escape(correction.Groups["prep"].Value) + @"\s)";
                MatchCollection found = Regex.Matches(text, @"\b" + pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                text = text.Substring(0, found[0].Index) + text.Substring(found[0].Index + found[0].Length);
            }
            return text;
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
            text = RepairNarratorVerbs(source, text);
            string[,] forms = SourceNarratorForms(source);
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
                    string tail = ConjugateWith(clause.Groups["tail"].Value.TrimEnd(), forms);
                    if (tail.Length < 3) continue;
                    // A matching predicate used by another person in the source
                    // makes alignment ambiguous. Leave it for validation/review.
                    if (Regex.Matches(ConjugateWith(source, forms), @"\b" + to + @"\s+" + Regex.Escape(tail) + @"(?=[,.!?;:]|$)", RegexOptions.IgnoreCase).Count != 1) continue;
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
                    // "acredita que visitou": complement of a narrator verb already
                    // has the narrator as subject; adding it again only adds noise.
                    if (Regex.IsMatch(boundary, @"^que\s", RegexOptions.IgnoreCase) &&
                        IsNarratorVerb(Regex.Match(text.Substring(0, candidate.Index), @"\p{L}+(?=\s*$)").Value, forms)) continue;
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
            text = RepairEllipticalNarrator(source, text, fullOutput, forms);
            text = RepairNarratorPronoun(source, text, fullOutput, forms);
            text = RepairRecipients(source, text, fullOutput);
            text = RepairParallelCorrections(source, text);
            // "nossa conversa": an interaction the narrator took part in. Say
            // exactly that, without naming who else took part.
            foreach (Match shared in Regex.Matches(text, @"\b(?<p>[Nn]oss[ao])\s+(?<n>conversa|reunião|discussão|ligação|chamada|encontro)\b"))
            {
                if (!Regex.IsMatch(source, @"\b" + Regex.Escape(shared.Value) + @"\b", RegexOptions.IgnoreCase)) continue;
                string article = shared.Groups["p"].Value.EndsWith("a") ? "a" : "o";
                text = text.Replace(shared.Value, MatchCase(shared.Value, article) + " " + shared.Groups["n"].Value + " de que o depoente participava");
            }
            // Contract preposition + article unless the article opens the subject
            // of an infinitive ("antes de o depoente sair").
            text = Regex.Replace(text, @"\b([Dd])e o depoente\b(?!\s+(?:não\s+)?\p{L}+(?:ar|er|ir|or)\b)", "$1o depoente");
            text = Regex.Replace(text, @"\bem o depoente\b", "no depoente");
            text = Regex.Replace(text, @"\bEm o depoente\b", "No depoente");
            text = Regex.Replace(text, @"\b([Pp])or o depoente\b(?!\s+(?:não\s+)?\p{L}+(?:ar|er|ir|or)\b)", "$1elo depoente");
            text = Regex.Replace(text, @";\s+O depoente\b", "; o depoente");
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
                // Person conversion and a paraphrased speech verb are not omissions.
                string third = ThirdPerson(w);
                if (third != null) w = third;
                if (w == "tava") w = "estava";
                if (w == "tavam") w = "estavam";
                if (Regex.IsMatch(w, @"^(?:falou|falar|fala|respondeu|responder|responde|contou|contar|afirmou|declarou|comentou|explicou|avisou|mencionou|dizer|diz)$")) w = "fala";
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
            // Irregular and -er/-ir forms of this input, and any first-person form
            // right after the narrator subject ("o depoente li", "o depoente trabalho").
            foreach (Match word in Regex.Matches(source, @"\b\p{L}+\b"))
            {
                string third = ThirdPerson(word.Value);
                if (third != null && third != word.Value && Regex.IsMatch(rendered, @"\b" + Regex.Escape(word.Value) + @"\b"))
                    return "fidelidade: possível primeira pessoa residual: " + word.Value;
            }
            foreach (Match narrator in Regex.Matches(rendered, @"\bo depoente\s+(?:(?:não|nunca|também|só|ainda|já)\s+)*(?:(?:se|lhe|me)\s+)?(?<w>\p{L}+)\b"))
            {
                string w = narrator.Groups["w"].Value;
                if (Lookup(BareFirstPerson(), w) != null || Lookup(SubjectOnlyFirstPerson(), w) != null || (ThirdPerson(w) != null && Regex.IsMatch(source, @"\b" + Regex.Escape(w) + @"\b")))
                    return "fidelidade: possível primeira pessoa residual: o depoente " + w;
            }
            if (Regex.IsMatch(source, @"\b(?:nós|nosso|nossa|nossos|nossas|conosco)\b"))
            {
                Match plural = Regex.Match(rendered, @"\b(?:nós|nosso|nossa|nossos|nossas|conosco)\b");
                if (plural.Success) return "fidelidade: possível primeira pessoa residual: " + plural.Value;
            }
            // Perception is an operator: the output may not assert more acts of
            // seeing/hearing than the input, nor turn the narrator's own action
            // into something perceived ("conferi" -> "viu conferir").
            const string seenSource = @"\b(?:vi|viu|vimos|viram|ouvi|ouviu|ouvimos|ouviram|presenciei|presenciou|presenciaram)\b";
            if (Regex.Matches(rendered, seenSource).Count > Regex.Matches(source, seenSource).Count)
                return "fidelidade: percepção acrescentada ao relato";
            foreach (Match perceived in Regex.Matches(rendered, @"\b(?:viu|ouviu)\s+(?<stem>\p{L}+?)(?<end>ar|er|ir)\b"))
            {
                string stem = perceived.Groups["stem"].Value;
                string own = perceived.Groups["end"].Value == "ar" ? stem + "ei" : stem + "i";
                if (Regex.IsMatch(source, @"\b" + Regex.Escape(own) + @"\b") && !Regex.IsMatch(source, @"\b(?:vi|ouvi)\s+(?:\p{L}+\s+){0,3}" + Regex.Escape(stem + perceived.Groups["end"].Value) + @"\b"))
                    return "fidelidade: ação do depoente convertida em percepção: " + perceived.Value;
            }
            // A named speaker addressing the narrator must keep the narrator as
            // recipient; with a second recipient in the sentence, lhe is ambiguous.
            string verbs = SpeechVerbs();
            foreach (Match report in Regex.Matches(Unquote(original), @"\b(?<subj>\p{Lu}[\p{Ll}\p{M}]+|[Ee]les?|[Ee]las?)\s+(?:não\s+)?me\s+(?<v>" + verbs + @")\b"))
            {
                string subj = Regex.Escape(report.Groups["subj"].Value.ToLowerInvariant()), v = report.Groups["v"].Value;
                if (Regex.IsMatch(source, @"\b" + subj + @"\s+(?:não\s+)?" + v + @"\b")) continue;
                if (Regex.IsMatch(rendered, @"\b" + subj + @"\s+o depoente\s+" + v + @"\b"))
                    return "fidelidade: destinatário do relato convertido em sujeito: " + report.Value;
                if (Regex.IsMatch(rendered, @"\b" + subj + @"\s+(?:não\s+)?" + v + @"\b") && !Regex.IsMatch(rendered, @"\b" + subj + @"\s+(?:não\s+)?" + v + @"\s+(?:\p{L}+\s+){0,3}?(?:ao|para o|pro) depoente\b"))
                    return "fidelidade: destinatário do relato omitido: " + report.Value;
                bool otherRecipient = Regex.IsMatch(SentenceAt(Unquote(original), report.Index), @"\blhe\b", RegexOptions.IgnoreCase);
                if (otherRecipient && Regex.IsMatch(rendered, @"\b" + subj + @"\s+(?:não\s+)?lhe\s+" + v + @"\b"))
                    return "fidelidade: destinatário do relato ambíguo: " + report.Value;
            }
            // The narrator stays explicit where a converted verb opens a sentence
            // right after another participant's sentence, or a relative clause.
            string[,] narratorForms = SourceNarratorForms(original);
            // "Ela pediu" must not become "o depoente pediu": a preterite the
            // source uses only for someone else cannot be the narrator's act.
            foreach (Match act in Regex.Matches(rendered, @"\bo depoente\s+(?:não\s+)?(?:(?:também|só|ainda|já)\s+)?(?:(?:se|lhe)\s+)?(?<v>\p{L}+(?:ou|eu|iu))\b"))
            {
                string v = act.Groups["v"].Value;
                bool narrators = false;
                for (int i = 0; i < narratorForms.GetLength(0); i++) if (narratorForms[i,1] == v) narrators = true;
                if (!narrators && Regex.IsMatch(source, @"\b" + Regex.Escape(v) + @"\b"))
                    return "fidelidade: ação de participante atribuída ao depoente: " + act.Value;
            }
            for (int i = 0; i < narratorForms.GetLength(0); i++)
            {
                string from = narratorForms[i,0], to = narratorForms[i,1];
                foreach (Match clause in Regex.Matches(source, @"(?:^|[.!?]\s+)(?:não\s+)?" + from + @"\s+(?<w1>\p{L}+)\s+(?<w2>\p{L}+)"))
                {
                    string next = Regex.Escape(clause.Groups["w1"].Value) + @"\s+" + Regex.Escape(clause.Groups["w2"].Value);
                    foreach (Match elliptic in Regex.Matches(rendered, @"(?<=^|[.!?]\s)(?:não\s+)?" + to + @"\s+" + next + @"\b"))
                    {
                        string previous = rendered.Substring(0, elliptic.Index).TrimEnd();
                        previous = previous.Length == 0 ? "" : previous.Substring(SentenceStart(previous, previous.Length - 1));
                        // The previous sentence must itself open with the narrator.
                        if (previous.Length > 0 && !Regex.IsMatch(previous, @"^\s*(?:\S+\s+){0,2}o depoente\b"))
                            return "fidelidade: sujeito do depoente implícito após ação de outro participante: " + elliptic.Value;
                    }
                }
                if (Regex.IsMatch(source, @"(?:^|[.!?;:]\s+)" + from + @"\s+que\s+eu\b") && Regex.IsMatch(rendered, @"(?:^|[.!?;:]\s)" + to + @"\s+que\s+o depoente\b"))
                    return "fidelidade: sujeito do depoente deslocado para a oração subordinada: " + to + " que o depoente";
                foreach (Match clause in Regex.Matches(source, @"\b(?<left>\p{L}+\s+(?:que|quando|onde|enquanto))\s+(?:não\s+)?" + from + @"\b"))
                    if (!IsNarratorComplement(clause.Groups["left"].Value, narratorForms) &&Regex.IsMatch(rendered, @"\b" + Regex.Escape(clause.Groups["left"].Value) + @"\s+(?:não\s+)?" + to + @"\b"))
                        return "fidelidade: sujeito do depoente implícito em oração subordinada: " + clause.Value;
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
                string best = ""; double bestScore = -1; int bestSize = Int32.MaxValue;
                foreach (string candidate in dstSentences)
                {
                    var found = Anchors(candidate); int count = 0;
                    foreach (string anchor in anchors) if (found.Contains(anchor)) count++;
                    double score = (double)count / anchors.Count;
                    // On equal coverage prefer the more specific sentence, so a
                    // longer neighbour sharing the same words is not compared.
                    if (score > bestScore || (score == bestScore && found.Count < bestSize)) { bestScore = score; best = candidate; bestSize = found.Count; }
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
