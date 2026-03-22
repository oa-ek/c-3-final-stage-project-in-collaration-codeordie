using Microsoft.AspNetCore.Mvc;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TripsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var tripsFromDb = _unitOfWork.Trip.GetAll();

            var tripViewModels = tripsFromDb.Select(t => new TripListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                DepartureLocation = t.DepartureLocation,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                StatusName = "Заплановано"
            }).ToList();

            return View(tripViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTripViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newTrip = new Trip
            {
                Title = model.Title,
                Description = model.Description,
                DepartureLocation = model.DepartureLocation,
                ReturnLocation = model.ReturnLocation,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                BaseCurrency = model.BaseCurrency,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatorId = "temporary-user-id"
            };

            _unitOfWork.Trip.Add(newTrip);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}