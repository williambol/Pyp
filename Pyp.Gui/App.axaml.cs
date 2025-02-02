using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pyp.Cli.Commands;
using Pyp.Cli.FileLogger;
using Pyp.Cli.Services;
using Pyp.Gui.ViewModels;
using Pyp.Gui.Views;

namespace Pyp.Gui;

public partial class App : Application
{
    public App()
    {
        Services = ConfigureServices();
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IPodcastService, PodcastService>();
        
        services.AddTransient<MainWindowViewModel>();

        services.AddHttpClient<IPodcastService, PodcastService>();
        
        string logDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pyp.Cli", "Logs");
        Directory.CreateDirectory(logDirectory);

// Define the path to the text file
        string logFilePath = Path.Combine(logDirectory, $"log-cli-{DateTime.Now:yyyyMMddHHmmssfff}.log");

// Create a StreamWriter to write logs to a text file
        StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true);
        services.AddLogging(builder => builder.AddProvider(new FileLoggerProvider(logFileWriter)));

        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}