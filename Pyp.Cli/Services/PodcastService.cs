using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;
using Pyp.Cli.Extensions;
using Exception = System.Exception;

namespace Pyp.Cli.Services;

public partial class PodcastService : IPodcastService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public PodcastService(ILogger<PodcastService> logger, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public Task<SyndicationFeed?> GetPodcastFeed(string uri)
    {
        LogFetchingPodcast(_logger, uri);
        SyndicationFeed? feed = null;
        try
        {
            XmlReader reader = XmlReader.Create(uri);
            feed = SyndicationFeed.Load(reader);
            reader.Close();
            reader.Dispose();
        }
        catch (Exception e)
        {
            LogErrorFetchingPodcast(_logger, e);
        }
        
        return Task.FromResult(feed);
    }
    
    public async Task<bool> DownloadEpisode(string url, FileInfo outputStream, IProgress<float> progress)
    {
        LogDownloadingEpisode(_logger, url, outputStream);
        try
        {
            await _httpClient.DownloadAsync(url, outputStream.OpenWrite(), progress);
            return true;
        }
        catch (Exception e) 
        {
            LogErrorDownloadingEpisode(_logger, e);
        }
        
        return false;
    }
}