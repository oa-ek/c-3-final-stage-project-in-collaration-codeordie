using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class ExpenseSplitRepository : Repository<ExpenseSplit>, IExpenseSplitRepository
    {
        public ExpenseSplitRepository(ApplicationDbContext ctx) : base(ctx) { }

        public void Update(ExpenseSplit entity)
        {
            var objFromDb = _ctx.ExpenseSplits.FirstOrDefault(x => x.Id == entity.Id);

            if (objFromDb is not null)
            {
                objFromDb.OwedAmount = entity.OwedAmount;
                objFromDb.IsSettled = entity.IsSettled;

                // Обдумати логіку
            }
        }
    }
}
    
