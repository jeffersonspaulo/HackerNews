namespace HackerNews.BestStories.Api.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converte um timestamp Unix para DateTime em UTC
        /// </summary>
        public static DateTime FromUnixTimestamp(long unixTimestamp)
        {
            return DateTime.UnixEpoch.AddSeconds(unixTimestamp);
        }
    }
}