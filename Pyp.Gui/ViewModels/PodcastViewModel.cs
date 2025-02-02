using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Pyp.Cli.Services;

namespace Pyp.Gui.ViewModels;

public partial class PodcastViewModel : ViewModelBase
{
    private readonly Dictionary<string, string> _mimeTypes = new (StringComparer.OrdinalIgnoreCase)
    {
        { "audio/mpeg", ".mp3" },
        { "audio/wav", ".wav" },
        { "audio/ogg", ".ogg" },
        { "audio/flac", ".flac" },
        { "audio/aac", ".aac" }
    };
    
    private IPodcastService _podcastService;
    
    [ObservableProperty]
    private double _headerHeight = 100;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private string _authors = string.Empty;
    
    [ObservableProperty]
    private Uri? _imageUri;
    
    public bool? IsAllSelected => PodcastEpisodes.All(e => e.ShouldDownload) ? true : PodcastEpisodes.Any(e => e.ShouldDownload) ? null : false;

    [ObservableProperty]
    private ObservableCollection<PodcastEpisodeViewModel> _podcastEpisodes = new();
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadSelectedCommand))]
    private bool _isDownloading;
    
    [ObservableProperty]
    private ObservableCollection<PodcastEpisodeDownloadViewModel> _downloadEpisodes = new();
    
    [RelayCommand]
    private void SelectAll(string feed)
    {
        bool shouldDownload = IsAllSelected != true;
        
        foreach (var podcastEpisodeViewModel in PodcastEpisodes)
        {
            podcastEpisodeViewModel.ShouldDownload = shouldDownload;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private void DownloadSelected()
    {
        _ = Task.Run(RunDownloadSelected);
    }
    
    private async Task RunDownloadSelected()
    {
        try
        {
            Dispatcher.UIThread.Post(() => { IsDownloading = true; });
            var currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadDirectory = Path.Combine(currentDirectory, "PYP", Title);
            var downloadDirectoryInfo = new DirectoryInfo(downloadDirectory);

            if (downloadDirectoryInfo.Exists)
            {
                downloadDirectoryInfo.Delete(true);
            }

            downloadDirectoryInfo.Create();

            List<PodcastEpisodeDownloadViewModel> episodeDownloadViewModels = [];

            foreach (PodcastEpisodeViewModel podcastEpisodeViewModel in PodcastEpisodes.Where(e => e.ShouldDownload))
            {
                var downloadFile = new FileInfo(Path.Combine(downloadDirectory,
                    $"{podcastEpisodeViewModel.Title}{_mimeTypes[podcastEpisodeViewModel.Enclosure?.MediaType.ToLower() ?? ""]}"));
                
                episodeDownloadViewModels.Add(new PodcastEpisodeDownloadViewModel(podcastEpisodeViewModel, downloadFile));
            }
            
            Dispatcher.UIThread.Post(() =>
            {
                DownloadEpisodes.AddRange(episodeDownloadViewModels);
                OnPropertyChanged(nameof(DownloadEpisodes));
            });

            foreach (var podcastEpisodeDownload in episodeDownloadViewModels)
            {
                await _podcastService.DownloadEpisode(podcastEpisodeDownload.Url,
                    podcastEpisodeDownload.File, podcastEpisodeDownload.ProgressReporter);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsDownloading = false;
            });
        }
    }
    
    private bool CanDownload()
    {
        return IsAllSelected != false && !IsDownloading;
    }
    
    public PodcastViewModel(SyndicationFeed syndicationFeed, IPodcastService podcastService)
    {
        _podcastService = podcastService;
        
        Title = syndicationFeed.Title.Text;
        ImageUri = syndicationFeed.ImageUrl;
        Description = syndicationFeed.Description.Text;
        SyndicationElementExtension? reader = syndicationFeed.ElementExtensions.FirstOrDefault(e => e.OuterName == "author");
        if (reader != null)
        {
            Authors = reader.GetReader().ReadElementContentAsString();
        }
        
        foreach (var episode in syndicationFeed.Items.Select(e => new PodcastEpisodeViewModel(e)))
        {
            episode.PropertyChanged += PodcastEpisode_PropertyChanged;
            PodcastEpisodes.Add(episode);
        }
    }
    
    private void PodcastEpisode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PodcastEpisodeViewModel.ShouldDownload))
        {
            OnPropertyChanged(nameof(IsAllSelected));
            DownloadSelectedCommand.NotifyCanExecuteChanged();
        }
    }
}