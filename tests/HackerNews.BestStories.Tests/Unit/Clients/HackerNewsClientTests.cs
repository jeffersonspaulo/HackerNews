using System.Net;
using System.Text;
using FluentAssertions;
using HackerNews.BestStories.Api.Infrastructure.HttpClients;
using HackerNews.BestStories.Api.Shared.Options;
using HackerNews.BestStories.Tests.Unit.Clients.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HackerNews.BestStories.Tests.Unit.Clients
{
    public class HackerNewsClientTests
    {
        private static HackerNewsClient CreateSut(StubHttpMessageHandler handler, HackerNewsOptions? options = null)
        {
            options ??= new HackerNewsOptions
            {
                BaseUrl = "https://example.test/v0/",
                RequestTimeoutSeconds = 5
            };

            var httpClient = new HttpClient(handler);
            return new HackerNewsClient(
                httpClient,
                Options.Create(options),
                NullLogger<HackerNewsClient>.Instance);
        }

        [Fact]
        public async Task GetBestStoriesIdsAsync_WhenSuccess_ShouldReturnIds_AndCallExpectedEndpoint()
        {
            // Arrange
            var json = "[1,2,3]";
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                req.Method.Should().Be(HttpMethod.Get);
                req.RequestUri!.ToString().Should().Be("https://example.test/v0/beststories.json");

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetBestStoriesIdsAsync();

            // Assert
            result.Should().Equal(new List<int> { 1, 2, 3 });
        }

        [Fact]
        public async Task GetBestStoriesIdsAsync_WhenNonSuccess_ShouldThrowHttpRequestException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return Task.FromResult(response);
            });

            var sut = CreateSut(handler);

            // Act
            Func<Task> act = async () => await sut.GetBestStoriesIdsAsync();

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetStoryAsync_WhenSuccess_ShouldDeserializeAndReturnContract()
        {
            // Arrange
            var json = """
            {
              "id": 123,
              "title": "Hello",
              "url": "https://example.com",
              "by": "jeff",
              "time": 1700000000,
              "score": 42,
              "descendants": 7,
              "type": "story"
            }
            """;

            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                req.RequestUri!.ToString().Should().Be("https://example.test/v0/item/123.json");

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetStoryAsync(123);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(123);
            result.Title.Should().Be("Hello");
            result.Url.Should().Be("https://example.com");
            result.By.Should().Be("jeff");
            result.Time.Should().Be(1700000000);
            result.Score.Should().Be(42);
            result.Descendants.Should().Be(7);
            result.Type.Should().Be("story");
        }

        [Fact]
        public async Task GetStoryAsync_WhenNonSuccessStatus_ShouldReturnNull()
        {
            // Arrange
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                return Task.FromResult(response);
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetStoryAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStoryAsync_WhenHttpRequestException_ShouldReturnNull()
        {
            // Arrange
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                throw new HttpRequestException("network error");
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetStoryAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStoryAsync_WhenTimeout_ShouldReturnNull()
        {
            // Arrange
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                throw new TaskCanceledException("timeout");
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetStoryAsync(1, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStoryAsync_WhenRequestIsCancelled_ShouldPropagateOperationCanceledException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler((req, ct) =>
            {
                throw new OperationCanceledException(ct);
            });

            var sut = CreateSut(handler);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () => await sut.GetStoryAsync(1, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}