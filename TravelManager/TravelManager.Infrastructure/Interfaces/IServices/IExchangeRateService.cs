using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Application.DTOs.External;

namespace TravelManager.Infrastructure.Interfaces.IServices
{
    public interface IExchangeRateService
    {
        Task<List<ExchangeRateInfo>> GetRatesAsync(IEnumerable<string> currencyCodes);
        Task<ExchangeRateInfo?> GetRateAsync(string currencyCode);
        Task<List<ExchangeRateInfo>> GetAllCurrenciesAsync();

    }
}
