using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedditSharp;

namespace srutil
{
    public class Reset : ISubredditOperation
    {
        [Flags]
        public enum ResetOperations
        {
            None = 0,
            CSS = 1,
            Images = 2,
            Settings = 4,
            Sidebar = 8,
            Flair = 16,
            All = CSS | Images | Settings | Sidebar | Flair
        }

        public string[] GetOperationNames()
        {
            return new[] { "reset", "clear" };
        }

        public void Execute(string[] args, Reddit reddit)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid usage. See 'srutil help reset' for more information.");
                return;
            }
            // TODO: Handle errors
            var subreddit = reddit.GetSubreddit(args[1]);
            var filterStrings = args[2].Split(',');
            ResetOperations filter = ResetOperations.None;
            foreach (var item in filterStrings)
                filter |= (ResetOperations)Enum.Parse(typeof(ResetOperations), item, true);

            // Do reset
            var style = subreddit.GetStylesheet();
            var settings = subreddit.GetSettings();
            if (filter.HasFlag(ResetOperations.CSS))
            {
                Console.WriteLine("Clearing CSS...");
                style.CSS = string.Empty;
                style.UpdateCss();
                Console.WriteLine("Done.");
            }
            if (filter.HasFlag(ResetOperations.Flair))
            {
                Console.WriteLine("Clearing flair templates...");
                subreddit.ClearFlairTemplates(FlairType.Link);
                subreddit.ClearFlairTemplates(FlairType.User);
                Console.WriteLine("Done.");
            }
            if (filter.HasFlag(ResetOperations.Images))
            {
                Console.WriteLine("Clearing stylesheet images...");
                int total = style.Images.Count;
                int current = 1;
                var left = Console.CursorLeft; var top = Console.CursorTop;
                while (style.Images.Any())
                {
                    style.Images[0].Delete();
                    Console.CursorLeft = left; Console.CursorTop = top;
                    Console.Write("{0}/{1}", current++, total);
                }
                Console.WriteLine();
                Console.WriteLine("Done.");
            }
            if (filter.HasFlag(ResetOperations.Settings))
            {
                Console.WriteLine("Resetting settings...");
                settings.AllowAsDefault = true;
                settings.ContentOptions = ContentOptions.All;
                settings.HeaderHoverText = "";
                settings.Language = "en";
                settings.NSFW = false;
                settings.PublicDescription = "";
                settings.ShowThumbnails = true;
                settings.SubredditType = SubredditType.Public;
                settings.Title = subreddit.DisplayName;
                settings.WikiEditAge = 0;
                settings.WikiEditKarma = 100;
                settings.WikiEditMode = WikiEditMode.All;
                settings.UpdateSettings();
                settings.ResetHeaderImage();
                Console.WriteLine("Done.");
            }
            if (filter.HasFlag(ResetOperations.Sidebar))
            {
                Console.WriteLine("Clearing sidebar...");
                settings.Sidebar = "";
                settings.UpdateSettings();
                Console.WriteLine("Done.");
            }
            Console.WriteLine("Finished resetting " + subreddit.DisplayName + ".");
        }

        public void ShowHelp()
        {
            Console.WriteLine("Usage: srutil reset /r/subreddit [filters]\n" +
                "Resets subreddit styles, images, sidebar, etc\n" +
                "You must be a moderator of /r/subreddit.\n" +
                "[filters] may be 'ALL' to clear all data, or any combination of:\n" +
                "css,images,settings,sidebar,flair");
        }
    }
}
