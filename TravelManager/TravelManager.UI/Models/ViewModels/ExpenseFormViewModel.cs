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

        [Required(ErrorMessage = "Введіть опис/назву витрати")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        [Required(ErrorMessage = "Введіть суму")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сума має бути більшою за нуль")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Оберіть валюту")]
        public string Currency { get; set; } = "UAH";
        public IEnumerable<SelectListItem>? CurrencyList { get; set; }

        [Required(ErrorMessage = "Вкажіть дату")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Url(ErrorMessage = "Введіть коректне посилання на чек (URL)")]
        public string? ReceiptImageUrl { get; set; }

        [Required(ErrorMessage = "Вкажіть, хто заплатив")]
        public string PayerId { get; set; }
        public IEnumerable<SelectListItem>? PayerList { get; set; }

        public List<ExpenseSplitItemModel> Splits { get; set; } = new List<ExpenseSplitItemModel>();

        public int? TransitId { get; set; }
        public IEnumerable<SelectListItem>? TransitList { get; set; }

        public int? AccommodationId { get; set; }
        public IEnumerable<SelectListItem>? AccommodationList { get; set; }

        public int? TripActivityId { get; set; }
        public IEnumerable<SelectListItem>? ActivityList { get; set; }
    }
}