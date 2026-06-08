using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class ChecklistTemplatesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public ChecklistTemplatesController(IUnitOfWork unitOfWork, UserManager<User> userManager)
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

            var viewModels = templates.Select(t => new TemplateListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description ?? "Немає опису",
                IconClass = t.IconClass ?? "bi-card-checklist",
                ItemsCount = t.Items?.Count ?? 0,
                IsSystem = t.OwnerId == null
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TemplateCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var newTemplate = new ChecklistTemplate
            {
                OwnerId = userId,
                Title = model.Title,
                Description = model.Description,
                IconClass = "bi-backpack4-fill" 
            };

            _unitOfWork.ChecklistTemplate.Add(newTemplate);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Шаблон успішно створено! Додайте сюди перші речі.";
            return RedirectToAction(nameof(Edit), new { id = newTemplate.Id });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id, includeProperties: "Items");
            if (template == null) return NotFound();

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

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id, includeProperties: "Items");

            if (template == null) return NotFound();
            if (template.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "Ви не можете редагувати базові системні шаблони.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TemplateEditViewModel
            {
                Id = template.Id,
                Title = template.Title,
                Items = template.Items.OrderBy(i => i.Id).Select(i => new TemplateItemViewModel
                {
                    Id = i.Id,
                    Content = i.Content
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TemplateEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == model.Id, includeProperties: "Items");

            if (template == null) return NotFound();
            if (template.OwnerId != userId) return Forbid();

            template.Title = model.Title;

            if (model.Items != null)
            {
                foreach (var submittedItem in model.Items)
                {
                    var dbItem = template.Items.FirstOrDefault(i => i.Id == submittedItem.Id);
                    if (dbItem != null)
                    {
                        dbItem.Content = submittedItem.Content?.Trim() ?? string.Empty;
                    }
                }
            }

            _unitOfWork.ChecklistTemplate.Update(template);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Шаблон та його елементи успішно оновлено!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int templateId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Назва речі не може бути порожньою.";
                return RedirectToAction(nameof(Edit), new { id = templateId });
            }

            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == templateId, includeProperties: "Items");

            if (template == null) return NotFound();
            if (template.OwnerId != userId) return Forbid();

            var newItem = new ChecklistTemplateItem
            {
                ChecklistTemplateId = templateId,
                Content = content.Trim()
            };

            _unitOfWork.ChecklistTemplateItem.Add(newItem);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Edit), new { id = templateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int templateId)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == templateId);

            if (template == null) return NotFound();
            if (template.OwnerId != userId) return Forbid();

            var item = _unitOfWork.ChecklistTemplateItem.Get(i => i.Id == itemId);
            if (item != null)
            {
                _unitOfWork.ChecklistTemplateItem.Remove(item);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = templateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(int id)
        {
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id, includeProperties: "Items");
            if (template == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var clonedTemplate = new ChecklistTemplate
            {
                OwnerId = userId,
                Title = template.Title + " (Копія)",
                Description = template.Description,
                IconClass = template.IconClass ?? "bi-backpack4-fill"
            };

            _unitOfWork.ChecklistTemplate.Add(clonedTemplate);
            await _unitOfWork.SaveAsync();

            foreach (var item in template.Items)
            {
                _unitOfWork.ChecklistTemplateItem.Add(new ChecklistTemplateItem
                {
                    ChecklistTemplateId = clonedTemplate.Id,
                    Content = item.Content
                });
            }
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Базовий шаблон скопійовано! Тепер ви можете його налаштувати.";
            return RedirectToAction(nameof(Edit), new { id = clonedTemplate.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var template = _unitOfWork.ChecklistTemplate.Get(t => t.Id == id);

            if (template == null) return NotFound();
            if (template.OwnerId != userId) return Forbid();

            _unitOfWork.ChecklistTemplate.Remove(template);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Шаблон успішно видалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}