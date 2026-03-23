namespace TravelManager.UI.Models.ViewModels
{
    public class ExpenseSplitListViewModel
    {
        public int Id { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public decimal OwedAmount { get; set; }
        public bool IsSettled { get; set; }
    }
}