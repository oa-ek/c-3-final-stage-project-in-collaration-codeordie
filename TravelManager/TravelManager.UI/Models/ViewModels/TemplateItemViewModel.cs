namespace TravelManager.UI.Models.ViewModels
{
    public class TemplateItemViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    public class TemplateCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; } = "bi-card-checklist";
    }
}
