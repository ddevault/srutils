# Subreddit Utilities

This is a tool for working with subreddits you moderate on [Reddit](http://reddit.com). It allows you to easily perform
a variety of tasks, such as resetting your subreddit settings to defaults, copying one subreddit to another, or taking
backups of your subreddits.

[Download Latest Version](http://sircmpwn.github.com/srutils/srutil.exe)

## How to use

If you are on Linux or Mac, you will need to install [Mono](http://mono-project.com) to use this tool. Preface all commands
with `mono` on these platforms. On Windows, you will need the .NET 4.0 Framework, which you may already have installed.

    srutil [credentials] [command] [arguments...]

You may call srutil and provide it a command you wish to perform from the list below. All of these commands take a unique
set of arguments based on what the command does.

[credentials] is optional, but you may use something like `srutil --username foo --password bar [command] [arguments...]`
if you wish. This may be useful if you are calling srutil in an automated fashion. If these aren't provided, you will be
prompted to supply them.

## Commands

### reset

Resets a subreddit's settings and style to the default.

    srutil reset /r/example [filters...]

*Aliases*: clear

The filters determine which parts of the subreddit are reset. It's a comma-delimited list of filters.

Valid filters include: css,images,settings,sidebar,flair

You may also specify "all".

**Examples**

To completely reset a subreddit to default: `srutil reset /r/example all`

To reset CSS and images: `srutil reset /r/example css,images`

To remove all flair: `srutil reset /r/example flair`

### duplicate

Duplicates the styles and settings of one subreddit into another.

    srutil duplicate /r/from /r/to

*Aliases*: dup dupe copy

All settings and styles will be reset in /r/to, then /r/from copied into /r/to.

You must be a moderator of /r/to, but needn't moderate /r/from.

### backup

Backs up a subreddit's settings, styles, etc into a zip archive.

    srutil backup /r/example example.zip

Includes settings, stylesheet, images, sidebar, and flair templates.

### restore

Restores a subreddit from a backup zip archive.

    srutil restore /r/example example.zip

Includes settings, stylesheet, images, sidebar, and flair templates.