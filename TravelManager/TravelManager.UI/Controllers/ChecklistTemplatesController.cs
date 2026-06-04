using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class ChecklistTemplatesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        // ВИПРАВЛЕННЯ: Явно вказуємо повний шлях до моделі User
        private readonly UserManager<TravelManager.Domain.Entities.User> _userManager;

        public ChecklistTemplatesController(IUnitOfWork unitOfWork, UserManager<TravelManager.Domain.Entities.User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var templates = _unitOfWork.ChecklistTemplate
                .GetAll(t => t.OwnerId == null || t.OwnerId == userId, includeProperties: "Items");

            // Перетворюємо Entities у ViewModel
            var viewModels = templates.Select(t => new TemplateListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description ?? "",
                IconClass = t.IconClass ?? "bi-card-checklist",
                ItemsCount = t.Items?.Count ?? 0,
                IsSystem = t.OwnerId == null
            }).ToList();

            return View(viewModels); // Передаємо чисту ViewModel
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(int id)
        {
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id, includeProperties: "Items");
            if (template == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var newTemplate = new ChecklistTemplate
            {
                OwnerId = userId,
                Title = template.Title + " (Моя копія)",
                Description = template.Description,
                IconClass = template.IconClass ?? "bi-card-checklist",
                Items = template.Items.Select(i => new ChecklistTemplateItem
                {
                    Content = i.Content,
                    SortOrder = i.SortOrder
                }).ToList()
            };

            _unitOfWork.ChecklistTemplate.Add(newTemplate);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Шаблон успішно скопійовано! Тепер ви можете редагувати його під себе.";
            return RedirectToAction(nameof(Edit), new { id = newTemplate.Id });
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id, includeProperties: "Items");

            if (template == null) return NotFound();
            if (template.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "Створіть копію для редагування.";
                return RedirectToAction(nameof(Index));
            }

            // Перетворюємо Entity у ViewModel
            var viewModel = new TemplateEditViewModel
            {
                Id = template.Id,
                Title = template.Title,
                Items = template.Items.OrderBy(i => i.Content).Select(i => new TemplateItemViewModel
                {
                    Id = i.Id,
                    Content = i.Content
                }).ToList()
            };

            return View(viewModel); // Передаємо чисту ViewModel
        }

        // ВИПРАВЛЕННЯ: Додаємо пункт прямо в список Items ігрового шаблону
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int templateId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(Edit), new { id = templateId });

            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == templateId, includeProperties: "Items");

            if (template != null && template.OwnerId == userId)
            {
                template.Items.Add(new ChecklistTemplateItem
                {
                    Content = content
                });
                _unitOfWork.ChecklistTemplate.Update(template);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Edit), new { id = templateId });
        }

        // ВИПРАВЛЕННЯ: Видаляємо пункт так само через колекцію Items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int templateId)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == templateId, includeProperties: "Items");

            if (template != null && template.OwnerId == userId)
            {
                var itemToRemove = template.Items.FirstOrDefault(i => i.Id == itemId);
                if (itemToRemove != null)
                {
                    template.Items.Remove(itemToRemove);
                    _unitOfWork.ChecklistTemplate.Update(template);
                    await _unitOfWork.SaveAsync();
                }
            }
            return RedirectToAction(nameof(Edit), new { id = templateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id);

            if (template != null && template.OwnerId == userId)
            {
                _unitOfWork.ChecklistTemplate.Remove(template);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Шаблон видалено.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}