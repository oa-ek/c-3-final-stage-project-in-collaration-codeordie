using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class ChecklistRepository : Repository<Checklist>, IChecklistRepository
    {
        private readonly ApplicationDbContext _db;

        public ChecklistRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Checklist obj)
        {
            _db.Checklists.Update(obj);
        }
    }
}