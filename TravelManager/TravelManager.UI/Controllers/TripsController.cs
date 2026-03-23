using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public TripsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var tripsFromDb = _unitOfWork.Trip.GetAll(includeProperties: "Status");

            var tripViewModels = tripsFromDb.Select(t => new TripListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                DepartureLocation = t.DepartureLocation,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                StatusName = t.Status?.Name ?? "Planned"
            }).ToList();

            return View(tripViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateTripViewModel
            {
                UserList = GetUserList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.UserList = GetUserList();
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
                CreatorId = model.CreatorId
            };

            _unitOfWork.Trip.Add(newTrip);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            var model = new CreateTripViewModel
            {
                Title = trip.Title,
                Description = trip.Description,
                DepartureLocation = trip.DepartureLocation,
                ReturnLocation = trip.ReturnLocation,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                BaseCurrency = trip.BaseCurrency,
                CreatorId = trip.CreatorId,
                UserList = GetUserList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateTripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.UserList = GetUserList();
                return View(model);
            }

            var tripFromDb = _unitOfWork.Trip.Get(u => u.Id == id);
            if (tripFromDb == null)
            {
                return NotFound();
            }

            tripFromDb.Title = model.Title;
            tripFromDb.Description = model.Description;
            tripFromDb.DepartureLocation = model.DepartureLocation;
            tripFromDb.ReturnLocation = model.ReturnLocation;
            tripFromDb.StartDate = model.StartDate;
            tripFromDb.EndDate = model.EndDate;
            tripFromDb.BaseCurrency = model.BaseCurrency;
            tripFromDb.CreatorId = model.CreatorId;

            _unitOfWork.Trip.Update(tripFromDb);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            _unitOfWork.Trip.Remove(trip);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetUserList()
        {
            return _userManager.Users.ToList().Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id
            });
        }
    }
}