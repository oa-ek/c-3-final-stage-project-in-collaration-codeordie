using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateService> _logger;

        private const string AllRatesCacheKey = "nbu:all_rates";

        public ExchangeRateService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<ExchangeRateInfo>> GetRatesAsync(IEnumerable<string> currencyCodes)
        {
            var allRates = await GetAllRatesAsync();
            if (allRates == null) return new List<ExchangeRateInfo>();

            var codes = currencyCodes.Select(c => c.ToUpper()).ToHashSet();
            return allRates
                .Where(r => codes.Contains(r.CurrencyCode.ToUpper()))
                .ToList();
        }

        public async Task<ExchangeRateInfo?> GetRateAsync(string currencyCode)
        {
            var allRates = await GetAllRatesAsync();
            return allRates?.FirstOrDefault(r =>
                r.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<ExchangeRateInfo>?> GetAllRatesAsync()
        {
            if (_cache.TryGetValue(AllRatesCacheKey, out List<ExchangeRateInfo>? cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync(
                    "NBUStatService/v1/statdirectory/exchange?json");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonSerializer.Deserialize<List<ExchangeRateDto>>(json);

                if (dtos == null) return null;

                var result = dtos.Select(d => new ExchangeRateInfo
                {
                    CurrencyCode = d.CurrencySymbol ?? string.Empty,
                    CurrencyName = d.CurrencyName ?? string.Empty,
                    RateToUah = d.Rate,
                    ExchangeDate = d.ExchangeDate ?? string.Empty
                }).ToList();

                // Кешуємо до кінця дня — НБУ оновлює курси раз на день
                var midnight = DateTime.Today.AddDays(1);
                _cache.Set(AllRatesCacheKey, result, midnight);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NBU API недоступний");
                return null;
            }
        }
    }
}
