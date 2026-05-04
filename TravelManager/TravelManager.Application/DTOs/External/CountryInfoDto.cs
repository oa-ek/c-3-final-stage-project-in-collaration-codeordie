using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class CountryInfoDto
    {
        [JsonPropertyName("name")]
        public CountryNameDto? Name { get; set; }

        [JsonPropertyName("flags")]
        public CountryFlagsDto? Flags { get; set; }

        [JsonPropertyName("currencies")]
        public Dictionary<string, CurrencyDetailDto>? Currencies { get; set; }

        [JsonPropertyName("languages")]
        public Dictionary<string, string>? Languages { get; set; }

        [JsonPropertyName("capital")]
        public List<string>? Capital { get; set; }

        [JsonPropertyName("population")]
        public long Population { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }
    }

    public class CountryInfo
    {
        public string CommonName { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string FlagUrl { get; set; } = string.Empty;
        public string FlagAlt { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public long Population { get; set; }
        public string Languages { get; set; } = string.Empty;

        // Валюта з REST Countries (для назви)
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;

    }
    }
