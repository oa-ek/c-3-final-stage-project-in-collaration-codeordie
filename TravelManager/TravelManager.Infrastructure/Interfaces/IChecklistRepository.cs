using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface IChecklistRepository : IRepository<Checklist>
    {
        void Update(Checklist obj);
    }
}