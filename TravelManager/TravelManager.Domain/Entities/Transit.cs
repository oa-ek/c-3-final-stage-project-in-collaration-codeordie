using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class Transit
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int TransitTypeId { get; set; }
        public virtual TransitType TransitType { get; set; }

        public int BookingStatusId { get; set; }
        public virtual BookingStatus BookingStatus { get; set; }

        [Required, MaxLength(200)] public string DepartureLocation { get; set; }
        [Required, MaxLength(200)] public string ArrivalLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        [MaxLength(100)] public string? CarrierInfo { get; set; }
        [MaxLength(50)] public string? BookingReference { get; set; }
    }
}
