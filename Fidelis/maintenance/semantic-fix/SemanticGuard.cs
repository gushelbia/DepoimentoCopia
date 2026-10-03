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
                    // Round 9: «sua» is ambiguous also when another person is in the
                    // sentence («achou que era seu namorado», «ligado do seu celular»).
                    if (Regex.IsMatch(text.Substring(start, m.Index - start), @"\bdepoente\b", RegexOptions.IgnoreCase)
                        && !OtherPersonInSentence(SentenceAt(text, m.Index), m.Groups["n"].Value)) continue;
                    // «sua colega Bianca»: the name right after already identifies the person.
                    if (Regex.IsMatch(text.Substring(start, m.Index - start), @"\bdepoente\b", RegexOptions.IgnoreCase)
                        && Regex.IsMatch(text.Substring(m.Index + m.Length), @"^,?\s+(?:o\s+|a\s+)?\p{Lu}\p{Ll}+")) continue;
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
            string step = output;
            output = RepairNameSpelling(original, output);
            NoteRepair("grafia-de-nomes", step, output);
            if (String.IsNullOrWhiteSpace(original) || String.IsNullOrWhiteSpace(output)) return output;
            step = output;
            output = RepairSwappedSubject(original, output);
            NoteRepair("sujeito-trocado", step, output);
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
            step = output;
            output = FinalLead(output);
            NoteRepair("abertura", step, output);
            // Round 10: names kept in capitals and «Eu, Marta, sou…» → «O depoente, Marta, relatou que é…».
            step = output;
            output = RepairNamesAndOpening(original, output);
            NoteRepair("nome-na-abertura", step, output);
            return output;
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

        // ---- Log without testimony text (round 8) ---------------------------
        // AppLog.Write calls LogMessage first. In normal use the log keeps only
        // technical data (events, times, sizes, status, error categories and the
        // names of the repairs applied); any excerpt of the input or the output is
        // removed. The full text is kept only in diagnostic mode, which only the
        // tests turn on (environment variable DEPOIMENTOLOCAL_LOG_DIAGNOSTICO=1).
        public static bool DiagnosticLog()
        {
            return Environment.GetEnvironmentVariable("DEPOIMENTOLOCAL_LOG_DIAGNOSTICO") == "1";
        }

        public static string LogMessage(string eventName, string details)
        {
            string message = details ?? "";
            if (!DiagnosticLog()) message = RedactLog(message);
            if (eventName == "APP_START") message += (message.Length > 0 ? "; " : "") + (DiagnosticLog() ? "log=diagnóstico (texto completo; só para testes)" : "log=técnico (sem texto do depoimento)");
            if (Regex.IsMatch(eventName ?? "", @"^(?:LOCAL_REPAIR|BLOCK_OK|BLOCK_RETRY_START|BLOCK_RETRY_END|BLOCK_REJECTED|BLOCK_ACCEPTED_WITH_WARNING)$"))
            {
                string repairs = TakeRepairs();
                if (repairs.Length > 0) message += "; repairs=" + repairs;
            }
            return message;
        }

        public const string OmittedText = "[texto omitido]";

        // Keeps the category of each validator message («fidelidade: possível
        // omissão de oração») and drops what follows it; quoted excerpts go too.
        public static string RedactLog(string message)
        {
            if (String.IsNullOrEmpty(message)) return message;
            string text = Regex.Replace(message, "«[^»\r\n]*»|“[^”\r\n]*”|\"[^\"\r\n]*\"", "«…»");
            text = Regex.Replace(text, @"(?<cat>fidelidade:\s*[^:;\r\n]+?):[^\r\n]*?(?=;\s*[A-Za-z]+=|\r?$)", "${cat}: " + OmittedText, RegexOptions.Multiline);
            // Engine messages with a payload after a colon in a text-bearing field.
            text = Regex.Replace(text, @"(?<key>\b(?:issue|before|after|detail)=)(?!\s*fidelidade:)(?<value>[^;\r\n]*?):(?!\s*\[texto omitido\])[^\r\n]*?(?=;\s*[A-Za-z]+=|\r?$)", "${key}${value}: " + OmittedText, RegexOptions.Multiline);
            // Review message of a rejected block: «… ainda precisa de revisão: <motivo>».
            text = Regex.Replace(text, @"(?<head>ainda precisa de revisão:\s*)(?!\s|fidelidade:|\[texto omitido\])[^\r\n]*", "${head}" + OmittedText);
            return text;
        }

        // Names of the repairs that changed the text, collected per block and
        // written with the next block event. Kept in the AppDomain (the engine
        // copy of this class has methods only, no fields).
        public const string RepairsKey = "DepoimentoLocal.SemanticGuard.Repairs";

        public static void NoteRepair(string name, string before, string after)
        {
            if (before == after) return;
            string current = AppDomain.CurrentDomain.GetData(RepairsKey) as string ?? "";
            if (("," + current + ",").Contains("," + name + ",")) return;
            AppDomain.CurrentDomain.SetData(RepairsKey, current.Length == 0 ? name : current + "," + name);
        }

        public static string TakeRepairs()
        {
            string current = AppDomain.CurrentDomain.GetData(RepairsKey) as string ?? "";
            AppDomain.CurrentDomain.SetData(RepairsKey, "");
            return current;
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

        // ---- Round 10 (realistic battery 3) ----------------------------------

        // 1) Other first-person forms: «comigo», «em/de/por… mim», «foi eu», «foi meu».
        // The model writes «com ele», «nele», «sido ele», «foi seu»: with a single
        // correspondence, the narrator is restored («com o depoente», «sido o depoente»).
        public static string[,] MimForms()
        {
            return new string[,] {
                {"em", "nele|nela", "no depoente"}, {"de", "dele|dela", "do depoente"}, {"por", "por ele|por ela", "pelo depoente"},
                {"sobre", "sobre ele|sobre ela", "sobre o depoente"}, {"contra", "contra ele|contra ela", "contra o depoente"},
                {"sem", "sem ele|sem ela", "sem o depoente"}, {"até", "até ele|até ela", "até o depoente"}
            };
        }

        // Frames of the source and the wrong output form for each: (source pattern, output pattern, replacement).
        public static List<string[]> OtherFirstPersonFrames(string source)
        {
            var frames = new List<string[]>();
            string plain = Unquote(source);
            foreach (Match m in Regex.Matches(plain, @"\b(?<w>\p{L}{3,})\s+comigo\b", RegexOptions.IgnoreCase))
                frames.Add(new string[] { m.Value, @"\b" + Regex.Escape(m.Groups["w"].Value) + @"\s+(?<x>com\s+(?:ele|ela))\b", "com o depoente", Regex.Escape(m.Groups["w"].Value) + @"\s+com\s+(?:ele|ela)\b" });
            string[,] mim = MimForms();
            for (int i = 0; i < mim.GetLength(0); i++)
                foreach (Match m in Regex.Matches(plain, @"\b(?<w>\p{L}{3,})\s+" + mim[i, 0] + @"\s+mim\b", RegexOptions.IgnoreCase))
                    frames.Add(new string[] { m.Value, @"\b" + Regex.Escape(m.Groups["w"].Value) + @"\s+(?<x>" + mim[i, 1] + @")\b", mim[i, 2], Regex.Escape(m.Groups["w"].Value) + @"\s+(?:" + mim[i, 1] + @")\b" });
            foreach (Match m in Regex.Matches(plain, @"\b(?<v>foi|era|sido|fosse|é|seria|será)\s+eu\b", RegexOptions.IgnoreCase))
                frames.Add(new string[] { m.Value, @"\b" + m.Groups["v"].Value + @"\s+(?<x>ele|ela)\b(?!\s+(?:mesm|própri))", "o depoente", m.Groups["v"].Value + @"\s+(?:ele|ela)\b" });
            foreach (Match m in Regex.Matches(plain, @"\b(?<v>foi|era|é|sido|fosse)\s+(?:meu|minha|meus|minhas)(?=\s*[,.;!?])", RegexOptions.IgnoreCase))
                frames.Add(new string[] { m.Value, @"\b" + m.Groups["v"].Value + @"\s+(?<x>seu|sua|seus|suas|dele|dela)(?=\s*[,.;!?])", "do depoente", m.Groups["v"].Value + @"\s+(?:seu|sua|seus|suas|dele|dela)(?=\s*[,.;!?])" });
            return frames;
        }

        public static string RepairOtherFirstPerson(string source, string text, string fullOutput)
        {
            string plain = Unquote(source);
            foreach (string[] f in OtherFirstPersonFrames(source))
            {
                if (Regex.Matches(plain, Regex.Escape(f[0]), RegexOptions.IgnoreCase).Count != 1) continue;
                if (Regex.IsMatch(plain, @"\b" + f[3], RegexOptions.IgnoreCase)) continue;      // the source has the third person itself
                if (Regex.Matches(fullOutput, f[1], RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, f[1], RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Group x = found[0].Groups["x"];
                text = text.Substring(0, x.Index) + f[2] + text.Substring(x.Index + x.Length);
            }
            return text;
        }

        public static string OtherFirstPersonIssue(string original, string output)
        {
            string plain = Unquote(original), rendered = Unquote(output);
            foreach (string[] f in OtherFirstPersonFrames(original))
            {
                if (Regex.IsMatch(plain, @"\b" + f[3], RegexOptions.IgnoreCase)) continue;
                Match x = Regex.Match(rendered, f[1], RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: depoente trocado por outra pessoa: no original «" + f[0] + "», na saída «" + x.Value + "»";
            }
            return null;
        }

        // 2) Approximation lost: «umas sete e meia» → «às sete e meia».
        public static string NumberPhrase()
        {
            const string word = @"(?:um|uma|dois|duas|três|tres|quatro|cinco|seis|sete|oito|nove|dez|onze|doze|treze|quatorze|catorze|quinze|dezesseis|dezessete|dezoito|dezenove|vinte|trinta|quarenta|cinquenta|sessenta|setenta|oitenta|noventa|cem|cento|duzentos|duzentas|trezentos|quinhentos|mil)";
            return @"(?:\d{1,4}(?:h\d{0,2}|:\d{2})?|" + word + @"(?:\s+e\s+(?:meia|pouco|" + word + @"))*)";
        }

        public static string RepairLostApproximation(string source, string text, string fullOutput)
        {
            string plain = Unquote(source);
            foreach (Match m in Regex.Matches(plain, @"\b(?<a>umas|uns|por\s+volta\s+d[aoe]s?|cerca\s+de|lá\s+pel[ao]s|aproximadamente)\s+(?<n>" + NumberPhrase() + @")\b", RegexOptions.IgnoreCase))
            {
                string n = m.Groups["n"].Value;
                if (Regex.Matches(plain, @"\b" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                string pattern = @"(?<pre>\b\p{L}+\s+)?\b(?<n>" + Regex.Escape(n) + @")\b";
                var unmarked = new List<Match>();
                foreach (Match x in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    string before = text.Substring(0, x.Groups["n"].Index);
                    if (Regex.IsMatch(before, @"\b(?:umas|uns|volta\s+d[aoe]s?|cerca\s+de|pel[ao]s|aproximadamente|mais\s+ou\s+menos|em\s+torno\s+d[aeo]s?|perto\s+d[aeo]s?|quase)\s+$", RegexOptions.IgnoreCase)) continue;
                    unmarked.Add(x);
                }
                if (unmarked.Count != 1 || Regex.Matches(fullOutput, @"\b" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                Match u = unmarked[0];
                string pre = u.Groups["pre"].Value;
                if (Regex.IsMatch(pre, @"^(?:às|as)\s+$", RegexOptions.IgnoreCase))
                    text = text.Substring(0, u.Groups["pre"].Index) + MatchCase(pre, "por volta das ") + text.Substring(u.Groups["n"].Index);
                else if (Regex.IsMatch(pre, @"^(?:à|a)\s+$", RegexOptions.IgnoreCase))
                    text = text.Substring(0, u.Groups["pre"].Index) + MatchCase(pre, "por volta da ") + text.Substring(u.Groups["n"].Index);
                else
                    text = text.Substring(0, u.Groups["n"].Index) + "cerca de " + text.Substring(u.Groups["n"].Index);
            }
            return text;
        }

        public static string LostApproximationIssue(string original, string output)
        {
            string plain = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(plain, @"\b(?<a>umas|uns|por\s+volta\s+d[aoe]s?|cerca\s+de|lá\s+pel[ao]s|aproximadamente)\s+(?<n>" + NumberPhrase() + @")\b", RegexOptions.IgnoreCase))
            {
                string n = m.Groups["n"].Value;
                foreach (Match x in Regex.Matches(rendered, @"\b" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase))
                {
                    string before = rendered.Substring(0, x.Index);
                    if (Regex.IsMatch(before, @"\b(?:umas|uns|volta\s+d[aoe]s?|cerca\s+de|pel[ao]s|aproximadamente|mais\s+ou\s+menos|em\s+torno\s+d[aeo]s?|perto\s+d[aeo]s?|quase)\s+$", RegexOptions.IgnoreCase)) continue;
                    if (Regex.IsMatch(before, @"\b(?:às|as|à|a)\s+$", RegexOptions.IgnoreCase))
                        return "fidelidade: aproximação perdida: no original «" + m.Value + "», na saída exato";
                }
            }
            return null;
        }

        // 3) First person plural copied from the source. Form chosen by the user:
        // an explicit number in the same or the previous sentence → «os quatro»; exactly
        // one other member named safely in the same sentence (before «a gente») or in the
        // previous one → «o depoente e a Olívia», verb in the plural; otherwise, or in
        // doubt → «o grupo».
        public static string[,] PluralVerbs()
        {
            return new string[,] {
                {"estávamos","estava"}, {"éramos","era"}, {"fomos","foi"}, {"ficamos","ficou"}, {"chegamos","chegou"}, {"saímos","saiu"},
                {"vimos","viu"}, {"voltamos","voltou"}, {"fizemos","fez"}, {"tínhamos","tinha"}, {"íamos","ia"}, {"podíamos","podia"},
                {"sabíamos","sabia"}, {"estamos","está"}, {"somos","é"}, {"temos","tem"}, {"vamos","vai"}, {"entramos","entrou"}
            };
        }

        // Third person plural of a third person singular verb; null when unknown.
        public static string PluralOf(string verb)
        {
            string v = verb.ToLowerInvariant();
            string[,] irregular = {
                {"foi","foram"}, {"era","eram"}, {"fez","fizeram"}, {"está","estão"}, {"é","são"}, {"tem","têm"}, {"vai","vão"},
                {"saiu","saíram"}, {"pode","podem"}, {"deu","deram"}, {"veio","vieram"}, {"teve","tiveram"}, {"disse","disseram"},
                {"quis","quiseram"}, {"pôs","puseram"}, {"viu","viram"}, {"ia","iam"}
            };
            for (int i = 0; i < irregular.GetLength(0); i++) if (irregular[i, 0] == v) return irregular[i, 1];
            if (v.Length < 4) return null;
            if (v.EndsWith("ou")) return v.Substring(0, v.Length - 2) + "aram";
            if (v.EndsWith("eu")) return v.Substring(0, v.Length - 2) + "eram";
            if (v.EndsWith("iu")) return v.Substring(0, v.Length - 2) + "iram";
            if (v.EndsWith("ava") || v.EndsWith("ia")) return v + "m";
            return null;
        }

        // Sentences of a text; quoted speech never splits a sentence.
        public static MatchCollection PluralSentences(string text)
        {
            return Regex.Matches(text, "(?:\"[^\"]*\"|“[^”]*”|«[^»]*»|[^.!?\"“«])+[.!?]*");
        }

        public static bool HasLetter(string text)
        {
            foreach (char c in text) if (Char.IsLetter(c)) return true;
            return false;
        }

        // The group of source sentence i: «os quatro», «o depoente e a Olívia» or «o grupo».
        public static string GroupOf(List<string> sentences, int i)
        {
            string here = Unquote(sentences[i]), before = i > 0 ? Unquote(sentences[i - 1]) : "";
            string count = @"\b(?:(?:nós|somos|éramos)\s+(?<n>dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez)\b|em\s+(?<n>dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez)(?=\s*(?:[,.;:!?]|$)|\s+(?:no|na|nos|nas|num|numa|e|pessoas)\b))";
            Match number = Regex.Match(here, count, RegexOptions.IgnoreCase);
            if (!number.Success) number = Regex.Match(before, count, RegexOptions.IgnoreCase);
            if (number.Success)
            {
                string n = number.Groups["n"].Value.ToLowerInvariant();
                return (n == "duas" ? "as " : "os ") + n;
            }
            Match first = Regex.Match(here, @"\b(?:a\s+gente|nós|nos|noss[ao]s?|conosco)\b", RegexOptions.IgnoreCase);
            string scope = before + " . " + (first.Success ? here.Substring(0, first.Index) : here);
            string after = first.Success ? here.Substring(first.Index) : "";
            bool narrator = Regex.IsMatch(scope, @"\beu\b", RegexOptions.IgnoreCase);
            var names = new List<string>(); var articles = new List<string>();
            foreach (Match c in Regex.Matches(scope, @"(?:^\s*|[.!?]\s+)(?<art>[OA])\s+(?<n>\p{Lu}\p{Ll}+)(?!\s*(?:,\s*|e\s+)(?:o|a)\s+\p{Lu})\s+\p{Ll}|\b[Ee]u\s+e\s+(?:(?:o|a)\s+)?(?:(?:meu|minha)\s+\p{Ll}+,\s*)?(?<art>o|a)\s+(?<n>\p{Lu}\p{Ll}+)(?!\s*(?:,\s*|e\s+)(?:o|a)\s+\p{Lu})|\bcom\s+(?<art>o|a)\s+(?<n>\p{Lu}\p{Ll}+)(?!\s*(?:,\s*|e\s+)(?:o|a)\s+\p{Lu})"))
            {
                string n = c.Groups["n"].Value;
                if (c.Value.StartsWith("com") && !narrator) continue;
                if (Regex.IsMatch(after, @"\b" + Regex.Escape(n) + @"\b")) continue;   // named after «a gente»: not a member
                if (names.Contains(n)) continue;
                names.Add(n); articles.Add(c.Groups["art"].Value.ToLowerInvariant());
            }
            if (names.Count == 1) return "o depoente e " + articles[0] + " " + names[0];
            return "o grupo";
        }

        // «da gente» → «do grupo» / «dos quatro» / «do depoente e da Olívia».
        public static string GroupWith(string prep, string group)
        {
            if (group == "o grupo") return prep == "de" ? "do grupo" : prep == "em" ? "no grupo" : prep + " o grupo";
            if (group.StartsWith("os ") || group.StartsWith("as "))
                return prep == "de" ? "d" + group : prep == "em" ? "n" + group : prep + " " + group;
            Match m = Regex.Match(group, @"^o depoente e (?<art>o|a) (?<n>.+)$");
            if (m.Success && prep == "de") return "do depoente e d" + m.Groups["art"].Value + " " + m.Groups["n"].Value;
            if (m.Success && prep == "em") return "no depoente e n" + m.Groups["art"].Value + " " + m.Groups["n"].Value;
            return prep + " " + group;
        }

        public static string Capitalized(string text)
        {
            return text.Length == 0 ? text : Char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        public static string RepairFirstPersonPlural(string source, string text)
        {
            string plain = Unquote(source);
            if (!Regex.IsMatch(plain, @"\b(?:nós|nos|nosso|nossa|nossos|nossas|conosco|a\s+gente|estávamos|éramos|fomos)\b", RegexOptions.IgnoreCase)) return text;
            var sources = new List<string>();
            foreach (Match m in PluralSentences(source)) if (HasLetter(m.Value)) sources.Add(m.Value);
            MatchCollection parts = PluralSentences(text);
            int count = 0;
            foreach (Match m in parts) if (HasLetter(m.Value)) count++;
            bool aligned = count == sources.Count;
            var result = new System.Text.StringBuilder(); int start = 0, index = 0;
            foreach (Match m in parts)
            {
                result.Append(text.Substring(start, m.Index - start));
                if (!HasLetter(m.Value)) result.Append(m.Value);
                else
                {
                    string group = aligned ? GroupOf(sources, index) : "o grupo";
                    result.Append(ConvertPluralPart(plain, m.Value, group));
                    index++;
                }
                start = m.Index + m.Length;
            }
            result.Append(text.Substring(start));
            return result.ToString();
        }

        public static string ConvertPluralPart(string plain, string sentence, string group)
        {
            bool plural = group != "o grupo";
            bool counted = group.StartsWith("os ") || group.StartsWith("as ");
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(sentence, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = sentence.Substring(start, quote.Index - start);
                // «nós três» with the group «os três»: the number goes with the group.
                if (counted) part = Regex.Replace(part, @"\b(?<s>nós|a\s+gente)\s+" + Regex.Escape(group.Substring(3)) + @"\b", "${s}", RegexOptions.IgnoreCase);
                string[,] verbs = PluralVerbs();
                for (int i = 0; i < verbs.GetLength(0); i++)
                    if (Regex.IsMatch(plain, @"\b" + verbs[i, 0] + @"\b", RegexOptions.IgnoreCase))
                        part = Regex.Replace(part, @"\b(?<s>(?:nós|a\s+gente|o\s+grupo)\s+(?:não\s+)?)" + verbs[i, 0] + @"\b", "${s}" + verbs[i, 1], RegexOptions.IgnoreCase);
                if (Regex.IsMatch(plain, @"\ba\s+gente\b|\bnós\b", RegexOptions.IgnoreCase))
                {
                    part = Regex.Replace(part, @"\b[Dd]a\s+gente\b", "§de§");
                    part = Regex.Replace(part, @"\b[Nn]a\s+gente\b", "§em§");
                    part = Regex.Replace(part, @"\b(?<p>com|para|pra)\s+a\s+gente\b", "§${p}§", RegexOptions.IgnoreCase);
                    // Subject at a clause start: with names or a number, the verb goes to the plural.
                    MatchCollection subjects = Regex.Matches(part, @"(?<b>^\s*|[,;:]\s*|\b(?:que|e|quando|porque|onde|mas|como)\s+)(?<s>a\s+gente|nós)\s+(?<neg>não\s+)?(?<v>\p{L}+)(?<rest>(?:\s+[\p{L}-]+){0,4})", RegexOptions.IgnoreCase);
                    for (int i = subjects.Count - 1; i >= 0; i--)
                    {
                        Match s = subjects[i];
                        string head = group, v = s.Groups["v"].Value, rest = s.Groups["rest"].Value;
                        if (plural)
                        {
                            string pl = PluralOf(v);
                            if (pl == null) head = "o grupo";
                            else
                            {
                                v = pl;
                                rest = Regex.Replace(rest, @"^(\s+)junto\b", "$1juntos");
                                if (counted) rest = Regex.Replace(rest, @"^\s+em\s+" + Regex.Escape(group.Substring(3)) + @"\b", "");
                            }
                        }
                        if (Char.IsUpper(s.Groups["s"].Value[0])) head = Capitalized(head);
                        part = part.Substring(0, s.Groups["s"].Index) + head + " " + s.Groups["neg"].Value + v + rest + part.Substring(s.Index + s.Length);
                    }
                    // Object or anything else left.
                    part = Regex.Replace(part, @"\bA\s+gente\b", Capitalized(group));
                    part = Regex.Replace(part, @"\ba\s+gente\b", group);
                    part = Regex.Replace(part, @"\bNós\b", Capitalized(group));
                    part = Regex.Replace(part, @"\bnós\b", group);
                    part = part.Replace("§de§", GroupWith("de", group)).Replace("§em§", GroupWith("em", group));
                    part = Regex.Replace(part, @"§(?<p>com|para|pra|Com|Para|Pra)§", "${p} " + group);
                }
                if (Regex.IsMatch(plain, @"\bnos\s+\p{L}+", RegexOptions.IgnoreCase))
                    part = Regex.Replace(part, @"\bnos\s+(?<v>\p{L}+(?:ou|eu|iu|ava|ia|aram|eram|iram))\b", "${v} " + group);
                if (Regex.IsMatch(plain, @"\bnoss[ao]s?\b", RegexOptions.IgnoreCase))
                    part = Regex.Replace(part, @"\bnoss[ao]s?(?=\s*[,.;!?])", GroupWith("de", group));
                // «…, eu, a Débora…»: the narrator inside an enumeration of the group.
                if (Regex.IsMatch(plain, @",\s*eu\s*(?:,|\be\b)", RegexOptions.IgnoreCase))
                    part = Regex.Replace(part, @",\s*eu(?=\s*(?:,|\be\b))", ", o depoente");
                result.Append(part); result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        // 4) Speech markers: isolated «olha», «tipo», «né», «aí», «sei lá» are dropped;
        // «tava» → «estava»; a doubt («sei lá se foi ele») stays («não sabe se foi ele»).
        public static string RepairSpeechMarkers(string source, string text)
        {
            string plain = Unquote(source);
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(text, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = text.Substring(start, quote.Index - start);
                string unchanged = part;
                if (Regex.IsMatch(plain, @"\b(?:sei\s+lá)\s+se\b", RegexOptions.IgnoreCase))
                    part = Regex.Replace(part, @"\b(?:sei|sabe)\s+lá\s+se\b", "não sabe se", RegexOptions.IgnoreCase);
                if (Regex.IsMatch(plain, @"(?:^|[,.!?]\s*)sei\s+lá\s*(?:,|\.|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    part = Regex.Replace(part, @"(?<=[,]|\bque)\s*(?:sei|sabe)\s+lá\s*,", "", RegexOptions.IgnoreCase);
                foreach (string marker in new string[] { "olha", "tipo", "né", "aí" })
                {
                    if (!Regex.IsMatch(plain, @"(?:^|[,.!?]\s*)" + marker + @"\s*(?:,|\.|!|\?|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline)) continue;
                    part = Regex.Replace(part, @"(?<=\bque|[.!?])\s*" + marker + @"\s*,", "", RegexOptions.IgnoreCase);
                    part = Regex.Replace(part, @",\s*" + marker + @"\s*(?=,)", "", RegexOptions.IgnoreCase);
                    part = Regex.Replace(part, @",\s*" + marker + @"\s*(?=[.!?]|$)", "", RegexOptions.IgnoreCase);
                }
                if (Regex.IsMatch(plain, @"\btava\b", RegexOptions.IgnoreCase)) part = Regex.Replace(part, @"\b([Tt])ava\b", "estava");
                if (Regex.IsMatch(plain, @"\btavam\b", RegexOptions.IgnoreCase)) part = Regex.Replace(part, @"\b([Tt])avam\b", "estavam");
                part = Regex.Replace(part, @"\bque\s+,\s*", "que ");
                // Capital letter again at a sentence start left by a removed marker.
                MatchCollection lowStarts = Regex.Matches(part, @"(?:^|[.!?]\s+)(?<c>[a-zà-ú])");
                for (int i = part == unchanged ? -1 : lowStarts.Count - 1; i >= 0; i--)
                {
                    Group c = lowStarts[i].Groups["c"];
                    if (c.Index == 0 && start > 0) continue;
                    part = part.Substring(0, c.Index) + Char.ToUpperInvariant(c.Value[0]) + part.Substring(c.Index + 1);
                }
                result.Append(part); result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        // Isolated speech markers («olha,», «, tipo,», «, né.», «sei lá,») removed from a
        // source sentence before the omission check; «sei lá se» (a doubt) stays.
        public static string StripSpeechMarkers(string sentence)
        {
            string s = Regex.Replace(sentence, @"(?:^|(?<=[,.!?]))\s*(?:olha|tipo|né|aí|sei\s+lá)\s*(?=[,.!?]|$)", "", RegexOptions.IgnoreCase);
            return Regex.Replace(s, @"^[\s,]+|,\s*(?=[,.!?])", "");
        }

        // 5) Names of the source in lower case («relatou que marta») and the opening
        // «Eu, Marta, sou…» → «O depoente, Marta, relatou que é …».
        public static string RepairNamesAndOpening(string source, string text)
        {
            string plain = Unquote(source);
            var names = new List<string>();
            foreach (Match n in Regex.Matches(plain, @"(?:^|[.!?]\s+)Eu,\s*(?<n>\p{Lu}\p{Ll}{2,}),|\b(?i:[oa]|com|para|pra|segundo)\s+(?<n>\p{Lu}\p{Ll}{2,})\b"))
                if (!names.Contains(n.Groups["n"].Value)) names.Add(n.Groups["n"].Value);
            foreach (string name in names)
            {
                string lower = name.ToLowerInvariant();
                if (Regex.IsMatch(plain, @"\b" + Regex.Escape(lower) + @"\b")) continue;           // also a common word in the source
                text = Regex.Replace(text, @"(?<![\p{L}""“«'])" + Regex.Escape(lower) + @"(?![\p{L}])", name);
            }
            Match intro = Regex.Match(plain, @"^\s*Eu,\s*(?<n>\p{Lu}\p{Ll}{2,}),\s*sou\b");
            if (!intro.Success) return text;
            string nm = intro.Groups["n"].Value;
            Match app = Regex.Match(text, @"^(?<ws>\s*)O depoente relatou que\s+" + Regex.Escape(nm) + @",\s*(?<app>[^,.]+?),\s*o depoente\s+(?<rest>\p{L})");
            if (app.Success)
                return app.Groups["ws"].Value + "O depoente, " + nm + ", relatou que é " + app.Groups["app"].Value + " e " + text.Substring(app.Groups["rest"].Index);
            if (!Regex.IsMatch(text, @"\b" + Regex.Escape(nm) + @"\b"))
            {
                Match lead = Regex.Match(text, @"^(?<ws>\s*)O depoente relatou que\s+");
                if (lead.Success) return lead.Groups["ws"].Value + "O depoente, " + nm + ", relatou que " + text.Substring(lead.Length);
            }
            return text;
        }

        // 6) A first-person singular verb left in the output, with the deponent as the
        // subject of its clause («a depoente nunca falou…, nem conheço») → third person.
        public static string RepairFirstPersonLeft(string source, string text)
        {
            string plain = Unquote(source);
            var candidates = new List<string>();
            foreach (Match w in Regex.Matches(plain, @"\b(?:" + FirstPersonWords() + @")\b", RegexOptions.IgnoreCase))
                if (!candidates.Contains(w.Value.ToLowerInvariant())) candidates.Add(w.Value.ToLowerInvariant());
            foreach (Match w in Regex.Matches(plain, @"\beu\s+(?:(?:não|nem|já|também|só|ainda|nunca)\s+)?(?<w>\p{L}{3,}(?:o|ei|i))\b", RegexOptions.IgnoreCase))
            {
                string v = w.Groups["w"].Value.ToLowerInvariant();
                if ((FirstToThird(v) != null || Lookup(SubjectOnlyFirstPerson(), v) != null) && !candidates.Contains(v)) candidates.Add(v);
            }
            var result = new System.Text.StringBuilder(); int start = 0;
            foreach (Match quote in Regex.Matches(text, "\"[^\"]*\"|“[^”]*”|«[^»]*»|'[^']*'|$"))
            {
                string part = text.Substring(start, quote.Index - start);
                foreach (string c in candidates)
                {
                    string third = FirstToThird(c);
                    if (third == null) third = Lookup(SubjectOnlyFirstPerson(), c);
                    if (third == null || third == c) continue;
                    MatchCollection hits = Regex.Matches(part, @"(?<![\p{L}-])" + Regex.Escape(c) + @"(?![\p{L}-])", RegexOptions.IgnoreCase);
                    for (int i = hits.Count - 1; i >= 0; i--)
                    {
                        Match h = hits[i];
                        string before = part.Substring(0, h.Index);
                        if (Regex.IsMatch(before, @"\b(?:o|a|os|as|um|uma|do|da|no|na|ao|à|pelo|pela|de|em|seu|sua|meu|minha|este|esse)\s+$", RegexOptions.IgnoreCase)) continue;
                        string sentence = before.Substring(SentenceStart(before, before.Length));
                        Match dep = Regex.Match(sentence, @"\b(?:o|a)\s+depoente\b(?<after>[^.!?]*)$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                        if (!dep.Success) continue;
                        // No other subject between the deponent and the verb.
                        string between = dep.Groups["after"].Value;
                        if (Regex.IsMatch(between, @"\b(?:ele|ela|eles|elas|que|quem|você)\b", RegexOptions.IgnoreCase) || Regex.IsMatch(between, @"(?<=[\p{Ll},;]\s)\p{Lu}\p{Ll}+")) continue;
                        part = part.Substring(0, h.Index) + MatchCase(h.Value, third) + part.Substring(h.Index + h.Length);
                    }
                }
                result.Append(part); result.Append(quote.Value); start = quote.Index + quote.Length;
            }
            return result.ToString();
        }

        // 7) The narrator rendered as «ele/ela»: «Eu devolvi…» → «ele devolveu…», «eu
        // nem conheço» → «ele nem conhece». With «eu V» in the source, a single «ele/ela
        // V-3rd» in the output and no «ele/ela V» in the source, the pronoun becomes
        // «o depoente». Otherwise nothing changes.
        public static string RepairNarratorAsPronoun(string source, string text, string fullOutput)
        {
            string plain = Unquote(source);
            var done = new List<string>();
            foreach (Match m in Regex.Matches(plain, @"\beu\s+(?:(?:não|nunca|nem|só|também|já|ainda)\s+)?(?<v>\p{L}{3,})\b", RegexOptions.IgnoreCase))
            {
                string v = m.Groups["v"].Value;
                string third = FirstToThird(v);
                if (third == null) third = Lookup(SubjectOnlyFirstPerson(), v);
                if (third == null || third == v.ToLowerInvariant() || done.Contains(third)) continue;
                done.Add(third);
                // Someone else does the same thing in the source: not clear.
                if (Regex.IsMatch(plain, @"\b(?:ele|ela|eles|elas)\s+(?:(?:não|nunca|nem|só|também|já|ainda)\s+)?" + Regex.Escape(third) + @"\b", RegexOptions.IgnoreCase)) continue;
                if (Regex.IsMatch(plain, @"\b(?:\p{Lu}\p{Ll}+|o\s+\p{Ll}+|a\s+\p{Ll}+)\s+(?:(?:não|nunca|nem|só|também|já|ainda)\s+)?" + Regex.Escape(third) + @"\b")) continue;
                string pattern = @"\b(?<p>[Ee]le|[Ee]la)(?<rest>\s+(?:(?:não|nunca|nem|só|também|já|ainda)\s+)?" + Regex.Escape(third) + @")\b";
                if (Regex.Matches(fullOutput, pattern).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern);
                if (found.Count != 1) continue;
                Match x = found[0];
                text = text.Substring(0, x.Index) + MatchCase(x.Groups["p"].Value, "o depoente") + x.Groups["rest"].Value + text.Substring(x.Index + x.Length);
            }
            return text;
        }

        // ---- Round 9 (realistic battery 2) -----------------------------------

        // Another person in the sentence besides the deponent: a pronoun, a name
        // inside the sentence, or a person noun («a mãe», «o professor»). `owned`
        // is the noun of the possessive itself, which does not count.
        public static bool OtherPersonInSentence(string sentence, string owned)
        {
            if (Regex.IsMatch(sentence, @"\b(?:ele|ela|eles|elas)\b", RegexOptions.IgnoreCase)) return true;
            foreach (Match name in Regex.Matches(sentence, @"(?<=[\p{Ll},;]\s+)\p{Lu}\p{Ll}{2,}\b"))
                if (!Regex.IsMatch(name.Value, @"^(?:Relatou|Depois|Então|Aí|Quando|Mas|Ontem|Hoje|Lá|Ali|Aqui)$")) return true;
            foreach (Match noun in Regex.Matches(sentence, @"\b(?:o|a|os|as|um|uma|do|da|dos|das|ao|à|pelo|pela|com\s+o|com\s+a)\s+(?<n>mãe|pai|irmão|irmã|professor|professora|chefe|colega|vizinho|vizinha|amigo|amiga|namorado|namorada|marido|esposa|filho|filha|aluno|aluna|rapaz|moça|homem|mulher|coordenador|coordenadora|diretor|diretora|segurança|policial|médico|médica|enfermeiro|enfermeira|menino|menina|senhor|senhora|banco)\b", RegexOptions.IgnoreCase))
                if (!noun.Groups["n"].Value.Equals(owned, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        // Present first-person forms of common pronominal verbs and their third person.
        public static string[,] PronominalPresent()
        {
            return new string[,] {
                {"lembro","lembra"}, {"recordo","recorda"}, {"sinto","sente"}, {"arrependo","arrepende"}, {"preocupo","preocupa"},
                {"sento","senta"}, {"levanto","levanta"}, {"afasto","afasta"}, {"escondo","esconde"}, {"defendo","defende"},
                {"acostumo","acostuma"}, {"queixo","queixa"}, {"chamo","chama"}, {"machuco","machuca"}, {"assusto","assusta"},
                {"aproximo","aproxima"}, {"recuso","recusa"}, {"esqueço","esquece"}, {"divirto","diverte"}, {"visto","veste"},
                {"deito","deita"}, {"irrito","irrita"}, {"envergonho","envergonha"}, {"culpo","culpa"}, {"considero","considera"},
                {"comporto","comporta"}, {"dirijo","dirige"}, {"apresento","apresenta"}, {"acalmo","acalma"}, {"nego","nega"},
                {"canso","cansa"}, {"importo","importa"}, {"incomodo","incomoda"}, {"mudo","muda"}, {"perco","perde"}
            };
        }

        // Third person of a first-person verb form, or null when the form is not first person.
        public static string FirstToThird(string verb)
        {
            string w = verb.ToLowerInvariant();
            string known = Lookup(PronominalPresent(), w);
            if (known != null) return known;
            string third = ThirdPerson(w);
            if (third != null && third != w) return third;
            return null;
        }

        // Heads that find the same verb in the output («sinto» ~ «sente», «sentia»).
        public static string[] PronominalHeads(string verb)
        {
            string head = StemHead(verb);
            string v = verb.ToLowerInvariant();
            if (v.StartsWith("sint")) return new string[] { head, "sent" };
            if (v.StartsWith("divirt")) return new string[] { head, "divert" };
            if (v.StartsWith("vist")) return new string[] { head, "vest" };
            if (v.StartsWith("dirij")) return new string[] { head, "dirig" };
            if (v.StartsWith("perc")) return new string[] { head, "perd" };
            if (v.StartsWith("esqueç")) return new string[] { head, "esquec" };
            return new string[] { head };
        }

        // «me» of a source match whose subject is the narrator: «eu me lembro», «me
        // arrependo», «não me sinto», «falei que não me sentia», «eu ia me arrepender».
        public static bool NarratorPronominal(string source, Match m)
        {
            string v = m.Groups["v"].Value, pre = m.Groups["pre"].Value.Trim().ToLowerInvariant();
            if (pre == "eu") return true;
            if (FirstToThird(v) != null) return true;                                // «me arrependo», «me machuquei»
            if (Lookup(SubjectOnlyFirstPerson(), v) != null) return true;
            // A finite third-person form («chamou», «disseram», «respeitassem») is never the narrator's.
            if (Regex.IsMatch(v, @"(?:ou|eu|iu|am|em|ão|ez|ôs)$", RegexOptions.IgnoreCase)) return false;
            // Same form for 1st and 3rd person (sentia, ia, infinitive, gerund): the
            // nearest subject before «me» in the sentence decides (the word right
            // before «me» included).
            int meIndex = m.Index + m.Groups["pre"].Length;
            int start = SentenceStart(source, meIndex);
            MatchCollection words = Regex.Matches(source.Substring(start, meIndex - start), @"[\p{L}-]+");
            for (int i = words.Count - 1; i >= 0; i--)
            {
                string w = words[i].Value, lw = w.ToLowerInvariant();
                if (lw == "eu") return true;
                if (Regex.IsMatch(lw, @"^(?:ele|ela|eles|elas|você|vocês|alguém|ninguém|todos|todas)$")) return false;
                if (i > 0 && Char.IsUpper(w[0]) && !Regex.IsMatch(w, @"^(?:Eu|Depois|Então|Aí|Quando|Mas|Ontem|Hoje)$")) return false;
                string t = FirstToThird(w);
                if (t != null) return true;                                          // «falei que não me sentia»
                if (Regex.IsMatch(lw, @"\p{L}+(?:ou|eu|iu)$") && lw.Length > 3 && lw != "eu") return false;   // a third-person preterite before
            }
            return false;
        }

        // «o depoente lhe arrependo» / «não lhe sentia» / «ia lhe arrepender» / «sentia o
        // depoente» → «se arrepende» / «não se sentia» / «ia se arrepender» / «se sentia».
        public static string RepairNarratorPronominal(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, MeObjectSource(), RegexOptions.IgnoreCase))
            {
                if (!NarratorPronominal(source, m)) continue;
                foreach (string head in PronominalHeads(m.Groups["v"].Value))
                {
                    if (head.Length < 3) continue;
                    if (Regex.Matches(source, @"\b" + Regex.Escape(head) + @"\p{L}*", RegexOptions.IgnoreCase).Count > 1) continue;
                    string w = @"(?<w>" + Regex.Escape(head) + @"\p{L}*)";
                    string pattern = @"\b(?:(?<cl>lhe|me)\s+" + w + @"|" + w.Replace("<w>", "<w2>") + @"\s+(?<obj>o depoente)|(?<subj>depoente\s+(?:(?:não|já|ainda|também|só|nunca)\s+)?)" + w.Replace("<w>", "<w3>") + @")\b";
                    MatchCollection inFull = Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase);
                    MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                    if (inFull.Count != 1 || found.Count != 1) continue;
                    Match x = found[0];
                    if (x.Groups["subj"].Success)
                    {
                        // Only a copied first-person form right after the deponent («o depoente arrependo»).
                        string bare = x.Groups["w3"].Value;
                        string t3 = FirstToThird(bare);
                        if (t3 == null) continue;
                        // «o depoente arrependo [o depoente]» → «o depoente se arrepende».
                        string tail = text.Substring(x.Groups["w3"].Index + bare.Length);
                        Match obj = Regex.Match(tail, @"^\s+o depoente\b");
                        if (obj.Success) tail = tail.Substring(obj.Length);
                        text = text.Substring(0, x.Groups["w3"].Index) + "se " + MatchCase(bare, t3) + tail;
                        break;
                    }
                    string verb = x.Groups["w"].Success ? x.Groups["w"].Value : x.Groups["w2"].Value;
                    string third = FirstToThird(verb);
                    string head2 = text.Substring(0, x.Index);
                    string se = Regex.IsMatch(head2, @"\bse\s+$", RegexOptions.IgnoreCase) ? "" : "se ";
                    text = head2 + se + (third != null ? third : verb) + text.Substring(x.Index + x.Length);
                    break;
                }
            }
            return text;
        }

        public static string NarratorPronominalIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, MeObjectSource(), RegexOptions.IgnoreCase))
            {
                if (!NarratorPronominal(source, m)) continue;
                foreach (string head in PronominalHeads(m.Groups["v"].Value))
                {
                    if (head.Length < 3) continue;
                    Match x = Regex.Match(rendered, @"\b(?:lhe|me)\s+" + Regex.Escape(head) + @"\p{L}*\b|\b" + Regex.Escape(head) + @"\p{L}*\s+o depoente\b", RegexOptions.IgnoreCase);
                    if (x.Success) return "fidelidade: verbo pronominal do depoente sem «se»: no original «" + m.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
                }
            }
            return null;
        }

        // First person left outside quotes («nem conheço», «juro»).
        public static string FirstPersonWords()
        {
            return "juro|conheço|acho|sei|prometo|garanto|confesso|admito|imagino|suponho|reconheço|lembro|recordo|sinto|arrependo|quero|posso|tenho|estou|sou|vou|faço|vejo|ouço|moro|vim|fiz|tive|estive|pude|fui";
        }

        public static string FirstPersonLeftIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            var candidates = new List<string>();
            foreach (Match w in Regex.Matches(source, @"\b(?:" + FirstPersonWords() + @")\b", RegexOptions.IgnoreCase))
                if (!candidates.Contains(w.Value.ToLowerInvariant())) candidates.Add(w.Value.ToLowerInvariant());
            foreach (Match w in Regex.Matches(source, @"\beu\s+(?:(?:não|nem|já|também|só|ainda|nunca)\s+)?(?<w>\p{L}{4,}(?:o|ei|i))\b", RegexOptions.IgnoreCase))
            {
                string v = w.Groups["w"].Value.ToLowerInvariant();
                if (FirstToThird(v) != null || Lookup(SubjectOnlyFirstPerson(), v) != null)
                    if (!candidates.Contains(v)) candidates.Add(v);
            }
            foreach (string c in candidates)
            {
                // «fui» is also third person of «ir/ser» only as «foi»; the 1st-person forms never are.
                // Not after an article or preposition: «o trabalho», «no trabalho» are nouns.
                foreach (Match x in Regex.Matches(rendered, @"(?<![\p{L}-])" + Regex.Escape(c) + @"(?![\p{L}-])", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(rendered.Substring(0, x.Index), @"\b(?:o|a|os|as|um|uma|do|da|dos|das|no|na|nos|nas|ao|à|pelo|pela|de|em|seu|sua|meu|minha|este|esse|esta|essa|aquele|aquela)\s+$", RegexOptions.IgnoreCase)) continue;
                    return "fidelidade: primeira pessoa fora das aspas: «" + x.Value + "»";
                }
            }
            return null;
        }

        // «disse pra ela», «tinham ligado pra ela»: the other person as recipient must
        // not be lost («o banco disse que…», «tinham ligado do celular…»).
        public static string LostRecipientIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, @"\b(?<v>\p{L}{3,})\s+(?:pra|para)\s+(?<p>ele|ela|eles|elas)\b(?!\s+(?:\p{L}+r|ir|ficar|fazer|ser|ter|estar)\b)", RegexOptions.IgnoreCase))
            {
                string v = m.Groups["v"].Value;
                if (!RecipientVerb(v) && !Regex.IsMatch(v, @"^(?:ligad|liga|ligou|ligaram|telefon)", RegexOptions.IgnoreCase)) continue;
                int sourceCount = Regex.Matches(source, @"\b" + Regex.Escape(v) + @"\s+(?:pra|para)\s+(?:ele|ela|eles|elas)\b", RegexOptions.IgnoreCase).Count;
                MatchCollection outs = Regex.Matches(rendered, @"(?<pre>\b\p{L}+\s+)?\b" + Regex.Escape(v) + @"\b(?<post>(?:\s+[\p{L}-]+){0,3})", RegexOptions.IgnoreCase);
                if (outs.Count == 0) continue;                       // paraphrased verb: not this check
                int marked = 0;
                foreach (Match o in outs)
                    if (Regex.IsMatch(o.Groups["pre"].Value, @"^(?:lhe|lhes)\s+$", RegexOptions.IgnoreCase)
                        || Regex.IsMatch(o.Groups["post"].Value, @"^\s+(?:(?:\p{L}+\s+){0,2})?(?:pra|para|a|à|ao)\s+(?:ele|ela|eles|elas|a\s+\p{L}+|o\s+\p{L}+|\p{Lu}\p{Ll}+)", RegexOptions.IgnoreCase)
                        || Regex.IsMatch(o.Value, @"-lhes?\b", RegexOptions.IgnoreCase)) marked++;
                if (marked < sourceCount) return "fidelidade: destinatário perdido: no original «" + m.Value.Trim() + "»";
            }
            return null;
        }

        // Round 9b: give back the recipient «pra/para ele/ela» when the source frame is
        // unique and the output has exactly one form of the verb without a recipient
        // («tinham ligado do celular» → «tinham ligado para ela do celular»).
        public static string RepairLostRecipient(string source, string text, string fullOutput)
        {
            string plain = Unquote(source);
            foreach (Match m in Regex.Matches(plain, @"\b(?<v>\p{L}{3,})\s+(?:pra|para)\s+(?<p>ele|ela|eles|elas)\b(?!\s+(?:\p{L}+r|ir|ficar|fazer|ser|ter|estar)\b)", RegexOptions.IgnoreCase))
            {
                string v = m.Groups["v"].Value, p = m.Groups["p"].Value.ToLowerInvariant();
                if (!RecipientVerb(v) && !Regex.IsMatch(v, @"^(?:ligad|liga|ligou|ligaram|telefon)", RegexOptions.IgnoreCase)) continue;
                if (Regex.Matches(plain, @"\b" + Regex.Escape(v) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                string form = @"(?<pre>\b\p{L}+\s+)?\b(?<v>" + Regex.Escape(v) + @")\b(?<post>(?:\s+[\p{L}-]+){0,3})";
                MatchCollection all = Regex.Matches(fullOutput, form, RegexOptions.IgnoreCase);
                MatchCollection found = Regex.Matches(text, form, RegexOptions.IgnoreCase);
                if (all.Count != 1 || found.Count != 1) continue;
                Match x = found[0];
                bool marked = Regex.IsMatch(x.Groups["pre"].Value, @"^(?:lhe|lhes)\s+$", RegexOptions.IgnoreCase)
                    || Regex.IsMatch(x.Groups["post"].Value, @"^\s+(?:(?:\p{L}+\s+){0,2})?(?:pra|para|a|à|ao)\s+(?:ele|ela|eles|elas|a\s+\p{L}+|o\s+\p{L}+|\p{Lu}\p{Ll}+)", RegexOptions.IgnoreCase)
                    || Regex.IsMatch(x.Value, @"-lhes?\b", RegexOptions.IgnoreCase);
                if (marked) continue;
                Group g = x.Groups["v"];
                text = text.Substring(0, g.Index + g.Length) + " para " + p + text.Substring(g.Index + g.Length);
            }
            return text;
        }

        // Round 9b: «juro» isolated between commas («não sabia, juro.») is the speaker's
        // oath in the first person: removed when the source has it isolated too.
        public static string RepairIsolatedOath(string source, string text)
        {
            if (!Regex.IsMatch(Unquote(source), @",\s*juro\s*(?=[.,;!?])", RegexOptions.IgnoreCase)) return text;
            // «jura»: the model sometimes conjugates the oath; it is still the speaker's oath.
            return Regex.Replace(text, @"\s*,\s*jur[oa]\s*(?=[.,;!?])", "", RegexOptions.IgnoreCase);
        }

        // ---- The narrator as object (round 8, realistic battery) -------------
        // Every «me»/«meu»/«minha» of the source needs a narrator reference in the
        // output. Repaired from the source with a single correspondence; otherwise
        // Validate reports it (retry, then rejection with the incomplete mark).

        // Verbs whose narrator object is indirect («lhe disse», «lhe pediu», «lhe deu»).
        public static bool RecipientVerb(string verb)
        {
            string v = verb.ToLowerInvariant();
            if (Regex.IsMatch(v, @"^(?:deu|dá|dar|dava|dando|dado|dera|daria|deram)$")) return true;
            return Regex.IsMatch(v, @"^(?:dis|diz|dit|cont|fal|pergunt|ped|mand|avis|inform|explic|entreg|mostr|emprest|ofere|respond|telefon|lig|escrev|envi|pass|jur|garant|promet|confess|ensin|devolv|vend|pag|sugeri|suger|recomend|comunic|repass|agradec|desej|permit|proib|orden|neg|apresent|indic|fornec|revel|anunci)");
        }

        // Either regency depending on the meaning («me levou até lá» / «me levou um café»):
        // never changed between «o» and «lhe».
        public static bool EitherRegencyVerb(string verb)
        {
            return Regex.IsMatch(verb.ToLowerInvariant(), @"^(?:lev|trouxe|traz|trag|cobr|jog|atir|serv|atend|pux)");
        }

        // «disse», «contou», «falou»…: paraphrased freely; checked by the recipient rules.
        public static bool SayVerb(string verb)
        {
            return Regex.IsMatch(verb.ToLowerInvariant(), @"^(?:disse|diss|dis|diz|dit|cont|fal|inform|explic|relat|avis|respond|coment|garant|confirm|repet|jur)");
        }

        public static string StemHead(string verb)
        {
            string stem = VerbStem(verb);
            return stem.Length >= 4 ? stem.Substring(0, 4) : stem;
        }

        // Source «me V» where the narrator is the object (not «eu me V», not «me machuquei»).
        public static string MeObjectSource()
        {
            return @"(?<pre>\b\p{L}+\s+)?\bme\s+(?<v>\p{L}{3,})";
        }

        public static bool NarratorAsObject(string source, Match m)
        {
            // Round 9: the narrator's pronominal verbs («me arrependo», «não me sinto», «falei
            // que não me sentia», «eu ia me arrepender») are not the narrator as object.
            return !NarratorPronominal(source, m);
        }

        // A narrator reference right at this word of the output: «lhe V», «o V», «V-o»,
        // «V o depoente», «V ao depoente», «o depoente foi V-ado».
        public static bool NarratorMarked(string text, int index, int length)
        {
            string before = text.Substring(0, index), word = text.Substring(index, length);
            string after = text.Substring(index + length);
            if (Regex.IsMatch(before, @"\b(?:lhe|o|a|me|os|as)\s+(?:(?:não|já|também|ainda|só)\s+)?$", RegexOptions.IgnoreCase)) return true;
            if (Regex.IsMatch(after, @"^-(?:o|a|lhe)\b", RegexOptions.IgnoreCase)) return true;
            if (Regex.IsMatch(after, @"^(?:\s+[\p{L}-]+){0,5}?\s+(?:o|a|ao|à|do|da|no|na|pelo|pela|com\s+o|com\s+a|para\s+o|para\s+a|pro|pra)\s+depoente\b", RegexOptions.IgnoreCase))
            {
                // The deponent must not be the subject of a new clause in between.
                Match m = Regex.Match(after, @"^(?<mid>(?:\s+[\p{L}-]+){0,5}?)\s+(?:o|a|ao|à|do|da|no|na|pelo|pela|com\s+o|com\s+a|para\s+o|para\s+a|pro|pra)\s+depoente\b", RegexOptions.IgnoreCase);
                if (!Regex.IsMatch(m.Groups["mid"].Value, @"\b(?:e|que|quando|mas|porque|enquanto|onde)\b", RegexOptions.IgnoreCase)) return true;
            }
            // Passive with the narrator as subject: «o depoente foi empurrado», «Relatou que
            // foi cercado» (the opening's subject is the narrator).
            if (Regex.IsMatch(word, @"(?:ad[oa]s?|id[oa]s?)$", RegexOptions.IgnoreCase)
                && Regex.IsMatch(before, @"(?:\bdepoente|^\s*[Rr]elatou\s+que)(?:\s+\p{L}+){0,3}?\s+(?:não\s+)?(?:foi|era|ficou|estava|acabou|tinha\s+sido|havia\s+sido)\s+$", RegexOptions.IgnoreCase)) return true;
            return false;
        }

        // 1) Object dropped («e me ameaçou» → «e ameaçou»), turned into «se» («veio me
        // xingando» → «veio se xingando») or given as «lhe» with a direct-object verb
        // («lhe chamou», «ia lhe reprovar»), and «o» with a recipient verb («o pediu»).
        public static string RepairNarratorObject(string source, string text, string fullOutput)
        {
            foreach (Match m in Regex.Matches(source, MeObjectSource(), RegexOptions.IgnoreCase))
            {
                if (!NarratorAsObject(source, m)) continue;
                string v = m.Groups["v"].Value, head = StemHead(v);
                if (head.Length < 3 || SayVerb(v)) continue;
                // One source use of this verb, one output form of it.
                if (Regex.Matches(source, @"\b" + Regex.Escape(head) + @"\p{L}*", RegexOptions.IgnoreCase).Count != 1) continue;
                string form = @"\b(?<w>" + Regex.Escape(head) + @"\p{L}*)\b";
                if (Regex.Matches(fullOutput, form, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, form, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Group w = found[0].Groups["w"];
                string before = text.Substring(0, w.Index);
                if (EitherRegencyVerb(w.Value)) continue;
                bool recipient = RecipientVerb(w.Value);
                // «lhe chamou» → «chamou o depoente» (direct-object verb).
                Match lhe = Regex.Match(before, @"\blhe\s+$", RegexOptions.IgnoreCase);
                if (lhe.Success && !recipient && !Regex.IsMatch(before, @"\bdepoente\s+(?:\p{L}+\s+)?lhe\s+$", RegexOptions.IgnoreCase))
                {
                    // «e lhe atacou» right after a link word: the source's subject was
                    // deleted too («e ele me atacou»); give it back with the object.
                    string subject = "";
                    string pre = m.Groups["pre"].Value.Trim();
                    if (Regex.IsMatch(before, @"\b(?:e|que|mas|quando|porque)\s+lhe\s+$") && Regex.IsMatch(pre, @"^(?:ele|ela|eles|elas)$", RegexOptions.IgnoreCase))
                        subject = pre.ToLowerInvariant() + " ";
                    text = text.Substring(0, lhe.Index) + subject + w.Value + " o depoente" + text.Substring(w.Index + w.Length);
                    continue;
                }
                // «o pediu» → «lhe pediu» (recipient verb, finite form).
                Match o = Regex.Match(before, @"\b(?:o|a)\s+$");
                if (o.Success && recipient && Regex.IsMatch(w.Value, @"(?:ou|eu|iu|ia|ava|isse|aram|eram|iram|avam|iam)$", RegexOptions.IgnoreCase))
                {
                    text = text.Substring(0, o.Index) + "lhe " + text.Substring(w.Index);
                    continue;
                }
                // «veio se xingando» → «veio xingando o depoente» (another person's action).
                Match se = Regex.Match(before, @"\bse\s+$", RegexOptions.IgnoreCase);
                if (se.Success && !Regex.IsMatch(source, @"\bse\s+" + Regex.Escape(head), RegexOptions.IgnoreCase) && !NarratorReflexive(source, v)
                    && !Regex.IsMatch(before.Substring(SentenceStart(before, before.Length)), @"\bo depoente\s+(?:(?:não|já|também|ainda|só)\s+)?se\s+$", RegexOptions.IgnoreCase))
                {
                    string fixedVerb = recipient ? "lhe " + w.Value : w.Value + " o depoente";
                    text = text.Substring(0, se.Index) + fixedVerb + text.Substring(w.Index + w.Length);
                    continue;
                }
                if (NarratorMarked(text, w.Index, w.Length)) continue;
                // Dropped object of a finite verb: «e ameaçou.» → «e ameaçou o depoente.»;
                // «perguntou as horas» → «lhe perguntou as horas».
                if (!Regex.IsMatch(w.Value, @"(?:ou|eu|iu)$", RegexOptions.IgnoreCase)) continue;
                if (Regex.IsMatch(before, @"\bdepoente\s+(?:(?:não|já|também|ainda|só)\s+)?$", RegexOptions.IgnoreCase)) continue;   // the deponent is the subject: not this case
                if (recipient) text = text.Substring(0, w.Index) + "lhe " + text.Substring(w.Index);
                else text = text.Substring(0, w.Index + w.Length) + " o depoente" + text.Substring(w.Index + w.Length);
            }
            return text;
        }

        public static string NarratorObjectIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match m in Regex.Matches(source, MeObjectSource(), RegexOptions.IgnoreCase))
            {
                if (!NarratorAsObject(source, m)) continue;
                string v = m.Groups["v"].Value, head = StemHead(v);
                if (head.Length < 3 || SayVerb(v)) continue;
                MatchCollection forms = Regex.Matches(rendered, @"\b(?<w>" + Regex.Escape(head) + @"\p{L}*)\b", RegexOptions.IgnoreCase);
                bool marked = false;
                foreach (Match f in forms)
                {
                    Group w = f.Groups["w"];
                    string before = rendered.Substring(0, w.Index);
                    if (Regex.IsMatch(before, @"\bse\s+$", RegexOptions.IgnoreCase) && !NarratorReflexive(source, v)) continue;
                    if (Regex.IsMatch(before, @"\blhe\s+$", RegexOptions.IgnoreCase) && !RecipientVerb(w.Value)) continue;
                    if (NarratorMarked(rendered, w.Index, w.Length)) { marked = true; break; }
                }
                if (!marked) return "fidelidade: depoente omitido ou trocado como objeto: no original «" + m.Value.Trim() + "»" + (forms.Count == 0 ? ", ação ausente na saída" : "");
            }
            return null;
        }

        // 2) Narrator possessive dropped: «Meu orientador» → «o orientador» becomes
        // «o orientador do depoente»; «puxou da minha mão» → «puxou a mão do depoente»
        // gets its preposition back («puxou da mão do depoente»).
        public static string RepairNarratorOwner(string source, string text, string fullOutput)
        {
            foreach (Match mine in Regex.Matches(source, @"\b(?<p>[Mm]eu|[Mm]inha|[Mm]eus|[Mm]inhas)\s+(?<n>\p{L}{3,})\b"))
            {
                string n = mine.Groups["n"].Value;
                // One narrator «meu N» and no «o N» of someone else in the source.
                if (Regex.Matches(source, @"\b(?:meu|minha|meus|minhas)\s+" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase).Count != 1) continue;
                if (Regex.IsMatch(source, @"\b(?:o|a|os|as|seu|sua|seus|suas|dele|dela)\s+" + Regex.Escape(n) + @"\b", RegexOptions.IgnoreCase)) continue;
                if (Regex.IsMatch(fullOutput, @"\b(?:seu|sua|seus|suas)\s+" + Regex.Escape(n) + @"\b|\b" + Regex.Escape(n) + @"\s+d[oa]\s+depoente\b|\b" + Regex.Escape(n) + @"\s+(?:dele|dela)\b", RegexOptions.IgnoreCase)) continue;
                string pattern = @"\b(?<art>[Oo]|[Aa]|[Oo]s|[Aa]s)\s+(?<n>" + Regex.Escape(n) + @")\b(?!\s+(?:d[oa]s?|de)\s)";
                if (Regex.Matches(fullOutput, pattern).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern);
                if (found.Count != 1) continue;
                Match x = found[0];
                text = text.Substring(0, x.Index) + x.Value + " do depoente" + text.Substring(x.Index + x.Length);
            }
            foreach (Match from in Regex.Matches(source, @"\b(?<v>\p{L}{3,})\s+(?<p>da|do|das|dos)\s+(?:minha|meu|minhas|meus)\s+(?<n>\p{L}{3,})\b", RegexOptions.IgnoreCase))
            {
                string head = StemHead(from.Groups["v"].Value);
                string pattern = @"\b(?<v>" + Regex.Escape(head) + @"\p{L}*)\s+(?<art>a|o|as|os)\s+(?<n>" + Regex.Escape(from.Groups["n"].Value) + @")\s+(?<own>d[oa]\s+depoente)\b";
                if (Regex.Matches(fullOutput, pattern, RegexOptions.IgnoreCase).Count != 1) continue;
                MatchCollection found = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (found.Count != 1) continue;
                Match x = found[0];
                string art = x.Groups["art"].Value.ToLowerInvariant();
                string prep = art == "a" ? "da" : art == "o" ? "do" : art == "as" ? "das" : "dos";
                text = text.Substring(0, x.Groups["art"].Index) + prep + text.Substring(x.Groups["art"].Index + x.Groups["art"].Length);
            }
            return text;
        }

        public static string NarratorOwnerIssue(string original, string output)
        {
            string source = Unquote(original), rendered = Unquote(output);
            foreach (Match from in Regex.Matches(source, @"\b(?<v>\p{L}{3,})\s+(?<p>da|do|das|dos)\s+(?:minha|meu|minhas|meus)\s+(?<n>\p{L}{3,})\b", RegexOptions.IgnoreCase))
            {
                Match x = Regex.Match(rendered, @"\b" + Regex.Escape(StemHead(from.Groups["v"].Value)) + @"\p{L}*\s+(?:a|o|as|os)\s+" + Regex.Escape(from.Groups["n"].Value) + @"\s+d[oa]\s+depoente\b", RegexOptions.IgnoreCase);
                if (x.Success) return "fidelidade: papel do depoente trocado: no original «" + from.Value.Trim() + "», na saída «" + x.Value.Trim() + "»";
            }
            return null;
        }

        // 3) «cento e cinquenta» never becomes «cem e cinquenta» («cem e …» is not a number).
        public static string RepairNumberWords(string source, string text)
        {
            foreach (Match m in Regex.Matches(text, @"\b(?<c>[Cc])em\s+e\s+(?<n>\p{L}+)\b"))
            {
                if (!Regex.IsMatch(source, @"\bcento\s+e\s+" + Regex.Escape(m.Groups["n"].Value) + @"\b", RegexOptions.IgnoreCase)) continue;
                text = Regex.Replace(text, @"\b" + Regex.Escape(m.Value) + @"\b", m.Groups["c"].Value + "ento e " + m.Groups["n"].Value);
            }
            return text;
        }

        // 4) Broken by repetition: «foi o depoente que o depoente achou» → «foi o depoente
        // que achou». Only this exact pattern «foi X que X <verbo>»; any other
        // repetition is reported by RepeatedNarratorIssue.
        public static string RepairRepeatedSubject(string text)
        {
            return Regex.Replace(text, @"\b(?<head>[Ff]oi\s+(?<x>(?:o|a)\s+\p{L}+)\s+que)\s+\k<x>\s+(?<v>\p{L}+)\b", "${head} ${v}");
        }

        public static string RepeatedNarratorIssue(string output)
        {
            Match x = Regex.Match(Unquote(output), @"\b[Oo] depoente\s+(?:(?:que|quem)\s+)?o depoente\b");
            return x.Success ? "fidelidade: frase quebrada por repetição: «" + x.Value + "»" : null;
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
            string step = text;
            text = RepairNarratorVerbs(source, text);
            NoteRepair("verbos-do-depoente", step, text);
            step = text;
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
            NoteRepair("ancoras-do-original", step, text);
            step = text; text = RepairEllipticalNarrator(source, text, fullOutput, forms); NoteRepair("sujeito-eliptico", step, text);
            step = text; text = RepairNarratorPronoun(source, text, fullOutput, forms); NoteRepair("pronome-do-depoente", step, text);
            // Round 7: roles (after the narrator pronoun, so «ele era culpado» is
            // already «o depoente era culpado» when the deleted subject is restored).
            step = text; text = RepairGerundObject(source, text, fullOutput); NoteRepair("gerundio-objeto", step, text);
            step = text; text = RepairSwappedRelative(source, text, fullOutput); NoteRepair("papeis-trocados", step, text);
            step = text; text = RepairDeletedSubject(source, text, fullOutput); NoteRepair("sujeito-apagado", step, text);
            step = text; text = RepairAmbiguousObject(source, text, fullOutput); NoteRepair("voz-passiva", step, text);
            step = text; text = RepairWholeSentenceNarrator(source, text, fullOutput, forms); NoteRepair("frase-do-depoente", step, text);
            step = text; text = RepairNarratorPossessive(source, text); NoteRepair("possessivo", step, text);
            step = text; text = RepairRecipients(source, text, fullOutput); NoteRepair("destinatario", step, text);
            step = text; text = RepairParallelCorrections(source, text); NoteRepair("autocorrecao", step, text);
            // Round 8: the narrator as object and owner; number words. After the older
            // possessive and recipient repairs, which rely on the sentence before them.
            step = text; text = RepairNarratorPronominal(source, text, fullOutput); NoteRepair("pronominal-do-depoente", step, text);
            step = text; text = RepairNarratorObject(source, text, fullOutput); NoteRepair("depoente-objeto", step, text);
            step = text; text = RepairNarratorOwner(source, text, fullOutput); NoteRepair("possessivo-do-depoente", step, text);
            step = text; text = RepairNumberWords(source, text); NoteRepair("valores", step, text);
            step = text; text = RepairRepeatedSubject(text); NoteRepair("repeticao", step, text);
            step = text; text = RepairLostRecipient(source, text, fullOutput); NoteRepair("destinatario-devolvido", step, text);
            step = text; text = RepairIsolatedOath(source, text); NoteRepair("juro-isolado", step, text);
            // Round 10: other first-person forms, approximation, plural, speech markers, first person left.
            step = text; text = RepairOtherFirstPerson(source, text, fullOutput); NoteRepair("primeira-pessoa-outras-formas", step, text);
            step = text; text = RepairLostApproximation(source, text, fullOutput); NoteRepair("aproximacao", step, text);
            step = text; text = RepairFirstPersonPlural(source, text); NoteRepair("plural-do-grupo", step, text);
            step = text; text = RepairSpeechMarkers(source, text); NoteRepair("marcadores-da-fala", step, text);
            step = text; text = RepairFirstPersonLeft(source, text); NoteRepair("primeira-pessoa-restante", step, text);
            step = text; text = RepairNarratorAsPronoun(source, text, fullOutput); NoteRepair("depoente-como-ele", step, text);
            step = text;
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
            NoteRepair("contracoes", step, text);
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
            string done = result.ToString();
            NoteRepair("primeira-pessoa", text, done);
            return done;
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
            // Round 8: the narrator as object or owner; broken by repetition.
            if (role == null) role = RepeatedNarratorIssue(output);
            if (role == null) role = NarratorObjectIssue(original, output);
            if (role == null) role = NarratorOwnerIssue(original, output);
            // Round 9: pronominal verbs of the narrator, first person left, lost recipient.
            if (role == null) role = NarratorPronominalIssue(original, output);
            if (role == null) role = FirstPersonLeftIssue(original, output);
            if (role == null) role = LostRecipientIssue(original, output);
            // Round 10: other first-person forms turned into «ele/ela»; approximation made exact.
            if (role == null) role = OtherFirstPersonIssue(original, output);
            if (role == null) role = LostApproximationIssue(original, output);
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
                // Round 10: isolated speech markers are not content to keep.
                var anchors = Anchors(StripSpeechMarkers(sentence));
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
