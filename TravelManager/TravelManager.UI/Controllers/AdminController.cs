using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data; 
using TravelManager.UI.Models.ViewModels.Admin;

namespace TravelManager.UI.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var totalUsers = await _userManager.Users.CountAsync();
            var totalTrips = await _context.Trips.CountAsync(); 

            var model = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalAdmins = admins.Count,
                TotalTrips = totalTrips
            };

            return View(model);
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