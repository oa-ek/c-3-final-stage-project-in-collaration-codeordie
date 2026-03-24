using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripActivityFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть поїздку")]
        public int TripId { get; set; }
        public IEnumerable<SelectListItem>? TripList { get; set; }

        [Required(ErrorMessage = "Оберіть статус бронювання")]
        public int BookingStatusId { get; set; }
        public IEnumerable<SelectListItem>? BookingStatusList { get; set; }

        [Required(ErrorMessage = "Вкажіть назву активності")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Вкажіть час початку")]
        public DateTime StartTime { get; set; } = DateTime.Today.AddHours(10);

        public DateTime? EndTime { get; set; }

        public string? Notes { get; set; }
    }
}