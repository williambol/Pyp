// See https://aka.ms/new-console-template for more information
//https://feeds.simplecast.com/bohNKLUL?_ga=2.236344321.1436236783.1735443888-528807958.1735443888

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Pyp.Commands;
using Pyp.FileLogger;

string logDirectory =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pyp", "Logs");
Directory.CreateDirectory(logDirectory);

// Define the path to the text file
string logFilePath = Path.Combine(logDirectory, $"log-cli-{DateTime.Now:yyyyMMddHHmmssfff}.log");

// Create a StreamWriter to write logs to a text file
await using StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true);
// Create an ILoggerFactory
ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    // Add a custom log provider to write logs to text files
    builder.AddProvider(new FileLoggerProvider(logFileWriter));
});

// Create an ILogger
PodcastCommand rootCommand = new PodcastCommand(loggerFactory);

return await rootCommand.InvokeAsync(args);