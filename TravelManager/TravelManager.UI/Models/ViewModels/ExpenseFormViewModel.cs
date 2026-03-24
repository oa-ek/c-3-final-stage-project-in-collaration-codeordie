using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class ExpenseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть поїздку")]
        public int TripId { get; set; }

        public IEnumerable<SelectListItem>? TripList { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        [Required(ErrorMessage = "Введіть суму")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сума має бути більшою за нуль")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Оберіть валюту")]
        public string Currency { get; set; } = "UAH";

        public IEnumerable<SelectListItem>? CurrencyList { get; set; }

        [Required(ErrorMessage = "Введіть опис витрати")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Вкажіть дату")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}