using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class ChecklistItemFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть список")]
        [Display(Name = "Список")]
        public int ChecklistId { get; set; }
        public IEnumerable<SelectListItem>? ChecklistList { get; set; }

        [Required(ErrorMessage = "Введіть назву речі або завдання")]
        [MaxLength(255)]
        [Display(Name = "Що взяти / зробити")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Виконано (Зібрано)")]
        public bool IsChecked { get; set; }
    }
}