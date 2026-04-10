using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TransitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public TransitsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
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

            var transits = _unitOfWork.Transit.
                GetAll(a => myTripIds.Contains(a.TripId), includeProperties: "Trip,TransitType,BookingStatus");

            var viewModels = transits.Select(t => new TransitListViewModel
            {
                Id = t.Id,
                TripTitle = t.Trip?.Title ?? string.Empty,
                TransitTypeName = t.TransitType?.Name ?? string.Empty,
                DepartureLocation = t.DepartureLocation,
                ArrivalLocation = t.ArrivalLocation,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                BookingStatusName = t.BookingStatus?.Name ?? string.Empty
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

                var selectedTrip = allowedTrips.FirstOrDefault(t => t.Value == tripId.Value.ToString());
                if (selectedTrip != null) selectedTrip.Selected = true;
            }

            var model = new TransitFormViewModel
            {
                TripId = tripId ?? 0,
                TripList = allowedTrips,
                TransitTypeList = GetTransitTypeList(),
                BookingStatusList = GetBookingStatusList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransitFormViewModel model)
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
                model.TransitTypeList = GetTransitTypeList();
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }

            var entity = new Transit
            {
                TripId = model.TripId,
                TransitTypeId = model.TransitTypeId,
                BookingStatusId = model.BookingStatusId,
                DepartureLocation = model.DepartureLocation,
                ArrivalLocation = model.ArrivalLocation,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                CarrierInfo = model.CarrierInfo,
                BookingReference = model.BookingReference
            };

            _unitOfWork.Transit.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
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
            var model = new TransitFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                TransitTypeId = entity.TransitTypeId,
                BookingStatusId = entity.BookingStatusId,
                DepartureLocation = entity.DepartureLocation,
                ArrivalLocation = entity.ArrivalLocation,
                DepartureTime = entity.DepartureTime,
                ArrivalTime = entity.ArrivalTime,
                CarrierInfo = entity.CarrierInfo,
                BookingReference = entity.BookingReference,
                TripList = GetAllowedTripsForUser(),
                TransitTypeList = GetTransitTypeList(),
                BookingStatusList = GetBookingStatusList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransitFormViewModel model)
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
                model.TransitTypeList = GetTransitTypeList();
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }

            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.TripId = model.TripId;
            entity.TransitTypeId = model.TransitTypeId;
            entity.BookingStatusId = model.BookingStatusId;
            entity.DepartureLocation = model.DepartureLocation;
            entity.ArrivalLocation = model.ArrivalLocation;
            entity.DepartureTime = model.DepartureTime;
            entity.ArrivalTime = model.ArrivalTime;
            entity.CarrierInfo = model.CarrierInfo;
            entity.BookingReference = model.BookingReference;

            _unitOfWork.Transit.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
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

            _unitOfWork.Transit.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });
        }

        private IEnumerable<SelectListItem> GetTransitTypeList()
        {
            return _unitOfWork.TransitType.GetAll().Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.Id.ToString()
            });
        }

        private IEnumerable<SelectListItem> GetBookingStatusList()
        {
            return _unitOfWork.BookingStatus.GetAll().Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()
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