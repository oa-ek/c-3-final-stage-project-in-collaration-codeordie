using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelManager.Application.DTOs.External
{
    public class ExchangeRateDto
    {
        [JsonPropertyName("r030")]
        public int CurrencyCode { get; set; }

        [JsonPropertyName("txt")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        [JsonPropertyName("cc")]
        public string? CurrencySymbol { get; set; }

        [JsonPropertyName("exchangedate")]
        public string? ExchangeDate { get; set; }
    }

    public class ExchangeRateInfo
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public double RateToUah { get; set; }
        public string ExchangeDate { get; set; } = string.Empty;
    }
}
