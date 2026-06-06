using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelManager.Domain.Entities
{
    public class ChecklistTemplate
    {
        public int Id { get; set; }

        public string? OwnerId { get; set; }
        public virtual User? Owner { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(60)]
        public string? IconClass { get; set; }

        public virtual ICollection<ChecklistTemplateItem> Items { get; set; } = new List<ChecklistTemplateItem>();
    }
}