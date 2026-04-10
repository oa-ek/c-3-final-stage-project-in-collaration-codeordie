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

        [HttpGet]
        public IActionResult Index()
        {
            var expenses = _unitOfWork.Expense.GetAll(includeProperties: "Trip,Category");

            var viewModels = expenses.Select(e => new ExpenseListViewModel
            {
                Id = e.Id,
                Description = e.Title,
                Amount = e.TotalAmount,
                Currency = e.Currency,
                Date = e.Date,
                CategoryName = e.Category?.Name ?? "Невідомо",
                TripTitle = e.Trip?.Title ?? "Невідомо"
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? tripId) 
        {
            var allowedTrips = GetAllowedTripsForUser();

            if (!allowedTrips.Any())
            {
                TempData["ErrorMessage"] = "У вас немає поїздок, де ви можете додавати записи.";
                return RedirectToAction("Index", "Trips");
            }

            if (tripId.HasValue)
            {
                var role = GetUserRoleInTrip(tripId.Value);
                if (role == "Viewer" || role == "None")
                {
                    TempData["ErrorMessage"] = "Глядачі не можуть додавати записи в цю поїздку.";
                    return RedirectToAction("Index", "Trips"); 
                }
                var selectedTrip = allowedTrips.FirstOrDefault(t => t.Value == tripId.Value.ToString());
                if (selectedTrip != null) selectedTrip.Selected = true;
            }

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == tripId, includeProperties: "User")
                .ToList();

            var model = new ExpenseFormViewModel
            {
                TripId = tripId ?? 0,
                TripList = allowedTrips,
                Date = DateTime.Today,
                CategoryList = GetCategoryList(),
                CurrencyList = GetCurrencyList(),
                TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == tripId)
                    .Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() }),
                AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == tripId)
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() }),
                ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == tripId)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseFormViewModel model)
        {
            var role = GetUserRoleInTrip(model.TripId);
            if (role == "Viewer" || role == "None")
            {
                TempData["ErrorMessage"] = "Відмовлено в доступі. Ви не можете додавати записи в цю поїздку.";
                return RedirectToAction("Index", "Trips");
            }

            if (!ModelState.IsValid)
            {
                model.TripList = GetAllowedTripsForUser(); 
                return View(model);
            }
            decimal totalSplits = model.Splits.Sum(s => s.OwedAmount);
            if (totalSplits != model.TotalAmount)
            {
                TempData["ErrorMessage"] = $"Помилка: Сума часток ({totalSplits}) не збігається із загальною сумою ({model.TotalAmount})!";

                var participants = _unitOfWork.TripParticipant.GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
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
            if (entity == null)
            {
                return NotFound();
            }
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

                TripList = GetTripList(),
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
            model.TripList = GetAllowedTripsForUser();

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
                model.TripList = GetAllowedTripsForUser();
                model.CategoryList = GetCategoryList();
                model.CurrencyList = GetCurrencyList();
                model.PayerList = participants.Select(p => new SelectListItem { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });

                model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId).Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
                model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
                model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

                return View(model);
            }

            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

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
            if (entity == null)
            {
                return NotFound();
            }
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

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });
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

        private string GetUserRoleInTrip(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId, includeProperties: "Role");

            return participant?.Role?.Name ?? "None";
        }

        private IEnumerable<SelectListItem> GetAllowedTripsForUser()
        {
            var currentUserId = _userManager.GetUserId(User);

            return _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId && tp.Role.Name != "Viewer", includeProperties: "Trip,Role")
                .Select(tp => new SelectListItem
                {
                    Text = tp.Trip.Title,
                    Value = tp.TripId.ToString()
                }).ToList();
        }

    }
}