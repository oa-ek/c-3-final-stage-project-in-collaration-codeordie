using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class ChecklistTemplateRepository : Repository<ChecklistTemplate>, IChecklistTemplateRepository
    {
        private readonly ApplicationDbContext _db;

        public ChecklistTemplateRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ChecklistTemplate obj)
        {
            _db.ChecklistTemplates.Update(obj);
        }
    }
}