using System.Collections.Generic;

namespace TravelManager.UI.Models.ViewModels
{
    public class ChecklistDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TripName { get; set; } = string.Empty;

        public IEnumerable<ChecklistItemListViewModel> Items { get; set; } = new List<ChecklistItemListViewModel>();
    }
}