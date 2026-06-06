using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class PexelsProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PexelsProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET /PexelsProxy/Video?query=Prague
        [HttpGet]
        public async Task<IActionResult> Video([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("query is required");

            var apiKey = _configuration["Pexels:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(503, "Pexels API key not configured");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var searchQuery = Uri.EscapeDataString(query.Trim() + " city");
            var url = $"https://api.pexels.com/videos/search?query={searchQuery}&per_page=15&orientation=landscape";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Pexels request failed");

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
