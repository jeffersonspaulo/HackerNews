using FluentAssertions;
using HackerNews.BestStories.Api.Infrastructure.HttpClients.Interfaces;
using HackerNews.BestStories.Api.Models.ExternalContracts;
using HackerNews.BestStories.Api.Services;
using HackerNews.BestStories.Api.Shared.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace HackerNews.BestStories.Tests.Unit.Services
{
    public class BestStoriesServiceTests
    {
        private static BestStoriesService CreateSut(
            Mock<IHackerNewsClient> clientMock,
            IMemoryCache cache,
            BestStoriesOptions? bestStoriesOptions = null,
            CacheOptions? cacheOptions = null)
        {
            bestStoriesOptions ??= new BestStoriesOptions
            {
                DefaultN = 10,
                MaxN = 200,
                MaxConcurrency = 10
            };

            cacheOptions ??= new CacheOptions
            {
                BestStoriesTtlSeconds = 60,
                ItemTtlMinutes = 10
            };

            return new BestStoriesService(
                clientMock.Object,
                NullLogger<BestStoriesService>.Instance,
                Options.Create(bestStoriesOptions),
                Options.Create(cacheOptions),
                cache);
        }

        private static IMemoryCache CreateMemoryCache()
            => new MemoryCache(new MemoryCacheOptions());

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(201)]
        public async Task GetBestStoriesAsync_WhenNIsInvalid_ShouldThrowArgumentException(int n)
        {
            // Arrange
            var clientMock = new Mock<IHackerNewsClient>(MockBehavior.Strict);
            using var cache = CreateMemoryCache();
            var sut = CreateSut(clientMock, cache);

            // Act
            Func<Task> act = async () => await sut.GetBestStoriesAsync(n);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("N must be between 1 and 200*");
        }

        [Fact]
        public async Task GetBestStoriesAsync_ShouldReturnStoriesOrderedByScoreDesc()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };

            var clientMock = new Mock<IHackerNewsClient>(MockBehavior.Strict);

            clientMock
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids);

            clientMock
                .Setup(c => c.GetStoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(1, 10));

            clientMock
                .Setup(c => c.GetStoryAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(2, 50));

            clientMock
                .Setup(c => c.GetStoryAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(3, 30));

            using var cache = CreateMemoryCache();
            var sut = CreateSut(clientMock, cache);

            // Act
            var result = await sut.GetBestStoriesAsync(3);

            // Assert
            result.Should().HaveCount(3);
            result.Select(s => s.Score)
                  .Should().Equal(new[] { 50, 30, 10 });
        }

        [Fact]
        public async Task GetBestStoriesAsync_WhenBestStoryIdsCacheHit_ShouldNotCallClientAgain()
        {
            // Arrange
            var ids = Enumerable.Range(1, 10).ToList();

            var clientMock = new Mock<IHackerNewsClient>(MockBehavior.Loose);

            clientMock
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids);

            clientMock
                .Setup(c => c.GetStoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => TestHnItem(id, id));

            using var cache = CreateMemoryCache();
            var sut = CreateSut(clientMock, cache);

            // Act
            await sut.GetBestStoriesAsync(5);
            await sut.GetBestStoriesAsync(5);

            // Assert
            clientMock.Verify(
                c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetBestStoriesAsync_WhenSomeStoriesFail_ShouldReturnRemaining()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3, 4, 5 };

            var clientMock = new Mock<IHackerNewsClient>(MockBehavior.Strict);

            clientMock
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids);

            clientMock.Setup(c => c.GetStoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(1, 10));

            clientMock.Setup(c => c.GetStoryAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(2, 20));

            clientMock.Setup(c => c.GetStoryAsync(3, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("boom"));

            clientMock.Setup(c => c.GetStoryAsync(4, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(4, 40));

            clientMock.Setup(c => c.GetStoryAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHnItem(5, 50));

            using var cache = CreateMemoryCache();
            var sut = CreateSut(clientMock, cache);

            // Act
            var result = await sut.GetBestStoriesAsync(5);

            // Assert
            result.Should().HaveCount(4);
            result.Select(x => x.Score)
                  .Should().Equal(new[] { 50, 40, 20, 10 });
        }

        private static HackerNewsItemContract TestHnItem(int id, int score)
        {
            return new HackerNewsItemContract
            {
                Id = id,
                Title = $"Story {id}",
                Url = $"https://example.com/{id}",
                By = $"user{id}",
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Score = score,
                Descendants = id * 2,
                Type = "story"
            };
        }
    }
}