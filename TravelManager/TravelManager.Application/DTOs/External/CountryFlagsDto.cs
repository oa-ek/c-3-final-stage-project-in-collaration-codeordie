using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class CountryFlagsDto
    {
        [JsonPropertyName("svg")]
        public string? Svg { get; set; }

        [JsonPropertyName("png")]
        public string? Png { get; set; }

        [JsonPropertyName("alt")]
        public string? Alt { get; set; }
    }
}
