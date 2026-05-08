using Microsoft.Extensions.Logging;
using TravelManager.Application.DTOs.External;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices;

namespace TravelManager.Infrastructure.Services
{
    public class DestinationInfoService : IDestinationInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWeatherApiService _weatherService;
        private readonly ICountryInfoService _countryService;
        private readonly IExchangeRateService _exchangeService;
        private readonly INominatimService _nominatimService;
        private readonly ILogger<DestinationInfoService> _logger;

        public DestinationInfoService(
            IUnitOfWork unitOfWork,
            IWeatherApiService weatherService,
            ICountryInfoService countryService,
            IExchangeRateService exchangeService,
            INominatimService nominatimService,
            ILogger<DestinationInfoService> logger)
        {
            _unitOfWork = unitOfWork;
            _weatherService = weatherService;
            _countryService = countryService;
            _exchangeService = exchangeService;
            _nominatimService = nominatimService;
            _logger = logger;
        }

        public async Task<DestinationInfoViewModel?> GetDestinationInfoAsync(int destinationId)
        {
            var destination = _unitOfWork.TripDestination.Get(d => d.Id == destinationId);
            if (destination == null) return null;

            var trip = _unitOfWork.Trip.Get(t => t.Id == destination.TripId);

            var vm = new DestinationInfoViewModel
            {
                DestinationId = destination.Id,
                TripId = destination.TripId,
                TripTitle = trip?.Title ?? string.Empty,
                CityName = destination.CityName,
                Country = destination.Country ?? string.Empty,
                Latitude = destination.Latitude,
                Longitude = destination.Longitude,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
            };

            // Якщо координати відсутні — геокодуємо по назві міста та країни,
            // щоб погода могла завантажитись навіть без збережених координат у БД
            double? lat = destination.Latitude;
            double? lon = destination.Longitude;

            if (lat == null || lon == null)
            {
                _logger.LogInformation(
                    "Координати відсутні для дестинації {Id} ({City}), виконуємо геокодування...",
                    destinationId, destination.CityName);
                try
                {
                    var query = string.IsNullOrWhiteSpace(destination.Country)
                        ? destination.CityName
                        : $"{destination.CityName}, {destination.Country}";

                    var geo = await _nominatimService.GeocodeAsync(query);
                    if (geo != null)
                    {
                        lat = geo.Latitude;
                        lon = geo.Longitude;
                        vm.Latitude = lat;
                        vm.Longitude = lon;

                        // Якщо країна ще не заповнена — підставляємо з геокодування
                        if (string.IsNullOrWhiteSpace(vm.Country) && !string.IsNullOrWhiteSpace(geo.Country))
                            vm.Country = geo.Country;

                        _logger.LogInformation(
                            "Геокодування успішне: {Lat}, {Lon} для '{City}'", lat, lon, destination.CityName);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Геокодування не знайшло результат для '{City}'", destination.CityName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Помилка геокодування для '{City}'", destination.CityName);
                }
            }

            // Визначаємо країну для API — пріоритет: збережена в destination, потім з vm після геокодування
            var countryForApi = destination.Country ?? vm.Country;

            // Три API паралельно
            var weatherTask = GetWeatherSafeAsync(lat, lon);
            var countryTask = GetCountryInfoSafeAsync(countryForApi);
            var ratesTask = GetExchangeRatesSafeAsync(countryForApi);

            await Task.WhenAll(weatherTask, countryTask, ratesTask);

            vm.Weather = weatherTask.Result;
            vm.Country_Info = countryTask.Result;
            vm.ExchangeRates = ratesTask.Result;

            return vm;
        }

        private async Task<WeatherInfo?> GetWeatherSafeAsync(double? lat, double? lon)
        {
            if (lat == null || lon == null) return null;
            try { return await _weatherService.GetWeatherAsync(lat.Value, lon.Value); }
            catch (Exception ex) { _logger.LogWarning(ex, "Помилка погоди"); return null; }
        }

        private async Task<CountryInfo?> GetCountryInfoSafeAsync(string? country)
        {
            if (string.IsNullOrWhiteSpace(country)) return null;
            try { return await _countryService.GetCountryInfoAsync(country); }
            catch (Exception ex) { _logger.LogWarning(ex, "Помилка інформації про країну"); return null; }
        }

        private async Task<List<ExchangeRateInfo>> GetExchangeRatesSafeAsync(string? country)
        {
            try
            {
                var codes = new List<string> { "EUR", "USD" };
                if (!string.IsNullOrWhiteSpace(country))
                {
                    var countryInfo = await _countryService.GetCountryInfoAsync(country);
                    if (!string.IsNullOrWhiteSpace(countryInfo?.CurrencyCode)
                        && countryInfo.CurrencyCode != "EUR"
                        && countryInfo.CurrencyCode != "USD")
                        codes.Add(countryInfo.CurrencyCode);
                }
                return await _exchangeService.GetRatesAsync(codes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Помилка курсів валют");
                return new List<ExchangeRateInfo>();
            }
        }
    }
}
