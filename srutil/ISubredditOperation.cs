using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedditSharp;

namespace srutil
{
    public interface ISubredditOperation
    {
        string[] GetOperationNames();
        void Execute(string[] args, Reddit reddit);
        void ShowHelp();
    }
}
