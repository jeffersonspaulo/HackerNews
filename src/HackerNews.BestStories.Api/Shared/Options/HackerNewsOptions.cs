namespace HackerNews.BestStories.Api.Shared.Options
{
    public class HackerNewsOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int RequestTimeoutSeconds { get; set; }
    }
}
