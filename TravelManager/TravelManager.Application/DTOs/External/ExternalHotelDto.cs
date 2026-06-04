namespace TravelManager.Application.DTOs.External
{
    public class ExternalHotelDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "UAH";
        public string ImageUrl { get; set; } = string.Empty;
        public string ExternalLink { get; set; } = string.Empty;
    }
}