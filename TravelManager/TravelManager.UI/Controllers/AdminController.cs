using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using TravelManager.UI.Models.ViewModels.Admin;
using TravelManager.Infrastructure.Interfaces;
using System;

namespace TravelManager.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(UserManager<User> userManager, ApplicationDbContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var totalTrips = await _context.Trips.CountAsync();

            var registrationsPerMonth = users
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", new System.Globalization.CultureInfo("uk-UA")),
                    Count = g.Count()
                })
                .TakeLast(6)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalUsers = users.Count,
                TotalAdmins = admins.Count,
                TotalTrips = totalTrips,
                RegistrationMonths = registrationsPerMonth.Select(r => r.Month).ToList(),
                RegistrationsCount = registrationsPerMonth.Select(r => r.Count).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastEmail(string subject, string message, string targetGroup)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Тема та текст листа не можуть бути порожніми!";
                return RedirectToAction(nameof(AdminDashboard));
            }

            var allUsers = await _userManager.Users.ToListAsync();
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(a => a.Id).ToList();

            var targetUsers = new List<User>();
            if (targetGroup == "Admins")
            {
                targetUsers = admins.ToList();
            }
            else if (targetGroup == "Users")
            {
                targetUsers = allUsers.Where(u => !adminIds.Contains(u.Id)).ToList();
            }
            else
            {
                targetUsers = allUsers; // "All"
            }

            string formattedMessage = message.Replace("\n", "<br>");
            string mailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8fafc; border-radius: 10px;'>
                    <h2 style='color: #0369a1;'>Повідомлення від адміністрації TravelManager</h2>
                    <div style='font-size: 16px; color: #334155; line-height: 1.6;'>
                        {formattedMessage}
                    </div>
                </div>";

            int successCount = 0;
            int errorCount = 0;
            string lastErrorMessage = "";

            foreach (var user in targetUsers)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(user.Email, subject, mailBody);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        // Тепер ми ловимо помилку!
                        errorCount++;
                        lastErrorMessage = ex.Message;
                    }
                }
            }

            if (successCount > 0)
            {
                TempData["SuccessMessage"] = $"Розсилку успішно відправлено {successCount} користувачам (Група: {targetGroup})!";
                if (errorCount > 0)
                {
                    TempData["SuccessMessage"] += $" Проте {errorCount} листів не відправлено.";
                }
            }
            else
            {
                // Якщо 0 успіхів, показуємо реальну причину:
                TempData["ErrorMessage"] = $"Помилка відправки. Перевірте налаштування SMTP. Деталі: {lastErrorMessage}";
            }

            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            var users = await _userManager.Users.ToListAsync();
            var modelList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                modelList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "Невідомо",
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(modelList.OrderByDescending(u => u.CreatedAt));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Ви не можете видалити власний акаунт!";
                return RedirectToAction(nameof(UserList));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Користувача успішно видалено.";
            else
                TempData["ErrorMessage"] = "Помилка при видаленні користувача.";

            return RedirectToAction(nameof(UserList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "Ви не можете змінити роль самому собі!";
                return RedirectToAction(nameof(UserList));
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = $"Користувача {user.Email} понижено до 'User'.";
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["SuccessMessage"] = $"Користувача {user.Email} підвищено до 'Admin'.";
            }

            return RedirectToAction(nameof(UserList));
        }
    }
}