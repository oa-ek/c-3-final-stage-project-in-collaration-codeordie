// TravelManager.Infrastructure/Services/AiRecommendationService.cs

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AiRecommendationService> _logger;
        private readonly IConfiguration _configuration;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

        public AiRecommendationService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<AiRecommendationService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AiRecommendationResult?> GetRecommendationsAsync(
            string cityName,
            string? country,
            string language = "uk")
        {
            var cacheKey = $"ai_rec:{cityName.ToLower()}:{country?.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out AiRecommendationResult? cached))
                return cached;

            try
            {
                var locationStr = string.IsNullOrWhiteSpace(country)
                    ? cityName
                    : $"{cityName}, {country}";

                var apiKey = _configuration["Gemini:ApiKey"];
                _logger.LogInformation("=== GEMINI START === City: {City}, ApiKey starts with: {KeyStart}",
                    cityName, apiKey?.Substring(0, Math.Min(10, apiKey?.Length ?? 0)));

                var prompt = $@"Ти досвідчений тревел-гід. Надай рекомендації для туриста, який відвідує {locationStr}.

Відповідай ВИКЛЮЧНО валідним JSON без жодного markdown. Лише JSON:
{{
  ""attractions"": [
    {{""name"":""назва"",""description"":""опис"",""category"":""тип"",""priceRange"":""Free"",""emoji"":""🏛"",""bestTime"":""будь-коли""}}
  ],
  ""restaurants"": [
    {{""name"":""назва"",""description"":""опис"",""category"":""тип"",""priceRange"":""$$"",""emoji"":""🍽"",""bestTime"":""обід""}}
  ],
  ""tips"": [
    {{""title"":""порада"",""body"":""деталі"",""emoji"":""💡""}}
  ]
}}
attractions рівно 5, restaurants рівно 4, tips рівно 4, мова: українська, ТІЛЬКИ JSON.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 3000,
                        responseMimeType = "application/json"
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
                _logger.LogInformation("=== GEMINI REQUEST URL: {Url}", url.Replace(apiKey ?? "", "***KEY***"));

                var response = await _httpClient.PostAsync(url, content);

                _logger.LogInformation("=== GEMINI HTTP STATUS: {Status}", response.StatusCode);

                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("=== GEMINI RAW RESPONSE (first 500): {Response}",
                    responseJson.Length > 500 ? responseJson.Substring(0, 500) : responseJson);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("=== GEMINI ERROR RESPONSE: {Response}", responseJson);
                    return null;
                }

                using var doc = JsonDocument.Parse(responseJson);

                var textContent = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                _logger.LogInformation("=== GEMINI TEXT CONTENT (first 300): {Text}",
                    textContent.Length > 300 ? textContent.Substring(0, 300) : textContent);

                var cleanJson = textContent.Trim();
                if (cleanJson.StartsWith("```"))
                {
                    cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
                }

                using var resultDoc = JsonDocument.Parse(cleanJson);
                var root = resultDoc.RootElement;

                var result = new AiRecommendationResult
                {
                    CityName = cityName,
                    Attractions = ParsePlaces(root, "attractions"),
                    Restaurants = ParsePlaces(root, "restaurants"),
                    PracticalTips = ParseTips(root)
                };

                _logger.LogInformation("=== GEMINI SUCCESS === Attractions: {A}, Restaurants: {R}, Tips: {T}",
                    result.Attractions.Count, result.Restaurants.Count, result.PracticalTips.Count);

                _cache.Set(cacheKey, result, CacheDuration);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== GEMINI EXCEPTION === Type: {Type}, Message: {Message}",
                    ex.GetType().Name, ex.Message);
                return null;
            }
        }

        private static List<AiPlaceCard> ParsePlaces(JsonElement root, string arrayName)
        {
            var list = new List<AiPlaceCard>();
            if (!root.TryGetProperty(arrayName, out var arr)) return list;
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(new AiPlaceCard
                {
                    Name = GetString(item, "name"),
                    Description = GetString(item, "description"),
                    Category = GetString(item, "category"),
                    PriceRange = GetString(item, "priceRange"),
                    Emoji = GetString(item, "emoji", "📍"),
                    BestTime = GetString(item, "bestTime")
                });
            }
            return list;
        }

        private static List<AiTip> ParseTips(JsonElement root)
        {
            var list = new List<AiTip>();
            if (!root.TryGetProperty("tips", out var arr)) return list;
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(new AiTip
                {
                    Title = GetString(item, "title"),
                    Body = GetString(item, "body"),
                    Emoji = GetString(item, "emoji", "💡")
                });
            }
            return list;
        }

        private static string GetString(JsonElement el, string prop, string fallback = "") =>
            el.TryGetProperty(prop, out var v) ? v.GetString() ?? fallback : fallback;
    }
}