using Microsoft.EntityFrameworkCore;
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
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        public TripRepository(ApplicationDbContext ctx) : base(ctx)
        {
        }

        public void Update(Trip entity)
        {
            var tripFromDb = _ctx.Trips.FirstOrDefault(x => x.Id == entity.Id);

            if (tripFromDb is not null)
            {
                tripFromDb.Title = entity.Title;
                tripFromDb.Description = entity.Description;
                tripFromDb.DepartureLocation = entity.DepartureLocation;
                tripFromDb.ReturnLocation = entity.ReturnLocation;
                tripFromDb.StartDate = entity.StartDate;
                tripFromDb.EndDate = entity.EndDate;
                tripFromDb.StatusId = entity.StatusId;
                tripFromDb.BaseCurrency = entity.BaseCurrency;
            }
        }
    }
}