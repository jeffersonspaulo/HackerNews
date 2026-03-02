using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Infrastructure.HttpClients;
using HackerNews.BestStories.Api.Services.Interfaces;
using HackerNews.BestStories.Api.Services;
using HackerNews.BestStories.Api.Shared.Options;
using Polly.Extensions.Http;
using Polly;

namespace HackerNews.BestStories.Api.Configurations
{
    public static class ApplicationConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HackerNewsOptions>(configuration.GetSection("HackerNews"));
            services.Configure<BestStoriesOptions>(configuration.GetSection("BestStories"));
            services.Configure<CacheOptions>(configuration.GetSection("Cache"));

            services.AddMemoryCache();

            services.AddHttpClient<IHackerNewsClient, HackerNewsClient>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy()); 

            services.AddScoped<IBestStoriesService, BestStoriesService>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt))
                );
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,  
                    durationOfBreak: TimeSpan.FromSeconds(15) 
                );
        }
    }
}
