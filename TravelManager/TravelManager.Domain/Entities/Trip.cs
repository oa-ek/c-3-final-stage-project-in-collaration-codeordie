using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class Trip
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; }
        public string? Description { get; set; }

        [Required, MaxLength(200)] public string DepartureLocation { get; set; }
        [MaxLength(200)] public string? ReturnLocation { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required, MaxLength(10)] public string BaseCurrency { get; set; }

        public string CreatorId { get; set; }
        public virtual User Creator { get; set; }

        public int StatusId { get; set; }
        public virtual TripStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<TripDestination> Destinations { get; set; } = new List<TripDestination>();
        public virtual ICollection<TripParticipant> Participants { get; set; } = new List<TripParticipant>();
        public virtual ICollection<Transit> Transits { get; set; } = new List<Transit>();
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public virtual ICollection<TripActivity> TripActivities { get; set; } = new List<TripActivity>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<TripDocument> TripDocuments { get; set; } = new List<TripDocument>();
        public virtual ICollection<Checklist> Checklists { get; set; } = new List<Checklist>();
    }
}
