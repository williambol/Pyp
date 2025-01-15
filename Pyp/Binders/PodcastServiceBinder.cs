using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Pyp.Services;

namespace Pyp.Binders;

public class PodcastServiceBinder : BinderBase<IPodcastService>
{
    private readonly ILoggerFactory _loggerFactory;
    
    public PodcastServiceBinder(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }
    
    [Experimental("EXTEXP0001")]
    protected override IPodcastService GetBoundValue(BindingContext bindingContext) =>
        GetPodcastService(bindingContext);
    
    [Experimental("EXTEXP0001")]
    private IPodcastService GetPodcastService(BindingContext _)
    {
        var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3
            })
            .Build();

        var socketHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15)
        };
        var resilienceHandler = new ResilienceHandler(retryPipeline)
        {
            InnerHandler = socketHandler,
        };

        var httpClient = new HttpClient(resilienceHandler);
        
        return new PodcastService(_loggerFactory.CreateLogger<PodcastService>(), httpClient);
    }
}