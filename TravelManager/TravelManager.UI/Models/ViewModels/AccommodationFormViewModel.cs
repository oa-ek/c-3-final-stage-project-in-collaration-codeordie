using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class AccommodationFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; } = DateTime.Today.AddHours(14);

        [Required]
        public DateTime CheckOutTime { get; set; } = DateTime.Today.AddDays(1).AddHours(12);

        public string? BookingReference { get; set; }

        [Required(ErrorMessage = "Оберіть поїздку")]
        public int TripId { get; set; }

        public IEnumerable<SelectListItem>? TripList { get; set; }
    }
}