using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class CountryNameDto
    {
        [JsonPropertyName("common")]
        public string? Common { get; set; }

        [JsonPropertyName("official")]
        public string? Official { get; set; }
    }
}
