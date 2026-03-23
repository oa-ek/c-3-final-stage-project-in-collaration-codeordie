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
        public IExpenseSplitRepository ExpenseSplit { get; }
        public ITransitRepository Transit { get; }
        public ITripActivityRepository TripActivity { get; }
        public ITripDestinationRepository TripDestination { get; }

      
        public IChecklistRepository Checklist { get; private set; }

        public IRepository<TripStatus> TripStatus { get; private set; }
        public IRepository<BookingStatus> BookingStatus { get; private set; }
        public IRepository<TransitType> TransitType { get; private set; }
        public IRepository<ExpenseCategory> ExpenseCategory { get; private set; }

        public UnitOfWork(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            Trip = new TripRepository(_ctx);
            Expense = new ExpenseRepository(_ctx);
            Accommodation = new AccommodationRepository(_ctx);
            ExpenseSplit = new ExpenseSplitRepository(_ctx);
            Transit = new TransitRepository(_ctx);
            TripActivity = new TripActivityRepository(_ctx);
            TripDestination = new TripDestinationRepository(_ctx);

          
            Checklist = new ChecklistRepository(_ctx);

            TripStatus = new Repository<TripStatus>(_ctx);
            BookingStatus = new Repository<BookingStatus>(_ctx);
            TransitType = new Repository<TransitType>(_ctx);
            ExpenseCategory = new Repository<ExpenseCategory>(_ctx);
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