using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int CategoryId { get; set; }
        public virtual ExpenseCategory Category { get; set; }

        [Required, MaxLength(200)] public string Title { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
        [Required, MaxLength(10)] public string Currency { get; set; }
        public DateTime Date { get; set; }
        public bool IsRefund { get; set; } = false;

        [MaxLength(255)] public string? ReceiptImageUrl { get; set; }

        public string PayerId { get; set; }
        public virtual User Payer { get; set; }

        public int? TransitId { get; set; }
        public virtual Transit? Transit { get; set; }
        public int? AccommodationId { get; set; }
        public virtual Accommodation? Accommodation { get; set; }
        public int? TripActivityId { get; set; }
        public virtual TripActivity? TripActivity { get; set; }

        public virtual ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
    }
}
