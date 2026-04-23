using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class CreateTripViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву поїздки")]
        [MaxLength(200, ErrorMessage = "Назва не може перевищувати 200 символів")]
        [Display(Name = "Назва поїздки")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Опис занадто довгий")]
        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Вкажіть місце відправлення")]
        [MaxLength(200, ErrorMessage = "Назва місця не може перевищувати 200 символів")]
        [Display(Name = "Звідки")]
        public string DepartureLocation { get; set; }

        [MaxLength(200, ErrorMessage = "Назва місця не може перевищувати 200 символів")]
        [Display(Name = "Куди (повернення)")]
        public string? ReturnLocation { get; set; }

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [Display(Name = "Дата початку")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Вкажіть дату завершення")]
        [Display(Name = "Дата завершення")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Оберіть базову валюту")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Некоректний формат валюти")]
        [Display(Name = "Основна валюта")]
        public string BaseCurrency { get; set; } = "UAH";

        
        public List<SelectListItem> CurrencyList { get; set; } = new()
        {
            new SelectListItem { Value = "UAH", Text = "₴ UAH (Гривня)" },
            new SelectListItem { Value = "USD", Text = "$ USD (Долар)" },
            new SelectListItem { Value = "EUR", Text = "€ EUR (Євро)" },
            new SelectListItem { Value = "PLN", Text = "zł PLN (Злотий)" },
            new SelectListItem { Value = "GBP", Text = "£ GBP (Фунт)" }
        };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Подорож не може закінчитися раніше, ніж почнеться.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}