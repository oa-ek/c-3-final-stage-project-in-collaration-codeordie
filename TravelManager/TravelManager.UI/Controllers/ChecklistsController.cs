using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{

    [Authorize]
    public class ChecklistsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public ChecklistsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();

            var checklists = _unitOfWork.Checklist
                .GetAll(a => myTripIds.Contains(a.TripId), includeProperties: "Trip");

            var viewModels = checklists.Select(c => new ChecklistListViewModel
            {
                Id = c.Id,
                TripName = c.Trip?.Title ?? string.Empty,
                Title = c.Title
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var checklist = _unitOfWork.Checklist.Get(c => c.Id == id, includeProperties: "Trip,Items");
            if (checklist == null) return NotFound();

            // Перевірка доступу
            var role = GetUserRoleInTrip(checklist.TripId);
            if (role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає доступу до цього чекліста.";
                return RedirectToAction("Index", "Trips");
            }

            var currentUserId = _userManager.GetUserId(User);
            var templates = _unitOfWork.ChecklistTemplate.GetAll(t => t.OwnerId == null || t.OwnerId == currentUserId);

            ViewBag.Templates = templates.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Title
            }).ToList();

            var model = new ChecklistDetailsViewModel
            {
                Id = checklist.Id,
                Title = checklist.Title,
                TripName = checklist.Trip?.Title ?? "Невідомо",
                Items = checklist.Items.Select(i => new ChecklistItemListViewModel
                {
                    Id = i.Id,
                    ChecklistName = checklist.Title,
                    Content = i.Content,
                    IsChecked = i.IsChecked
                }).OrderBy(i => i.IsChecked).ThenBy(i => i.Content).ToList()
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult Create(int? tripId)
        {
            var allowedTrips = GetAllowedTripsForUser().ToList();

            if (!allowedTrips.Any())
            {
                TempData["ErrorMessage"] = "У вас немає поїздок, де ви можете додавати записи.";
                return RedirectToAction("Index", "Trips");
            }

            int activeTripId = (tripId.HasValue && tripId.Value > 0)
                ? tripId.Value
                : int.Parse(allowedTrips.First().Value);

            if (tripId.HasValue)
            {
                var role = GetUserRoleInTrip(activeTripId);
                if (role == "Viewer" || role == "None")
                {
                    TempData["ErrorMessage"] = "Глядачі не можуть додавати записи в цю поїздку.";
                    return RedirectToAction("Index", "Trips");
                }
            }

            ModelState.Clear();

            foreach (var t in allowedTrips)
            {
                t.Selected = (t.Value == activeTripId.ToString());
            }

            var model = new ChecklistFormViewModel
            {
                TripId = activeTripId,
                TripList = allowedTrips
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChecklistFormViewModel model)
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

            var entity = new Checklist
            {
                TripId = model.TripId,
                Title = model.Title
            };

            _unitOfWork.Checklist.Add(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Чекліст успішно створено!";
            return RedirectToAction("Details", "Trips", new { id = entity.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування.";
                return RedirectToAction("Index", "Trips");
            }

            var model = new ChecklistFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                Title = entity.Title,
                TripList = GetAllowedTripsForUser()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChecklistFormViewModel model)
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

            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.TripId = model.TripId;
            entity.Title = model.Title;

            _unitOfWork.Checklist.Update(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Чекліст оновлено!";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для видалення.";
                return RedirectToAction("Index", "Trips");
            }

            int tripId = entity.TripId;
            _unitOfWork.Checklist.Remove(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Чекліст видалено.";
            return RedirectToAction("Details", "Trips", new { id = tripId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTemplate(int id, int templateId)
        {
            if (templateId <= 0)
            {
                TempData["ErrorMessage"] = "Будь ласка, виберіть коректний шаблон.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var checklist = _unitOfWork.Checklist.Get(c => c.Id == id, includeProperties: "Items");
            if (checklist == null) return NotFound();

            var role = GetUserRoleInTrip(checklist.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для додавання записів у цей чекліст.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == templateId, includeProperties: "Items");
            if (template == null)
            {
                TempData["ErrorMessage"] = "Обраний шаблон не знайдено.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            foreach (var templateItem in template.Items)
            {
                if (!checklist.Items.Any(i => i.Content.Equals(templateItem.Content, StringComparison.OrdinalIgnoreCase)))
                {
                    checklist.Items.Add(new ChecklistItem
                    {
                        ChecklistId = id,
                        Content = templateItem.Content,
                        IsChecked = false 
                    });
                }
            }

            _unitOfWork.Checklist.Update(checklist);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Речі з шаблону '{template.Title}' успішно скопійовано до вашої валізи!";
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}