using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Models
{
    public class ExtractedTask
    {
        public string? Description { get; set; }
        public DateTime? TaskTime { get; set; } // Nullable in case no time is mentioned
    }

    public class TaskExtractor
    {
        private readonly Regex dateRegex = new Regex(@"\b([0-2]?[0-9]|3[01])/(0?[1-9]|1[0-2])(/(\d{4}|\d{2}))?\b");
        private readonly Regex time24Regex = new Regex(@"\b([01]?[0-9]|2[0-3]):[0-5][0-9]\b");
        private readonly string[] splitters = new string[]
        {
            " and also ",      
            " and ",           
            " then ",          
            " also ",          
            " plus ",          
            " after that ",    
            " afterward ",     
            " followed by ",   
            " next ",          
            " afterwards ",    
            " as well as ",    
            " along with ",    
            " in addition ",   
            " together with ", 
            " moreover ",      
            " subsequently ",  
            " concurrently ",  
            " meanwhile ",     
            " plus also ",     
            " then also "      
        };
   
        private readonly string[] verbs = new string[]
        {
            "be", "have", "do", "say", "go", "get", "make", "know", "think", "take",
            "see", "come", "want", "use", "find", "give", "tell", "work", "call", "try",
            "ask", "need", "feel", "become", "leave", "put", "mean", "keep", "let", "begin",
            "seem", "help", "talk", "turn", "start", "show", "hear", "play", "run", "move",
            "like", "live", "believe", "hold", "bring", "happen", "write", "provide", "sit", "stand",
            "lose", "pay", "meet", "include", "continue", "set", "learn", "change", "lead", "understand",
            "watch", "follow", "stop", "create", "speak", "read", "allow", "add", "spend", "grow",
            "open", "walk", "win", "offer", "remember", "love", "consider", "appear", "buy", "wait",
            "serve", "die", "send", "expect", "build", "stay", "fall", "cut", "reach", "kill",
            "remain", "fight", "forget", "catch", "hurt", "rise", "lie", "point", "pick", "look",
            "beat", "carry", "choose", "drive", "eat", "fall", "fly", "hang", "hit", "hold",
            "keep", "lay", "lead", "leave", "lend", "let", "lie", "light", "lose", "make",
            "mean", "meet", "pay", "put", "read", "ride", "ring", "rise", "run", "say",
            "see", "sell", "send", "set", "shake", "shine", "shoot", "show", "shut", "sing",
            "sit", "sleep", "smell", "speak", "spend", "stand", "steal", "swim", "take", "teach",
            "tear", "tell", "think", "throw", "understand", "wake", "wear", "win", "write", "clean",
            "cook", "wash", "walk", "talk", "look", "use", "work", "play", "start", "show",
            "hear", "listen", "wait", "walk", "count", "say", "get", "give", "find", "become",
            "seem", "follow", "stop", "create", "speak", "read", "allow", "add", "spend", "grow",
            "open", "walk", "win", "offer", "remember", "love", "consider", "appear", "buy", "wait",
            "serve", "die", "send", "expect", "build", "stay", "fall", "cut", "reach", "kill",
            "travel", "visit", "watch", "listen", "play", "exercise", "run", "jump", "drive", "park",
            "study", "learn", "organize", "plan", "create", "draw", "paint", "design", "fix", "repair",
            "check", "review", "update", "delete", "install", "build", "deliver", "schedule", "arrange", "report",
            "share", "post", "email", "upload", "download", "test", "open", "close", "move", "remove",
            "add", "replace", "assemble", "measure", "cut", "fold", "pack", "text", "message", "notify",
            "sign", "submit", "approve", "reserve", "book", "charge", "pay", "withdraw", "deposit", "invest",
            "research", "explore", "listen", "watch", "read", "write", "speak", "sing", "dance", "laugh",
            "cry", "smile", "hug", "kiss", "love", "hate", "help", "hurt", "save", "break"
        };

        public List<ExtractedTask> ExtractTasks(string input)
        {
            var results = new List<ExtractedTask>();
            string cleanInput = input.Replace("?", "").Replace("!", "");
            var parts = SplitInput(cleanInput);

            foreach (var part in parts)
            {
                string lowerPart = part.ToLower();
                int earliestVerbIndex = FindEarliestVerbIndex(lowerPart);

                if (earliestVerbIndex != -1)
                {
                    string fullTaskText = part.Substring(earliestVerbIndex).Trim();
                    
                    var timeMatch = time24Regex.Match(fullTaskText);
                    var dateMatch = dateRegex.Match(fullTaskText);

                    DateTime baseDate = DateTime.Today;
                    bool dateFound = false;

                    // חילוץ תאריך - תומך בפורמטים שונים
                    if (dateMatch.Success)
                    {
                        string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "d/M/yy", "dd/MM/yy", "d/M", "dd/MM" };
                        if (DateTime.TryParseExact(dateMatch.Value, formats, 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            baseDate = parsedDate;
                            dateFound = true;
                        }
                    }

                    DateTime? finalDateTime = null;
                    if (timeMatch.Success)
                    {
                        if (DateTime.TryParseExact(timeMatch.Value, new[] { "H:mm", "HH:mm" }, 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempTime))
                        {
                            finalDateTime = baseDate.Date.Add(tempTime.TimeOfDay);
                        }
                    }
                    else if (dateFound)
                    {
                        finalDateTime = baseDate.Date.AddHours(9); // ברירת מחדל אם יש רק תאריך
                    }

                    // --- ניקוי התיאור בצורה אגרסיבית ---
                    string cleanedDescription = fullTaskText;

                    // 1. הסרת התאריך (כולל השנה אם קיימת)
                    if (dateMatch.Success)
                    {
                        cleanedDescription = dateRegex.Replace(cleanedDescription, "");
                    }

                    // 2. הסרת השעה
                    if (timeMatch.Success)
                    {
                        cleanedDescription = time24Regex.Replace(cleanedDescription, "");
                    }

                    // 3. הסרת מילות קישור יתומות וניקוי סימני פיסוק שנשארו
                    // הוספתי כאן הסרה של המילה "on" וכל מה שביניהן
                    cleanedDescription = Regex.Replace(cleanedDescription, @"\b(on|at|in|by|for|from|to)\b", "", RegexOptions.IgnoreCase);
                    
                    // הסרת תווים מיותרים כמו / או : שנשארו בטעות
                    cleanedDescription = Regex.Replace(cleanedDescription, @"[:\-/]", " ");

                    // 4. ניקוי רווחים כפולים
                    cleanedDescription = Regex.Replace(cleanedDescription, @"\s+", " ").Trim();

                    results.Add(new ExtractedTask
                    {
                        Description = cleanedDescription,
                        TaskTime = finalDateTime
                    });
                }
            }
            return results;
        }

        private int FindEarliestVerbIndex(string lowerPart)
        {
            int earliestIndex = -1;
            foreach (var verb in verbs)
            {
                int index = lowerPart.IndexOf(verb);
                if (index != -1 && (earliestIndex == -1 || index < earliestIndex))
                {
                    earliestIndex = index;
                }
            }
            return earliestIndex;
        }

        private List<string> SplitInput(string input)
        {
            var tempList = new List<string> { input };
            foreach (var splitter in splitters)
            {
                var newList = new List<string>();
                foreach (var part in tempList)
                {
                    if (part.Contains(splitter))
                        newList.AddRange(part.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries));
                    else
                        newList.Add(part);
                }
                tempList = newList;
            }
            return tempList;
        }
    }
}