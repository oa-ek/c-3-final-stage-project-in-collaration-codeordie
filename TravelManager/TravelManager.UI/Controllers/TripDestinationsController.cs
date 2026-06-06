using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Application.DTOs.External;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices;
using TravelManager.Infrastructure.Services;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class TripDestinationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly INominatimService _nominatimService;
        private readonly IDestinationInfoService _destinationInfoService;
        private readonly IAiRecommendationService _aiService;

        public TripDestinationsController(
     IUnitOfWork unitOfWork,
     UserManager<User> userManager,
     INominatimService nominatimService,
     IDestinationInfoService destinationInfoService,
     IAiRecommendationService aiService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _nominatimService = nominatimService;
            _destinationInfoService = destinationInfoService;
            _aiService = aiService;
        }

        [HttpGet]
        public IActionResult Index(int? tripId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();

            var destinations = _unitOfWork.TripDestination
                .GetAll(d => myTripIds.Contains(d.TripId), includeProperties: "Trip");


            if (tripId.HasValue)
            {
                destinations = destinations.Where(d => d.TripId == tripId.Value);
                ViewBag.Transits = _unitOfWork.Transit
                    .GetAll(t => t.TripId == tripId.Value, includeProperties: "TransitType")
                    .ToList();
            }
            else
            {
                ViewBag.Transits = _unitOfWork.Transit
                    .GetAll(t => myTripIds.Contains(t.TripId), includeProperties: "TransitType")
                    .ToList();
            }

            var viewModels = destinations.Select(d => new TripDestinationListViewModel
            {
                Id = d.Id,
                TripName = d.Trip?.Title ?? string.Empty,
                CityName = d.CityName,
                Country = d.Country,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                ArrivalDate = d.ArrivalDate,
                DepartureDate = d.DepartureDate,
                TripId = d.TripId
            }).OrderBy(d => d.ArrivalDate).ToList();

            ViewBag.CurrentTripId = tripId;
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Geocode(string city, string? country)
        {
            var query = string.IsNullOrWhiteSpace(country)
                ? city
                : $"{city}, {country}";

            var result = await _nominatimService.GeocodeAsync(query);

            if (result == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                lat = result.Latitude,
                lon = result.Longitude,
                displayName = result.DisplayName,
                country = result.Country
            });
        }
        [HttpGet]
        public async Task<IActionResult> AiRecommendations(string city, string country)
        {
            var result = await _aiService.GetRecommendationsAsync(city, country);
            if (result == null)
                return Json(new { error = "Не вдалося отримати рекомендації" });

            return Json(new
            {
                attractions = result.Attractions,
                restaurants = result.Restaurants,
                tips = result.PracticalTips
            });
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
                    TempData["ErrorMessage"] = "У вас немає прав для додавання записів.";
                    return RedirectToAction("Index", "Trips");
                }

                var selectedTrip = allowedTrips.FirstOrDefault(t => t.Value == tripId.Value.ToString());
                if (selectedTrip != null) selectedTrip.Selected = true;
            }

            var model = new TripDestinationFormViewModel
            {
                TripId = tripId ?? 0,
                TripList = allowedTrips,
                ArrivalDate = DateTime.Today,
                DepartureDate = DateTime.Today.AddDays(1)
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
                TempData["ErrorMessage"] = "У вас немає прав для додавання записів.";
                return RedirectToAction("Index", "Trips");
            }

            ModelState.Remove("TripList");
            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");

            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser();
                return View(model);
            }

            if (double.TryParse(Request.Form["Latitude"].ToString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat))
            {
                model.Latitude = lat;
            }
            if (double.TryParse(Request.Form["Longitude"].ToString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
            {
                model.Longitude = lng;
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

            // --- КЛЮЧОВА ЗМІНА: АВТОЗАПОВНЕННЯ МИНУЛОЇ ТОЧКИ ---
            var prevDest = _unitOfWork.TripDestination
                .GetAll(d => d.TripId == model.TripId && d.ArrivalDate <= model.ArrivalDate && d.Id != entity.Id)
                .OrderByDescending(d => d.ArrivalDate)
                .FirstOrDefault();

            string depLoc = prevDest?.CityName ?? "";
            if (string.IsNullOrEmpty(depLoc))
            {
                var trip = _unitOfWork.Trip.Get(t => t.Id == model.TripId);
                depLoc = trip?.DepartureLocation ?? "";
            }

            TempData["SuccessMessage"] = $"Місто {model.CityName} додано! Заплануйте транспорт з {depLoc}.";

            return RedirectToAction("Create", "Transits", new
            {
                tripId = model.TripId,
                arrivalLocation = model.CityName,
                arrivalDate = model.ArrivalDate.ToString("yyyy-MM-dd"),
                departureLocation = depLoc
            });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування.";
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
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування.";
                return RedirectToAction("Index", "Trips");
            }

            ModelState.Remove("TripList");
            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");

            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser();
                return View(model);
            }

            if (double.TryParse(Request.Form["Latitude"].ToString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat))
            {
                model.Latitude = lat;
            }
            if (double.TryParse(Request.Form["Longitude"].ToString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
            {
                model.Longitude = lng;
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

            TempData["SuccessMessage"] = $"Місто {model.CityName} оновлено!";
            return RedirectToAction(nameof(Index), new { tripId = entity.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для видалення.";
                return RedirectToAction("Index", "Trips");
            }

            int tripId = entity.TripId;
            _unitOfWork.TripDestination.Remove(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Місто видалено з маршруту.";
            return RedirectToAction(nameof(Index), new { tripId });
        }
        [HttpGet]
        public async Task<IActionResult> GetAiRecommendations(int destinationId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var destination = _unitOfWork.TripDestination.Get(d => d.Id == destinationId);
            if (destination == null) return NotFound();

            // Перевіряємо доступ
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == destination.TripId && tp.UserId == currentUserId);
            if (participant == null) return Forbid();

            var result = await _aiService.GetRecommendationsAsync(
                destination.CityName, destination.Country);

            if (result == null)
                return StatusCode(503, new { error = "AI сервіс тимчасово недоступний" });

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var destination = _unitOfWork.TripDestination.Get(d => d.Id == id);
            if (destination == null) return NotFound();

            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == destination.TripId && tp.UserId == currentUserId);
            if (participant == null)
            {
                TempData["ErrorMessage"] = "У вас немає доступу до цієї поїздки.";
                return RedirectToAction(nameof(Index));
            }

            var model = await _destinationInfoService.GetDestinationInfoAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // GET /TripDestinations/WeatherCard?destinationId=5
        // Повертає JSON з погодою і прапором для рядка таблиці маршруту
        [HttpGet]
        public async Task<IActionResult> WeatherCard(int destinationId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var destination = _unitOfWork.TripDestination.Get(d => d.Id == destinationId);
            if (destination == null) return NotFound();

            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == destination.TripId && tp.UserId == currentUserId);
            if (participant == null) return Forbid();

            var info = await _destinationInfoService.GetDestinationInfoAsync(destinationId);
            if (info == null) return NotFound();

            return Json(new
            {
                temp = info.Weather?.Temperature,
                icon = info.Weather?.Icon,
                desc = info.Weather?.Description,
                humidity = info.Weather?.Humidity,
                windSpeed = info.Weather?.WindSpeed,
                flagUrl = info.Country_Info?.FlagUrl,
                flagAlt = info.Country_Info?.FlagAlt,
                country = info.Country_Info?.CommonName,
            });
        }

        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");
            return participant?.Role?.Name ?? "None";
        }

        private List<SelectListItem> GetAllowedTripsForUser()
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