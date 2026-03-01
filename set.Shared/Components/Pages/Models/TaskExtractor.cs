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

        public List<string> ExtractTasks(string input)
        {
            var tasks = new List<string>();
            
            string cleanInput = input.Replace("?", "").Replace("!", "").Replace(".", "");
            
            
            var parts = SplitInput(cleanInput);

            foreach (var part in parts)
            {
                string lowerPart = part.ToLower();
                int earliestVerbIndex = -1;
                string foundVerb = "";

                
                foreach (var verb in verbs)
                {
                    int index = lowerPart.IndexOf(verb);
                    if (index != -1 && (earliestVerbIndex == -1 || index < earliestVerbIndex))
                    {
                        earliestVerbIndex = index;
                        foundVerb = verb;
                    }
                }

                
                if (earliestVerbIndex != -1)
                {
                    
                    string actualTask = part.Substring(earliestVerbIndex);
                    tasks.Add(actualTask.Trim());
                }
            }

            return tasks;
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