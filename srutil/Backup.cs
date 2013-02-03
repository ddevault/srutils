using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using RedditSharp;

namespace srutil
{
    public class Backup : ISubredditOperation
    {
        public string[] GetOperationNames()
        {
            return new[] { "backup", "back" };
        }

        public void Execute(string[] args, Reddit reddit)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid usage. See 'srutil help backup' for more information.");
                return;
            }
            var subreddit = reddit.GetSubreddit(args[1]);
            using (var file = new ZipOutputStream(File.Create(args[2])))
            {
                file.SetLevel(5);
                var writer = new StreamWriter(file);
                writer.AutoFlush = true;
                var settings = subreddit.GetSettings();
                var serializer = new JsonSerializer();
                // Save settings
                Console.WriteLine("Saving settings...");
                var entry = new ZipEntry("settings.json");
                entry.DateTime = DateTime.Now;
                file.PutNextEntry(entry);
                serializer.Serialize(writer, settings);
                file.CloseEntry();
                // Save flair
                Console.WriteLine("Saving flair templates...");
                var flair = subreddit.GetUserFlairTemplates();
                entry = new ZipEntry("flair.json");
                file.PutNextEntry(entry);
                serializer.Serialize(writer, flair);
                file.CloseEntry();
                // Save styles
                Console.WriteLine("Saving CSS...");
                var styles = subreddit.GetStylesheet();
                entry = new ZipEntry("stylesheet.css");
                file.PutNextEntry(entry);
                writer.Write(styles.CSS);
                file.CloseEntry();
                // Save images
                Console.WriteLine("Saving images...");
                var webClient = new WebClient();
                var left = Console.CursorLeft; var top = Console.CursorTop;
                int progress = 1;
                foreach (var image in styles.Images)
                {
                    var data = webClient.DownloadData(image.Url);
                    entry = new ZipEntry("images/" + image.Name + Path.GetExtension(image.Url));
                    file.PutNextEntry(entry);
                    file.Write(data, 0, data.Length); file.Flush();
                    file.CloseEntry();
                    Console.CursorLeft = left; Console.CursorTop = top;
                    Console.WriteLine("{0}/{1}", progress++, styles.Images.Count);
                }
                Console.WriteLine("Done. {0} backed up to {1}.", subreddit.DisplayName, args[2]);
            }
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil backup /r/example <filename>\n" +
                "Backs upstyles, sidebar, images, etc, of /r/example locally in a zip file.");
        }
    }
}
