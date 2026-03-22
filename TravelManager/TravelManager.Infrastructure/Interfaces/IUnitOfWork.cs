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

        void Save();
        Task SaveAsync();
    }
}
