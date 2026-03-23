using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class ChecklistItemRepository : Repository<ChecklistItem>, IChecklistItemRepository
    {
        private readonly ApplicationDbContext _db;

        public ChecklistItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ChecklistItem obj)
        {
            _db.ChecklistItems.Update(obj);
        }
    }
}