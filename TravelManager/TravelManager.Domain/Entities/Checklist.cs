using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class Checklist
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }
        [Required, MaxLength(150)] public string Title { get; set; }
        public virtual ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }
}
