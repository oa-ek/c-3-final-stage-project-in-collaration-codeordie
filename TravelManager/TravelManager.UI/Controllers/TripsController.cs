using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public TripsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // 1. Отримуємо ID поточного авторизованого користувача
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Шукаємо в таблиці TripParticipant всі записи, де UserId співпадає з поточним
            var userTrips = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId, includeProperties: "Trip")
                .Select(tp => tp.Trip) // Витягуємо самі об'єкти Trip
                .Distinct() // Прибираємо дублікати
                .Select(t => new TravelManager.UI.Models.ViewModels.TripListViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    // Description = t.Description, <-- Рядок видалено, щоб уникнути помилки!
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToList();

            // 3. Передаємо у View ТІЛЬКИ поїздки цього користувача
            return View(userTrips);
        }


        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateTripViewModel
            {
                UserList = GetUserList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            ModelState.Remove("CreatorId");
            ModelState.Remove("Participants");
            ModelState.Remove("UserList");

            if (!ModelState.IsValid)
            {
                model.UserList = GetUserList();
                return View(model);
            }

            var newTrip = new Trip
            {
                Title = model.Title,
                Description = model.Description,
                DepartureLocation = model.DepartureLocation,
                ReturnLocation = model.ReturnLocation,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                BaseCurrency = model.BaseCurrency,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatorId = _userManager.GetUserId(User) 
            };

            _unitOfWork.Trip.Add(newTrip);
            await _unitOfWork.SaveAsync();

            var currentUserId = _userManager.GetUserId(User);

            var ownerRole = _unitOfWork.TripRole.Get(r => r.Name == "Owner")
                            ?? _unitOfWork.TripRole.GetAll().FirstOrDefault();

            if (currentUserId != null && ownerRole != null)
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
        public IActionResult Edit(int id)
        {
            ModelState.Remove("CreatorId");
            ModelState.Remove("Participants");
            ModelState.Remove("UserList");
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            var model = new CreateTripViewModel
            {
                Title = trip.Title,
                Description = trip.Description,
                DepartureLocation = trip.DepartureLocation,
                ReturnLocation = trip.ReturnLocation,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                BaseCurrency = trip.BaseCurrency,
                CreatorId = trip.CreatorId,
                UserList = GetUserList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateTripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.UserList = GetUserList();
                return View(model);
            }

            var tripFromDb = _unitOfWork.Trip.Get(u => u.Id == id);
            if (tripFromDb == null)
            {
                return NotFound();
            }

            tripFromDb.Title = model.Title;
            tripFromDb.Description = model.Description;
            tripFromDb.DepartureLocation = model.DepartureLocation;
            tripFromDb.ReturnLocation = model.ReturnLocation;
            tripFromDb.StartDate = model.StartDate;
            tripFromDb.EndDate = model.EndDate;
            tripFromDb.BaseCurrency = model.BaseCurrency;
            tripFromDb.CreatorId = model.CreatorId;

            _unitOfWork.Trip.Update(tripFromDb);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            _unitOfWork.Trip.Remove(trip);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetUserList()
        {
            return _userManager.Users.ToList().Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id
            });
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var trip = _unitOfWork.Trip.Get(u => u.Id == id);
            if (trip == null) return NotFound();

            var participants = _unitOfWork.TripParticipant.GetAll(tp => tp.TripId == id, includeProperties: "User,Role");
            var currentUserId = _userManager.GetUserId(User);

            var currentUserParticipant = participants.FirstOrDefault(p => p.UserId == currentUserId);
            var currentUserRole = currentUserParticipant?.Role?.Name ?? "None";

            var allRoles = _unitOfWork.TripRole.GetAll().Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            });

            ViewBag.AvailableRoles = allRoles;
            ViewBag.CurrentUserRole = currentUserRole;

            var model = new CreateTripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Participants = participants.Select(p => new TripParticipantViewModel
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Role = p.Role?.Name ?? "Member"
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteParticipant(int tripId, string email, int roleId)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email не може бути порожнім.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var userToInvite = await _userManager.FindByEmailAsync(email);
            if (userToInvite == null)
            {
                TempData["ErrorMessage"] = "Користувача з таким Email не знайдено в системі.";
                return RedirectToAction("Details", new { id = tripId });
            }

            // Перевірка, чи він вже не в поїздці
            var existingParticipant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == userToInvite.Id);

            if (existingParticipant != null)
            {
                TempData["ErrorMessage"] = "Цей користувач вже є учасником поїздки.";
                return RedirectToAction("Details", new { id = tripId });
            }

            // Знаходимо роль "Member"
            var role = _unitOfWork.TripRole.Get(r => r.Name == "Member")
                       ?? _unitOfWork.TripRole.GetAll().FirstOrDefault();

            var newParticipant = new TripParticipant
            {
                TripId = tripId,
                UserId = userToInvite.Id,
                RoleId = roleId 
            };

            _unitOfWork.TripParticipant.Add(newParticipant);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Користувача {userToInvite.UserName} успішно додано до поїздки!";
            return RedirectToAction("Details", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant(int tripId, string userId)
        {
            var participant = _unitOfWork.TripParticipant.Get(p => p.TripId == tripId && p.UserId == userId, includeProperties: "Role");

            if (participant == null) return NotFound();

            if (participant.Role?.Name == "Owner")
            {
                TempData["ErrorMessage"] = "Неможливо видалити власника поїздки.";
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
            var currentUserId = _userManager.GetUserId(User);
            var currentUserParticipant = _unitOfWork.TripParticipant.Get(p => p.TripId == tripId && p.UserId == currentUserId, includeProperties: "Role");

            // Перевірка прав (тільки Owner або Editor може змінювати ролі)
            if (currentUserParticipant?.Role?.Name != "Owner" && currentUserParticipant?.Role?.Name != "Editor")
            {
                TempData["ErrorMessage"] = "У вас немає прав для зміни ролей.";
                return RedirectToAction("Details", new { id = tripId });
            }

            var participantToUpdate = _unitOfWork.TripParticipant.Get(p => p.TripId == tripId && p.UserId == userId, includeProperties: "Role");
            if (participantToUpdate == null) return NotFound();

            // Заборона змінювати роль власника (Owner), щоб випадково не залишити поїздку без керівника
            if (participantToUpdate.Role?.Name == "Owner" && roleId != participantToUpdate.RoleId)
            {
                TempData["ErrorMessage"] = "Неможливо змінити роль Власника напряму.";
                return RedirectToAction("Details", new { id = tripId });
            }

            participantToUpdate.RoleId = roleId;
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Роль учасника успішно оновлено!";
            return RedirectToAction("Details", new { id = tripId });
        }

    }
}