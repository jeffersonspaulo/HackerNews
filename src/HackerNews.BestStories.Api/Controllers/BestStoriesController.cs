using HackerNews.BestStories.Api.Models.Dtos.Response;
using HackerNews.BestStories.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.BestStories.Api.Controllers
{
    [ApiController]
    [Route("api/beststories")]
    public class BestStoriesController : ControllerBase
    {
        private readonly IBestStoriesService _bestStoriesService;
        private readonly ILogger<BestStoriesController> _logger;

        public BestStoriesController(IBestStoriesService bestStoriesService, ILogger<BestStoriesController> logger)
        {
            _bestStoriesService = bestStoriesService ?? throw new ArgumentNullException(nameof(bestStoriesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the best n stories from Hacker News API
        /// </summary>
        /// <param name="n">Number of stories to retrieve (1-200, default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array of best stories sorted by score in descending order</returns>
        /// <response code="200">Returns the array of best stories</response>
        /// <response code="400">If n is invalid (not between 1 and 200)</response>
        /// <response code="500">If an error occurred while fetching stories</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<StoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBestStories([FromQuery] int n = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Request to get {Count} best stories", n);

                var stories = await _bestStoriesService.GetBestStoriesAsync(n, cancellationToken);
                return Ok(stories);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching best stories from Hacker News API");
                return StatusCode(500, new { error = "Failed to fetch stories from external API" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching best stories");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }
    }
}
