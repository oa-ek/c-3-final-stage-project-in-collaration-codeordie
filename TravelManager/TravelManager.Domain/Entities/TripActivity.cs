using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class TripActivity
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int BookingStatusId { get; set; }
        public virtual BookingStatus BookingStatus { get; set; }

        [Required, MaxLength(200)] public string Title { get; set; }
        [MaxLength(500)] public string? Address { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
