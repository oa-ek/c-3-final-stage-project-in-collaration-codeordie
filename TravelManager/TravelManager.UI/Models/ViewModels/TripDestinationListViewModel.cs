using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripDestinationListViewModel
    {
        public int Id { get; set; }
        public string TripName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}