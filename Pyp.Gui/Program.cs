using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyp.Cli.Commands;
using Pyp.Cli.FileLogger;

namespace Pyp.Gui;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            // Run as a command-line application
            await RunCommandLine(args);
        }
        else
        {
            // Run as an AvaloniaUI application
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }
    
    private static async Task RunCommandLine(string[] args)
    {
        string logDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pyp.Cli", "Logs");
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

        await rootCommand.InvokeAsync(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}