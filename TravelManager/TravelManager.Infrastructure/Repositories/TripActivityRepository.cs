using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Repositories
{
    public class TripActivityRepository : Repository<TripActivity>, ITripActivityRepository
    {
        public TripActivityRepository(ApplicationDbContext ctx) : base(ctx) { }

        public void Update(TripActivity entity)
        {
            var objFromDb = _ctx.TripActivities.FirstOrDefault(x => x.Id == entity.Id);

            if (objFromDb is not null)
            {
                objFromDb.Title = entity.Title;
                objFromDb.Address = entity.Address;
                objFromDb.StartTime = entity.StartTime;
                objFromDb.EndTime = entity.EndTime;
                objFromDb.Notes = entity.Notes;
                objFromDb.BookingStatusId = entity.BookingStatusId;
            }
        }
    }
}
