using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class TransitType { 
        public int Id { get; set; } 
        [Required, MaxLength(50)] 
        public string Name { get; set; } 
    }

}
