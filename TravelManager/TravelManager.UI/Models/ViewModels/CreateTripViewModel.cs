using System;
using System.ComponentModel.DataAnnotations;

namespace TravelManager.UI.Models.ViewModels
{
    public class CreateTripViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву поїздки")]
        [Display(Name = "Назва поїздки")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Display(Name = "Місце відправлення")]
        public string? DepartureLocation { get; set; }

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

        [Display(Name = "Базова валюта")]
        public string? BaseCurrency { get; set; } = "UAH";
    }
}