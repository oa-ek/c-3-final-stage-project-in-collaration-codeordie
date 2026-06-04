using System.ComponentModel.DataAnnotations;

namespace TravelManager.Domain.Entities
{
    public class ChecklistTemplateItem
    {
        public int Id { get; set; }

        public int ChecklistTemplateId { get; set; }
        public virtual ChecklistTemplate ChecklistTemplate { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Content { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;
    }
}