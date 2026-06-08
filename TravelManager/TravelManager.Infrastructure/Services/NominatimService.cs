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

        public async Task<GeocodingResult?> GeocodeAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var cacheKey = $"nominatim:query:{query.ToLower().Trim()}";
            if (_cache.TryGetValue(cacheKey, out GeocodingResult? cached))
                return cached;

            try
            {
                var encoded = Uri.EscapeDataString(query);

                var urlUk = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";
                var responseUk = await _httpClient.GetAsync(urlUk);
                responseUk.EnsureSuccessStatusCode();

                var jsonUk = await responseUk.Content.ReadAsStringAsync();
                var resultsUk = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonUk);
                var firstUk = resultsUk?.FirstOrDefault();

                if (firstUk == null) return null;

                string? countryEn = null;
                try
                {
                    var urlEn = $"search?format=json&limit=1&accept-language=en&q={encoded}&addressdetails=1";
                    var responseEn = await _httpClient.GetAsync(urlEn);
                    if (responseEn.IsSuccessStatusCode)
                    {
                        var jsonEn = await responseEn.Content.ReadAsStringAsync();
                        var resultsEn = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonEn);
                        countryEn = resultsEn?.FirstOrDefault()?.Address?.Country;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не вдалося отримати англійську назву країни для '{Query}'", query);
                }

                var result = MapToResult(firstUk, countryEn);

                _cache.Set(cacheKey, result, TimeSpan.FromDays(7));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim недоступний для запиту '{Query}'", query);
                return null;
            }
        }

        public async Task<GeocodingResult?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var cacheKey = $"nominatim:address:{address.ToLower().Trim()}";
            if (_cache.TryGetValue(cacheKey, out GeocodingResult? cached))
                return cached;

            try
            {
                var encoded = Uri.EscapeDataString(address);

                var urlUk = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";
                var responseUk = await _httpClient.GetAsync(urlUk);
                responseUk.EnsureSuccessStatusCode();

                var jsonUk = await responseUk.Content.ReadAsStringAsync();
                var resultsUk = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonUk);
                var firstUk = resultsUk?.FirstOrDefault();

                if (firstUk == null) return null;

                string? countryEn = null;
                try
                {
                    var urlEn = $"search?format=json&limit=1&accept-language=en&q={encoded}&addressdetails=1";
                    var responseEn = await _httpClient.GetAsync(urlEn);
                    if (responseEn.IsSuccessStatusCode)
                    {
                        var jsonEn = await responseEn.Content.ReadAsStringAsync();
                        var resultsEn = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonEn);
                        countryEn = resultsEn?.FirstOrDefault()?.Address?.Country;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не вдалося отримати англійську назву країни для адреси '{Address}'", address);
                }

                var result = MapToResult(firstUk, countryEn);

                _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim недоступний для адреси '{Address}'", address);
                return null;
            }
        }

        private static GeocodingResult MapToResult(NominatimResultDto dto, string? countryOverride = null)
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
                Country = countryOverride ?? dto.Address?.Country
            };
        }
    }
}
