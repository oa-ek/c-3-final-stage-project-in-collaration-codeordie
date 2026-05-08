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
        /// Геокодування по назві міста/країни.
        /// DisplayName повертається українською (для відображення),
        /// Country — англійською (для REST Countries API).
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

                // Запит 1: українська локалізація — для DisplayName та координат
                var urlUk = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";
                var responseUk = await _httpClient.GetAsync(urlUk);
                responseUk.EnsureSuccessStatusCode();

                var jsonUk = await responseUk.Content.ReadAsStringAsync();
                var resultsUk = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonUk);
                var firstUk = resultsUk?.FirstOrDefault();

                if (firstUk == null) return null;

                // Запит 2: англійська локалізація — тільки для назви країни (потрібна REST Countries API)
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
        /// Геокодування по адресі (для готелів).
        /// Country зберігається англійською для сумісності з REST Countries API.
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

                // Запит 1: українська локалізація — для DisplayName та координат
                var urlUk = $"search?format=json&limit=1&accept-language=uk&q={encoded}&addressdetails=1";
                var responseUk = await _httpClient.GetAsync(urlUk);
                responseUk.EnsureSuccessStatusCode();

                var jsonUk = await responseUk.Content.ReadAsStringAsync();
                var resultsUk = JsonSerializer.Deserialize<List<NominatimResultDto>>(jsonUk);
                var firstUk = resultsUk?.FirstOrDefault();

                if (firstUk == null) return null;

                // Запит 2: англійська локалізація — тільки для назви країни
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

        /// <summary>
        /// countryOverride — англійська назва країни для REST Countries API.
        /// Якщо не передана — береться з українського результату (як fallback).
        /// </summary>
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
                // Пріоритет: англійська назва (для API) → українська (fallback)
                Country = countryOverride ?? dto.Address?.Country
            };
        }
    }
}
