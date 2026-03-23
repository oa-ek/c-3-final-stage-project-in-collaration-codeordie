using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class TripDestinationRepository : Repository<TripDestination>, ITripDestinationRepository
    {
        private readonly ApplicationDbContext _db;

        public TripDestinationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TripDestination obj)
        {
            _db.TripDestinations.Update(obj);
        }
    }
}