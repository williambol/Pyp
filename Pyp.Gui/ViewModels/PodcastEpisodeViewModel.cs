using System;
using System.Linq;
using System.ServiceModel.Syndication;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pyp.Gui.ViewModels;

public partial class PodcastEpisodeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private Uri? _imageUri;
    
    [ObservableProperty]
    private bool _shouldDownload;
    
    public SyndicationLink? Enclosure { get; }

    public PodcastEpisodeViewModel(SyndicationItem item)
    {
        Title = item.Title.Text;
        Description = item.Summary.Text;
        Enclosure = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
        SyndicationElementExtension? reader = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "image");
        if (reader != null)
        {
            var imageString = reader.GetReader().GetAttribute("href");
            ImageUri = Uri.TryCreate(imageString, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;
        }
    }
}