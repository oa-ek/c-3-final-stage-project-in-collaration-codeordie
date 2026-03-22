using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class TripParticipant
    {
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int RoleId { get; set; }
        public virtual TripRole Role { get; set; }
    }
}
