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
                Console.WriteLine("Resetting {0} to default settings...", subreddit.DisplayName);
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
                        serializer.Populate(new JsonTextReader(stream), settings);
                        settings.UpdateSettings();
                    }
                    else if (entry.Name == "flair.json") // TODO: Link flair templates, selected flair for users
                    {
                        var stream = new StreamReader(file.GetInputStream(entry));
                        var flair = serializer.Deserialize<UserFlairTemplate[]>(new JsonTextReader(stream));
                        if (flair.Any())
                        {
                            Console.WriteLine("Restoring user flair templates...");
                            int progress = 1;
                            int left = Console.CursorLeft; int top = Console.CursorTop;
                            foreach (var item in flair)
                            {
                                subreddit.AddFlairTemplate(item.CssClass, FlairType.User, item.Text, true);
                                Console.CursorLeft = left; Console.CursorTop = top;
                                Console.WriteLine("{0}/{1}", progress++, flair.Length);
                            }
                        }
                    }
                    else if (entry.Name == "header.png" || entry.Name == "header.jpg")
                    {
                        var image = Path.GetFileName(entry.Name);
                        Console.WriteLine("Restoring header image...");
                        var stream = file.GetInputStream(entry);
                        var data = new byte[entry.Size];
                        stream.Read(data, 0, (int)entry.Size);
                        subreddit.UploadHeaderImage(entry.Name,
                            Path.GetExtension(entry.Name) == "png" ? ImageType.PNG : ImageType.JPEG, data);
                    }
                    else if (entry.Name.StartsWith("images/"))
                    {
                        var image = Path.GetFileName(entry.Name);
                        Console.WriteLine("Restoring image: " + image);
                        var stream = file.GetInputStream(entry);
                        var data = new byte[entry.Size];
                        stream.Read(data, 0, (int)entry.Size);
                        styles.UploadImage(Path.GetFileNameWithoutExtension(image),
                            Path.GetExtension(image) == ".png" ? ImageType.PNG : ImageType.JPEG, data);
                    }
                }
                Console.WriteLine("Restoring CSS...");
                styles.UpdateCss();
                Console.WriteLine("Finished restoring {0}", subreddit.DisplayName);
            }
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil backup /r/example <filename>\n" +
                "Backs upstyles, sidebar, images, etc, of /r/example locally in a zip file.");
        }
    }
}
