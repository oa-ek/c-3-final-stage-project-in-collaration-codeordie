using System;
using TravelManager.Domain.Entities;

namespace TravelManager.UI.Models.ViewModels
{
    public class AccommodationListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public Trip Trip { get; set; }
    }
}