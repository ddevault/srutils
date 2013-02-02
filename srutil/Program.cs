using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace srutil
{
    class Program
    {
        private static Dictionary<string, Type> OperationTypes;

        static void Main(string[] args)
        {
            // Register operations
            OperationTypes = new Dictionary<string, Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes().Where(t => 
                    typeof(ISubredditOperation).IsAssignableFrom(t) &&
                    !t.IsAbstract && !t.IsInterface);
                foreach (var type in types)
                {
                    var instance = (ISubredditOperation)Activator.CreateInstance(type);
                    foreach (var keyword in instance.GetOperationNames())
                        OperationTypes[keyword] = type;
                }
            }
            // Start parsing arguments
            if (args.Length < 1)
            {
                Console.WriteLine("Error: No operation specified. Use \"srutil help\" for more information.");
                return;
            }
            if (!OperationTypes.ContainsKey(args[0]))
            {
                if (args[0] == "help")
                {
                    ShowHelp(args);
                    return;
                }
                Console.WriteLine("Error: Operation \"" + args[0] + "\" not found.");
            }
        }

        private static void ShowHelp(string[] args)
        {
            if (args.Length == 1)
            {
                Console.WriteLine("Usage: srutil [operation] [parameters]\n" +
                                  "Use srutil help [operation] for operation-specific help.\n" +
                                  "Available operations:");
                foreach (var operation in OperationTypes)
                {
                    // TODO: Don't show duplicates
                    var instance = (ISubredditOperation)Activator.CreateInstance(operation.Value);
                    Console.WriteLine("-" + operation.Key);
                }
            }
            else
            {
                if (!OperationTypes.ContainsKey(args[1]))
                {
                    Console.WriteLine("Error: Operation \"" + args[1] + "\" not found.");
                    return;
                }
                var instance = (ISubredditOperation)Activator.CreateInstance(OperationTypes[args[1]]);
                instance.ShowHelp();
            }
        }
    }
}
