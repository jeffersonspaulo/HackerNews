using FluentAssertions;
using HackerNews.BestStories.Api.Models.Dtos.Response;
using HackerNews.BestStories.Api.Models.ExternalContracts;
using HackerNews.BestStories.Tests.Integration.Factories;
using System.Net.Http.Json;
using System.Net;
using HackerNews.BestStories.Tests.Integration.Fakes;
using HackerNews.BestStories.Tests.Integration.TestData;

namespace HackerNews.BestStories.Tests.Integration
{
    public class BestStoriesEndpointTests
    {
        [Fact]
        public async Task GetBestStories_WhenValidN_ShouldReturn200_AndOrderedByScoreDesc()
        {
            // Arrange
            var fakeClient = new FakeHackerNewsClient(
                bestIds: new List<int> { 1, 2, 3 },
                storiesById: new Dictionary<int, HackerNewsItemContract>
                {
                    [1] = HackerNewsItemFactory.Create(1, score: 10),
                    [2] = HackerNewsItemFactory.Create(2, score: 50),
                    [3] = HackerNewsItemFactory.Create(3, score: 30),
                });

            await using var factory = new CustomWebApplicationFactory(fakeClient);
            var http = factory.CreateClient();

            // Act
            var response = await http.GetAsync("/api/beststories?n=3");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<StoryResponse>>();
            body.Should().NotBeNull();
            body!.Should().HaveCount(3);
            body.Select(x => x.Score).Should().Equal(new[] { 50, 30, 10 });
        }

        [Fact]
        public async Task GetBestStories_WhenNIsInvalid_ShouldReturn400()
        {
            // Arrange
            var fakeClient = new FakeHackerNewsClient(
                bestIds: new List<int> { 1, 2, 3 },
                storiesById: new Dictionary<int, HackerNewsItemContract>
                {
                    [1] = HackerNewsItemFactory.Create(1, 10),
                    [2] = HackerNewsItemFactory.Create(2, 20),
                    [3] = HackerNewsItemFactory.Create(3, 30),
                });

            await using var factory = new CustomWebApplicationFactory(fakeClient);
            var http = factory.CreateClient();

            // Act
            var response = await http.GetAsync("/api/beststories?n=0");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetBestStories_WhenOneStoryFails_ShouldStillReturn200_WithRemaining()
        {
            // Arrange
            var fakeClient = new FakeHackerNewsClient(
                bestIds: new List<int> { 1, 2, 3, 4, 5 },
                storiesById: new Dictionary<int, HackerNewsItemContract>
                {
                    [1] = HackerNewsItemFactory.Create(1, 10),
                    [2] = HackerNewsItemFactory.Create(2, 20),
                    // 3 will throw
                    [4] = HackerNewsItemFactory.Create(4, 40),
                    [5] = HackerNewsItemFactory.Create(5, 50),
                },
                throwOnStoryId: 3);

            await using var factory = new CustomWebApplicationFactory(fakeClient);
            var http = factory.CreateClient();

            // Act
            var response = await http.GetAsync("/api/beststories?n=5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<StoryResponse>>();
            body.Should().NotBeNull();
            body!.Should().HaveCount(4);
            body.Select(x => x.Score).Should().Equal(new[] { 50, 40, 20, 10 });
        }
    }
}
