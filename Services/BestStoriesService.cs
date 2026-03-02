using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Models.Dtos.Response;
using HackerNews.BestStories.Api.Services.Interfaces;
using HackerNews.BestStories.Api.Shared.Extensions;
using HackerNews.BestStories.Api.Shared.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace HackerNews.BestStories.Api.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IHackerNewsClient _client;
        private readonly ILogger<BestStoriesService> _logger;
        private readonly BestStoriesOptions _bestStoriesOptions;
        private readonly CacheOptions _cacheOptions;
        private readonly IMemoryCache _cache;

        public BestStoriesService(
            IHackerNewsClient client,
            ILogger<BestStoriesService> logger,
            IOptions<BestStoriesOptions> bestStoriesOptions,
            IOptions<CacheOptions> cacheOptions,
            IMemoryCache cache)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bestStoriesOptions = bestStoriesOptions?.Value ?? throw new ArgumentNullException(nameof(bestStoriesOptions));
            _cacheOptions = cacheOptions?.Value ?? throw new ArgumentNullException(nameof(cacheOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<List<StoryResponse>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default)
        {
            ValidateInput(n);

            _logger.LogInformation("Fetching {Count} best stories", n);

            var bestStoriesIds = await GetBestStoriesIdsCachedAsync(cancellationToken);

            if (bestStoriesIds?.Count == 0)
            {
                _logger.LogWarning("No best stories IDs found");
                return new List<StoryResponse>();
            }

            var idsToFetch = bestStoriesIds.Take(n).ToList();

            var stories = await FetchStoriesWithConcurrencyControlAsync(idsToFetch, cancellationToken);

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

        private async Task<List<int>> GetBestStoriesIdsCachedAsync(CancellationToken cancellationToken)
        {
            const string cacheKey = "hn:beststories:ids";

            if (_cache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds is not null)
                return cachedIds;

            var ids = await _client.GetBestStoriesIdsAsync(cancellationToken) ?? new List<int>();

            _cache.Set(
                cacheKey,
                ids,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheOptions.BestStoriesTtlSeconds)
                });

            return ids;
        }

        private async Task<StoryResponse?> GetStoryCachedAsync(int storyId, CancellationToken cancellationToken)
        {
            var cacheKey = $"hn:story:{storyId}";

            if (_cache.TryGetValue(cacheKey, out StoryResponse? cached))
                return cached;

            var story = await _client.GetStoryAsync(storyId, cancellationToken);
            if (story is null) return null;

            var mapped = new StoryResponse
            {
                Title = story.Title,
                Uri = story.Url,
                PostedBy = story.By,
                Time = DateTimeExtensions.FromUnixTimestamp(story.Time),
                Score = story.Score,
                CommentCount = story.Descendants
            };

            _cache.Set(
                cacheKey,
                mapped,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheOptions.ItemTtlMinutes)
                });

            return mapped;
        }

        private async Task<List<StoryResponse>> FetchStoriesWithConcurrencyControlAsync(List<int> storyIds, CancellationToken cancellationToken)
        {
            using var semaphore = new SemaphoreSlim(_bestStoriesOptions.MaxConcurrency);

            var tasks = storyIds.Select(async id =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await GetStoryCachedAsync(id, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => r is not null)
                .Cast<StoryResponse>()
                .ToList();
        }
    }
}
