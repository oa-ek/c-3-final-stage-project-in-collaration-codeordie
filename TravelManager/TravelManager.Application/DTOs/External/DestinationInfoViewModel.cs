namespace TravelManager.Application.DTOs.External

{
    /// <summary>
    /// Агрегована модель для сторінки деталей пункту призначення.
    /// Обʼєднує локальні дані з БД + дані з 3 зовнішніх API:
    /// Open-Meteo (погода), REST Countries (країна), NBU (курс валют).
    /// </summary>
    public class DestinationInfoViewModel
    {
        // ── Дані з локальної БД (TripDestination) ──────────────────────
        public int DestinationId { get; set; }
        public int TripId { get; set; }
        public string TripTitle { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }

        // ── Open-Meteo: погода ──────────────────────────────────────────
        public WeatherInfo? Weather { get; set; }
        public bool WeatherAvailable => Weather != null;

        // ── REST Countries: інформація про країну ───────────────────────
        public CountryInfo? Country_Info { get; set; }
        public bool CountryAvailable => Country_Info != null;

        // ── NBU: курс валют ─────────────────────────────────────────────
        public List<ExchangeRateInfo> ExchangeRates { get; set; } = new();
        public bool ExchangeRatesAvailable => ExchangeRates.Any();
    }

}
