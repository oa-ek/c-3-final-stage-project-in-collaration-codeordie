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
    }
}