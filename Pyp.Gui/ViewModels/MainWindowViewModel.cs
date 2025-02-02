using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pyp.Cli.Services;

namespace Pyp.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IPodcastService _podcastService;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadFeedCommand))]
    private bool _isLoading;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadFeedCommand))]
    private string _feed = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPodcastSelected))]
    private PodcastViewModel? _selectedPodcast;
    
    public bool IsPodcastSelected => SelectedPodcast is not null;

    public MainWindowViewModel(IPodcastService podcastService)
    {
        _podcastService = podcastService;
    }

    public MainWindowViewModel()
    {
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(CanLoadFeed))]
    private void LoadFeed(string feed)
    {
        _ = Task.Run(() => OnLoadFeedAsync(feed));
    }
    
    private bool CanLoadFeed(string feed)
    {
        return !string.IsNullOrWhiteSpace(feed) && !IsLoading;
    }
    
    private async void OnLoadFeedAsync(string feed)
    {
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsLoading = true;
            });
            SyndicationFeed? podcast = await _podcastService.GetPodcastFeed(feed);
            
            PodcastViewModel? podcastViewModel = null;
            
            if (podcast != null)
            {
                podcastViewModel = new PodcastViewModel(podcast, _podcastService);
            }
            Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
                
                if (podcastViewModel != null)
                {
                    SelectedPodcast = podcastViewModel;
                    Feed = string.Empty;
                }
            });
        }
        catch (Exception)
        {
            throw; // Todo: Handle exception.
        }
    }
}