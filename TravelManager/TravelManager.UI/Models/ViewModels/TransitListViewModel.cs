using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class TransitListViewModel
    {
        public int Id { get; set; }
        public string TripTitle { get; set; } = string.Empty;
        public string TransitTypeName { get; set; } = string.Empty;
        public string DepartureLocation { get; set; } = string.Empty;
        public string ArrivalLocation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string BookingStatusName { get; set; } = string.Empty;
    }
}