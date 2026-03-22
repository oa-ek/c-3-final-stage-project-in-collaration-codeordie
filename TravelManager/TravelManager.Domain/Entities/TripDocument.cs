using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class TripDocument
    { 
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }
        [Required, MaxLength(255)] public string FileName { get; set; }
        [Required] public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? TransitId { get; set; }
        public virtual Transit? Transit { get; set; }
        public int? AccommodationId { get; set; }
        public virtual Accommodation? Accommodation { get; set; }
        public int? TripActivityId { get; set; }
        public virtual TripActivity? TripActivity { get; set; }
    }
}
