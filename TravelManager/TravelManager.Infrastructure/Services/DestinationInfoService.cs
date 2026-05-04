using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ILogger<DestinationInfoService> _logger;

        public DestinationInfoService(
            IUnitOfWork unitOfWork,
            IWeatherApiService weatherService,
            ICountryInfoService countryService,
            IExchangeRateService exchangeService,
            ILogger<DestinationInfoService> logger)
        {
            _unitOfWork = unitOfWork;
            _weatherService = weatherService;
            _countryService = countryService;
            _exchangeService = exchangeService;
            _logger = logger;
        }

        public async Task<DestinationInfoViewModel?> GetDestinationInfoAsync(int destinationId)
        {
            // Синхронний Get з фільтром — саме такий метод є в IRepository
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

            // Три API паралельно — вони async, БД вже отримана вище синхронно
            var weatherTask = GetWeatherSafeAsync(destination.Latitude, destination.Longitude);
            var countryTask = GetCountryInfoSafeAsync(destination.Country);
            var ratesTask = GetExchangeRatesSafeAsync(destination.Country);

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
            catch (Exception ex) { _logger.LogWarning(ex, "Помилка країни"); return null; }
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
            catch (Exception ex) { _logger.LogWarning(ex, "Помилка курсів"); return new List<ExchangeRateInfo>(); }
        }
    }
}
