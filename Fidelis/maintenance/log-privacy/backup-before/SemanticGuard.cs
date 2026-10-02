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
                // Round 7: the next word may differ in gender («eu era culpada» → «ele era culpado»).
                string body = NarratorBody(clause, forms);
                if (Regex.IsMatch(conjugated, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase)) continue;
                if (Regex.Matches(fullOutput, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, @"\b(?<pro>[Ee]le|[Ee]la)(?<rest>\s+" + body + ")", RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match m = found[0];
                text = text.Substring(0, m.Index) + MatchCase(m.Groups["pro"].Value, "o depoente") + m.Groups["rest"].Value + text.Substring(m.Index + m.Length);
            }
            return text;
        }

        // Plain text only (no symbols): survives the text box, WM_GETTEXT, the
        // clipboard and "Adicionar". Idempotent; an empty screen stays empty.
        public static string MarkIncomplete(string text, string reason)
        {
            const string head = "[SAÍDA INCOMPLETA E NÃO VALIDADA - NÃO UTILIZAR COMO REFORMULAÇÃO]";
            if (String.IsNullOrWhiteSpace(text) || text.StartsWith(head)) return text;
            return head + "\r\nMotivo: " + (reason ?? "erro durante a reformulação").Trim() + "\r\n\r\n" + text.Trim() +
                "\r\n\r\n[FIM DO TRECHO PARCIAL - a transcrição não foi reformulada por completo]";
        }

        public static string SentenceKey(string sentence)
        {
            string key = Regex.Replace(sentence.ToLowerInvariant(), @"\b(?:eu|o depoente)\b", " ");
            key = Regex.Replace(key, @"[^\p{L}\p{N}\s-]", " ");
            return Regex.Replace(key, @"\s+", " ").Trim();
        }

        // Verbs whose 1st and 3rd person singular are identical.
        public static string SameFormVerbs()
        {
            return "disse|tinha|estava|havia|podia|sabia|queria|ia|era|fazia|quis|trouxe|soube|ficava|achava|lembrava|conhecia|precisava|conseguia|via|ouvia|falava|pensava|sentia|vinha";
        }

        // A source sentence without a subject is the narrator's only when the
        // input itself leaves no doubt. For a narrator-only form (perguntei) the
        // morphology decides. For a same-form verb (disse) the previous source
        // sentence must be the narrator's alone: it starts with the narrator and
        // names no other person, pronoun or third-person verb. Otherwise: unknown.
        public static bool IsNarratorSentence(string sentence, string previous, string[,] forms)
        {
            Match lead = Regex.Match(sentence, @"^\s*(?<eu>eu\s+)?(?:(?:não|nunca|também)\s+)?(?<v>\p{L}+)\b", RegexOptions.IgnoreCase);
            if (!lead.Success) return false;
            string v = lead.Groups["v"].Value.ToLowerInvariant();
            if (lead.Groups["eu"].Success) return true;
            for (int i = 0; i < forms.GetLength(0); i++) if (forms[i,0] == v && forms[i,1] != v) return true;
            if (!Regex.IsMatch(v, "^(?:" + SameFormVerbs() + ")$")) return false;
            if (previous == null) return false;
            Match prevLead = Regex.Match(previous, @"^\s*(?<eu>eu\s+)?(?:(?:não|nunca|também)\s+)?(?<v>\p{L}+)\b", RegexOptions.IgnoreCase);
            if (!prevLead.Success) return false;
            bool narratorStart = prevLead.Groups["eu"].Success;
            string pv = prevLead.Groups["v"].Value.ToLowerInvariant();
            for (int i = 0; i < forms.GetLength(0); i++) if (forms[i,0] == pv && forms[i,1] != pv) narratorStart = true;
            if (!narratorStart) return false;
            string rest = previous.Substring(prevLead.Length);
            if (Regex.IsMatch(rest, @"\p{Lu}")) return false;
            if (Regex.IsMatch(previous, @"\b(?:ele|ela|eles|elas|alguém|ninguém|você|vocês|todos|todas|ambos|ambas)\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(rest, @"\b(?:" + SameFormVerbs() + @")\b", RegexOptions.IgnoreCase)) return false;
            foreach (Match w in Regex.Matches(rest, @"\b\p{L}+(?:ou|eu|iu|am|em)\b", RegexOptions.IgnoreCase))
                if (!Regex.IsMatch(w.Value, @"^(?:eu|meu|seu|teu|ou|sou|vou|estou|dou|também|porém|alguém|ninguém|além|nem|sem|em|bem|quem|tem|vem|cem|mesmo|um|algum|nenhum|comum)$", RegexOptions.IgnoreCase)) return false;
            return true;
        }

        // A whole output sentence identical to a narrator sentence of the input
        // (after person conversion) but without a subject gets "O depoente".
        public static string RepairWholeSentenceNarrator(string source, string text, string fullOutput, string[,] forms)
        {
            string[] sentences = Regex.Split(Unquote(source).Trim(), @"(?<=[.!?])\s+");
            var keys = new List<string>();
            foreach (string s in sentences) keys.Add(SentenceKey(ConjugateWith(s, forms)));
            var outputKeys = new List<string>();
            foreach (string s in Regex.Split(fullOutput.Trim(), @"(?<=[.!?])\s+")) outputKeys.Add(SentenceKey(s));
            for (int i = 0; i < sentences.Length; i++)
            {
                string key = keys[i];
                if (key.Length == 0 || keys.IndexOf(key) != keys.LastIndexOf(key)) continue;
                // An abandoned fragment ("Eu entrei...") is not a statement to complete.
                if (Regex.IsMatch(sentences[i].TrimEnd(), @"(?:\.{2,}|…)$")) continue;
                if (outputKeys.IndexOf(key) < 0 || outputKeys.IndexOf(key) != outputKeys.LastIndexOf(key)) continue;
                // The same predicate inside another sentence (another actor) is ambiguous.
                bool shared = false;
                for (int j = 0; j < keys.Count; j++) if (j != i && keys[j].Contains(key)) shared = true;
                foreach (string other in outputKeys) if (other != key && other.Contains(key)) shared = true;
                if (shared) continue;
                if (!IsNarratorSentence(sentences[i], i > 0 ? sentences[i - 1] : null, forms)) continue;
                foreach (Match m in Regex.Matches(text, @"(?:^|(?<=[.!?]\s))(?<s>[^.!?]+[.!?])"))
                {
                    string candidate = m.Groups["s"].Value;
                    if (SentenceKey(candidate) != key) continue;
                    if (Regex.IsMatch(candidate, @"^\s*(?:relatou que\b|o depoente\b)", RegexOptions.IgnoreCase)) break;
                    // Only a subjectless start: (não|nunca|também)? + converted verb.
                    Match verb = Regex.Match(candidate, @"^(?<pre>\s*)(?<adv>(?:(?:não|nunca|também)\s+)?)(?<v>\p{L}+)\b", RegexOptions.IgnoreCase);
                    if (!verb.Success || Regex.IsMatch(verb.Groups["v"].Value, @"^(?:ele|ela|eles|elas|isso|aquilo|alguém|ninguém)$", RegexOptions.IgnoreCase)) break;
                    string fixedSentence = verb.Groups["pre"].Value + "O depoente " + (verb.Groups["adv"].Value + verb.Groups["v"].Value).ToLowerInvariant() + candidate.Substring(verb.Length);
                    text = text.Substring(0, m.Groups["s"].Index) + fixedSentence + text.Substring(m.Groups["s"].Index + candidate.Length);
                    break;
                }
            }
            return text;
        }

        // "na minha presença" -> "na sua presença" leaves "sua" open to the
        // sentence's other subject. Where the narrator has not appeared earlier in
        // that sentence, name the owner. Skipped if the input also has sua/seu N.
        public static string RepairNarratorPossessive(string source, string text)
        {
            string plain = Unquote(source);
            foreach (Match mine in Regex.Matches(plain, @"\b(?<p>minha|meu|minhas|meus)\s+(?<n>\p{L}+)\b", RegexOptions.IgnoreCase))
            {
                string p = mine.Groups["p"].Value.ToLowerInvariant(), n = mine.Groups["n"].Value;
                string third = p == "minha" ? "sua" : p == "meu" ? "seu" : p == "minhas" ? "suas" : "seus";
                string article = p == "minha" ? "a" : p == "meu" ? "o" : p == "minhas" ? "as" : "os";
                if (Regex.IsMatch(plain, @"\b(?:sua|seu|suas|seus)\s+" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase)) continue;
                MatchCollection found = Regex.Matches(text, @"\b(?:(?<prep>na|no|nas|nos|da|do|das|dos|pela|pelo|pelas|pelos|à|ao|às|aos|a|o|as|os|em|de|por|com)\s+)?(?<poss>" + third + @")\s+(?<n>" + Regex.Escape(n) + @")\b", RegexOptions.IgnoreCase);
                for (int i = found.Count - 1; i >= 0; i--)
                {
                    Match m = found[i];
                    int start = SentenceStart(text, m.Index);
                    if (Regex.IsMatch(text.Substring(start, m.Index - start), @"\bdepoente\b", RegexOptions.IgnoreCase)) continue;
                    string prep = m.Groups["prep"].Value, lower = prep.ToLowerInvariant(), head;
                    if (lower == "em") head = "n" + article;
                    else if (lower == "de") head = "d" + article;
                    else if (lower == "por") head = "pel" + article;
                    else if (lower == "com") head = "com " + article;
                    else if (lower.Length > 0) head = lower;
                    else head = article;
                    head = MatchCase(m.Value, head);
                    text = text.Substring(0, m.Index) + head + " " + m.Groups["n"].Value + " do depoente" + text.Substring(m.Index + m.Length);
                }
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
            output = RepairSwappedSubject(original, output);
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
            return FinalLead(output);
        }

        // ---- Opening (round 6, option A) -----------------------------------
        // The engine keeps its internal opening «Relatou que » while all repairs
        // and checks run (they rely on «o depoente» being explicit). As the last
        // step of the final repair, the first block's opening becomes
        // «O depoente relatou que …»: «Relatou que o depoente estava…» →
        // «O depoente relatou que estava…»; «Relatou que ele lhe disse…» →
        // «O depoente relatou que ele lhe disse…». The repeated «o depoente» is
        // kept where it is not the subject of a predicate («o depoente e a esposa»).
        public static string FinalLead(string text)
        {
            if (String.IsNullOrEmpty(text)) return text;
            Match lead = Regex.Match(text, @"^(?<ws>\s*)Relatou que\s+", RegexOptions.IgnoreCase);
            if (!lead.Success) return text;
            string rest = text.Substring(lead.Length);
            Match dup = Regex.Match(rest, @"^o depoente\s+(?<next>\p{L}+)", RegexOptions.IgnoreCase);
            if (dup.Success && !Regex.IsMatch(dup.Groups["next"].Value, @"^(?:e|ou|nem|com|junto|acompanhado|acompanhada|mais|assim|bem)$", RegexOptions.IgnoreCase))
                rest = rest.Substring(dup.Groups["next"].Index);
            return lead.Groups["ws"].Value + "O depoente relatou que " + rest;
        }
        // ---- Swapped subject / reflexive (round 6) -------------------------
        // The model sometimes writes «O depoente me disse» for «Ela me contou»
        // (narrator put in the other person's place, «me» kept); the residual
        // first-person repair then turns «me» into «lhe», which hides the swap.
        // With a unique correspondence in the source, restore the source's
        // subject; a reflexive of the narrator («me defendi») becomes «se».
        // Otherwise leave the text unchanged: Validate reports the swap.
        public static string InvertibleSubjects()
        {
            return @"\b(?<subj>[Ee]les?|[Ee]las?|(?!(?:Depois|Então|Aí|Quando|Ontem|Hoje|Também|Já|Só|Não|Mas|Que|Se|Eu|Logo|Antes|Agora|Lá|Ali|Aqui|Ainda|Mesmo|Até|Nunca|Sempre|Talvez|Acho|Ninguém|Alguém|Todos|Cada|Um|Uma|Esse|Essa|Este|Esta|Isso|Aquele|Aquela|Outro|Outra|Meu|Minha|Seu|Sua|No|Na|Do|Da|Em|De|Por|Para|Com|Sem)\b)\p{Lu}[\p{Ll}\p{M}]+)\s+(?<neg>não\s+)?(?:(?:só|também|já|ainda|então|depois)\s+)?me\s+(?<v>\p{L}+)";
        }

        public static string NarratorSlot()
        {
            return @"\bo depoente\s+(?<neg>não\s+)?(?<adv>(?:só|também|já|ainda|então|depois|apenas)\s+)?(?<cl>lhe|me)\s+(?<v>\p{L}+)";
        }

        public static string VerbStem(string verb)
        {
            string v = verb.ToLowerInvariant();
            string[] ends = { "aram", "eram", "iram", "ou", "eu", "iu", "ava", "ia", "sse", "ei", "i", "a", "e", "o" };
            foreach (string end in ends)
                if (v.Length > end.Length + 2 && v.EndsWith(end)) return v.Substring(0, v.Length - end.Length);
            return v;
        }

        public static bool SpeechVerb(string verb)
        {
            return Regex.IsMatch(verb.ToLowerInvariant(), @"^(?:disse|diss|dis|cont|fal|inform|explic|relat|pergunt|ped|avis|respond|garant|confirm|coment|repet|mand|mostr|entreg|d[eá]|deu|pass|envi|devolv|apresent|indic|ensin|jur)");
        }

        // Same action: same stem («bateu»/«bateu»), or both speech verbs («contou»/«disse»).
        public static bool SameAction(string sourceVerb, string outputVerb)
        {
            string a = VerbStem(sourceVerb), b = VerbStem(outputVerb);
            int n = Math.Min(3, Math.Min(a.Length, b.Length));
            if (n >= 3 && a.Substring(0, n) == b.Substring(0, n)) return true;
            return SpeechVerb(sourceVerb) && SpeechVerb(outputVerb);
        }

        // The narrator did this action himself in the source («eu bati», «contei»).
        public static bool NarratorDid(string source, string verb)
        {
            string stem = VerbStem(verb);
            string head = stem.Length >= 3 ? stem.Substring(0, 3) : stem;
            if (SpeechVerb(verb) && Regex.IsMatch(source, @"\b(?:eu\s+(?:não\s+)?(?:lhe\s+)?(?:disse|contei|falei|perguntei|pedi|avisei|expliquei|respondi|informei|garanti|confirmei|comentei|mandei|mostrei|dei|entreguei)|contei|falei|perguntei|avisei|expliquei|respondi|informei|garanti|comentei|mandei|mostrei|entreguei)\b", RegexOptions.IgnoreCase)) return true;
            return Regex.IsMatch(source, @"\b(?:eu\s+)?(?:não\s+)?(?:lhe\s+|o\s+|a\s+)?" + Regex.Escape(head) + @"\p{L}*(?:ei|i)\b(?!\s+(?:ele|ela))", RegexOptions.IgnoreCase)
                && !Regex.IsMatch(source, @"\bme\s+" + Regex.Escape(head), RegexOptions.IgnoreCase);
        }

        // «me defendi», «me escondi»: reflexive of the narrator in the source.
        public static bool NarratorReflexive(string source, string verb)
        {
            string stem = VerbStem(verb);
            string head = stem.Length >= 4 ? stem.Substring(0, 4) : stem;
            foreach (Match m in Regex.Matches(source, @"(?<pre>\b\p{L}+\s+)?(?:(?:não|só|também|já|ainda|então|apenas)\s+)*me\s+(?<v>" + Regex.Escape(head) + @"\p{L}*(?:ei|i))\b", RegexOptions.IgnoreCase))
            {
                string pre = m.Groups["pre"].Value.Trim().ToLowerInvariant();
                if (Regex.IsMatch(pre, @"^(?:ele|ela|eles|elas)$") || (pre.Length > 0 && Char.IsUpper(m.Groups["pre"].Value.Trim()[0]) && !Regex.IsMatch(pre, @"^(?:eu|e|depois|então|aí|quando|mas|só|também)$"))) continue;
                return true;
            }
            return false;
        }

        public static string RepairSwappedSubject(string original, string output)
        {
            string source = Unquote(original);
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(output, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = output.Substring(start, quote.Index - start);
                result.Append(RepairSwappedPart(source, part, Unquote(output)));
                result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        public static string RepairSwappedPart(string source, string part, string fullOutput)
        {
            MatchCollection slots = Regex.Matches(part, NarratorSlot(), RegexOptions.IgnoreCase);
            for (int s = slots.Count - 1; s >= 0; s--)
            {
                Match slot = slots[s];
                string v = slot.Groups["v"].Value;
                // 1) Reflexive of the narrator: «o depoente só lhe defendeu» → «… só se defendeu».
                if (NarratorReflexive(source, v))
                {
                    Group cl = slot.Groups["cl"];
                    part = part.Substring(0, cl.Index) + "se" + part.Substring(cl.Index + cl.Length);
                    continue;
                }
                // 2) Swapped subject: exactly one «X me <same action>» in the source,
                // one narrator slot with this action in the whole output, and the
                // narrator never does this action himself in the source.
                var candidates = new List<Match>();
                foreach (Match c in Regex.Matches(source, InvertibleSubjects()))
                    if (SameAction(c.Groups["v"].Value, v)) candidates.Add(c);
                if (candidates.Count != 1 || NarratorDid(source, v)) continue;
                int sameSlots = 0;
                foreach (Match o in Regex.Matches(fullOutput, NarratorSlot(), RegexOptions.IgnoreCase))
                    if (SameAction(candidates[0].Groups["v"].Value, o.Groups["v"].Value)) sameSlots++;
                if (sameSlots != 1) continue;
                string subj = candidates[0].Groups["subj"].Value;
                if (Regex.IsMatch(subj, @"^(?:Ele|Ela|Eles|Elas)$")) subj = subj.ToLowerInvariant();
                string neg = slot.Groups["neg"].Value, adv = slot.Groups["adv"].Value;
                string replacement;
                if (SpeechVerb(v)) replacement = subj + " " + neg + adv + "lhe " + v;
                else if (VerbStem(v) == "bat") replacement = subj + " " + neg + adv + v + " no depoente";
                else replacement = subj + " " + neg + adv + v + " o depoente";
                if (Char.IsUpper(slot.Value[0])) replacement = Char.ToUpperInvariant(replacement[0]) + replacement.Substring(1);
                string after = part.Substring(slot.Index + slot.Length);
                // A coordinated verb that was the narrator's keeps the narrator:
                // «ela xingou o depoente e respondeu» → «… e o depoente respondeu».
                Match coord = Regex.Match(after, @"^(?<mid>[^.;!?]*?\s)e\s+(?<rest>(?:(?:só|também|já|ainda|então|depois)\s+)?(?:se\s+|lhe\s+)?(?<v2>\p{L}+(?:ou|eu|iu))\b)", RegexOptions.IgnoreCase);
                if (coord.Success && !Regex.IsMatch(coord.Groups["mid"].Value, @"\b(?:o depoente|ele|ela|eles|elas)\b", RegexOptions.IgnoreCase)
                    && Regex.IsMatch(source, @"\b(?:eu\s+)?(?:(?:só|também|já|ainda)\s+)?(?:me\s+)?" + Regex.Escape(VerbStem(coord.Groups["v2"].Value)) + @"\p{L}*(?:ei|i)\b", RegexOptions.IgnoreCase))
                    after = coord.Groups["mid"].Value + "e o depoente " + coord.Groups["rest"].Value + after.Substring(coord.Length);
                part = part.Substring(0, slot.Index) + replacement + after;
            }
            return part;
        }

        // The swap that could not be repaired is reported (retry, then rejection).
        public static string SwappedSubjectIssue(string original, string output)
        {
            string source = Unquote(original);
            foreach (Match slot in Regex.Matches(Unquote(output), NarratorSlot(), RegexOptions.IgnoreCase))
            {
                string v = slot.Groups["v"].Value;
                if (NarratorReflexive(source, v) || NarratorDid(source, v)) continue;
                foreach (Match c in Regex.Matches(source, InvertibleSubjects()))
                    if (SameAction(c.Groups["v"].Value, v))
                        return "fidelidade: autor da ação trocado com o depoente: no original «" + c.Value.Trim() + "», na saída «" + slot.Value.Trim() + "»";
            }
            return null;
        }

        // ---- Roles (round 7) ------------------------------------------------
        // Four inversions of the 3B model, each repaired only from the source and
        // only with a single possible correspondence; otherwise Validate reports.

        // 1) Another person's subject deleted: «Meu marido foi preso» →
        // «Relatou que foi preso» / «o depoente foi preso». The source predicate
        // (verb + next word) of a non-narrator subject follows the narrator.
        public static string OtherSubjects()
        {
            return @"(?<s>\b(?:[Ee]le|[Ee]la|[Ee]les|[Ee]las)|\b(?:[Oo]|[Aa]|[Oo]s|[Aa]s|[Mm]eu|[Mm]inha|[Mm]eus|[Mm]inhas)\s+(?!depoente\b)\p{Ll}{3,}|(?<=[\p{Ll},;]\s+)\p{Lu}\p{Ll}{2,})\s+(?:(?:não|já|também|ainda|só|então)\s+)*(?:se\s+)?(?<v>\p{L}{3,})\s+(?<w>\p{L}+)";
        }

        public static bool NotPredicateVerb(string v)
        {
            return Regex.IsMatch(v, @"^(?:que|quando|porque|para|com|mas|enquanto|onde|como|pela|pelo|pelas|pelos|depois|antes|dele|dela|lhe|nos|nas|uma|uns|umas|isso|aqui|ali|muito|mesmo|também|me|te)$", RegexOptions.IgnoreCase);
        }

        public static string PredicatePattern(string v, string w)
        {
            string stem = w.Length > 3 ? w.Substring(0, w.Length - 1) : w;
            return Regex.Escape(v) + @"\s+" + Regex.Escape(stem) + (w.Length > 3 ? @"\p{L}{0,2}" : "") + @"\b";
        }

        // The narrator does the same verb anywhere in the source («eu estava…»,
        // «fiquei…», «peguei minha bolsa» for «pegou sua bolsa»): not clear, no repair.
        public static bool NarratorPredicate(string source, string v, string w)
        {
            if (Regex.IsMatch(source, @"\beu\s+(?:não\s+)?(?:já\s+)?(?:me\s+|se\s+|lhe\s+)?" + Regex.Escape(v) + @"\b", RegexOptions.IgnoreCase)) return true;
            foreach (Match f in Regex.Matches(source, @"\b\p{L}+\b"))
            {
                string third = ThirdPerson(f.Value);
                if (third != null && third == v.ToLowerInvariant() && third != f.Value.ToLowerInvariant()) return true;
            }
            return false;
        }

        // «Meu marido» → «o marido do depoente»; «O porteiro» → «o porteiro»; «Ele» → «ele».
        public static string SubjectPhrase(string s)
        {
            Match own = Regex.Match(s, @"^(?<p>[Mm]eu|[Mm]inha|[Mm]eus|[Mm]inhas)\s+(?<n>\p{L}+)$");
            if (own.Success)
            {
                string p = own.Groups["p"].Value.ToLowerInvariant();
                string article = p == "meu" ? "o" : p == "minha" ? "a" : p == "meus" ? "os" : "as";
                return article + " " + own.Groups["n"].Value + " do depoente";
            }
            if (Regex.IsMatch(s, @"^(?:[Oo]|[Aa]|[Oo]s|[Aa]s|[Ee]le|[Ee]la|[Ee]les|[Ee]las)\b")) return Char.ToLowerInvariant(s[0]) + s.Substring(1);
            return s;
        }

        // The word that identifies the subject («marido», «porteiro», «ele», «Carlos»).
        public static string SubjectKey(string s)
        {
            return Regex.Match(s, @"\p{L}+$").Value;
        }

        public static string DeletedSubjectLead()
        {
            return @"(?<lead>\b[Rr]elatou que|\b[Oo] depoente)(?<mid>\s+(?:(?:não|já|também|ainda|só|então)\s+)*(?:se\s+)?)";
        }

        public static string RepairDeletedSubject(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, OtherSubjects()))
            {
                string v = m.Groups["v"].Value, w = m.Groups["w"].Value, s = m.Groups["s"].Value;
                if (NotPredicateVerb(v)) continue;
                string predicate = PredicatePattern(v, w);
                if (Regex.Matches(source, @"\b" + predicate, RegexOptions.IgnoreCase).Count != 1 || NarratorPredicate(source, v, w)) continue;
                string pattern = DeletedSubjectLead() + @"(?<p>" + predicate + ")";
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match x = found[0];
                // The same person already named in this output sentence: which one is it? Not clear.
                string sentence = SentenceAt(text, x.Index);
                if (Regex.IsMatch(sentence, @"\b" + Regex.Escape(SubjectKey(s)) + @"\b", RegexOptions.IgnoreCase)) continue;
                string pred = x.Groups["p"].Value;
                Match pw = Regex.Match(pred, @"(?<v>\p{L}+)\s+(?<w>\p{L}+)$");
                // Agreement as in the source («estava nervoso» → «estava nervosa» for «Ela estava nervosa»).
                if (pw.Success && pw.Groups["w"].Value != w && w.Length > 3) pred = pred.Substring(0, pw.Groups["w"].Index) + w;
                string subject = SubjectPhrase(s);
                string lead = x.Groups["lead"].Value, replacement;
                if (lead.ToLowerInvariant() == "relatou que") replacement = lead + " " + subject + x.Groups["mid"].Value + pred;
                else replacement = MatchCase(lead, subject) + x.Groups["mid"].Value + pred;
                text = text.Substring(0, x.Index) + replacement + text.Substring(x.Index + x.Length);
            }
            return text;
        }

        public static string DeletedSubjectIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, OtherSubjects()))
            {
                string v = m.Groups["v"].Value, w = m.Groups["w"].Value;
                if (NotPredicateVerb(v)) continue;
                string predicate = PredicatePattern(v, w);
                if (Regex.Matches(source, @"\b" + predicate, RegexOptions.IgnoreCase).Count != 1 || NarratorPredicate(source, v, w)) continue;
                Match x = Regex.Match(rendered, DeletedSubjectLead() + @"(?<p>" + predicate + ")", RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: sujeito de outra pessoa apagado: no original «" + m.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            return null;
        }

        // 2) Roles swapped around the narrator: «O Carlos entrou depois de mim» →
        // «Relatou que entrou depois do Carlos». Restore the source's order.
        public static string RelativeToMe()
        {
            return @"(?<s>\b(?:[Ee]le|[Ee]la|(?:[Oo]|[Aa])\s+\p{Ll}{3,}|(?:[Oo]|[Aa])\s+\p{Lu}\p{Ll}+|(?<=[\p{Ll},;]\s+)\p{Lu}\p{Ll}{2,}))\s+(?<v>\p{L}{3,})\s+(?<rel>depois|antes|atrás|perto|longe|ao lado|na frente|em frente)\s+de\s+mim\b";
        }

        public static string SwappedRelativePattern(Match m)
        {
            string key = SubjectKey(m.Groups["s"].Value);
            return DeletedSubjectLead() + @"(?<v>" + Regex.Escape(m.Groups["v"].Value) + @")\s+" + Regex.Escape(m.Groups["rel"].Value) + @"\s+(?:d[oa]|de)\s+(?:\p{L}+\s+)?" + Regex.Escape(key) + @"\b";
        }

        public static string RepairSwappedRelative(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, RelativeToMe()))
            {
                if (Regex.Matches(source, Regex.Escape(m.Value)).Count != 1) continue;
                // «depois do Carlos» in the source itself: no swap to repair.
                if (Regex.IsMatch(source, @"\b" + Regex.Escape(m.Groups["rel"].Value) + @"\s+(?:d[oa]|de)\s+(?:\p{L}+\s+)?" + Regex.Escape(SubjectKey(m.Groups["s"].Value)) + @"\b", RegexOptions.IgnoreCase)) continue;
                string pattern = SwappedRelativePattern(m);
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match x = found[0];
                string subject = SubjectPhrase(m.Groups["s"].Value), lead = x.Groups["lead"].Value;
                string body = x.Groups["mid"].Value + x.Groups["v"].Value + " " + m.Groups["rel"].Value + " do depoente";
                string replacement = lead.ToLowerInvariant() == "relatou que" ? lead + " " + subject + body : MatchCase(lead, subject) + body;
                text = text.Substring(0, x.Index) + replacement + text.Substring(x.Index + x.Length);
            }
            // «ficou me encarando» → «ficou lhe encarando»: «lhe» with a direct action;
            // the source's object is the narrator.
            foreach (Match g in Regex.Matches(source, @"\bme\s+(?<g>\p{L}+ndo)\b", RegexOptions.IgnoreCase))
            {
                string pattern = @"\blhe\s+(?<g>" + Regex.Escape(g.Groups["g"].Value) + @")\b";
                if (Regex.Matches(source, @"\bme\s+" + Regex.Escape(g.Groups["g"].Value) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                text = text.Substring(0, found[0].Index) + found[0].Groups["g"].Value + " o depoente" + text.Substring(found[0].Index + found[0].Length);
            }
            return text;
        }

        public static string SwappedRoleIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, RelativeToMe()))
            {
                if (Regex.IsMatch(source, @"\b" + Regex.Escape(m.Groups["rel"].Value) + @"\s+(?:d[oa]|de)\s+(?:\p{L}+\s+)?" + Regex.Escape(SubjectKey(m.Groups["s"].Value)) + @"\b", RegexOptions.IgnoreCase)) continue;
                Match x = Regex.Match(rendered, SwappedRelativePattern(m), RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: papéis trocados: no original «" + m.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            // «eu era culpada» → «ele era culpado»: the narrator rendered as another person.
            string[,] forms = SourceNarratorForms(original);
            string conjugated = ConjugateWith(source, forms);
            foreach (Match clause in Regex.Matches(source, @"\beu\s+(?<neg>não\s+)?(?<v>\p{L}+)\s+(?<next>\p{L}+)", RegexOptions.IgnoreCase))
            {
                string body = NarratorBody(clause, forms);
                if (Regex.IsMatch(conjugated, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase)) continue;
                Match x = Regex.Match(rendered, @"\b(?:ele|ela)\s+" + body, RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: depoente trocado por outra pessoa: no original «" + clause.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            return null;
        }

        // Narrator predicate after person conversion; the next word may change gender
        // («eu era culpada» / «ele era culpado»).
        public static string NarratorBody(Match clause, string[,] forms)
        {
            string to = ConjugateWith(clause.Groups["v"].Value.ToLowerInvariant(), forms);
            string next = clause.Groups["next"].Value;
            string nextPattern = next.Length > 3 && Regex.IsMatch(next, @"[oa]s?$", RegexOptions.IgnoreCase)
                ? Regex.Escape(Regex.Replace(next, @"[oa](s?)$", "")) + @"[oa]s?"
                : Regex.Escape(next);
            return Regex.Escape(clause.Groups["neg"].Value) + Regex.Escape(to) + @"\s+" + nextPattern + @"\b";
        }

        // 3) Ambiguous pronoun: «ele me atacou» → «ele o atacou» (o = another man?).
        // Prefer the passive with the narrator as subject: «o depoente foi atacado por ele».
        public static string Participle(string verb)
        {
            string v = verb.ToLowerInvariant();
            if (Regex.IsMatch(v, @"^(?:deu|viu|bateu|abriu|cobriu|descobriu|escreveu|fez|disse|trouxe|pôs|quis|foi|teve|veio|leu)$")) return null;
            if (v.Length > 4 && v.EndsWith("ou")) return v.Substring(0, v.Length - 2) + "ado";
            if (v.Length > 4 && (v.EndsWith("eu") || v.EndsWith("iu"))) return v.Substring(0, v.Length - 2) + "ido";
            return null;
        }

        public static string AmbiguousObjectSource()
        {
            return @"\b(?<s>[Ee]le|[Ee]la|[Ee]les|[Ee]las)\s+(?<neg>não\s+)?(?<adv>(?:já|também|ainda|depois|então)\s+)?me\s+(?<v>\p{L}+(?:ou|eu|iu))\b";
        }

        public static string AmbiguousObjectPattern(Match m)
        {
            string stem = VerbStem(m.Groups["v"].Value);
            string head = stem.Length >= 4 ? stem.Substring(0, 4) : stem;
            return @"\b(?<s>" + Regex.Escape(m.Groups["s"].Value) + @")\s+(?<neg>não\s+)?(?<adv>(?:já|também|ainda|depois|então)\s+)?(?:o|a)\s+(?<v>" + Regex.Escape(head) + @"\p{L}*(?:ou|eu|iu))\b";
        }

        public static string RepairAmbiguousObject(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, AmbiguousObjectSource()))
            {
                if (SpeechVerb(m.Groups["v"].Value) || Participle(m.Groups["v"].Value) == null) continue;
                if (Regex.Matches(source, Regex.Escape(m.Value), RegexOptions.IgnoreCase).Count != 1) continue;
                string pattern = AmbiguousObjectPattern(m);
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match x = found[0];
                string participle = Participle(x.Groups["v"].Value);
                if (participle == null) continue;
                string s = x.Groups["s"].Value;
                string replacement = MatchCase(s, "o depoente ") + x.Groups["neg"].Value + x.Groups["adv"].Value + "foi " + participle + " por " + s.ToLowerInvariant();
                text = text.Substring(0, x.Index) + replacement + text.Substring(x.Index + x.Length);
            }
            return text;
        }

        public static string AmbiguousObjectIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, AmbiguousObjectSource()))
            {
                if (SpeechVerb(m.Groups["v"].Value) || Participle(m.Groups["v"].Value) == null) continue;
                Match x = Regex.Match(rendered, AmbiguousObjectPattern(m), RegexOptions.IgnoreCase);
                if (x.Success && Participle(x.Groups["v"].Value) != null) return "fidelidade: pronome ambíguo: no original «" + m.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            return null;
        }

        // 4a) «ele estava me enforcando» (narrator as object of a gerund). The model
        // writes «ele estava o depoente enforcando» (broken) or «o depoente estava
        // enforcando» (roles swapped). With a single source frame, restore it:
        // «ele estava enforcando o depoente». Otherwise Validate reports it.
        public static string GerundSource()
        {
            return @"(?<s>\b(?:[Ee]le|[Ee]la|[Ee]les|[Ee]las)|\b(?:[Oo]|[Aa])\s+\p{Ll}{3,}|(?<=[\p{Ll},;]\s+)\p{Lu}\p{Ll}{2,})\s+(?<neg>não\s+)?(?<aux>estava|estavam|está|estão|ficou|ficava|continuava|continuou|começou a|tentava|tentou|ia|vinha)\s+me\s+(?<g>\p{L}+ndo)\b";
        }

        public static string RepairGerundObject(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, GerundSource()))
            {
                string g = m.Groups["g"].Value, aux = m.Groups["aux"].Value;
                // One source frame with this gerund, and the narrator never does it himself.
                if (Regex.Matches(source, @"\b" + Regex.Escape(g) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                if (Regex.IsMatch(source, @"\beu\s+(?:não\s+)?" + Regex.Escape(aux) + @"\s+" + Regex.Escape(g), RegexOptions.IgnoreCase)) continue;
                // Broken: «estava o depoente enforcando» → «estava enforcando o depoente».
                string broken = @"\b(?<aux>" + Regex.Escape(aux) + @")\s+o depoente\s+(?<g>" + Regex.Escape(g) + @")\b";
                if (Regex.Matches(fullOutput, broken, RegexOptions.IgnoreCase).Count == 1)
                {
                    MatchCollection found = Regex.Matches(text, broken, RegexOptions.IgnoreCase);
                    if (found.Count == 1)
                    {
                        Match x = found[0];
                        text = text.Substring(0, x.Index) + x.Groups["aux"].Value + " " + x.Groups["g"].Value + " o depoente" + text.Substring(x.Index + x.Length);
                        continue;
                    }
                }
                // Swapped: «o depoente estava enforcando» → «ele estava enforcando o depoente».
                string swapped = DeletedSubjectLead() + @"(?<neg>não\s+)?(?<aux>" + Regex.Escape(aux) + @")\s+(?<g>" + Regex.Escape(g) + @")\b(?!\s+(?:o|a)\s+depoente\b)";
                if (Regex.Matches(fullOutput, swapped, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection hits = Regex.Matches(text, swapped, RegexOptions.IgnoreCase);
                if (hits.Count != 1) continue;
                Match y = hits[0];
                string subject = SubjectPhrase(m.Groups["s"].Value), lead = y.Groups["lead"].Value;
                string body = y.Groups["mid"].Value + y.Groups["neg"].Value + y.Groups["aux"].Value + " " + y.Groups["g"].Value + " o depoente";
                string replacement = lead.ToLowerInvariant() == "relatou que" ? lead + " " + subject + body : MatchCase(lead, subject) + body;
                text = text.Substring(0, y.Index) + replacement + text.Substring(y.Index + y.Length);
            }
            return text;
        }

        public static string GerundObjectIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, GerundSource()))
            {
                if (Regex.IsMatch(source, @"\beu\s+(?:não\s+)?" + Regex.Escape(m.Groups["aux"].Value) + @"\s+" + Regex.Escape(m.Groups["g"].Value), RegexOptions.IgnoreCase)) continue;
                string pattern = DeletedSubjectLead() + @"(?:não\s+)?" + Regex.Escape(m.Groups["aux"].Value) + @"\s+" + Regex.Escape(m.Groups["g"].Value) + @"\b(?!\s+(?:o|a)\s+depoente\b)";
                Match x = Regex.Match(rendered, pattern, RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: papéis trocados: no original «" + m.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            return null;
        }

        // 4) Broken phrase: «ele estava o depoente enforcando» — the narrator inside a
        // verbal phrase. Never guessed; reported for a new attempt.
        public static string BrokenPhraseIssue(string output)
        {
            Match x = Regex.Match(Unquote(output), @"\b(?:estava|está|estavam|estão|ia|vai|tinha|havia|queria|começou a|continuou a|tentou|tentava)\s+o depoente\s+\p{L}+(?:ndo|ar|er|ir)\b", RegexOptions.IgnoreCase);
            return x.Success ? "fidelidade: frase quebrada: «o depoente» no meio da locução verbal: «" + x.Value + "»" : null;
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
            // Round 7: roles (after the narrator pronoun, so «ele era culpado» is
            // already «o depoente era culpado» when the deleted subject is restored).
            text = RepairGerundObject(source, text, fullOutput);
            text = RepairSwappedRelative(source, text, fullOutput);
            text = RepairDeletedSubject(source, text, fullOutput);
            text = RepairAmbiguousObject(source, text, fullOutput);
            text = RepairWholeSentenceNarrator(source, text, fullOutput, forms);
            text = RepairNarratorPossessive(source, text);
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
            string swapped = SwappedSubjectIssue(original, output);
            if (swapped != null) return swapped;
            // Round 7: roles that could not be repaired with certainty.
            string role = DeletedSubjectIssue(original, output);
            if (role == null) role = SwappedRoleIssue(original, output);
            if (role == null) role = AmbiguousObjectIssue(original, output);
            if (role == null) role = GerundObjectIssue(original, output);
            if (role == null) role = BrokenPhraseIssue(output);
            if (role != null) return role;
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
