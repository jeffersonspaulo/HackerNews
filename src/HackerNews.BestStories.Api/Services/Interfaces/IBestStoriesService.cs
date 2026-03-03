using HackerNews.BestStories.Api.Models.Dtos.Response;

namespace HackerNews.BestStories.Api.Services.Interfaces
{
    public interface IBestStoriesService
    {
        /// <summary>
        /// Gets the best n stories sorted by score in descending order
        /// </summary>
        /// <param name="n">Number of stories to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of best stories</returns>
        Task<List<StoryResponse>> GetBestStoriesAsync(int? n, CancellationToken cancellationToken = default);
    }
}
