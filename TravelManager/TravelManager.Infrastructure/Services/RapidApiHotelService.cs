using System.Text.Json;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class RapidApiHotelService : IHotelSearchService
    {
        private readonly HttpClient _httpClient;

        public RapidApiHotelService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ExternalHotelDto>> SearchHotelsAsync(string city, string checkIn, string checkOut)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com.p.rapidapi.com/v1/hotels/search?dest_type=city&search_type=CITY&query={city}&arrival_date={checkIn}&departure_date={checkOut}"),
                Headers =
                {
                    { "X-RapidAPI-Key", "ТВІЙ_КЛЮЧ_З_RAPIDAPI" },
                    { "X-RapidAPI-Host", "booking-com.p.rapidapi.com" },
                }
            };

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new List<ExternalHotelDto>();
            }

            var body = await response.Content.ReadAsStringAsync();

            return ParseHotelJson(body);
        }

        private List<ExternalHotelDto> ParseHotelJson(string jsonResponse)
        {
            var hotels = new List<ExternalHotelDto>();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);

                var results = doc.RootElement.GetProperty("result");

                foreach (var item in results.EnumerateArray().Take(10))
                {
                    hotels.Add(new ExternalHotelDto
                    {
                        Name = item.GetProperty("hotel_name").GetString() ?? "Unknown",
                        Address = item.GetProperty("address").GetString() ?? "Unknown",
                        Price = item.TryGetProperty("min_total_price", out var priceElement) ? priceElement.GetDecimal() : 0,
                        Currency = item.TryGetProperty("currencycode", out var currElement) ? currElement.GetString() ?? "UAH" : "UAH",
                        ImageUrl = item.GetProperty("max_photo_url").GetString() ?? "",
                        ExternalLink = item.GetProperty("url").GetString() ?? ""
                    });
                }
            }
            catch
            {

            }

            return hotels;
        }
    }
}