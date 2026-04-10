using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripActivitiesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public TripActivitiesController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var activities = _unitOfWork.TripActivity.GetAll(includeProperties: "Trip,BookingStatus");

            var viewModels = activities.Select(a => new TripActivityListViewModel
            {
                Id = a.Id,
                TripTitle = a.Trip?.Title ?? string.Empty,
                Title = a.Title,
                Address = a.Address,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                BookingStatusName = a.BookingStatus?.Name ?? string.Empty
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

            var model = new TripActivityFormViewModel
            {
                TripList = GetAllowedTripsForUser(),
                BookingStatusList = GetBookingStatusList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripActivityFormViewModel model)
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
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }

            var entity = new TripActivity
            {
                TripId = model.TripId,
                BookingStatusId = model.BookingStatusId,
                Title = model.Title,
                Address = model.Address,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Notes = model.Notes
            };

            _unitOfWork.TripActivity.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.TripActivity.Get(u => u.Id == id);
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

            var model = new TripActivityFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                BookingStatusId = entity.BookingStatusId,
                Title = entity.Title,
                Address = entity.Address,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Notes = entity.Notes,
                TripList = GetAllowedTripsForUser(),
                BookingStatusList = GetBookingStatusList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripActivityFormViewModel model)
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
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }

            var entity = _unitOfWork.TripActivity.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.TripId = model.TripId;
            entity.BookingStatusId = model.BookingStatusId;
            entity.Title = model.Title;
            entity.Address = model.Address;
            entity.StartTime = model.StartTime;
            entity.EndTime = model.EndTime;
            entity.Notes = model.Notes;

            _unitOfWork.TripActivity.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.TripActivity.Get(u => u.Id == id);
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

            _unitOfWork.TripActivity.Remove(entity);
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