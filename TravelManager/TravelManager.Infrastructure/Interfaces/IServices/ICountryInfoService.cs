using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Application.DTOs.External;

namespace TravelManager.Infrastructure.Interfaces.IServices
{
    public interface ICountryInfoService
    {
        Task<CountryInfo?> GetCountryInfoAsync(string countryName);
    }
}
