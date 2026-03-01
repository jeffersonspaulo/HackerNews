using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Models.ExternalContracts;
using HackerNews.BestStories.Api.Shared.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HackerNews.BestStories.Api.Infrastructure.HttpClients
{
    public class HackerNewsClient : IHackerNewsClient
    {
        private readonly HttpClient _httpClient;
        private readonly HackerNewsOptions _options;
        private readonly ILogger<HackerNewsClient> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public HackerNewsClient(HttpClient httpClient, IOptions<HackerNewsOptions> options, ILogger<HackerNewsClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
        }

        public async Task<List<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching best stories IDs from Hacker News API");

                var response = await _httpClient.GetAsync("beststories.json", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var ids = JsonSerializer.Deserialize<List<int>>(content, JsonOptions) ?? new List<int>();

                _logger.LogInformation("Successfully fetched {Count} best stories IDs", ids?.Count ?? 0);

                return ids;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching best stories IDs from Hacker News API");
                throw;
            }
        }

        public async Task<HackerNewsItemContract?> GetStoryAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Fetching story {StoryId} from Hacker News API", id);

                var response = await _httpClient.GetAsync($"item/{id}.json", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch story {StoryId}. Status: {StatusCode}", id, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var story = JsonSerializer.Deserialize<HackerNewsItemContract>(content, JsonOptions);

                return story;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching story {StoryId} from Hacker News API", id);
                return null;
            }
        }
    }
}
