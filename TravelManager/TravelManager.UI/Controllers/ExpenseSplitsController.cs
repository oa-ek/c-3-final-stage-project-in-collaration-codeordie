using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class ExpenseSplitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public ExpenseSplitsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var splits = _unitOfWork.ExpenseSplit.GetAll(includeProperties: "Expense,Debtor");

            var viewModels = splits.Select(s => new ExpenseSplitListViewModel
            {
                Id = s.Id,
                ExpenseName = s.Expense?.Title ?? string.Empty,
                DebtorName = s.Debtor?.UserName ?? string.Empty,
                OwedAmount = s.OwedAmount,
                IsSettled = s.IsSettled
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ExpenseSplitFormViewModel
            {
                ExpenseList = GetExpenseList(),
                DebtorList = GetDebtorList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseSplitFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ExpenseList = GetExpenseList();
                model.DebtorList = GetDebtorList();
                return View(model);
            }

            var entity = new ExpenseSplit
            {
                ExpenseId = model.ExpenseId,
                DebtorId = model.DebtorId,
                OwedAmount = model.OwedAmount,
                IsSettled = model.IsSettled
            };

            _unitOfWork.ExpenseSplit.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            var model = new ExpenseSplitFormViewModel
            {
                Id = entity.Id,
                ExpenseId = entity.ExpenseId,
                DebtorId = entity.DebtorId,
                OwedAmount = entity.OwedAmount,
                IsSettled = entity.IsSettled,
                ExpenseList = GetExpenseList(),
                DebtorList = GetDebtorList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseSplitFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ExpenseList = GetExpenseList();
                model.DebtorList = GetDebtorList();
                return View(model);
            }

            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.ExpenseId = model.ExpenseId;
            entity.DebtorId = model.DebtorId;
            entity.OwedAmount = model.OwedAmount;
            entity.IsSettled = model.IsSettled;

            _unitOfWork.ExpenseSplit.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            _unitOfWork.ExpenseSplit.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetExpenseList()
        {
            return _unitOfWork.Expense.GetAll().Select(e => new SelectListItem
            {
                Text = e.Title,
                Value = e.Id.ToString()
            });
        }

        private IEnumerable<SelectListItem> GetDebtorList()
        {
            return _userManager.Users.ToList().Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id
            });
        }
    }
}