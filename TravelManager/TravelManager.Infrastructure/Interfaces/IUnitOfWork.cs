using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITripRepository Trip { get; }
        IExpenseRepository Expense { get; }
        IAccommodationRepository Accommodation { get; }
        IExpenseSplitRepository ExpenseSplit { get; }
        ITransitRepository Transit { get; }
        ITripActivityRepository TripActivity { get; }
        ITripDestinationRepository TripDestination { get; }
        IChecklistRepository Checklist { get; }
        IChecklistItemRepository ChecklistItem { get; }

        IRepository<TripStatus> TripStatus { get; }
        IRepository<BookingStatus> BookingStatus { get; }
        IRepository<TransitType> TransitType { get; }
        IRepository<ExpenseCategory> ExpenseCategory { get; }

        void Save();
        Task SaveAsync();
    }
}