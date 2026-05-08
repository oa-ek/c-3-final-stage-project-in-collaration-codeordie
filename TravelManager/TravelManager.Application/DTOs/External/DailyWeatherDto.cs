using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class DailyWeatherDto
    {
        [JsonPropertyName("temperature_2m_max")]
        public List<double>? TempMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double>? TempMin { get; set; }

        [JsonPropertyName("precipitation_sum")]
        public List<double>? Precipitation { get; set; }

        [JsonPropertyName("time")]
        public List<string>? Time { get; set; }
    }

    public class DailyForecast
    {
        public string Date { get; set; } = string.Empty;
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public double Precipitation { get; set; }
    }

    public class WeatherInfo
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public int Humidity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;  // emoji або код іконки

        // 3-денний прогноз
        public List<DailyForecast> Forecast { get; set; } = new();
    }
}
