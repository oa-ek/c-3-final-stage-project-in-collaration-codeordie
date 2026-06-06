using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface IChecklistTemplateRepository : IRepository<ChecklistTemplate>
    {
        void Update(ChecklistTemplate obj);
    }
}