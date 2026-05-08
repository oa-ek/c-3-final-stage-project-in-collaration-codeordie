using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class WeatherApiService : IWeatherApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WeatherApiService> _logger;

        public WeatherApiService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<WeatherApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<WeatherInfo?> GetWeatherAsync(double latitude, double longitude)
        {
            // Округлюємо до 2 знаків для кращого кешування
            var lat = Math.Round(latitude, 2);
            var lon = Math.Round(longitude, 2);
            var cacheKey = $"weather:{lat}:{lon}";

            if (_cache.TryGetValue(cacheKey, out WeatherInfo? cached))
            {
                _logger.LogInformation("Weather cache hit for {Lat},{Lon}", lat, lon);
                return cached;
            }

            try
            {
                // ВИПРАВЛЕННЯ: явно використовуємо InvariantCulture для форматування координат.
                // Без цього на системі з uk-UA локаллю double 40.4168 перетворюється на "40,4168"
                // і Open-Meteo API повертає помилку бо не розуміє кому як десятковий роздільник.
                var latStr = lat.ToString("F4", CultureInfo.InvariantCulture);
                var lonStr = lon.ToString("F4", CultureInfo.InvariantCulture);

                var url = $"v1/forecast" +
                          $"?latitude={latStr}&longitude={lonStr}" +
                          $"&current=temperature_2m,wind_speed_10m,relative_humidity_2m,weather_code,is_day" +
                          $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
                          $"&forecast_days=3" +
                          $"&timezone=auto";

                _logger.LogInformation("Open-Meteo запит: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<WeatherResponseDto>(json);

                if (dto?.Current == null)
                {
                    _logger.LogWarning("Open-Meteo повернув порожню відповідь для {Lat},{Lon}", latStr, lonStr);
                    return null;
                }

                var result = new WeatherInfo
                {
                    Temperature = dto.Current.Temperature,
                    WindSpeed = dto.Current.WindSpeed,
                    Humidity = dto.Current.Humidity,
                    Description = GetWeatherDescription(dto.Current.WeatherCode),
                    Icon = GetWeatherIcon(dto.Current.WeatherCode, dto.Current.IsDay == 1),
                    Forecast = BuildForecast(dto)
                };

                // Кешуємо на 10 хвилин
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Open-Meteo недоступний для координат {Lat},{Lon}", lat, lon);
                return null;
            }
        }

        private static List<DailyForecast> BuildForecast(WeatherResponseDto dto)
        {
            var result = new List<DailyForecast>();
            if (dto.Daily?.Time == null) return result;

            for (int i = 0; i < Math.Min(3, dto.Daily.Time.Count); i++)
            {
                result.Add(new DailyForecast
                {
                    Date = dto.Daily.Time[i],
                    TempMax = dto.Daily.TempMax?.ElementAtOrDefault(i) ?? 0,
                    TempMin = dto.Daily.TempMin?.ElementAtOrDefault(i) ?? 0,
                    Precipitation = dto.Daily.Precipitation?.ElementAtOrDefault(i) ?? 0
                });
            }
            return result;
        }

        private static string GetWeatherDescription(int code) => code switch
        {
            0 => "Ясно",
            1 or 2 or 3 => "Переважно ясно",
            45 or 48 => "Туман",
            51 or 53 or 55 => "Мряка",
            61 or 63 or 65 => "Дощ",
            71 or 73 or 75 => "Сніг",
            80 or 81 or 82 => "Зливи",
            95 => "Гроза",
            _ => "Мінлива хмарність"
        };

        private static string GetWeatherIcon(int code, bool isDay) => code switch
        {
            0 => isDay ? "☀️" : "🌙",
            1 or 2 => isDay ? "⛅" : "🌤️",
            3 => "☁️",
            45 or 48 => "🌫️",
            51 or 53 or 55 or 61 or 63 or 65 => "🌧️",
            71 or 73 or 75 => "❄️",
            80 or 81 or 82 => "⛈️",
            95 => "🌩️",
            _ => "🌤️"
        };
    }
}
