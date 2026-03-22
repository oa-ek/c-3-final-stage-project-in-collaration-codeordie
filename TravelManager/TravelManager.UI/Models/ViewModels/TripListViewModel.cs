using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string DepartureLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StatusName { get; set; }
    }
}