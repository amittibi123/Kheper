using System.Globalization;
using System.Text.RegularExpressions;

namespace Kheper.Shared.Models
{
    public class ExtractedTask
    {
        public string? Description { get; set; }
        public DateTime? TaskTime { get; set; }
        public string? Sender { get; set; } // שם השולח אם זוהה
    }

    public class TaskExtractor
    {
        // ── Regexes ──────────────────────────────────────────────────────────
        private static readonly Regex DateRegex =
            new(@"\b([0-2]?[0-9]|3[01])/(0?[1-9]|1[0-2])(/(\d{4}|\d{2}))?\b");

        private static readonly Regex Time24Regex =
            new(@"\b([01]?[0-9]|2[0-3]):[0-5][0-9]\b");

        // זיהוי זמן יחסי: "in 2 hours", "in 30 minutes", "tomorrow", "next monday" וכו'
        private static readonly Regex RelativeTimeRegex =
            new(@"\b(in\s+(\d+)\s+(hour|hours|minute|minutes|min|mins|day|days))\b"
              + @"|\b(tomorrow(\s+morning|\s+afternoon|\s+evening|\s+night)?)\b"
              + @"|\b(next\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday|week|month))\b"
              + @"|\b(this\s+(morning|afternoon|evening|night|weekend))\b"
              + @"|\b(tonight)\b",
                RegexOptions.IgnoreCase);

        // חילוץ אימייל + שם מתוכו
        private static readonly Regex EmailRegex =
            new(@"<([^@\s]+)@([^>\s]+)>|([^@\s]+)@([^\s]+)");

        // ── Splitters ────────────────────────────────────────────────────────
        private static readonly string[] Splitters =
        {
            " and also ", " and ", " then ", " also ", " plus ",
            " after that ", " afterward ", " followed by ", " next ",
            " afterwards ", " as well as ", " along with ", " in addition ",
            " together with ", " moreover ", " subsequently ",
            " concurrently ", " meanwhile ", " plus also ", " then also "
        };

        // ── Verbs (ללא כפילויות) ─────────────────────────────────────────────
        private static readonly HashSet<string> Verbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "be","have","do","say","go","get","make","know","think","take",
            "see","come","want","use","find","give","tell","work","call","try",
            "ask","need","feel","become","leave","put","mean","keep","let","begin",
            "seem","help","talk","turn","start","show","hear","play","run","move",
            "like","live","believe","hold","bring","happen","write","provide","sit","stand",
            "lose","pay","meet","include","continue","set","learn","change","lead","understand",
            "watch","follow","stop","create","speak","read","allow","add","spend","grow",
            "open","walk","win","offer","remember","love","consider","appear","buy","wait",
            "serve","die","send","expect","build","stay","fall","cut","reach","kill",
            "remain","fight","forget","catch","hurt","rise","lie","point","pick","look",
            "beat","carry","choose","drive","eat","fly","hang","hit","lay","lend",
            "light","ride","ring","sell","shake","shine","shoot","shut","sing","sleep",
            "smell","steal","swim","teach","tear","throw","wake","wear","clean",
            "cook","wash","listen","count","travel","visit","exercise","jump","park",
            "study","organize","plan","draw","paint","design","fix","repair",
            "check","review","update","delete","install","deliver","schedule","arrange","report",
            "share","post","email","upload","download","test","close","replace","assemble",
            "measure","fold","pack","text","message","notify","sign","submit","approve",
            "reserve","book","charge","withdraw","deposit","invest","research","explore",
            "dance","laugh","cry","smile","hug","kiss","hate","save","break"
        };

        // ── מילות תפל שלא צריכות להיות בתיאור (רק אם הן לפני/אחרי זמן) ────
        private static readonly Regex TrailingTimePrepositions =
            new(@"\s*\b(on|at|by|from)\b\s*$", RegexOptions.IgnoreCase);

