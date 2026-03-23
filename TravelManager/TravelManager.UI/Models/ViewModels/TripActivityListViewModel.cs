using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripActivityListViewModel
    {
        public int Id { get; set; }
        public string TripTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string BookingStatusName { get; set; } = string.Empty;
    }
}