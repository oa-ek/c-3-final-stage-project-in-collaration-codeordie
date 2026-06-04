using TravelManager.Application.DTOs.External;

namespace TravelManager.Infrastructure.Interfaces.IServices
{
    public interface IHotelSearchService
    {
        Task<List<ExternalHotelDto>> SearchHotelsAsync(string city, string checkIn, string checkOut);
    }
}