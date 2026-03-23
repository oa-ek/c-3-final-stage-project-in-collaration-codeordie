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
    public class TransitRepository : Repository<Transit>, ITransitRepository
    {
        public TransitRepository(ApplicationDbContext ctx) : base(ctx) { }

        public void Update(Transit entity)
        {
            var objFromDb = _ctx.Transits.FirstOrDefault(x => x.Id == entity.Id);

            if (objFromDb is not null)
            {
                objFromDb.DepartureLocation = entity.DepartureLocation;
                objFromDb.ArrivalLocation = entity.ArrivalLocation;
                objFromDb.DepartureTime = entity.DepartureTime;
                objFromDb.ArrivalTime = entity.ArrivalTime;
                objFromDb.CarrierInfo = entity.CarrierInfo;
                objFromDb.BookingReference = entity.BookingReference;
                objFromDb.TransitTypeId = entity.TransitTypeId;
                objFromDb.BookingStatusId = entity.BookingStatusId;
            }
        }
    }
}
