using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Infrastructure.HttpClients;
using HackerNews.BestStories.Api.Services.Interfaces;
using HackerNews.BestStories.Api.Services;
using HackerNews.BestStories.Api.Shared.Options;

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

            services.AddHttpClient<IHackerNewsClient, HackerNewsClient>();
            services.AddScoped<IBestStoriesService, BestStoriesService>();
        }
    }
}
