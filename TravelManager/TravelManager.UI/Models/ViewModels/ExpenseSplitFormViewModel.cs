using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class ExpenseSplitFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть витрату")]
        public int ExpenseId { get; set; }
        public IEnumerable<SelectListItem>? ExpenseList { get; set; }

        [Required(ErrorMessage = "Оберіть боржника")]
        public string DebtorId { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? DebtorList { get; set; }

        [Required(ErrorMessage = "Вкажіть суму боргу")]
        public decimal OwedAmount { get; set; }

        public bool IsSettled { get; set; }
    }
}