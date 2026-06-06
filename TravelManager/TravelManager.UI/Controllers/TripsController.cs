using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices; // Додано для IExchangeRateService
using TravelManager.Infrastructure.Services;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IExchangeRateService _exchangeService;
        private readonly IEmailService _emailService;

        public TripsController(IUnitOfWork unitOfWork, UserManager<User> userManager, IExchangeRateService exchangeService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _exchangeService = exchangeService;
            _emailService = emailService;
        }

        private async Task<List<SelectListItem>> GetCurrencyDropdownListAsync()
        {
            var rawCurrencies = await _exchangeService.GetAllCurrenciesAsync();

            var dropdownList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UAH (Українська гривня)", Value = "UAH" }
            };

            dropdownList.AddRange(rawCurrencies.Select(r => new SelectListItem
            {
                Text = $"{r.CurrencyCode} ({r.CurrencyName})",
                Value = r.CurrencyCode
            }));

            return dropdownList;
        }

        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userTrips = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId, includeProperties: "Trip,Role")
                .Select(tp => new TripListViewModel
                {
                    Id = tp.Trip.Id,
                    Title = tp.Trip.Title,
                    StartDate = tp.Trip.StartDate,
                    EndDate = tp.Trip.EndDate,
                    CurrentUserRole = tp.Role?.Name ?? "None"
                })
                .ToList();

            return View(userTrips);
        }

        [HttpGet]
        public async Task<IActionResult> Create() // Зроблено Async
        {
            var model = new CreateTripViewModel
            {
                // Поля UserList немає в моделі, тому список користувачів (якщо потрібен) передаємо через ViewBag
                BaseCurrency = "UAH"
            };

            ViewBag.UserList = GetUserList(); // Передаємо список користувачів через ViewBag, якщо форма його очікує
            ViewBag.CurrencyList = await GetCurrencyDropdownListAsync(); // Передаємо динамічний список валют
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            // Видалено неіснуючі в моделі поля з ModelState.Remove
            ModelState.Remove("BaseCurrencyList");

            if (!ModelState.IsValid)
            {
                ViewBag.UserList = GetUserList();
                ViewBag.CurrencyList = await GetCurrencyDropdownListAsync(); // Передаємо знову при помилці валідації
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var newTrip = new Trip
            {
                Title = model.Title,
                Description = model.Description,
                DepartureLocation = model.DepartureLocation,
                ReturnLocation = model.ReturnLocation,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                BaseCurrency = model.BaseCurrency, // Зберігаємо обрану з повного списку валюту
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatorId = currentUserId
            };

            _unitOfWork.Trip.Add(newTrip);
            await _unitOfWork.SaveAsync();

            // Автоматично додаємо творця як Organizer
            var ownerRole = _unitOfWork.TripRole.Get(r => r.Name == "Organizer")
                            ?? _unitOfWork.TripRole.GetAll().FirstOrDefault();

            if (ownerRole != null)
            {
                var creatorParticipant = new TripParticipant
                {
                    TripId = newTrip.Id,
                    UserId = currentUserId,
                    RoleId = ownerRole.Id
                };
                _unitOfWork.TripParticipant.Add(creatorParticipant);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) // Зроблено Async
        {
            var role = GetUserRoleInTrip(id);
            if (role != "Organizer")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування цієї поїздки.";
                return RedirectToAction("Details", new { id });
            }

            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null) return NotFound();

            var model = new CreateTripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                DepartureLocation = trip.DepartureLocation,
                ReturnLocation = trip.ReturnLocation,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                BaseCurrency = trip.BaseCurrency
            };

            // Передаємо динамічний список валют та позначаємо вибрану
            var currencies = await GetCurrencyDropdownListAsync();
            foreach (var item in currencies)
            {
                if (item.Value == trip.BaseCurrency)
                {
                    item.Selected = true;
                }
            }

            ViewBag.UserList = GetUserList();
            ViewBag.CurrencyList = currencies;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateTripViewModel model)
        {
            var role = GetUserRoleInTrip(id);
            if (role != "Organizer")
            {
                TempData["ErrorMessage"] = "У вас немає прав для редагування цієї поїздки.";
                return RedirectToAction("Details", new { id });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserList = GetUserList();
                ViewBag.CurrencyList = await GetCurrencyDropdownListAsync(); // Передаємо знову при помилці валідації
                return View(model);
            }

            var tripFromDb = _unitOfWork.Trip.Get(u => u.Id == id);
            if (tripFromDb == null) return NotFound();

            tripFromDb.Title = model.Title;
            tripFromDb.Description = model.Description;
            tripFromDb.DepartureLocation = model.DepartureLocation;
            tripFromDb.ReturnLocation = model.ReturnLocation;
            tripFromDb.StartDate = model.StartDate;
            tripFromDb.EndDate = model.EndDate;
            tripFromDb.BaseCurrency = model.BaseCurrency;

            _unitOfWork.Trip.Update(tripFromDb);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = GetUserRoleInTrip(id);
            if (role != "Organizer")
            {
                TempData["ErrorMessage"] = "Тільки Організатор може видалити поїздку.";
                return RedirectToAction("Details", new { id });
            }

            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null) return NotFound();

            _unitOfWork.Trip.Remove(trip);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var trip = _unitOfWork.Trip.Get(u => u.Id == id, includeProperties: "Status");
            if (trip == null) return NotFound();

            var currentParticipant = _unitOfWork.TripParticipant
                .Get(p => p.TripId == id && p.UserId == currentUserId, includeProperties: "Role");

            if (currentParticipant == null)
            {
                TempData["ErrorMessage"] = "У вас немає доступу до цієї поїздки.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserRole = currentParticipant.Role?.Name ?? "None";

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == id, includeProperties: "User,Role");

            var allRoles = _unitOfWork.TripRole.GetAll().Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            });

            ViewBag.AvailableRoles = allRoles;
            ViewBag.CurrentUserRole = currentUserRole;

            var model = new TripDetailsViewModel
            {
                Trip = trip,
                CurrentUserRole = currentUserRole,
                Destinations = _unitOfWork.TripDestination
                    .GetAll(d => d.TripId == id)
                    .OrderBy(d => d.ArrivalDate)
                    .ToList(),
                Accommodations = _unitOfWork.Accommodation
                    .GetAll(a => a.TripId == id)
                    .ToList(),
                Transits = _unitOfWork.Transit
                    .GetAll(t => t.TripId == id, includeProperties: "TransitType")
                    .ToList(),
                Checklists = _unitOfWork.Checklist
                    .GetAll(c => c.TripId == id, includeProperties: "Items")
                    .ToList(),
                Expenses = _unitOfWork.Expense
                    .GetAll(e => e.TripId == id)
                    .ToList(),
                Participants = participants.Select(p => new TripParticipantViewModel
                {
                    UserId = p.UserId,
                    UserName = p.User?.UserName ?? "?",
                    Role = p.Role?.Name ?? "Participant"
                }).ToList()
            };

            return View(model);
        }

        // Цей метод вставляється у твій TripsController.cs замість старого InviteParticipant

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteParticipant(int tripId, string email, int roleId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentParticipant = _unitOfWork.TripParticipant
                .Get(p => p.TripId == tripId && p.UserId == currentUserId, includeProperties: "Role");

            if (currentParticipant?.Role?.Name != "Organizer")
            {
                TempData["ErrorMessage"] = "У вас немає прав для запрошення учасників.";
                return RedirectToAction("Details", new { id = tripId });
            }

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email не може бути порожнім.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var trip = _unitOfWork.Trip.Get(t => t.Id == tripId);
            if (trip == null) return NotFound();

            // ЗАХИСТ: Якщо роль не передалась з форми (дорівнює 0), беремо стандартну роль
            if (roleId == 0)
            {
                var defaultRole = _unitOfWork.TripRole.Get(r => r.Name == "Participant" || r.Name == "Member")
                                  ?? _unitOfWork.TripRole.GetAll().FirstOrDefault();
                if (defaultRole != null)
                {
                    roleId = defaultRole.Id;
                }
            }

            var userToInvite = await _userManager.FindByEmailAsync(email);

            // =========================================================================
            // СЦЕНАРІЙ А: Користувача НЕМАЄ в системі (Надсилаємо РЕФЕРАЛЬНЕ ЗАПРОШЕННЯ)
            // =========================================================================
            if (userToInvite == null)
            {
                var registerUrl = Url.Action("Register", "Account",
                    new { tripId = tripId, email = email, roleId = roleId },
                    protocol: HttpContext.Request.Scheme);

                string inviteSubject = $"Запрошення до подорожі \"{trip.Title}\"!";

                string inviteBody =
                    "<div style='font-family: Arial, sans-serif; padding: 25px; background-color: #f8fafc; border-radius: 12px; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0;'>" +
                        "<h2 style='color: #4f46e5; margin-top: 0;'>Привіт! 👋</h2>" +
                        "<p style='font-size: 16px; color: #334155; line-height: 1.6;'>" +
                            $"Вас запрошують приєднатися до спільного планування подорожі <strong>\"{trip.Title}\"</strong> у додатку <strong>TravelManager</strong>!" +
                        "</p>" +
                        "<p style='font-size: 14px; color: #64748b;'>" +
                            "Будь ласка, створіть акаунт за посиланням нижче, щоб автоматично долучитися до планів подорожі:" +
                        "</p>" +
                        "<div style='margin: 25px 0; text-align: center;'>" +
                            $"<a href='{registerUrl}' style='display: inline-block; padding: 14px 28px; background-color: #4f46e5; color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 15px; box-shadow: 0 4px 15px rgba(79, 70, 229, 0.35);'>Зареєструватися та Приєднатися</a>" +
                        "</div>" +
                        "<hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'>" +
                        "<small style='color: #94a3b8;'>З повагою, команда розробників TravelManager.</small>" +
                    "</div>";

                try
                {
                    await _emailService.SendEmailAsync(email, inviteSubject, inviteBody);
                    TempData["SuccessMessage"] = $"Користувача немає в системі. Надіслано посилання на реєстрацію на пошту {email}!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Помилка відправки листа: {ex.Message}";
                }

                return RedirectToAction("Details", new { id = tripId });
            }

            // =========================================================================
            // СЦЕНАРІЙ Б: Користувач ВЖЕ зареєстрований
            // =========================================================================
            var existingParticipant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == userToInvite.Id);

            if (existingParticipant != null)
            {
                TempData["ErrorMessage"] = "Цей користувач вже є учасником поїздки.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var organizerRole = _unitOfWork.TripRole.Get(r => r.Name == "Organizer");
            if (organizerRole != null && roleId == organizerRole.Id)
            {
                TempData["ErrorMessage"] = "Неможливо запросити користувача одразу як Організатора.";
                return RedirectToAction("Details", new { id = tripId });
            }

            try
            {
                // Додаємо в базу
                _unitOfWork.TripParticipant.Add(new TripParticipant
                {
                    TripId = tripId,
                    UserId = userToInvite.Id,
                    RoleId = roleId
                });
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Помилка при збереженні в БД: {ex.Message}";
                return RedirectToAction("Details", new { id = tripId });
            }

            // Відправляємо сповіщення зареєстрованому користувачу на пошту
            string notificationSubject = $"Вас додано до подорожі \"{trip.Title}\"!";
            var tripDetailsUrl = Url.Action("Details", "Trips", new { id = tripId }, protocol: HttpContext.Request.Scheme);

            string notificationBody =
                "<div style='font-family: Arial, sans-serif; padding: 25px; background-color: #f0fdf4; border-radius: 12px; max-width: 600px; margin: 0 auto; border: 1px solid #bbf7d0;'>" +
                    $"<h2 style='color: #16a34a; margin-top: 0;'>Вітаємо, {userToInvite.UserName}! 🎉</h2>" +
                    "<p style='font-size: 16px; color: #1e293b; line-height: 1.6;'>" +
                        $"Вас успішно додано як учасника до подорожі <strong>\"{trip.Title}\"</strong>!" +
                    "</p>" +
                    "<p style='font-size: 14px; color: #475569;'>" +
                        "Ви вже можете переглянути детальний маршрут, завантажувати чеки та чеклісти речей у своєму особистому кабінеті." +
                    "</p>" +
                    "<div style='margin: 25px 0; text-align: center;'>" +
                        $"<a href='{tripDetailsUrl}' style='display: inline-block; padding: 14px 28px; background-color: #16a34a; color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 15px;'>Переглянути поїздку</a>" +
                    "</div>" +
                "</div>";

            try
            {
                await _emailService.SendEmailAsync(userToInvite.Email, notificationSubject, notificationBody);
            }
            catch
            {
                // Ігноруємо помилку пошти, якщо збереження в БД пройшло успішно
            }

            TempData["SuccessMessage"] = $"Користувача {userToInvite.UserName} успішно додано до поїздки!";
            return RedirectToAction("Details", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant(int tripId, string userId)
        {
            var role = GetUserRoleInTrip(tripId);
            if (role != "Organizer")
            {
                TempData["ErrorMessage"] = "У вас немає прав для видалення учасників.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var participant = _unitOfWork.TripParticipant
                .Get(p => p.TripId == tripId && p.UserId == userId, includeProperties: "Role");

            if (participant == null) return NotFound();

            if (participant.Role?.Name == "Organizer")
            {
                TempData["ErrorMessage"] = "Неможливо видалити Організатора поїздки.";
                return RedirectToAction("Details", new { id = tripId });
            }

            if (participant.UserId == currentUserId()) // Виклик методу для отримання поточного користувача
            {
                TempData["ErrorMessage"] = "Ви не можете видалити самого себе з поїздки.";
                return RedirectToAction("Details", new { id = tripId });
            }

            _unitOfWork.TripParticipant.Remove(participant);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Учасника успішно видалено з поїздки.";
            return RedirectToAction("Details", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateParticipantRole(int tripId, string userId, int roleId)
        {
            var role = GetUserRoleInTrip(tripId);
            if (role != "Organizer")
            {
                TempData["ErrorMessage"] = "У вас немає прав для зміни ролей.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var participantToUpdate = _unitOfWork.TripParticipant
                .Get(p => p.TripId == tripId && p.UserId == userId, includeProperties: "Role");

            if (participantToUpdate == null) return NotFound();

            if (participantToUpdate.Role?.Name == "Organizer" && roleId != participantToUpdate.RoleId)
            {
                TempData["ErrorMessage"] = "Неможливо змінити роль Організатора напряму.";
                return RedirectToAction("Details", new { id = tripId });
            }

            participantToUpdate.RoleId = roleId;
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Роль учасника успішно оновлено!";
            return RedirectToAction("Details", new { id = tripId });
        }

        private IEnumerable<SelectListItem> GetUserList()
        {
            return _userManager.Users.ToList().Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id
            });
        }

        private string currentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}