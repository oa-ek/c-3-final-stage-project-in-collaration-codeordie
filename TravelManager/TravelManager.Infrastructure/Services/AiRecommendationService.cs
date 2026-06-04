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


    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromHours(6);

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
            var cacheKey =
                $"ai_rec:{cityName.ToLower()}:{country?.ToLower()}";

            if (_cache.TryGetValue(cacheKey,
                out AiRecommendationResult? cached))
            {
                return cached;
            }

            try
            {
                var locationStr =
                    string.IsNullOrWhiteSpace(country)
                        ? cityName
                        : $"{cityName}, {country}";

                var prompt = $@"
Ти досвідчений тревел-гід.

Надай рекомендації для міста {locationStr}.

Відповідай ТІЛЬКИ валідним JSON.

Формат:

{{
  ""attractions"": [
    {{
      ""name"": """",
      ""description"": """",
      ""category"": """",
      ""priceRange"": """",
      ""emoji"": """",
      ""bestTime"": """"
    }}
  ],
  ""restaurants"": [
    {{
      ""name"": """",
      ""description"": """",
      ""category"": """",
      ""priceRange"": """",
      ""emoji"": """",
      ""bestTime"": """"
    }}
  ],
  ""tips"": [
    {{
      ""title"": """",
      ""body"": """",
      ""emoji"": """"
    }}
  ]
}}

Вимоги:
- attractions рівно 5
- restaurants рівно 4
- tips рівно 4
- українська мова
- жодного markdown
- лише JSON
";


                var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 2000
                }
            };

                var json =
                    JsonSerializer.Serialize(requestBody);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                var apiKey =
                    _configuration["Gemini:ApiKey"];

                var response =
                    await _httpClient.PostAsync(
                        $"v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                        content);

                response.EnsureSuccessStatusCode();

                var responseJson =
                    await response.Content.ReadAsStringAsync();

                using var doc =
                    JsonDocument.Parse(responseJson);

                var textContent =
                    doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "";

                using var resultDoc =
                    JsonDocument.Parse(textContent.Trim());

                var root = resultDoc.RootElement;

                var result =
                    new AiRecommendationResult
                    {
                        CityName = cityName,
                        Attractions =
                            ParsePlaces(root, "attractions"),
                        Restaurants =
                            ParsePlaces(root, "restaurants"),
                        PracticalTips =
                            ParseTips(root)
                    };

                _cache.Set(
                    cacheKey,
                    result,
                    CacheDuration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Gemini recommendation failed for {City}",
                    cityName);

                return null;
            }
        }

        private static List<AiPlaceCard> ParsePlaces(
            JsonElement root,
            string arrayName)
        {
            var list = new List<AiPlaceCard>();

            if (!root.TryGetProperty(arrayName, out var arr))
                return list;

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

        private static List<AiTip> ParseTips(
            JsonElement root)
        {
            var list = new List<AiTip>();

            if (!root.TryGetProperty("tips", out var arr))
                return list;

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

        private static string GetString(
            JsonElement el,
            string prop,
            string fallback = "")
        {
            return el.TryGetProperty(prop, out var v)
                ? v.GetString() ?? fallback
                : fallback;
        }
  

  }
}
