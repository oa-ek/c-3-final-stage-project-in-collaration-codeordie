using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class WeatherResponseDto
    {
        [JsonPropertyName("current")]
        public CurrentWeatherDto? Current { get; set; }

        [JsonPropertyName("daily")]
        public DailyWeatherDto? Daily { get; set; }
    }


}
