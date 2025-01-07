using System.ServiceModel.Syndication;
using System.Xml;
using Pyp.Extensions;
using Exception = System.Exception;

namespace Pyp.Services;

public class PodcastService : IPodcastService
{
    private readonly HttpClient _httpClient;

    public PodcastService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public Task<SyndicationFeed?> GetPodcastFeed(Uri url)
    {
        SyndicationFeed? feed = null;
        try
        {
            XmlReader reader = XmlReader.Create(url.ToString());
            feed = SyndicationFeed.Load(reader);
            reader.Close();
            reader.Dispose();
        }
        catch (Exception e) 
        {
            //TODO add to logging
        }
        
        return Task.FromResult(feed);
    }
    
    public async Task<bool> DownloadEpisode(string url, FileInfo outputStream, IProgress<float> progress)
    {
        try
        {
            await _httpClient.DownloadAsync(url, outputStream.OpenWrite(), progress);
            return true;
        }
        catch (Exception e) 
        {
            //TODO add to logging
        }
        
        return false;
    }
}