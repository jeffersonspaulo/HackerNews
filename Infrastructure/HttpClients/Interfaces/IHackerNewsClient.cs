using HackerNews.BestStories.Api.Models.ExternalContracts;

namespace HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces
{
    public interface IHackerNewsClient
    {
        /// <summary>
        /// Gets the IDs of the best stories from Hacker News API
        /// </summary>
        Task<List<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the details of a specific story by its ID
        /// </summary>
        Task<HackerNewsItemContract?> GetStoryAsync(int id, CancellationToken cancellationToken = default);
    }
}
