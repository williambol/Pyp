using System.ServiceModel.Syndication;

namespace Pyp.Services;

public interface IPodcastService
{
    public Task<SyndicationFeed?> GetPodcastFeed(Uri url);

    public Task<bool> DownloadEpisode(string url, FileInfo outputStream, IProgress<float> progress);
}