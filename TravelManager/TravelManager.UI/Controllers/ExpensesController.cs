using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public ExpensesController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // --- ДОПОМІЖНІ МЕТОДИ ДЛЯ РОЛЕЙ ---
        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }

        // ВИПРАВЛЕНО: Додано activeTripId, щоб коректно зберігати вибір у випадаючому списку
        private IEnumerable<SelectListItem> GetAllowedTripsForUser(int activeTripId = 0)
        {
            var currentUserId = _userManager.GetUserId(User);
            return _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId && tp.Role.Name != "Viewer", includeProperties: "Trip")
                .Select(tp => tp.Trip)
                .Distinct() // Уникаємо дублікатів поїздок
                .Select(t => new SelectListItem
                {
                    Text = t.Title,
                    Value = t.Id.ToString(),
                    Selected = t.Id == activeTripId // Завжди позначаємо поточну поїздку
                }).ToList();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return RedirectToAction("Login", "Account");

            var myParticipants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId, includeProperties: "Role").ToList();
            var myTripIds = myParticipants.Select(tp => tp.TripId).ToList();

            var expenses = _unitOfWork.Expense
                .GetAll(e => myTripIds.Contains(e.TripId), includeProperties: "Trip,Category");

            var viewModels = expenses.Select(e =>
            {
                return new ExpenseListViewModel
                {
                    Id = e.Id,
                    Description = e.Title,
                    Amount = e.TotalAmount,
                    Currency = e.Currency,
                    Date = e.Date,
                    CategoryName = e.Category?.Name ?? "Невідомо",
                    TripTitle = e.Trip?.Title ?? "Невідомо"
                };
            }).ToList();

            ViewBag.CurrentUserRole = myParticipants.Any(p => p.Role?.Name == "Organizer" || p.Role?.Name == "Participant") ? "Participant" : "Viewer";

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? tripId)
        {
            var allowedTrips = GetAllowedTripsForUser();

            if (!allowedTrips.Any())
            {
                TempData["ErrorMessage"] = "У вас немає поїздок, де ви можете додавати витрати.";
                return RedirectToAction("Index", "Trips");
            }

            // Визначаємо активну поїздку
            int activeTripId = (tripId.HasValue && tripId.Value > 0) ? tripId.Value : int.Parse(allowedTrips.First().Value);

            var role = GetUserRoleInTrip(activeTripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть додавати витрати в цю поїздку.";
                return RedirectToAction("Index", "Trips");
            }

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == activeTripId, includeProperties: "User")
                .ToList();

            var model = new ExpenseFormViewModel
            {
                TripId = activeTripId,
                TripList = GetAllowedTripsForUser(activeTripId), // Передаємо ID, щоб він виділився у формі
                Date = DateTime.Today,
                CategoryList = GetCategoryList(),
                CurrencyList = GetCurrencyList(),
                TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == activeTripId)
                    .Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() }),
                AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == activeTripId)
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() }),
                ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == activeTripId)
                    .Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() }),

                PayerList = participants.Select(p => new SelectListItem
                {
                    Text = p.User.UserName ?? p.User.Email,
                    Value = p.UserId
                }),
                Splits = participants.Select(p => new ExpenseSplitItemModel
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? p.User.Email,
                    OwedAmount = 0
                }).ToList()
            };

            return View(model);
        }

        // НОВИЙ МЕТОД: Змінює поїздку, АЛЕ ЗБЕРІГАЄ ВВЕДЕНІ ДАНІ у формі
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReloadForm(ExpenseFormViewModel model)
        {
            ModelState.Clear(); // Очищаємо помилки, бо ми просто оновлюємо списки під нову поїздку

            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Відмовлено в доступі. Глядачі не можуть створювати витрати.";
                return RedirectToAction("Index", "Trips");
            }

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == model.TripId, includeProperties: "User")
                .ToList();

            model.TripList = GetAllowedTripsForUser(model.TripId);
            model.CategoryList = GetCategoryList();
            model.CurrencyList = GetCurrencyList();

            model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId)
                .Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
            model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId)
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
            model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId)
                .Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

            model.PayerList = participants.Select(p => new SelectListItem
            {
                Text = p.User.UserName ?? p.User.Email,
                Value = p.UserId
            });

            // Оновлюємо список боргів під нових учасників (скидаємо суми в 0)
            model.Splits = participants.Select(p => new ExpenseSplitItemModel
            {
                UserId = p.UserId,
                UserName = p.User.UserName ?? p.User.Email,
                OwedAmount = 0
            }).ToList();

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseFormViewModel model)
        {
            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Відмовлено в доступі. Ви не можете додавати витрати в цю поїздку.";
                return RedirectToAction("Index", "Trips");
            }

            if (!ModelState.IsValid)
            {
                var participants = _unitOfWork.TripParticipant.GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
                model.TripList = GetAllowedTripsForUser(model.TripId);
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                model.PayerList = participants.Select(p => new SelectListItem { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });

                model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId).Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
                model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
                model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

                return View(model);
            }

            if (model.Splits == null) model.Splits = new List<ExpenseSplitItemModel>();

            decimal totalSplits = model.Splits.Sum(s => s.OwedAmount);
            if (totalSplits != model.TotalAmount)
            {
                TempData["ErrorMessage"] = $"Помилка: Сума часток ({totalSplits}) не збігається із загальною сумою ({model.TotalAmount})!";

                var participants = _unitOfWork.TripParticipant.GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
                model.TripList = GetAllowedTripsForUser(model.TripId);
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                model.PayerList = participants.Select(p => new SelectListItem { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });

                model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId).Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
                model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
                model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

                return View(model);
            }

            var expense = new Expense
            {
                TripId = model.TripId,
                Title = model.Description,
                TotalAmount = model.TotalAmount,
                Currency = model.Currency,
                Date = model.Date,
                CategoryId = model.CategoryId,
                PayerId = model.PayerId,
                ReceiptImageUrl = model.ReceiptImageUrl,

                TransitId = model.TransitId,
                AccommodationId = model.AccommodationId,
                TripActivityId = model.TripActivityId
            };

            _unitOfWork.Expense.Add(expense);
            await _unitOfWork.SaveAsync();

            foreach (var split in model.Splits)
            {
                if (split.OwedAmount > 0)
                {
                    var expenseSplit = new ExpenseSplit
                    {
                        ExpenseId = expense.Id,
                        DebtorId = split.UserId,
                        OwedAmount = split.OwedAmount,
                        IsSettled = (split.UserId == model.PayerId)
                    };
                    _unitOfWork.ExpenseSplit.Add(expenseSplit);
                }
            }

            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = "Витрату успішно додано!";
            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index", "Trips");
            }

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == entity.TripId, includeProperties: "User")
                .ToList();

            var model = new ExpenseFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                CategoryId = entity.CategoryId,
                TotalAmount = entity.TotalAmount,
                Currency = entity.Currency,
                Description = entity.Title,
                Date = entity.Date,
                PayerId = entity.PayerId,
                ReceiptImageUrl = entity.ReceiptImageUrl,

                TransitId = entity.TransitId,
                AccommodationId = entity.AccommodationId,
                TripActivityId = entity.TripActivityId,

                TripList = GetAllowedTripsForUser(entity.TripId), // Передаємо ID
                CategoryList = GetCategoryList(),
                CurrencyList = GetCurrencyList(),

                TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == entity.TripId)
                    .Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() }),
                AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == entity.TripId)
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() }),
                ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == entity.TripId)
                    .Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() }),

                PayerList = participants.Select(p => new SelectListItem
                {
                    Text = p.User.UserName ?? p.User.Email,
                    Value = p.UserId
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
        {
            ModelState.Remove("Splits");

            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть редагувати записи.";
                return RedirectToAction("Index", "Trips");
            }

            if (!ModelState.IsValid)
            {
                var participants = _unitOfWork.TripParticipant.GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
                model.TripList = GetAllowedTripsForUser(model.TripId);
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                model.PayerList = participants.Select(p => new SelectListItem { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });

                model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId).Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
                model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
                model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

                return View(model);
            }

            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.TripId = model.TripId;
            entity.CategoryId = model.CategoryId;
            entity.TotalAmount = model.TotalAmount;
            entity.Currency = model.Currency;
            entity.Title = model.Description;
            entity.Date = model.Date;
            entity.PayerId = model.PayerId;
            entity.ReceiptImageUrl = model.ReceiptImageUrl;
            entity.TransitId = model.TransitId;
            entity.AccommodationId = model.AccommodationId;
            entity.TripActivityId = model.TripActivityId;

            _unitOfWork.Expense.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var role = GetUserRoleInTrip(entity.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Глядачі не можуть видаляти записи.";
                return RedirectToAction("Index", "Trips");
            }

            _unitOfWork.Expense.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
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
                new SelectListItem { Text = "EUR (Євро)", Value = "EUR" },
                new SelectListItem { Text = "PLN (Злотий)", Value = "PLN" },
                new SelectListItem { Text = "GBP (Фунт)", Value = "GBP" }
            };
        }
    }
}