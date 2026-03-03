using HackerNews.BestStories.Api.Models.ExternalContracts;

namespace HackerNews.BestStories.Tests.Integration.TestData
{
    internal static class HackerNewsItemFactory
    {
        public static HackerNewsItemContract Create(int id, int score)
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