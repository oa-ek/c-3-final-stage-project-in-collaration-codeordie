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
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        private readonly ApplicationDbContext _db;

        public ExpenseRepository(ApplicationDbContext ctx) : base(ctx)
        {
            _db = ctx;
        }

        public void Update(Expense entity)
        {
            var expenseFromDb = _ctx.Expenses.FirstOrDefault(x => x.Id == entity.Id);

            if (expenseFromDb is not null)
            {
                expenseFromDb.Title = entity.Title;
                expenseFromDb.TotalAmount = entity.TotalAmount;
                expenseFromDb.Currency = entity.Currency;
                expenseFromDb.Date = entity.Date;
                expenseFromDb.CategoryId = entity.CategoryId;
                expenseFromDb.ReceiptImageUrl = entity.ReceiptImageUrl;
                expenseFromDb.PayerId = entity.PayerId;        
                expenseFromDb.TripId = entity.TripId;          
                expenseFromDb.TransitId = entity.TransitId;  
                expenseFromDb.AccommodationId = entity.AccommodationId; 
                expenseFromDb.TripActivityId = entity.TripActivityId;
            }
        }
    }
}
