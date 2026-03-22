using System.ComponentModel.DataAnnotations;

namespace TravelManager.Domain.Entities
{
    public class TripRole { 
        public int Id { get; set; } 
        [Required, MaxLength(50)] 
        public string Name { get; set; } 
    }

}
