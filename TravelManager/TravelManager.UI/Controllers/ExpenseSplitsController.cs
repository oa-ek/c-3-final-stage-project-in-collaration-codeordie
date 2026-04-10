using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settle(int id)
        {
            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.IsSettled = true;

            _unitOfWork.ExpenseSplit.Update(entity);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Борг успішно позначено як погашений!";
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
    }
}