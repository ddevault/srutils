using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srutil
{
    public interface ISubredditOperation
    {
        string[] GetOperationNames();
        void Execute(string[] args);
        void ShowHelp();
    }
}
