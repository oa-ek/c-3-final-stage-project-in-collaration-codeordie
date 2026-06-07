// TravelManager.Infrastructure/Services/AiRecommendationService.cs

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
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

                // 1) Отримуємо реальні місця з Foursquare
                var realPlaces = await FetchFoursquarePlacesAsync(cityName, country);

                var apiKey = _configuration["Groq:ApiKey"];
                var model = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

                _logger.LogInformation("=== GROQ START === City: {City}", cityName);

                // 2) Передаємо реальні місця як контекст у промпт
                var attractionsContext = realPlaces.Attractions.Any()
                    ? "Реальні популярні атракції з Foursquare (використай ці назви):\n" +
                      string.Join("\n", realPlaces.Attractions.Select(p => $"- {p.Name} (рейтинг: {p.Rating}, категорія: {p.Category})"))
                    : "Використай свої знання про найвідоміші атракції міста.";

                var restaurantsContext = realPlaces.Restaurants.Any()
                    ? "Реальні популярні ресторани з Foursquare (використай ці назви):\n" +
                      string.Join("\n", realPlaces.Restaurants.Select(p => $"- {p.Name} (рейтинг: {p.Rating}, категорія: {p.Category})"))
                    : "Використай свої знання про найвідоміші ресторани міста.";

                var prompt = $@"Ти досвідчений тревел-гід. Надай детальні рекомендації для туриста в {locationStr}.

{attractionsContext}

{restaurantsContext}

ВАЖЛИВО: 
- Використовуй ТІЛЬКИ реальні назви з наданих списків вище
- Для кожного місця напиши детальний опис 3-4 речення: що це, чому варто відвідати, цікаві факти, практичні поради
- Для ресторанів: кухня, фірмові страви, атмосфера, приблизний чек
- Для порад: конкретна практична інформація про транспорт, валюту, безпеку, етикет, типові пастки для туристів

