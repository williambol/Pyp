using System;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pyp.Gui.ViewModels;

public partial class PodcastEpisodeDownloadViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private Uri? _imageUri;
    
    [ObservableProperty]
    private float _progress;

    public Progress<float> ProgressReporter { get; }
    
    public string Url { get; }
    
    public FileInfo File { get; }

    public PodcastEpisodeDownloadViewModel(PodcastEpisodeViewModel podcastEpisodeViewModel, FileInfo file)
    {
        Title = podcastEpisodeViewModel.Title;
        ImageUri = podcastEpisodeViewModel.ImageUri;

        ProgressReporter = new Progress<float>(progress => Dispatcher.UIThread.Post(() => Progress = progress * 100));
        
        Url = podcastEpisodeViewModel.Enclosure?.Uri.ToString() ?? "";
        File= file;
    }
}