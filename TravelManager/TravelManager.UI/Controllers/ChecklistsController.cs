using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

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
        public IActionResult Create()
        {
            var model = new ChecklistFormViewModel
            {
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

            var entity = new Checklist
            {
                TripId = model.TripId,
                Title = model.Title
            };

            _unitOfWork.Checklist.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

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
            if (entity == null)
            {
                return NotFound();
            }

            entity.TripId = model.TripId;
            entity.Title = model.Title;

            _unitOfWork.Checklist.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Checklist.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            _unitOfWork.Checklist.Remove(entity);
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
    }
}