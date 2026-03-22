using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class ChecklistItem
    {
        public int Id { get; set; }
        public int ChecklistId { get; set; }
        public virtual Checklist Checklist { get; set; }
        [Required, MaxLength(255)] public string Content { get; set; }
        public bool IsChecked { get; set; } = false;
    }
}
