using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class TripDestination
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        [Required, MaxLength(100)] public string CityName { get; set; }
        [MaxLength(100)] public string? Country { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
    }
}
