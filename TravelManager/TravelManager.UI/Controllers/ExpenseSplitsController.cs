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
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.CurrentUserId = currentUserId;

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();

            var splits = _unitOfWork.ExpenseSplit
    .GetAll(s => myTripIds.Contains(s.Expense.TripId),
            includeProperties: "Expense,Debtor,Expense.Trip,Expense.Payer,Expense.Transit,Expense.Accommodation,Expense.TripActivity");

            var viewModels = splits.Select(s => new ExpenseSplitListViewModel
            {
                Id = s.Id,
                ExpenseName = s.Expense?.Title ?? string.Empty,
                ExpenseTitle = s.Expense?.Title ?? string.Empty,
                DebtorName = s.Debtor?.UserName ?? string.Empty,
                OwedAmount = s.OwedAmount,
                IsSettled = s.IsSettled,
                ReceiptImageUrl = s.Expense?.ReceiptImageUrl,

                TripName = s.Expense?.Trip?.Title ?? "Без назви подорожі",
                TripId = s.Expense?.TripId ?? 0,
                Currency = s.Expense?.Currency ?? "UAH",
                PayerName = s.Expense?.Payer?.UserName ?? "Невідомий платник",
                PayerId = s.Expense?.PayerId ?? string.Empty,
                DebtorId = s.DebtorId ?? string.Empty,
                LinkedTransit = s.Expense?.Transit != null ? $"{s.Expense.Transit.DepartureLocation} - {s.Expense.Transit.ArrivalLocation}" : null,
                LinkedAccommodation = s.Expense?.Accommodation?.Name,
                LinkedActivity = s.Expense?.TripActivity?.Title
            }).ToList();

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSettled(int id)
        {
            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id, includeProperties: "Expense", tracked: true);
            if (entity == null)
            {
                return NotFound();
            }

            var role = GetUserRoleInTrip(entity.Expense.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть погашати борги.";
                return RedirectToAction(nameof(Index));
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
            var entity = _unitOfWork.ExpenseSplit.Get(u => u.Id == id, includeProperties: "Expense");
            if (entity == null)
            {
                return NotFound();
            }

            var role = GetUserRoleInTrip(entity.Expense.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть видаляти записи.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.ExpenseSplit.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }
    }
}