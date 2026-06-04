using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class RapidApiHotelService : IHotelSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiHost = "apidojo-booking-v1.p.rapidapi.com";

        public RapidApiHotelService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RapidApi:ApiKey"] ?? string.Empty;
        }

        public async Task<List<ExternalHotelDto>> SearchHotelsAsync(string city, string checkIn, string checkOut)
        {
            try
            {
                // Крок 1: отримати dest_id міста
                var destId = await GetDestinationIdAsync(city);
                if (string.IsNullOrEmpty(destId))
                    return new List<ExternalHotelDto>();

                // Крок 2: шукати готелі
                return await SearchPropertiesAsync(destId, checkIn, checkOut);
            }
            catch
            {
                return new List<ExternalHotelDto>();
            }
        }

        private async Task<string?> GetDestinationIdAsync(string city)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{ApiHost}/locations/auto-complete?text={Uri.EscapeDataString(city)}&languagecode=uk"),
                Headers =
                {
                    { "X-RapidAPI-Key", _apiKey },
                    { "X-RapidAPI-Host", ApiHost }
                }
            };

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            // Шукаємо перший результат типу "city"
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("dest_type", out var typeEl) &&
                    typeEl.GetString() == "city" &&
                    item.TryGetProperty("dest_id", out var idEl))
                {
                    return idEl.GetString();
                }
            }

            // Якщо city не знайдено — беремо перший результат
            var first = doc.RootElement.EnumerateArray().FirstOrDefault();
            if (first.ValueKind != JsonValueKind.Undefined &&
                first.TryGetProperty("dest_id", out var fallbackId))
            {
                return fallbackId.GetString();
            }

            return null;
        }

        private async Task<List<ExternalHotelDto>> SearchPropertiesAsync(string destId, string checkIn, string checkOut)
        {
            var url = $"https://{ApiHost}/properties/list" +
                      $"?offset=0&arrival_date={checkIn}&departure_date={checkOut}" +
                      $"&guest_qty=1&dest_ids={destId}&room_qty=1" +
                      $"&search_type=city&languagecode=uk&currency_code=UAH";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers =
                {
                    { "X-RapidAPI-Key", _apiKey },
                    { "X-RapidAPI-Host", ApiHost }
                }
            };

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<ExternalHotelDto>();

            var body = await response.Content.ReadAsStringAsync();
            return ParseProperties(body);
        }

        private List<ExternalHotelDto> ParseProperties(string json)
        {
            var hotels = new List<ExternalHotelDto>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Структура відповіді: { result: [ ... ] }
                if (!root.TryGetProperty("result", out var results))
                    return hotels;

                foreach (var item in results.EnumerateArray().Take(10))
                {
                    var name = item.TryGetProperty("hotel_name", out var n) ? n.GetString() ?? "Невідомо" : "Невідомо";
                    var address = item.TryGetProperty("address", out var a) ? a.GetString() ?? "" : "";
                    var city = item.TryGetProperty("city", out var c) ? c.GetString() ?? "" : "";
                    var fullAddress = string.IsNullOrEmpty(city) ? address : $"{address}, {city}";

                    decimal price = 0;
                    if (item.TryGetProperty("price_breakdown", out var pb) &&
                        pb.TryGetProperty("gross_price", out var gp))
                    {
                        price = gp.ValueKind == JsonValueKind.Number
                            ? gp.GetDecimal()
                            : decimal.TryParse(gp.GetString(), out var parsed) ? parsed : 0;
                    }

                    var currency = item.TryGetProperty("currencycode", out var cur) ? cur.GetString() ?? "UAH" : "UAH";
                    var image = item.TryGetProperty("max_photo_url", out var img) ? img.GetString() ?? "" : "";
                    var link = item.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

                    hotels.Add(new ExternalHotelDto
                    {
                        Name = name,
                        Address = fullAddress,
                        Price = price,
                        Currency = currency,
                        ImageUrl = image,
                        ExternalLink = link
                    });
                }
            }
            catch { }

            return hotels;
        }
    }
}