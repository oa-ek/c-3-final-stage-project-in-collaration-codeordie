namespace TravelManager.UI.Models.ViewModels
{
    public class TemplateEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<TemplateItemViewModel> Items { get; set; } = new List<TemplateItemViewModel>();
    }
}
