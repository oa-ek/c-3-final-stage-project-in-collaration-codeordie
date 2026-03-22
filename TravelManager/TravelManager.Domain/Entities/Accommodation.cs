using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class Accommodation
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int BookingStatusId { get; set; }
        public virtual BookingStatus BookingStatus { get; set; }

        [Required, MaxLength(200)] public string Name { get; set; }
        [MaxLength(500)] public string? Address { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        [MaxLength(50)] public string? BookingReference { get; set; }
        [MaxLength(50)] public string? ContactPhone { get; set; }
        [MaxLength(255)] public string? WebsiteUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
