using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Models.Dtos.Response;
using HackerNews.BestStories.Api.Services.Interfaces;
using HackerNews.BestStories.Api.Shared.Extensions;
using HackerNews.BestStories.Api.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HackerNews.BestStories.Api.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IHackerNewsClient _client;
        private readonly ILogger<BestStoriesService> _logger;
        private readonly BestStoriesOptions _bestStoriesOptions;

        public BestStoriesService(
            IHackerNewsClient client,
            ILogger<BestStoriesService> logger,
            IOptions<BestStoriesOptions> bestStoriesOptions)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bestStoriesOptions = bestStoriesOptions?.Value ?? throw new ArgumentNullException(nameof(bestStoriesOptions));
        }

        public async Task<List<StoryResponse>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default)
        {
            ValidateInput(n);

            _logger.LogInformation("Fetching {Count} best stories", n);

            // Get best stories IDs
            var bestStoriesIds = await _client.GetBestStoriesIdsAsync(cancellationToken);

            if (bestStoriesIds.Count == 0)
            {
                _logger.LogWarning("No best stories IDs found");
                return new List<StoryResponse>();
            }

            // Take only the first n IDs
            var idsToFetch = bestStoriesIds.Take(n).ToList();

            // Fetch story details
            var stories = new List<StoryResponse>();
            foreach (var storyId in idsToFetch)
            {
                var story = await _client.GetStoryAsync(storyId, cancellationToken);
                if (story != null)
                {
                    stories.Add(new StoryResponse
                    {
                        Title = story.Title,
                        Uri = story.Url,
                        PostedBy = story.By,
                        Time = DateTimeExtensions.FromUnixTimestamp(story.Time),
                        Score = story.Score,
                        CommentCount = story.Descendants
                    });
                }
            }

            // Sort by score descending
            var sortedStories = stories.OrderByDescending(s => s.Score).ToList();

            _logger.LogInformation("Successfully fetched {Count} best stories", sortedStories.Count);

            return sortedStories;
        }

        private void ValidateInput(int n)
        {
            if (n < 1 || n > _bestStoriesOptions.MaxN)
            {
                throw new ArgumentException(
                    $"N must be between 1 and {_bestStoriesOptions.MaxN}",
                    nameof(n));
            }
        }
    }
}
