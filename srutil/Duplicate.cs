using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedditSharp;

namespace srutil
{
    public class Duplicate : ISubredditOperation
    {
        public string[] GetOperationNames()
        {
            return new[] { "dup", "dupe", "duplicate" };
        }

        public void Execute(string[] args, Reddit reddit)
        {
            
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil dupe /r/source /r/destination\n" +
                "Duplicates styles, sidebar, images, etc, of /r/source into /r/destination.\n" +
                "You must be a moderator of /r/destination.");
        }
    }
}
