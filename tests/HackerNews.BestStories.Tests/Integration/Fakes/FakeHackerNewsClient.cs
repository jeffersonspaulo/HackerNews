using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Models.ExternalContracts;

namespace HackerNews.BestStories.Tests.Integration.Fakes
{
    public sealed class FakeHackerNewsClient : IHackerNewsClient
    {
        private readonly List<int> _bestIds;
        private readonly Dictionary<int, HackerNewsItemContract> _storiesById;
        private readonly int? _throwOnStoryId;

        public FakeHackerNewsClient(
            List<int> bestIds,
            Dictionary<int, HackerNewsItemContract> storiesById,
            int? throwOnStoryId = null)
        {
            _bestIds = bestIds;
            _storiesById = storiesById;
            _throwOnStoryId = throwOnStoryId;
        }

        public Task<List<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_bestIds);

        public Task<HackerNewsItemContract?> GetStoryAsync(int id, CancellationToken cancellationToken = default)
        {
            if (_throwOnStoryId.HasValue && id == _throwOnStoryId.Value)
                throw new TimeoutException("simulated failure");

            _storiesById.TryGetValue(id, out var story);
            return Task.FromResult<HackerNewsItemContract?>(story);
        }
    }
}
