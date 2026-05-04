using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class CurrentWeatherDto
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int Humidity { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("is_day")]
        public int IsDay { get; set; }
    }
}
