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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _ctx;

        public ITripRepository Trip { get; }
        public IExpenseRepository Expense { get; }
        public IAccommodationRepository Accommodation { get; }

        public UnitOfWork(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            Trip = new TripRepository(_ctx);
            Expense = new ExpenseRepository(_ctx);
            Accommodation = new AccommodationRepository(_ctx);
        }

        public void Save()
        {
            _ctx.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _ctx.SaveChangesAsync();
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}

