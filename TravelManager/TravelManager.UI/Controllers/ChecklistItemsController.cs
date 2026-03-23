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

        public ChecklistItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var items = _unitOfWork.ChecklistItem.GetAll(includeProperties: "Checklist");

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
        public IActionResult Create()
        {
            var model = new ChecklistItemFormViewModel
            {
                ChecklistList = GetChecklists()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChecklistItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ChecklistList = GetChecklists();
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

            var model = new ChecklistItemFormViewModel
            {
                Id = entity.Id,
                ChecklistId = entity.ChecklistId,
                Content = entity.Content,
                IsChecked = entity.IsChecked,
                ChecklistList = GetChecklists()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChecklistItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ChecklistList = GetChecklists();
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
    }
}