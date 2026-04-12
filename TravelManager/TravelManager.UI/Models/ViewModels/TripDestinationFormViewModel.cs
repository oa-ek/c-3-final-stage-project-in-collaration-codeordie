

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripDestinationFormViewModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Оберіть поїздку")]
        [Display(Name = "Поїздка")]
        public int TripId { get; set; }
        public IEnumerable<SelectListItem>? TripList { get; set; }

        [Required(ErrorMessage = "Введіть назву міста")]
        [MaxLength(100)]
        [Display(Name = "Місто")]
        public string CityName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Країна")]
        public string? Country { get; set; }

        [Display(Name = "Широта")]
        public double? Latitude { get; set; }

        [Display(Name = "Довгота")]
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Вкажіть дату прибуття")]
        [Display(Name = "Дата прибуття")]
        [DataType(DataType.Date)]
        public DateTime ArrivalDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Вкажіть дату відбуття")]
        [Display(Name = "Дата відбуття")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today.AddDays(1);
    }
}