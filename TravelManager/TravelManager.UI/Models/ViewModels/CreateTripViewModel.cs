using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class CreateTripViewModel
    {
        [Required(ErrorMessage = "Введіть назву поїздки")]
        [MaxLength(200, ErrorMessage = "Назва занадто довга")]
        [Display(Name = "Назва поїздки")]
        public string Title { get; set; }

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Вкажіть місце відправлення")]
        [Display(Name = "Місце відправлення")]
        public string DepartureLocation { get; set; }

        [Display(Name = "Місце повернення")]
        public string? ReturnLocation { get; set; }

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [Display(Name = "Дата початку")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Вкажіть дату завершення")]
        [Display(Name = "Дата завершення")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Оберіть базову валюту (напр. USD, UAH)")]
        [Display(Name = "Базова валюта")]
        public string BaseCurrency { get; set; } = "UAH";

        [Required(ErrorMessage = "Оберіть організатора поїздки")]
        [Display(Name = "Організатор поїздки")]
        public string CreatorId { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? UserList { get; set; }
    }
}