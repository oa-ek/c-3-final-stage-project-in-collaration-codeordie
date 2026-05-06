using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Application.DTOs.External;

namespace TravelManager.Infrastructure.Interfaces.IServices
{
    public interface INominatimService
    {
        Task<GeocodingResult?> GeocodeAsync(string query);
        Task<GeocodingResult?> GeocodeAddressAsync(string address);
    }
}
