using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class TransitFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть поїздку")]
        public int TripId { get; set; }
        public IEnumerable<SelectListItem>? TripList { get; set; }

        [Required(ErrorMessage = "Оберіть тип транспорту")]
        public int TransitTypeId { get; set; }
        public IEnumerable<SelectListItem>? TransitTypeList { get; set; }

        [Required(ErrorMessage = "Оберіть статус бронювання")]
        public int BookingStatusId { get; set; } = 1;
        public IEnumerable<SelectListItem>? BookingStatusList { get; set; }

        [Required(ErrorMessage = "Вкажіть місце відправлення")]
        public string DepartureLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть місце прибуття")]
        public string ArrivalLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть час відправлення")]
        public DateTime DepartureTime { get; set; } = DateTime.Today.AddHours(10);

        [Required(ErrorMessage = "Вкажіть час прибуття")]
        public DateTime ArrivalTime { get; set; } = DateTime.Today.AddHours(14);

        public string? CarrierInfo { get; set; }
        public string? BookingReference { get; set; }
    }
}