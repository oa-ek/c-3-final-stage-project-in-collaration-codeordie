using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class NominatimService : INominatimService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NominatimService> _logger;

        public NominatimService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<NominatimService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Геокодування по назві міста/країни
        /// </summary>
        public async Task<GeocodingResult?> GeocodeAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var cacheKey = $"nominatim:query:{query.ToLower().Trim()}";
            if (_cache.TryGetValue(cacheKey, out GeocodingResult? cached))
                return cached;

            try
            {
                var encoded = Uri.EscapeDataString(query);
                var url = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResultDto>>(json);
                var first = results?.FirstOrDefault();

                if (first == null) return null;

                var result = MapToResult(first);

                // Кешуємо на 7 днів — координати міст не змінюються
                _cache.Set(cacheKey, result, TimeSpan.FromDays(7));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim недоступний для запиту '{Query}'", query);
                return null;
            }
        }

        /// <summary>
        /// Геокодування по адресі (для готелів)
        /// </summary>
        public async Task<GeocodingResult?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var cacheKey = $"nominatim:address:{address.ToLower().Trim()}";
            if (_cache.TryGetValue(cacheKey, out GeocodingResult? cached))
                return cached;

            try
            {
                var encoded = Uri.EscapeDataString(address);
                var url = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResultDto>>(json);
                var first = results?.FirstOrDefault();

                if (first == null) return null;

                var result = MapToResult(first);

                // Кешуємо на 24 години
                _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim недоступний для адреси '{Address}'", address);
                return null;
            }
        }

        private static GeocodingResult MapToResult(NominatimResultDto dto)
        {
            double.TryParse(dto.Lat,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lat);
            double.TryParse(dto.Lon,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lon);

            return new GeocodingResult
            {
                Latitude = lat,
                Longitude = lon,
                DisplayName = dto.DisplayName,
                Country = dto.Address?.Country
            };
        }
    }

}
