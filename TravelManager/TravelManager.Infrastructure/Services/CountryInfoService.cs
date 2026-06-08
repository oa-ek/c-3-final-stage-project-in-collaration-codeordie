using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class CountryInfoService : ICountryInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CountryInfoService> _logger;

        public CountryInfoService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<CountryInfoService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CountryInfo?> GetCountryInfoAsync(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName)) return null;

            var cacheKey = $"country:{countryName.ToLower().Trim()}";

            if (_cache.TryGetValue(cacheKey, out CountryInfo? cached))
                return cached;

            try
            {
                var encoded = Uri.EscapeDataString(countryName);
                var fields = "fields=name,flags,currencies,languages,capital,population,region";

                var response = await _httpClient.GetAsync($"v3.1/name/{encoded}?{fields}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "REST Countries /name/ не знайшло '{Country}', пробуємо /translation/", countryName);
                    response = await _httpClient.GetAsync($"v3.1/translation/{encoded}?{fields}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "REST Countries /translation/ не знайшло '{Country}', пробуємо /name/ з fullText=false", countryName);
                    response = await _httpClient.GetAsync($"v3.1/name/{encoded}?{fields}&fullText=false");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "REST Countries не знайшло країну '{Country}' жодним способом. HTTP {Status}",
                        countryName, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var countries = JsonSerializer.Deserialize<List<CountryInfoDto>>(json);
                var dto = countries?.FirstOrDefault();

                if (dto == null) return null;

                var firstCurrency = dto.Currencies?.FirstOrDefault();

                var result = new CountryInfo
                {
                    CommonName = dto.Name?.Common ?? countryName,
                    OfficialName = dto.Name?.Official ?? countryName,
                    FlagUrl = dto.Flags?.Png ?? dto.Flags?.Svg ?? string.Empty,
                    FlagAlt = dto.Flags?.Alt ?? $"Прапор {countryName}",
                    Capital = dto.Capital?.FirstOrDefault() ?? string.Empty,
                    Region = dto.Region ?? string.Empty,
                    Population = dto.Population,
                    Languages = dto.Languages != null
                                        ? string.Join(", ", dto.Languages.Values)
                                        : string.Empty,
                    CurrencyCode = firstCurrency?.Key ?? string.Empty,
                    CurrencyName = firstCurrency?.Value?.Name ?? string.Empty,
                    CurrencySymbol = firstCurrency?.Value?.Symbol ?? string.Empty,
                };

                _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "REST Countries недоступний для країни '{Country}'", countryName);
                return null;
            }
        }
    }
}
