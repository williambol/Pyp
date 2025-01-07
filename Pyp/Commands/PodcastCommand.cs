using System.CommandLine;
using System.ServiceModel.Syndication;
using Pyp.Binders;
using Pyp.Services;
using Spectre.Console;

namespace Pyp.Commands;

public class PodcastCommand : RootCommand
{
    private readonly Dictionary<string, string> _mimeTypes = new (StringComparer.OrdinalIgnoreCase)
    {
        { "audio/mpeg", ".mp3" },
        { "audio/wav", ".wav" },
        { "audio/ogg", ".ogg" },
        { "audio/flac", ".flac" },
        { "audio/aac", ".aac" }
    };
    
    public PodcastCommand() : base("Some description")
    {
        var feedOption = new Option<string?>(
            name: "--feed",
            description: "The feed of the podcast to have its episodes downloaded.");

        this.SetHandler(ReadFeed, feedOption, new PodcastServiceBinder());
    }
    
    private async Task ReadFeed(string? feed, IPodcastService podcastService)
    {
        feed ??= AnsiConsole.Prompt(
            new TextPrompt<string>("What is the podcast feed?"));
        
        if (!Uri.TryCreate(feed, UriKind.Absolute, out Uri? uri))
        {
            AnsiConsole.MarkupLine("[red]Invalid podcast feed.[/]");
            return;
        }

        SyndicationFeed? podcastFeed = null;
        
        await AnsiConsole.Status()
            .StartAsync("Loading...", async _ => 
            {
                podcastFeed = await podcastService.GetPodcastFeed(uri);
            });

        if (podcastFeed == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load podcast feed.[/]");
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
        
        var currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadDirectory = Path.Combine(currentDirectory, "PYP", podcastFeed?.Title.Text ?? "Downloads");
        var downloadDirectoryInfo = new DirectoryInfo(downloadDirectory);

        if (downloadDirectoryInfo.Exists)
        {
            downloadDirectoryInfo.Delete(true);
        }
        
        downloadDirectoryInfo.Create();
        
        AnsiConsole.MarkupLine($"Downloading to {downloadDirectory}");
        
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

                    await podcastService.DownloadEpisode(enclosure.Uri.ToString(), downloadFile, progress);
                    
                    task.StopTask();
                }
            });
    }
}