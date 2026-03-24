using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class ChecklistFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть поїздку")]
        [Display(Name = "Поїздка")]
        public int TripId { get; set; }
        public IEnumerable<SelectListItem>? TripList { get; set; }

        [Required(ErrorMessage = "Введіть назву списку (наприклад: 'Речі в ручну поклажу')")]
        [MaxLength(150)]
        [Display(Name = "Назва списку")]
        public string Title { get; set; } = string.Empty;
    }
}