using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using RedditSharp;

namespace srutil
{
    public class Restore : ISubredditOperation
    {
        public string[] GetOperationNames()
        {
            return new[] { "restore" };
        }

        public void Execute(string[] args, Reddit reddit)
        {
            Console.WriteLine("This doesn't work yet");
            return;
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid usage. See 'srutil help backup' for more information.");
                return;
            }
            var subreddit = reddit.GetSubreddit(args[1]);
            // Verify archive
            var settings = new SubredditSettings(reddit, subreddit);
            var styles = new SubredditStyle(reddit, subreddit);
            var serializer = new JsonSerializer();
            using (var file = new ZipFile(File.OpenRead(args[2])))
            {
                if (file.GetEntry("stylesheet.css") == null ||
                    file.GetEntry("settings.json") == null ||
                    file.GetEntry("flair.json") == null)
                {
                    Console.WriteLine("{0} is not a valid subreddit backup.", args[2]);
                    return;
                }
                Console.WriteLine("Resetting {0} to default settings...");
                var reset = new Reset();
                reset.Execute(new[] { "reset", subreddit.DisplayName, "all" }, reddit);
                foreach (ZipEntry entry in file)
                {
                    if (!entry.IsFile)
                        continue;
                    if (entry.Name == "stylesheet.css")
                    {
                        var stream = new StreamReader(file.GetInputStream(entry));
                        styles.CSS = stream.ReadToEnd();
                        // We udpate the CSS last to avoid errors with images
                    }
                    else if (entry.Name == "settings.json")
                    {
                        Console.WriteLine("Restoring settings...");
                        var stream = new StreamReader(file.GetInputStream(entry));
                        settings = serializer.Deserialize<SubredditSettings>(new JsonTextReader(stream));
                        settings.UpdateSettings();
                    }
                    else if (entry.Name == "flair.json") // TODO: Link flair templates, selected flair for users
                    {
                        Console.WriteLine("Restoring user flair templates...");
                    }
                }
            }
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil backup /r/example <filename>\n" +
                "Backs upstyles, sidebar, images, etc, of /r/example locally in a zip file.");
        }
    }
}
