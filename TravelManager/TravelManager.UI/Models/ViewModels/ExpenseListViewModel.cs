using System;

namespace TravelManager.UI.Models.ViewModels
{
    public class ExpenseListViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; }
        public string TripTitle { get; set; }
        public int TripId { get; set; }
        public string? ReceiptImageUrl { get; set; }

        public int? TransitId { get; set; }
        public string? LinkedTransit { get; set; }

        public int? AccommodationId { get; set; }
        public string? LinkedAccommodation { get; set; }

        public int? TripActivityId { get; set; }
        public string? LinkedActivity { get; set; }

    }
}