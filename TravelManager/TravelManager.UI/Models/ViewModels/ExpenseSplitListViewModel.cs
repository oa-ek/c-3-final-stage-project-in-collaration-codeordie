namespace TravelManager.UI.Models.ViewModels
{
    public class ExpenseSplitListViewModel
    {
        public int Id { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public decimal OwedAmount { get; set; }
        public bool IsSettled { get; set; }
        public string? ReceiptImageUrl { get; set; }

        public string TripName { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string Currency { get; set; } = "UAH";
        public string ExpenseTitle { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public string PayerId { get; set; } = string.Empty;
        public string DebtorId { get; set; } = string.Empty;

    }
}