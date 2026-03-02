namespace HackerNews.BestStories.Api.Shared.Options
{
    public class CacheOptions
    {
        public int BestStoriesTtlSeconds { get; set; }
        public int ItemTtlMinutes { get; set; }
    }
}
