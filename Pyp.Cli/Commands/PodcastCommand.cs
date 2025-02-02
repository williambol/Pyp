using System.CommandLine;
using System.ServiceModel.Syndication;
using Microsoft.Extensions.Logging;
using Pyp.Cli.Binders;
using Pyp.Cli.Services;
using Spectre.Console;

namespace Pyp.Cli.Commands;

public partial class PodcastCommand : RootCommand
{
    private ILogger _logger;
    
    private readonly Dictionary<string, string> _mimeTypes = new (StringComparer.OrdinalIgnoreCase)
    {
        { "audio/mpeg", ".mp3" },
        { "audio/wav", ".wav" },
        { "audio/ogg", ".ogg" },
        { "audio/flac", ".flac" },
        { "audio/aac", ".aac" }
    };
    
    public PodcastCommand(ILoggerFactory loggerFactory) : base("Some description")
    {
        _logger = loggerFactory.CreateLogger<PodcastCommand>();
        
        LogStartupMessage(_logger);
        
        var feedOption = new Option<string?>(
            name: "--feed",
            description: "The feed of the podcast to have its episodes downloaded.");

        this.SetHandler(ReadFeed, feedOption, new PodcastServiceBinder(loggerFactory));
    }
    
    private async Task ReadFeed(string? feed, IPodcastService podcastService)
    {
        feed ??= AnsiConsole.Prompt(
            new TextPrompt<string>("What is the podcast feed?"));
        
        if (!Uri.IsWellFormedUriString(feed, UriKind.Absolute))
        {
            AnsiConsole.MarkupLine("[red]Invalid podcast feed.[/]");
            LogInvalidFeed(_logger, feed);
            return;
        }
        
        LogFeed(_logger, feed);

        SyndicationFeed? podcastFeed = null;
        
        await AnsiConsole.Status()
            .StartAsync("Loading...", async _ => 
            {
                podcastFeed = await podcastService.GetPodcastFeed(feed);
            });

        if (podcastFeed == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load podcast feed. Please ensure the uri points to a valid feed.[/]");
            LogInvalidRssFeed(_logger, feed);
            return;
        }
    
        AnsiConsole.MarkupLine($"[green]Found Podcast with title: {podcastFeed?.Title.Text} and {podcastFeed?.Items.Count()} episodes.[/]");
        
        List<SyndicationItem> selectedItems = AnsiConsole.Prompt(
            new MultiSelectionPrompt<SyndicationItem>()
                .Title("Which episodes would you like to download?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more episodes)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle an episode, " + 
                    "[green]<enter>[/] to accept)[/]")
                .AddChoiceGroup(new SyndicationItem("All", null, null), podcastFeed?.Items.ToList() ?? [])
                .UseConverter(item => item.Title.Text));
        
        LogSelectedEpisodes(_logger, selectedItems.Count);
        
        var currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadDirectory = Path.Combine(currentDirectory, "PYP", podcastFeed?.Title.Text ?? "Downloads");
        var downloadDirectoryInfo = new DirectoryInfo(downloadDirectory);

        if (downloadDirectoryInfo.Exists)
        {
            downloadDirectoryInfo.Delete(true);
        }
        
        downloadDirectoryInfo.Create();
        
        AnsiConsole.MarkupLine($"Downloading to {downloadDirectory}");
        LogDownloadDirectory(_logger, downloadDirectory);
        
        await AnsiConsole.Progress()
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn(), new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                Dictionary<string, ProgressTask> tasks = new();
                
                foreach (var selectedItem in selectedItems)
                {
                    ProgressTask task = ctx.AddTask(selectedItem.Title.Text, new ProgressTaskSettings
                    {
                        AutoStart = false,
                        MaxValue = 1,
                    });
                    
                    tasks.Add(selectedItem.Id, task);
                }
                
                foreach (var selectedItem in selectedItems)
                {
                    ProgressTask task = tasks[selectedItem.Id];
                    
                    task.StartTask();

                    SyndicationLink? enclosure = selectedItem.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
                    
                    if (enclosure == null || !_mimeTypes.ContainsKey(enclosure.MediaType.ToLower()))
                    {
                        task.StopTask();
                        continue;
                    }
                    
                    Progress<float> progress = new Progress<float>(progress => task.Value = progress);

                    var downloadFile = new FileInfo(Path.Combine(downloadDirectory, $"{selectedItem.Title.Text}{_mimeTypes[enclosure.MediaType.ToLower()]}"));

                    LogDownloadingFile(_logger, downloadFile.ToString());
                    
                    await podcastService.DownloadEpisode(enclosure.Uri.ToString(), downloadFile, progress);
                    
                    task.StopTask();
                }
            });
        
        LogDone(_logger);
        AnsiConsole.MarkupLine("[green] Done, you may now close this window.[/]");
    }
}