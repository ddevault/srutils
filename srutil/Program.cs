using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Reflection;
using RedditSharp;

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
            string username = null, password = null;
            while (args[0].StartsWith("--"))
            {
                if (args[0] == "--username")
                {
                    username = args[1];
                    args = args.Skip(2).ToArray();
                }
                else if (args[0] == "--password")
                {
                    password = args[1];
                    args = args.Skip(2).ToArray();
                }
				else if (args[0] == "--domain")
				{
					Reddit.RootDomain = args[1];
					args = args.Skip(2).ToArray();
				}
            }
            if (!OperationTypes.ContainsKey(args[0]))
            {
                if (args[0] == "help")
                {
                    ShowHelp(args);
                    return;
                }
                Console.WriteLine("Error: Operation \"" + args[0] + "\" not found.");
                return;
            }
            // Log into reddit
            var reddit = new Reddit();
            while (reddit.User == null)
            {
                if (username == null)
                {
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                }
                if (password == null)
                {
                    Console.Write("Password: ");
                    password = ReadPassword();
                }
                try
                {
                    Console.WriteLine("Logging in...");
                    reddit.LogIn(username, password);
                }
                catch (AuthenticationException e)
                {
                    Console.WriteLine("Incorrect login.");
                    username = password = null;
                }
            }
            // Execute command
            var operationType = OperationTypes[args[0]];
            var operation = (ISubredditOperation)Activator.CreateInstance(operationType);
            operation.Execute(args, reddit);
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

        public static string ReadPassword()
        {
            var passbits = new Stack<string>();
            //keep reading
            for (ConsoleKeyInfo cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true))
            {
                if (cki.Key == ConsoleKey.Backspace)
                {
                    //rollback the cursor and write a space so it looks backspaced to the user
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    passbits.Pop();
                }
                else
                {
                    Console.Write("*");
                    passbits.Push(cki.KeyChar.ToString());
                }
            }
            string[] pass = passbits.ToArray();
            Array.Reverse(pass);
            Console.Write(Environment.NewLine);
            return string.Join(string.Empty, pass);
        }
    }
}
