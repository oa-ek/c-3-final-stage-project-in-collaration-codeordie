using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelManager.UI.Controllers
{
    public class ChecklistsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChecklistsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var checklists = _unitOfWork.Checklist.GetAll(includeProperties: "Trip");
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
            var model = new ChecklistFormViewModel
            {
                TripId = tripId ?? 0,
                TripList = GetTripList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChecklistFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                return View(model);
            }

            var entity = new Checklist { TripId = model.TripId, Title = model.Title };
            _unitOfWork.Checklist.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var model = new ChecklistFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                Title = entity.Title,
                TripList = GetTripList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChecklistFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                return View(model);
            }

            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.TripId = model.TripId;
            entity.Title = model.Title;
            _unitOfWork.Checklist.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            int tripId = entity.TripId;
            _unitOfWork.Checklist.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem { Text = t.Title, Value = t.Id.ToString() });
        }
    }
}