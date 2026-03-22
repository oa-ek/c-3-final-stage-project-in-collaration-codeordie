using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelManager.Domain.Entities
{
    public class ExpenseSplit
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public virtual Expense Expense { get; set; }

        public string DebtorId { get; set; }
        public virtual User Debtor { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal OwedAmount { get; set; }
        public bool IsSettled { get; set; } = false;
    }
}
