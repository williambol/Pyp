using System.ServiceModel.Syndication;

namespace Pyp.Cli.Services;

public interface IPodcastService
{
    public Task<SyndicationFeed?> GetPodcastFeed(string uri);

    public Task<bool> DownloadEpisode(string url, FileInfo outputStream, IProgress<float> progress);
}