using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Services;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly ExcelExportService _excelService;

        public ExportController(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            ExcelExportService excelService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _excelService = excelService;
        }

        // ── GET /Export/Trip/{tripId} ──────────────────────────────────
        // Показує сторінку вибору параметрів (фільтр по учаснику)
        [HttpGet]
        public IActionResult Trip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var trip = _unitOfWork.Trip.Get(t => t.Id == tripId);
            if (trip == null) return NotFound();

            // Перевіряємо доступ
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId);
            if (participant == null)
            {
                TempData["ErrorMessage"] = "У вас немає доступу до цієї подорожі.";
                return RedirectToAction("Index", "Trips");
            }

            // Список учасників для фільтру
            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == tripId, includeProperties: "User")
                .Select(p => new SelectListItem
                {
                    Value = p.UserId,
                    Text = p.User.UserName ?? p.User.Email ?? p.UserId
                })
                .ToList();

            ViewBag.TripId = tripId;
            ViewBag.TripTitle = trip.Title;
            ViewBag.Participants = participants;

            return View();
        }

        // ── GET /Export/DownloadTripReport ─────────────────────────────
        // Генерує та повертає .xlsx файл
        [HttpGet]
        public IActionResult DownloadTripReport(int tripId, string? filterUserId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Перевірка доступу
            var access = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId);
            if (access == null)
                return Forbid();

            // Завантажуємо дані
            var trip = _unitOfWork.Trip.Get(t => t.Id == tripId);
            if (trip == null) return NotFound();

            var expenses = _unitOfWork.Expense
                .GetAll(e => e.TripId == tripId,
                        includeProperties: "Category,Payer,Trip")
                .ToList();

            var splits = _unitOfWork.ExpenseSplit
                .GetAll(s => s.Expense.TripId == tripId,
                        includeProperties: "Expense,Expense.Payer,Debtor")
                .ToList();

            // Ім'я фільтрованого користувача (для заголовку аркуша)
            string? filterUserName = null;
            if (!string.IsNullOrEmpty(filterUserId))
            {
                var filtered = _unitOfWork.TripParticipant
                    .Get(tp => tp.TripId == tripId && tp.UserId == filterUserId,
                         includeProperties: "User");
                filterUserName = filtered?.User?.UserName ?? filtered?.User?.Email;
            }

            // Генеруємо Excel
            var bytes = _excelService.GenerateTripReport(
                trip, expenses, splits, filterUserId, filterUserName);

            // Формуємо ім'я файлу: TravelManager_НазваПодорожі_YYYY-MM-DD.xlsx
            string safeName = string.Concat(trip.Title
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
                .Replace(" ", "_");
            string fileName = $"TravelManager_{safeName}_{DateTime.Now:yyyy-MM-dd}.xlsx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}