using System;
using System.Collections.Generic;

namespace Models
{
    public class TaskExtractor
    {
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
            "take", "write", "do", "make", "send", "call", "buy", "clean", "prepare", "cook",
            "read", "study", "learn", "organize", "plan", "create", "draw", "paint", "design",
            "fix", "repair", "check", "review", "update", "delete", "install", "build", "deliver",
            "schedule", "arrange", "report", "share", "post", "email", "upload", "download", "test",
            "open", "close", "move", "remove", "add", "replace", "assemble", "measure", "cut", "fold",
            "pack", "call", "text", "message", "notify", "sign", "submit", "approve", "reserve", "book",
            "charge", "pay", "withdraw", "deposit", "invest", "research", "explore", "travel", "visit",
            "watch", "listen", "play", "exercise", "run", "jump", "drive", "park", "cleanse", "report"
        };

        public List<string> ExtractTasks(string input)
        {
            var tasks = new List<string>();
            // ניקוי סימני פיסוק בסיסיים והפיכה לאותיות קטנות לבדיקה
            string cleanInput = input.Replace("?", "").Replace("!", "").Replace(".", "");
            
            // 1. פיצול לפי רשימת ה-splitters שכבר יש לך
            var parts = SplitInput(cleanInput);

            foreach (var part in parts)
            {
                string lowerPart = part.ToLower();
                int earliestVerbIndex = -1;
                string foundVerb = "";

                // 2. מציאת המיקום של הפועל הראשון בחלק הזה
                foreach (var verb in verbs)
                {
                    int index = lowerPart.IndexOf(verb);
                    if (index != -1 && (earliestVerbIndex == -1 || index < earliestVerbIndex))
                    {
                        earliestVerbIndex = index;
                        foundVerb = verb;
                    }
                }

                // 3. אם נמצא פועל, חתוך את הטקסט שמתחיל ממנו
                if (earliestVerbIndex != -1)
                {
                    // חותך מהפועל ועד סוף המשפט - זה יוריד את ה-"Hey what's up"
                    string actualTask = part.Substring(earliestVerbIndex);
                    tasks.Add(actualTask.Trim());
                }
            }

            return tasks;
        }

        // מתודת עזר לפיצול (הלוגיקה המקורית שלך)
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