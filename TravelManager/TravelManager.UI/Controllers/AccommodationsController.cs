using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class AccommodationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public AccommodationsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var accommodations = _unitOfWork.Accommodation.GetAll(includeProperties: "Trip");

            var viewModels = accommodations.Select(a => new AccommodationListViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Address = a.Address,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? tripId)
        {
            var allowedTrips = GetAllowedTripsForUser();

            if (!allowedTrips.Any())
            {
                TempData["ErrorMessage"] = "У вас немає поїздок, де ви можете додавати записи.";
                return RedirectToAction("Index", "Trips");
            }

            if (tripId.HasValue)
            {
                var role = GetUserRoleInTrip(tripId.Value);
                if (role == "Viewer" || role == "None")
                {
                    TempData["ErrorMessage"] = "Глядачі не можуть додавати записи в цю поїздку.";
                    return RedirectToAction("Index", "Trips");
                }
            }

            var model = new AccommodationFormViewModel
            {
                TripId = tripId ?? 0, 
                TripList = allowedTrips 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccommodationFormViewModel model)
        {
            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Відмовлено в доступі. Ви не можете додавати записи в цю поїздку.";
                return RedirectToAction("Index", "Trips");
            }

            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser(); 
                return View(model);
            }

            var entity = new Accommodation
            {
                Name = model.Name,
                Address = model.Address,
                CheckInTime = model.CheckInTime,
                CheckOutTime = model.CheckOutTime,
                BookingReference = model.BookingReference,
                TripId = model.TripId,
                BookingStatusId = 1
            };

            _unitOfWork.Accommodation.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }
            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index", "Trips"); 
            }

            var trips = _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });

            var model = new AccommodationFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                CheckInTime = entity.CheckInTime,
                CheckOutTime = entity.CheckOutTime,
                BookingReference = entity.BookingReference,
                TripId = entity.TripId,
                TripList = GetAllowedTripsForUser()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccommodationFormViewModel model)
        {
            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index", "Trips");
            }
            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser();
                return View(model);
            }

            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Address = model.Address;
            entity.CheckInTime = model.CheckInTime;
            entity.CheckOutTime = model.CheckOutTime;
            entity.BookingReference = model.BookingReference;
            entity.TripId = model.TripId;

            _unitOfWork.Accommodation.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть видаляти записи.";
                return RedirectToAction("Index", "Trips");
            }

            _unitOfWork.Accommodation.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }

        private IEnumerable<SelectListItem> GetAllowedTripsForUser()
        {
            var currentUserId = _userManager.GetUserId(User);

            return _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId && tp.Role.Name != "Viewer", includeProperties: "Trip,Role")
                .Select(tp => new SelectListItem
                {
                    Text = tp.Trip.Title,
                    Value = tp.TripId.ToString()
                }).ToList();
        }

    }
}