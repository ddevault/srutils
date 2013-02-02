using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RedditSharp;

namespace srutil
{
    public class Duplicate : ISubredditOperation
    {
        public string[] GetOperationNames()
        {
            return new[] { "dup", "dupe", "duplicate", "copy" };
        }

        public void Execute(string[] args, Reddit reddit)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid usage. See 'srutil help dupe' for more information.");
                return;
            }
            var from = reddit.GetSubreddit(args[1]);
            var to = reddit.GetSubreddit(args[2]);
            Console.WriteLine("Resetting {0} to default settings...", to.DisplayName);
            var reset = new Reset();
            reset.Execute(new[] { "reset", args[2], "all" }, reddit);
            // TODO: Better error handling
            Console.WriteLine("Copying {0} into {1}...", from.DisplayName, to.DisplayName);

            // Copy settings
            Console.WriteLine("Copying settings...");
            var settings = from.GetSettings();
            settings.Subreddit = to;
            settings.UpdateSettings();
            Console.WriteLine("Done.");

            // Copy images
            Console.WriteLine("Copying images...");
            var stylesFrom = from.GetStylesheet();
            var styleTo = to.GetStylesheet();

            var webClient = new WebClient();
            int left = Console.CursorLeft; int top = Console.CursorTop;
            int progress = 1;
            foreach (var image in stylesFrom.Images)
            {
                var data = webClient.DownloadData(image.Url);
                styleTo.UploadImage(image.Name, ImageType.PNG, data);
                Console.CursorLeft = left; Console.CursorTop = top;
                Console.WriteLine("{0}/{1}", progress++, stylesFrom.Images.Count);
            }
            Console.WriteLine("Done.");

            // Copy styles
            Console.WriteLine("Copying styles...");
            styleTo.CSS = stylesFrom.CSS;
            styleTo.UpdateCss();
            Console.WriteLine("Done.");

            // Header
            Console.WriteLine("Copying header image...");
            var headerImage = webClient.DownloadData(from.HeaderImage);
            to.UploadHeaderImage("header", ImageType.PNG, headerImage);
            Console.WriteLine("Done.");

            Console.WriteLine("Copied {0} to {1}.", from.DisplayName, to.DisplayName);
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil dupe /r/source /r/destination\n" +
                "Duplicates styles, sidebar, images, etc, of /r/source into /r/destination.\n" +
                "You must be a moderator of /r/destination.");
        }
    }
}
