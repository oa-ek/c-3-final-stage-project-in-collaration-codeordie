using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class AccommodationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly INominatimService _nominatimService;
        private readonly IHotelSearchService _hotelSearchService;

        public AccommodationsController(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            INominatimService nominatimService,
            IHotelSearchService hotelSearchService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _nominatimService = nominatimService;
            _hotelSearchService = hotelSearchService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();

            var accommodations = _unitOfWork.Accommodation
                .GetAll(a => myTripIds.Contains(a.TripId), includeProperties: "Trip");

            var viewModels = accommodations.Select(a => new AccommodationListViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Address = a.Address,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Trip = a.Trip
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
                    TempData["ErrorMessage"] = "У вас немає прав для додавання записів у цю поїздку.";
                    return RedirectToAction("Index", "Trips");
                }

                var selectedTrip = allowedTrips.FirstOrDefault(t => t.Value == tripId.Value.ToString());
                if (selectedTrip != null) selectedTrip.Selected = true;
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
                TempData["ErrorMessage"] = "У вас немає прав для додавання записів у цю поїздку.";
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
                TripId = model.TripId
            };

            _unitOfWork.Accommodation.Add(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Житло «{model.Name}» успішно додано!";
            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування.";
                return RedirectToAction("Index", "Trips");
            }

            var model = new AccommodationFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                CheckInTime = entity.CheckInTime,
                CheckOutTime = entity.CheckOutTime,
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
                TempData["ErrorMessage"] = "У вас немає прав для редагування.";
                return RedirectToAction("Index", "Trips");
            }

            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser();
                return View(model);
            }

            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.Name = model.Name;
            entity.Address = model.Address;
            entity.CheckInTime = model.CheckInTime;
            entity.CheckOutTime = model.CheckOutTime;
            entity.TripId = model.TripId;

            _unitOfWork.Accommodation.Update(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Житло «{model.Name}» оновлено!";
            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для видалення.";
                return RedirectToAction("Index", "Trips");
            }

            int tripId = entity.TripId;
            _unitOfWork.Accommodation.Remove(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Житло видалено.";
            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchExternal(int tripId)
        {
            var trip = _unitOfWork.Trip.Get(t => t.Id == tripId);
            if (trip == null) return NotFound();

            var destinations = _unitOfWork.TripDestination.GetAll(d => d.TripId == tripId).ToList();

            // ТУТ БУЛА ПОМИЛКА: Змінено .City на .CityName
            var mainCity = destinations.FirstOrDefault()?.CityName ?? "Kyiv";

            var checkIn = trip.StartDate.ToString("yyyy-MM-dd");
            var checkOut = trip.EndDate.ToString("yyyy-MM-dd");

            var hotels = await _hotelSearchService.SearchHotelsAsync(mainCity, checkIn, checkOut);

            ViewBag.TripId = tripId;
            ViewBag.City = mainCity;

            return View(hotels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExternal(int tripId, string name, string address, decimal price, string externalLink)
        {
            var trip = _unitOfWork.Trip.Get(t => t.Id == tripId);

            var accommodation = new Accommodation
            {
                TripId = tripId,
                Name = name,
                Address = $"{address} | {price} UAH | Link: {externalLink}",
                CheckInTime = trip?.StartDate ?? DateTime.UtcNow,
                CheckOutTime = trip?.EndDate ?? DateTime.UtcNow.AddDays(1)
            };

            _unitOfWork.Accommodation.Add(accommodation);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Index", new { tripId = tripId });
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
                .GetAll(tp => tp.UserId == currentUserId && tp.Role.Name != "Viewer",
                        includeProperties: "Trip,Role")
                .Select(tp => new SelectListItem
                {
                    Text = tp.Trip.Title,
                    Value = tp.TripId.ToString()
                }).ToList();
        }
    }
}