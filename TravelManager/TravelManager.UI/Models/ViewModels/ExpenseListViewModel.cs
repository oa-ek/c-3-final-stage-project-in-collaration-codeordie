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
    }
}