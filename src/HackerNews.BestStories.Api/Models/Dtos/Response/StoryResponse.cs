namespace HackerNews.BestStories.Api.Models.Dtos.Response
{
    public class StoryResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }
    }
}
