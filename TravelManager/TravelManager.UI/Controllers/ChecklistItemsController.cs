using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class ChecklistItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public ChecklistItemsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
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

            var items = _unitOfWork.ChecklistItem
    .GetAll(i => myTripIds.Contains(i.Checklist.TripId), includeProperties: "Checklist");

            var viewModels = items.Select(i => new ChecklistItemListViewModel
            {
                Id = i.Id,
                ChecklistName = i.Checklist?.Title ?? string.Empty,
                Content = i.Content,
                IsChecked = i.IsChecked
            }).OrderBy(i => i.ChecklistName).ThenBy(i => i.IsChecked).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? checklistId)
        {
            var allowedChecklists = GetAllowedChecklistsForUser();
            if (!allowedChecklists.Any())
            {
                TempData["ErrorMessage"] = "У вас немає списків, куди можна додати речі.";
                return RedirectToAction("Index");
            }
            if (checklistId.HasValue)
            {
                var role = GetUserRoleByChecklistId(checklistId.Value);
                if (role == "Viewer" || role == "None")
                {
                    TempData["ErrorMessage"] = "Глядачі не можуть додавати речі у цей список.";
                    return RedirectToAction("Details", "Checklists", new { id = checklistId.Value });
                }
            }

            var model = new ChecklistItemFormViewModel
            {
                ChecklistId = checklistId ?? 0, 
                ChecklistList = allowedChecklists
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChecklistItemFormViewModel model)
        {
            var role = GetUserRoleByChecklistId(model.ChecklistId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть додавати речі у список.";
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                model.ChecklistList = GetAllowedChecklistsForUser();
                return View(model);
            }

            var entity = new ChecklistItem
            {
                ChecklistId = model.ChecklistId,
                Content = model.Content,
                IsChecked = model.IsChecked
            };

            _unitOfWork.ChecklistItem.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Checklists", new { id = model.ChecklistId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.ChecklistItem.Get(u => u.Id == id);
            if (entity == null) return NotFound();
            var role = GetUserRoleByChecklistId(entity.ChecklistId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index");
            }

            var model = new ChecklistItemFormViewModel
            {
                Id = entity.Id,
                ChecklistId = entity.ChecklistId,
                Content = entity.Content,
                IsChecked = entity.IsChecked,
                ChecklistList = GetAllowedChecklistsForUser()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChecklistItemFormViewModel model)
        {
            var role = GetUserRoleByChecklistId(model.ChecklistId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                model.ChecklistList = GetAllowedChecklistsForUser();
                return View(model);
            }

            var entity = _unitOfWork.ChecklistItem.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.ChecklistId = model.ChecklistId;
            entity.Content = model.Content;
            entity.IsChecked = model.IsChecked;

            _unitOfWork.ChecklistItem.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Checklists", new { id = model.ChecklistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.ChecklistItem.Get(u => u.Id == id);
            if (entity == null) return NotFound();
            var role = GetUserRoleByChecklistId(entity.ChecklistId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть видаляти записи.";
                return RedirectToAction("Index");
            }

            var checklistId = entity.ChecklistId;

            _unitOfWork.ChecklistItem.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Checklists", new { id = checklistId });
        }

        private IEnumerable<SelectListItem> GetChecklists()
        {
            return _unitOfWork.Checklist.GetAll().Select(c => new SelectListItem
            {
                Text = c.Title,
                Value = c.Id.ToString()
            });
        }

        private string GetUserRoleByChecklistId(int checklistId)
        {
            var checklist = _unitOfWork.Checklist.Get(c => c.Id == checklistId);
            if (checklist == null) return "None";

            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == checklist.TripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }

        private IEnumerable<SelectListItem> GetAllowedChecklistsForUser()
        {
            var currentUserId = _userManager.GetUserId(User);
            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId && tp.Role.Name != "Viewer")
                .Select(tp => tp.TripId).ToList();

            return _unitOfWork.Checklist.GetAll(c => myTripIds.Contains(c.TripId))
                .Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.Id.ToString()
                });
        }
    }
}