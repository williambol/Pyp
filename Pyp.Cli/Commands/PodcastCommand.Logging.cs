using Microsoft.Extensions.Logging;

namespace Pyp.Cli.Commands;

public partial class PodcastCommand
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting CLI.")]
    static partial void LogStartupMessage(ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching feed. feed=\"{feedUri}\"")]
    static partial void LogFeed(ILogger logger, string feedUri);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "The input feed could not be converted to uri. Shutting down. feed=\"{feedUri}\"")]
    static partial void LogInvalidFeed(ILogger logger, string feedUri);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "RSS feed not found for the uri. Shutting down. feed=\"{feedUri}\"")]
    static partial void LogInvalidRssFeed(ILogger logger, string feedUri);

    [LoggerMessage(Level = LogLevel.Information, Message = "{count} episodes selected for download.")]
    static partial void LogSelectedEpisodes(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Downloading to \"{directory}\"")]
    static partial void LogDownloadDirectory(ILogger logger, string directory);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Downloading \"{file}\"")]
    static partial void LogDownloadingFile(ILogger logger, string file);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Done")]
    static partial void LogDone(ILogger logger);
}