        // ── Public API (לא השתנה!) ────────────────────────────────────────────
        public List<ExtractedTask> ExtractTasks(string input)
        {
            var results = new List<ExtractedTask>();

            // חילוץ שם שולח מאימייל אם קיים
            string? senderName = ExtractSenderName(ref input);

            string cleanInput = input.Replace("?", "").Replace("!", "");
            var parts = SplitInput(cleanInput);

            foreach (var part in parts)
            {
                string lowerPart = part.ToLower();
                int verbIdx = FindEarliestVerbIndex(lowerPart);
                if (verbIdx == -1) continue;

                string fullTaskText = part.Substring(verbIdx).Trim();

                // ── חילוץ תאריך/שעה מוחלטים ──
                var timeMatch = Time24Regex.Match(fullTaskText);
                var dateMatch = DateRegex.Match(fullTaskText);

                DateTime baseDate = DateTime.Today;
                bool dateFound = false;

                if (dateMatch.Success)
                {
                    string[] fmts = { "d/M/yyyy","dd/MM/yyyy","d/M/yy","dd/MM/yy","d/M","dd/MM" };
                    if (DateTime.TryParseExact(dateMatch.Value, fmts,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime pd))
                    {
                        baseDate = pd;
                        dateFound = true;
                    }
                }

                // ── חילוץ זמן יחסי ──
                DateTime? relativeDateTime = ParseRelativeTime(fullTaskText, DateTime.Now);

                DateTime? finalDateTime = null;
                if (timeMatch.Success)
                {
                    if (DateTime.TryParseExact(timeMatch.Value, new[] { "H:mm","HH:mm" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime t))
                        finalDateTime = baseDate.Date.Add(t.TimeOfDay);
                }
                else if (relativeDateTime.HasValue)
                {
                    finalDateTime = relativeDateTime;
                }
                else if (dateFound)
                {
                    finalDateTime = baseDate.Date.AddHours(9);
                }

                // ── ניקוי תיאור חכם ──
                string desc = fullTaskText;

                if (dateMatch.Success)
                    desc = DateRegex.Replace(desc, "");
                if (timeMatch.Success)
                    desc = Time24Regex.Replace(desc, "");

                // הסרת ביטויי זמן יחסי מהתיאור
                desc = RelativeTimeRegex.Replace(desc, "");

                // הסרת מילות יחס רק כשהן בסוף המשפט (נשארו "יתומות")
                desc = TrailingTimePrepositions.Replace(desc, "");

                // הסרת / ו-: שנשארו
                desc = Regex.Replace(desc, @"[:\-/]", " ");

                // ניקוי רווחים
                desc = Regex.Replace(desc, @"\s+", " ").Trim();

                // הוספת שם שולח לתיאור אם זוהה
                if (!string.IsNullOrEmpty(senderName))
                    desc = $"מ{senderName} – {desc}";

                results.Add(new ExtractedTask
                {
                    Description = desc,
                    TaskTime    = finalDateTime,
                    Sender      = senderName
                });
            }

            return results;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// מחלץ שם שולח מתוך פורמט אימייל כמו &lt;amittibi123@gmail.com&gt;
        /// ומנקה את האימייל מה-input.
        /// </summary>
        private static string? ExtractSenderName(ref string input)
        {
            var m = EmailRegex.Match(input);
            if (!m.Success) return null;

            // קח את החלק לפני ה-@
            string localPart = m.Groups[1].Success ? m.Groups[1].Value
                             : m.Groups[3].Success ? m.Groups[3].Value
                             : "";

            // נקה מספרים וסימנים -> "amittibi123" => "amittibi"
            string nameOnly = Regex.Replace(localPart, @"[\d._\-+]", " ").Trim();
            nameOnly = Regex.Replace(nameOnly, @"\s+", " ");

            // הפוך ל-Title Case
            if (!string.IsNullOrWhiteSpace(nameOnly))
            {
                nameOnly = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(nameOnly.ToLower());
            }

            // הסר את האימייל מה-input
            input = input.Remove(m.Index, m.Length).Trim();

            return string.IsNullOrWhiteSpace(nameOnly) ? null : nameOnly;
        }

        /// <summary>
        /// מנתח ביטויי זמן יחסיים ומחזיר DateTime מוחלט.
        /// </summary>
        private static DateTime? ParseRelativeTime(string text, DateTime now)
        {
            // "in X hours/minutes/days"
            var inMatch = Regex.Match(text,
                @"\bin\s+(\d+)\s+(hour|hours|minute|minutes|min|mins|day|days)\b",
                RegexOptions.IgnoreCase);
            if (inMatch.Success)
            {
                int amount = int.Parse(inMatch.Groups[1].Value);
                string unit = inMatch.Groups[2].Value.ToLower();
                return unit.StartsWith("h") ? now.AddHours(amount)
                     : unit.StartsWith("d") ? now.AddDays(amount)
                     : now.AddMinutes(amount);
            }

            // "tomorrow [morning/afternoon/evening/night]"
            var tomMatch = Regex.Match(text,
                @"\btomorrow(\s+(morning|afternoon|evening|night))?\b",
                RegexOptions.IgnoreCase);
            if (tomMatch.Success)
            {
                var part2 = tomMatch.Groups[2].Value.ToLower();
                DateTime tomorrow = now.Date.AddDays(1);
                return part2 switch
                {
                    "morning"   => tomorrow.AddHours(8),
                    "afternoon" => tomorrow.AddHours(13),
                    "evening"   => tomorrow.AddHours(18),
                    "night"     => tomorrow.AddHours(21),
                    _           => tomorrow.AddHours(9)
                };
            }

            // "tonight"
            if (Regex.IsMatch(text, @"\btonight\b", RegexOptions.IgnoreCase))
                return now.Date.AddHours(21);

            // "this morning/afternoon/evening/night/weekend"
            var thisMatch = Regex.Match(text,
                @"\bthis\s+(morning|afternoon|evening|night|weekend)\b",
                RegexOptions.IgnoreCase);
            if (thisMatch.Success)
            {
                return thisMatch.Groups[1].Value.ToLower() switch
                {
                    "morning"   => now.Date.AddHours(8),
                    "afternoon" => now.Date.AddHours(13),
                    "evening"   => now.Date.AddHours(18),
                    "night"     => now.Date.AddHours(21),
                    "weekend"   => GetNextWeekend(now),
                    _           => null
                };
            }

            // "next monday/tuesday/..." or "next week/month"
            var nextMatch = Regex.Match(text,
                @"\bnext\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday|week|month)\b",
                RegexOptions.IgnoreCase);
            if (nextMatch.Success)
            {
                string target = nextMatch.Groups[1].Value.ToLower();
                return target switch
                {
                    "week"  => now.Date.AddDays(7).AddHours(9),
                    "month" => now.Date.AddMonths(1).AddHours(9),
                    _       => GetNextWeekday(now, target)
                };
            }

            return null;
        }

        private static DateTime GetNextWeekday(DateTime from, string dayName)
        {
            var target = dayName switch
            {
                "monday"    => DayOfWeek.Monday,
                "tuesday"   => DayOfWeek.Tuesday,
                "wednesday" => DayOfWeek.Wednesday,
                "thursday"  => DayOfWeek.Thursday,
                "friday"    => DayOfWeek.Friday,
                "saturday"  => DayOfWeek.Saturday,
                "sunday"    => DayOfWeek.Sunday,
                _           => from.DayOfWeek
            };
            int diff = ((int)target - (int)from.DayOfWeek + 7) % 7;
            if (diff == 0) diff = 7;
            return from.Date.AddDays(diff).AddHours(9);
        }

        private static DateTime GetNextWeekend(DateTime from)
        {
            int diff = ((int)DayOfWeek.Saturday - (int)from.DayOfWeek + 7) % 7;
            if (diff == 0) diff = 7;
            return from.Date.AddDays(diff).AddHours(10);
        }

        /// <summary>
        /// מוצא את האינדקס של הפועל הראשון במשפט – חיפוש מילה שלמה בלבד.
        /// </summary>
        private static int FindEarliestVerbIndex(string lowerPart)
        {
            int earliest = -1;
            foreach (var verb in Verbs)
            {
                // \b מבטיח חיפוש מילה שלמה – "be" לא ימצא בתוך "label"
                var m = Regex.Match(lowerPart, $@"\b{Regex.Escape(verb)}\b");
                if (m.Success && (earliest == -1 || m.Index < earliest))
                    earliest = m.Index;
            }
            return earliest;
        }

        private static List<string> SplitInput(string input)
        {
            var list = new List<string> { input };
            foreach (var splitter in Splitters)
            {
                var next = new List<string>();
                foreach (var part in list)
                    next.AddRange(part.Contains(splitter)
                        ? part.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries)
                        : new[] { part });
                list = next;
            }
            return list;
        }
    }
}