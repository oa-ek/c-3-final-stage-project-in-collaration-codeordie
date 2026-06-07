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

            var timeline = new List<TimelineEvent>();

            if (model.Destinations != null)
            {
                timeline.AddRange(model.Destinations.Select(d => new TimelineEvent
                {
                    EventDate = d.ArrivalDate,
                    EventType = "Destination",
                    Title = $"Прибуття в {d.CityName}",
                    Subtitle = d.Country,
                    IconClass = "bi-geo-alt-fill",
                    IconColor = "#3b82f6"
                }));
            }

            if (model.Transits != null)
            {
                timeline.AddRange(model.Transits.Select(t => new TimelineEvent
                {
                    EventDate = t.DepartureTime,
                    EventType = "Transit",
                    Title = $"Транспорт: {t.DepartureLocation} → {t.ArrivalLocation}",
                    Subtitle = $"{t.TransitType?.Name ?? "Рейс"} (Квиток: {t.BookingReference ?? "-"})",
                    IconClass = "bi-airplane-fill",
                    IconColor = "#8b5cf6"
                }));
            }

            if (model.Accommodations != null)
            {
                timeline.AddRange(model.Accommodations.Select(a => new TimelineEvent
                {
                    EventDate = a.CheckInTime,
                    EventType = "Accommodation",
                    Title = $"Заселення: {a.Name}",
                    Subtitle = a.Address,
                    IconClass = "bi-building",
                    IconColor = "#14b8a6"
                }));
            }

            model.Timeline = timeline.OrderBy(e => e.EventDate).ToList();

            double totalDistance = 0;
            var destsWithCoords = model.Destinations
                .Where(d => d.Latitude.HasValue && d.Longitude.HasValue)
                .OrderBy(d => d.ArrivalDate)
                .ToList();

            for (int i = 0; i < destsWithCoords.Count - 1; i++)
            {
                totalDistance += CalculateHaversineDistance(
                    destsWithCoords[i].Latitude.Value, destsWithCoords[i].Longitude.Value,
                    destsWithCoords[i + 1].Latitude.Value, destsWithCoords[i + 1].Longitude.Value);
            }

            ViewBag.TotalDistance = Math.Round(totalDistance);
            var photoAlbums = _unitOfWork.TripDocument
    .GetAll(d => d.TripId == id && d.FileName.StartsWith("ALBUM|"))
    .ToList();
            ViewBag.PhotoAlbums = photoAlbums;

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

        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        [HttpGet]
        public IActionResult ExportToCsv(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);

            if (trip == null) return NotFound();

            var participant = _unitOfWork.TripParticipant.Get(p => p.TripId == id && p.UserId == currentUserId);
            if (participant == null) return Unauthorized();

            var dests = _unitOfWork.TripDestination.GetAll(d => d.TripId == id).ToList();
            var transits = _unitOfWork.Transit.GetAll(t => t.TripId == id, includeProperties: "TransitType").ToList();
            var accs = _unitOfWork.Accommodation.GetAll(a => a.TripId == id).ToList();

            var timeline = new List<TimelineEvent>();
            timeline.AddRange(dests.Select(d => new TimelineEvent { EventDate = d.ArrivalDate, EventType = "Місто", Title = d.CityName, Subtitle = d.Country }));
            timeline.AddRange(transits.Select(t => new TimelineEvent { EventDate = t.DepartureTime, EventType = "Транспорт", Title = $"{t.DepartureLocation} → {t.ArrivalLocation}", Subtitle = t.TransitType?.Name }));
            timeline.AddRange(accs.Select(a => new TimelineEvent { EventDate = a.CheckInTime, EventType = "Житло", Title = a.Name, Subtitle = a.Address }));

            timeline = timeline.OrderBy(e => e.EventDate).ToList();

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Дата;Тип події;Назва;Деталі");

            foreach (var item in timeline)
            {
                var date = item.EventDate.ToString("dd.MM.yyyy HH:mm");
                var type = item.EventType;
                var title = item.Title?.Replace(";", ",");
                var sub = item.Subtitle?.Replace(";", ",");

                sb.AppendLine($"{date};{type};{title};{sub}");
            }

            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            byte[] finalBytes = bom.Concat(csvBytes).ToArray();

            return File(finalBytes, "text/csv", $"Trip_{trip.Title.Replace(" ", "_")}_Plan.csv");
        }
        [HttpGet]
        public IActionResult ExportToWord(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);

            if (trip == null) return NotFound();

            var participant = _unitOfWork.TripParticipant.Get(p => p.TripId == id && p.UserId == currentUserId);
            if (participant == null) return Unauthorized();

            var dests = _unitOfWork.TripDestination.GetAll(d => d.TripId == id).ToList();
            var transits = _unitOfWork.Transit.GetAll(t => t.TripId == id, includeProperties: "TransitType").ToList();
            var accs = _unitOfWork.Accommodation.GetAll(a => a.TripId == id).ToList();
            var expenses = _unitOfWork.Expense.GetAll(e => e.TripId == id).ToList();

            var timeline = new List<TimelineEvent>();
            timeline.AddRange(dests.Select(d => new TimelineEvent { EventDate = d.ArrivalDate, EventType = "Місто", Title = d.CityName, Subtitle = d.Country }));
            timeline.AddRange(transits.Select(t => new TimelineEvent { EventDate = t.DepartureTime, EventType = "Транспорт", Title = $"{t.DepartureLocation} → {t.ArrivalLocation}", Subtitle = t.TransitType?.Name }));
            timeline.AddRange(accs.Select(a => new TimelineEvent { EventDate = a.CheckInTime, EventType = "Житло", Title = a.Name, Subtitle = a.Address }));

            timeline = timeline.OrderBy(e => e.EventDate).ToList();

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head><meta charset='utf-8'><style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; color: #111827; }");
            sb.AppendLine("h1 { color: #4f46e5; text-align: center; border-bottom: 2px solid #e5e7eb; padding-bottom: 10px; }");
            sb.AppendLine("h2 { color: #10b981; margin-top: 30px; border-bottom: 1px solid #e5e7eb; padding-bottom: 5px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("th, td { border: 1px solid #d1d5db; padding: 10px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f4f6; color: #374151; font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<h1>План подорожі: {trip.Title}</h1>");
            sb.AppendLine($"<p><strong>Маршрут:</strong> {trip.DepartureLocation} &rarr; {trip.ReturnLocation}</p>");
            sb.AppendLine($"<p><strong>Дати:</strong> {trip.StartDate:dd.MM.yyyy} &mdash; {trip.EndDate:dd.MM.yyyy}</p>");

            if (!string.IsNullOrEmpty(trip.Description))
            {
                sb.AppendLine($"<p><strong>Опис:</strong> {trip.Description.Replace("\n", "<br>")}</p>");
            }

            sb.AppendLine("<h2>Загальна хронологія подій</h2>");
            sb.AppendLine("<table><tr><th>Дата і час</th><th>Тип</th><th>Подія</th><th>Деталі</th></tr>");
            foreach (var item in timeline)
            {
                sb.AppendLine($"<tr><td>{item.EventDate:dd.MM.yyyy HH:mm}</td><td>{item.EventType}</td><td><strong>{item.Title}</strong></td><td>{item.Subtitle}</td></tr>");
            }
            sb.AppendLine("</table>");

            if (accs.Any())
            {
                sb.AppendLine("<h2>Заброньоване житло</h2>");
                sb.AppendLine("<table><tr><th>Назва</th><th>Адреса</th><th>Заїзд</th><th>Виїзд</th></tr>");
                foreach (var a in accs)
                {
                    sb.AppendLine($"<tr><td><strong>{a.Name}</strong></td><td>{a.Address}</td><td>{a.CheckInTime:dd.MM.yyyy HH:mm}</td><td>{a.CheckOutTime:dd.MM.yyyy HH:mm}</td></tr>");
                }
                sb.AppendLine("</table>");
            }

            if (transits.Any())
            {
                sb.AppendLine("<h2>Транспортні квитки</h2>");
                sb.AppendLine("<table><tr><th>Маршрут</th><th>Тип</th><th>Відправлення</th><th>Прибуття</th><th>Квиток (Номер)</th></tr>");
                foreach (var t in transits)
                {
                    sb.AppendLine($"<tr><td><strong>{t.DepartureLocation} &rarr; {t.ArrivalLocation}</strong></td><td>{t.TransitType?.Name}</td><td>{t.DepartureTime:dd.MM.yyyy HH:mm}</td><td>{t.ArrivalTime:dd.MM.yyyy HH:mm}</td><td>{t.BookingReference}</td></tr>");
                }
                sb.AppendLine("</table>");
            }

            if (expenses.Any())
            {
                sb.AppendLine("<h2>Зафіксовані витрати</h2>");
                sb.AppendLine("<table><tr><th>Назва</th><th>Сума</th><th>Валюта</th><th>Дата</th></tr>");
                foreach (var e in expenses)
                {
                   // sb.AppendLine($"<tr><td>{e.Description}</td><td>{e.TotalAmount:N2}</td><td>{e.Currency}</td><td>{e.Date:dd.MM.yyyy}</td></tr>");
                }
                sb.AppendLine("</table>");
            }

            sb.AppendLine("<br><hr><p style='text-align:center; color:#9ca3af; font-size: 12px;'>Згенеровано автоматично через TravelManager</p>");
            sb.AppendLine("</body></html>");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "application/vnd.ms-word", $"TravelManager_План_{trip.Title.Replace(" ", "_")}.doc");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhotoAlbum(int tripId, string albumName, string albumUrl)
        {
            var role = GetUserRoleInTrip(tripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для додавання фотоальбому.";
                return RedirectToAction("Details", new { id = tripId });
            }

            if (!string.IsNullOrWhiteSpace(albumUrl))
            {
                // Якщо користувач не ввів назву, даємо стандартну
                var safeName = string.IsNullOrWhiteSpace(albumName) ? "Спільний альбом" : albumName.Trim();

                var newAlbum = new TripDocument
                {
                    TripId = tripId,
                    // Додаємо префікс "ALBUM|", щоб відрізняти їх від інших документів (напр. квитків)
                    FileName = "ALBUM|" + safeName,
                    FilePath = albumUrl,
                    UploadedAt = DateTime.UtcNow
                };
                _unitOfWork.TripDocument.Add(newAlbum);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Альбом успішно додано!";
            }

            return RedirectToAction("Details", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhotoAlbum(int tripId, int documentId)
        {
            var role = GetUserRoleInTrip(tripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "У вас немає прав для видалення фотоальбому.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var album = _unitOfWork.TripDocument.Get(d => d.Id == documentId && d.TripId == tripId);
            if (album != null)
            {
                _unitOfWork.TripDocument.Remove(album);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Альбом видалено.";
            }

            return RedirectToAction("Details", new { id = tripId });
        }
    }
}