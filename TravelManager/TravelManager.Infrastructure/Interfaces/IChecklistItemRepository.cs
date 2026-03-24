using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface IChecklistItemRepository : IRepository<ChecklistItem>
    {
        void Update(ChecklistItem obj);
    }
}