using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripDestinationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public TripDestinationsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index(int? tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();

            var destinations = _unitOfWork.TripDestination.
                GetAll(a => myTripIds.Contains(a.TripId), includeProperties: "Trip");

            if (tripId.HasValue)
            {
                destinations = destinations.Where(d => d.TripId == tripId.Value);
            }

            var viewModels = destinations.Select(d => new TripDestinationListViewModel
            {
                Id = d.Id,
                TripName = d.Trip?.Title ?? string.Empty,
                CityName = d.CityName,
                Country = d.Country,
                ArrivalDate = d.ArrivalDate,
                DepartureDate = d.DepartureDate
            }).OrderBy(d => d.ArrivalDate).ToList();

            ViewBag.CurrentTripId = tripId;

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

                var selectedTrip = allowedTrips.FirstOrDefault(t => t.Value == tripId.Value.ToString());
                if (selectedTrip != null) selectedTrip.Selected = true;
            }
            var model = new TripDestinationFormViewModel
            {
                TripId = tripId ?? 0,
                TripList = GetAllowedTripsForUser(),
                ArrivalDate = DateTime.Now,
                DepartureDate = DateTime.Now.AddDays(1)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripDestinationFormViewModel model)
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

            var entity = new TripDestination
            {
                TripId = model.TripId,
                CityName = model.CityName,
                Country = model.Country,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ArrivalDate = model.ArrivalDate,
                DepartureDate = model.DepartureDate
            };

            _unitOfWork.TripDestination.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index), new { tripId = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
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

            var model = new TripDestinationFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                CityName = entity.CityName,
                Country = entity.Country,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ArrivalDate = entity.ArrivalDate,
                DepartureDate = entity.DepartureDate,
                TripList = GetAllowedTripsForUser()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripDestinationFormViewModel model)
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

            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.TripId = model.TripId;
            entity.CityName = model.CityName;
            entity.Country = model.Country;
            entity.Latitude = model.Latitude;
            entity.Longitude = model.Longitude;
            entity.ArrivalDate = model.ArrivalDate;
            entity.DepartureDate = model.DepartureDate;

            _unitOfWork.TripDestination.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index), new { tripId = entity.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
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
            int? tripId = entity.TripId;
            _unitOfWork.TripDestination.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index), new { tripId = tripId });
        }

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });
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