Відповідай ВИКЛЮЧНО валідним JSON без жодного markdown. Лише JSON:
{{
  ""attractions"": [
    {{""name"":""точна назва"",""description"":""3-4 речення детального опису"",""category"":""тип"",""priceRange"":""Free/$/$$/$$$"",""emoji"":""🏛"",""bestTime"":""найкращий час""}}
  ],
  ""restaurants"": [
    {{""name"":""точна назва"",""description"":""3-4 речення детального опису"",""category"":""тип кухні"",""priceRange"":""$/$$/$$$"",""emoji"":""🍽"",""bestTime"":""обід/вечеря""}}
  ],
  ""tips"": [
    {{""title"":""коротка назва"",""body"":""3-4 речення конкретної поради"",""emoji"":""💡""}}
  ]
}}
attractions рівно 8, restaurants рівно 6, tips рівно 7, мова: українська, ТІЛЬКИ JSON.";

                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 6000,
                    response_format = new { type = "json_object" }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync("openai/v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("=== GROQ HTTP STATUS: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("=== GROQ ERROR: {Response}", responseJson);
                    return null;
                }

                using var doc = JsonDocument.Parse(responseJson);
                var textContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                var cleanJson = textContent.Trim();
                if (cleanJson.StartsWith("```"))
                    cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();

                using var resultDoc = JsonDocument.Parse(cleanJson);
                var root = resultDoc.RootElement;

                var result = new AiRecommendationResult
                {
                    CityName = cityName,
                    Attractions = ParsePlaces(root, "attractions"),
                    Restaurants = ParsePlaces(root, "restaurants"),
                    PracticalTips = ParseTips(root)
                };

                _logger.LogInformation("=== SUCCESS === Attractions: {A}, Restaurants: {R}, Tips: {T}",
                    result.Attractions.Count, result.Restaurants.Count, result.PracticalTips.Count);

                _cache.Set(cacheKey, result, CacheDuration);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== EXCEPTION === {Message}", ex.Message);
                return null;
            }
        }
        public async Task<string> AnalyzeExpensesAsync(string destination, int days, string currency, decimal totalAmount, Dictionary<string, decimal> expensesByCategory)
        {
            var groqKey = _configuration["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(groqKey))
            {
                return "<div style='color: #ef4444; font-weight: bold;'>API ключ Groq відсутній.</div>";
            }

            var categoriesText = string.Join("\n", expensesByCategory.Select(kvp => $"- {kvp.Key}: {kvp.Value} {currency}"));

            var prompt = $@"Ти фінансовий тревел-експерт. Проаналізуй витрати мандрівника:

Локація: {destination}
Тривалість: {days} днів
Загальний бюджет: {totalAmount} {currency}

Витрати за категоріями:
{categoriesText}

Дай відповідь **виключно українською** мовою. Використовуй HTML-теги для красивого форматування.";

            try
            {
                // Використовуємо той самий _httpClient, що і в рекомендаціях
                var model = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                new { role = "system", content = "Ти досвідчений фінансовий тревел-аналітик. Відповідай чітко, професійно та корисною." },
                new { role = "user", content = prompt }
            },
                    temperature = 0.6,
                    max_tokens = 1500
                };

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqKey);

                var response = await _httpClient.PostAsJsonAsync("openai/v1/chat/completions", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Groq AnalyzeExpenses Error: {Status} - {Error}", response.StatusCode, errorContent);
                    return $"<div style='color: #ef4444;'>Помилка API: {response.StatusCode}</div>";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var resultText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return resultText ?? "AI не зміг сформувати відповідь.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyzeExpensesAsync exception");
                return $"<div style='color: #ef4444;'>Помилка: {ex.Message}</div>";
            }
        }
        // ── Foursquare Places ──────────────────────────────────────────────
        private async Task<FoursquarePlacesResult> FetchFoursquarePlacesAsync(string cityName, string? country)
        {
            var result = new FoursquarePlacesResult();
            var fsqKey = _configuration["Foursquare:ApiKey"];
            if (string.IsNullOrWhiteSpace(fsqKey))
                return result;

            var locationQuery = string.IsNullOrWhiteSpace(country) ? cityName : $"{cityName},{country}";

            try
            {
                using var fsqClient = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                });
                fsqClient.DefaultRequestHeaders.Add("Authorization", fsqKey);
                fsqClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // Атракції
                result.Attractions = await FetchFoursquareCategoryAsync(
                    fsqClient, locationQuery, "16000", 10); // 16000 = Arts & Entertainment

                // Ресторани
                result.Restaurants = await FetchFoursquareCategoryAsync(
                    fsqClient, locationQuery, "13000", 8); // 13000 = Dining and Drinking
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Foursquare fetch failed: {Message}", ex.Message);
            }

            return result;
        }

        private async Task<List<FoursquarePlace>> FetchFoursquareCategoryAsync(
            HttpClient client, string location, string categoryId, int limit)
        {
            var url = $"https://api.foursquare.com/v3/places/search" +
                      $"?near={Uri.EscapeDataString(location)}" +
                      $"&categories={categoryId}" +
                      $"&sort=RATING" +
                      $"&limit={limit}" +
                      $"&fields=name,rating,categories";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<FoursquarePlace>();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("=== FOURSQUARE RESPONSE (first 500): {Json}",
    json.Length > 500 ? json.Substring(0, 500) : json);
            using var doc = JsonDocument.Parse(json);

            var places = new List<FoursquarePlace>();
            if (!doc.RootElement.TryGetProperty("results", out var results)) return places;

            foreach (var item in results.EnumerateArray())
            {
                var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var rating = item.TryGetProperty("rating", out var r) ? r.GetDouble() : 0;
                var category = "";
                if (item.TryGetProperty("categories", out var cats) && cats.GetArrayLength() > 0)
                    category = cats[0].TryGetProperty("name", out var cn) ? cn.GetString() ?? "" : "";

                if (!string.IsNullOrEmpty(name))
                    places.Add(new FoursquarePlace { Name = name, Rating = rating, Category = category });
            }

            return places;
        }

        // ── Parsers ────────────────────────────────────────────────────────
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

        // ── Helper types ───────────────────────────────────────────────────
        private class FoursquarePlacesResult
        {
            public List<FoursquarePlace> Attractions { get; set; } = new();
            public List<FoursquarePlace> Restaurants { get; set; } = new();
        }

        private class FoursquarePlace
        {
            public string Name { get; set; } = "";
            public double Rating { get; set; }
            public string Category { get; set; } = "";
        }
    }
}
