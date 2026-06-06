using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class AccommodationListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public string? TripTitle { get; set; }

        public string BookingStatusName { get; set; } = string.Empty;

        public string? ContactPhone { get; set; }
        public string? WebsiteUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? BookingReference { get; set; }

        public DateTime CheckInDate => CheckInTime;
        public DateTime CheckOutDate => CheckOutTime;
    }
}