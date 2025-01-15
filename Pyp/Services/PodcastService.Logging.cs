using Microsoft.Extensions.Logging;

namespace Pyp.Services;

public partial class PodcastService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching podcast. feed=\"{uri}\"")]
    static partial void LogFetchingPodcast(ILogger logger, Uri uri);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Error while fetching podcast.")]
    static partial void LogErrorFetchingPodcast(ILogger logger, Exception ex);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Downloading \"{uri}\" to \"{destination}\"")]
    static partial void LogDownloadingEpisode(ILogger logger, string uri, FileInfo destination);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Error downloading episode.")]
    static partial void LogErrorDownloadingEpisode(ILogger logger, Exception ex);
}