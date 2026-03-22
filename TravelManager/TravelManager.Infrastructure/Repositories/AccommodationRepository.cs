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
    public class AccommodationRepository : Repository<Accommodation>, IAccommodationRepository
    {
        public AccommodationRepository(ApplicationDbContext ctx) : base(ctx)
        {
        }

        public void Update(Accommodation entity)
        {
            var accFromDb = _ctx.Accommodations.FirstOrDefault(x => x.Id == entity.Id);

            if (accFromDb is not null)
            {
                accFromDb.Name = entity.Name;
                accFromDb.Address = entity.Address;
                accFromDb.CheckInTime = accFromDb.CheckInTime;
                accFromDb.CheckOutTime = accFromDb.CheckOutTime;
                accFromDb.BookingReference = entity.BookingReference;
                accFromDb.BookingStatusId = entity.BookingStatusId;
            }
        }
    }
}
