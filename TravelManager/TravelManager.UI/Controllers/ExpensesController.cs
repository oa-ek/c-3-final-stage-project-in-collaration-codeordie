using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelManager.UI.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpensesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var expenses = _unitOfWork.Expense.GetAll(includeProperties: "Trip,Category");
            var viewModels = expenses.Select(e => new ExpenseListViewModel
            {
                Id = e.Id,
                Description = e.Title,
                Amount = e.TotalAmount,
                Currency = e.Currency,
                Date = e.Date,
                CategoryName = e.Category?.Name ?? "Невідомо",
                TripTitle = e.Trip?.Title ?? "Невідомо"
            }).ToList();
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? tripId)
        {
            var model = new ExpenseFormViewModel
            {
                TripId = tripId ?? 0,
                Date = DateTime.Now,
                TripList = GetTripList(),
                CategoryList = GetCategoryList(),
                CurrencyList = GetCurrencyList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                return View(model);
            }

            var entity = new Expense
            {
                TripId = model.TripId,
                CategoryId = model.CategoryId,
                TotalAmount = model.Amount,
                Currency = model.Currency,
                Title = model.Description,
                Date = model.Date,
                PayerId = "temporary-user-id"
            };

            _unitOfWork.Expense.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var model = new ExpenseFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                CategoryId = entity.CategoryId,
                Amount = entity.TotalAmount,
                Currency = entity.Currency,
                Description = entity.Title,
                Date = entity.Date,
                TripList = GetTripList(),
                CategoryList = GetCategoryList(),
                CurrencyList = GetCurrencyList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                return View(model);
            }

            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.TripId = model.TripId;
            entity.CategoryId = model.CategoryId;
            entity.TotalAmount = model.Amount;
            entity.Currency = model.Currency;
            entity.Title = model.Description;
            entity.Date = model.Date;

            _unitOfWork.Expense.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            int tripId = entity.TripId;
            _unitOfWork.Expense.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem { Text = t.Title, Value = t.Id.ToString() });
        }

        private IEnumerable<SelectListItem> GetCategoryList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Accommodation", Value = "1" },
                new SelectListItem { Text = "Transport", Value = "2" },
                new SelectListItem { Text = "Food", Value = "3" },
                new SelectListItem { Text = "Entertainment", Value = "4" },
                new SelectListItem { Text = "Shopping", Value = "5" },
                new SelectListItem { Text = "Other", Value = "6" }
            };
        }

        private IEnumerable<SelectListItem> GetCurrencyList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "UAH (Гривня)", Value = "UAH" },
                new SelectListItem { Text = "USD (Долар США)", Value = "USD" },
                new SelectListItem { Text = "EUR (Євро)", Value = "EUR" }
            };
        }
    }
}