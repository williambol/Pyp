using Microsoft.Extensions.Logging;

namespace Pyp.Cli.FileLogger;

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly StreamWriter _logFileWriter;

    public FileLogger(string categoryName, StreamWriter logFileWriter)
    {
        _categoryName = categoryName;
        _logFileWriter = logFileWriter;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        string logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{logLevel}\t[{_categoryName}:{eventId}]: {formatter(state, exception)}";
        
        if (exception != null)
        {
            logRecord += $"\nException: {exception}";
        }
        
        _logFileWriter.WriteLine(logRecord);
        _logFileWriter.Flush();
    }
}