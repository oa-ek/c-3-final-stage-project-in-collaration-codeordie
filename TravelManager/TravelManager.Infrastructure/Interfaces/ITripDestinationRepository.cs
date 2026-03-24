using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Interfaces
{
    public interface ITripDestinationRepository : IRepository<TripDestination>
    {
        void Update(TripDestination obj);
    }
}