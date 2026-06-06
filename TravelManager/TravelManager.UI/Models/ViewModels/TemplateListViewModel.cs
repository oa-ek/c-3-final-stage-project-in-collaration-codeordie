namespace TravelManager.UI.Models.ViewModels
{
    public class TemplateListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public int ItemsCount { get; set; }
        public bool IsSystem { get; set; } 
    }
}
