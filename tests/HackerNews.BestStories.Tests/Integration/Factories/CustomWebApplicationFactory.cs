using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HackerNews.BestStories.Tests.Integration.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly IHackerNewsClient _fakeClient;

        public CustomWebApplicationFactory(IHackerNewsClient fakeClient)
        {
            _fakeClient = fakeClient;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real registration (if any) and replace with fake
                services.RemoveAll<IHackerNewsClient>();
                services.AddSingleton(_fakeClient);
            });
        }
    }
}
