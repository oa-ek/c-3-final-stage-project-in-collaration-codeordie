using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface IExpenseSplitRepository : IRepository<ExpenseSplit>
    {
        void Update(ExpenseSplit entity);
    }
}
