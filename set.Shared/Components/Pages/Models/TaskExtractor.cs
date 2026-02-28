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
            string cleanInput = input.Replace("?", "").Replace("!", "").Replace(".", "");
            var tempList = new List<string> { cleanInput };

            foreach (var splitter in splitters)
            {
                var newList = new List<string>();
                foreach (var part in tempList)
                {
                    if (part.Contains(splitter))
                    {
                        var splitParts = part.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sp in splitParts)
                            newList.Add(sp.Trim());
                    }
                    else
                    {
                        newList.Add(part.Trim());
                    }
                }
                tempList = newList;
            }
            foreach (var part in tempList)
            {
                foreach (var verb in verbs)
                {
                    if (part.ToLower().Contains(verb))
                    {
                        tasks.Add(part.Trim());
                        break;
                    }
                }
            }

            return tasks;
        }
    }

}