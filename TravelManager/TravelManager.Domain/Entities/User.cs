using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<TripParticipant> Participations { get; set; } = new List<TripParticipant>();
        public virtual ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
        public virtual ICollection<ExpenseSplit> Debts { get; set; } = new List<ExpenseSplit>();
    }
}
