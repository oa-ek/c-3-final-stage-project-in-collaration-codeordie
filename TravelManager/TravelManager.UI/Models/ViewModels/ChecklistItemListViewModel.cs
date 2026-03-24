namespace TravelManager.UI.Models.ViewModels
{
    public class ChecklistItemListViewModel
    {
        public int Id { get; set; }
        public string ChecklistName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}