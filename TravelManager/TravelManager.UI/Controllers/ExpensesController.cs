using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IExchangeRateService _exchangeRateService;

        public ExpensesController(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IExchangeRateService exchangeRateService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myTripIds = _unitOfWork.TripParticipant
                .GetAll(tp => tp.UserId == currentUserId)
                .Select(tp => tp.TripId)
                .ToList();


            var expenses = _unitOfWork.Expense.GetAll(a => myTripIds.Contains(a.TripId), includeProperties: "Trip, Category");

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
        public IActionResult GetTripParticipants(int tripId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var participant = _unitOfWork.TripParticipant
                .Get(tp => tp.TripId == tripId && tp.UserId == currentUserId);

            if (participant == null)
                return Forbid();

            var participants = _unitOfWork.TripParticipant
                .GetAll(tp => tp.TripId == tripId, includeProperties: "User")
                .Select(p => new
                {
                    userId = p.UserId,
                    userName = p.User.UserName ?? p.User.Email ?? p.UserId
                })
                .ToList();

            return Json(participants);
        }


        [HttpGet]
        public async Task<IActionResult> Create(int? tripId)
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
                CurrencyList = await GetCurrencyListAsync(), // ← реальні курси
                TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == tripId)
                    .Select(t => new SelectListItem
                    {
                        Text = $"{t.DepartureLocation} - {t.ArrivalLocation}",
                        Value = t.Id.ToString()
                    }),
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
            for (int i = 0; i < model.Splits.Count; i++)
            {
                ModelState.Remove($"Splits[{i}].OwedAmount");

                var raw = Request.Form[$"Splits[{i}].OwedAmount"].ToString();
                if (!string.IsNullOrEmpty(raw))
                {
                    raw = raw.Replace(",", ".");
                    if (decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal parsed))
                    {
                        model.Splits[i].OwedAmount = parsed;
                    }
                }
            }
            ModelState.Remove("Splits");
            ModelState.Remove("Splits[0].UserName");
            ModelState.Remove("Splits[1].UserName");
            ModelState.Remove("Splits[2].UserName");
            ModelState.Remove("Splits[3].UserName");
            ModelState.Remove("PayerList");
            ModelState.Remove("TripList");
            ModelState.Remove("CategoryList");
            ModelState.Remove("CurrencyList");
            ModelState.Remove("TransitList");
            ModelState.Remove("AccommodationList");
            ModelState.Remove("ActivityList");

            if (!ModelState.IsValid)
            {

                model.TripList = GetAllowedTripsForUser();
                model.CategoryList = GetCategoryList();
                model.CurrencyList = await GetCurrencyListAsync();
                var pts = _unitOfWork.TripParticipant
                    .GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
                model.PayerList = pts.Select(p => new SelectListItem
                { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });
                model.Splits = pts.Select(p => new ExpenseSplitItemModel
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? p.User.Email,
                    OwedAmount = 0
                }).ToList();
                return View(model);
            }
            var trip = _unitOfWork.Trip.Get(t => t.Id == model.TripId);
            if (trip == null) return NotFound();

            string tripBaseCurrency = trip.BaseCurrency; 

            decimal expenseRate = 1.0m;
            decimal tripBaseRate = 1.0m;

            if (model.Currency != "UAH")
            {
                var rateInfo = await _exchangeRateService.GetRateAsync(model.Currency);
                if (rateInfo != null) expenseRate = (decimal)rateInfo.RateToUah;
            }

            if (tripBaseCurrency != "UAH")
            {
                var rateInfo = await _exchangeRateService.GetRateAsync(tripBaseCurrency);
                if (rateInfo != null) tripBaseRate = (decimal)rateInfo.RateToUah;
            }

            decimal convertedAmount = (model.TotalAmount * expenseRate) / tripBaseRate;

            decimal totalSplits = model.Splits?.Sum(s => s.OwedAmount) ?? 0;
            if (totalSplits == 0)
            {
                if (model.Splits == null || !model.Splits.Any())
                {
                    model.Splits = new List<ExpenseSplitItemModel>
        {
            new ExpenseSplitItemModel
            {
                UserId = model.PayerId,
                UserName = model.PayerId,
                OwedAmount = model.TotalAmount
            }
        };
                }
                else
                {
                    foreach (var s in model.Splits)
                        s.OwedAmount = s.UserId == model.PayerId ? model.TotalAmount : 0;
                }
            }
            else if (Math.Abs(totalSplits - model.TotalAmount) >= 0.01m)
            {
                TempData["ErrorMessage"] = $"Сума часток ({totalSplits}) ≠ загальній сумі ({model.TotalAmount})";
                model.TripList = GetAllowedTripsForUser();
                model.CategoryList = GetCategoryList();
                model.CurrencyList = await GetCurrencyListAsync();
                var pts = _unitOfWork.TripParticipant
                    .GetAll(tp => tp.TripId == model.TripId, includeProperties: "User").ToList();
                model.PayerList = pts.Select(p => new SelectListItem
                { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });
                return View(model);
            }
            decimal conversionFactor = expenseRate / tripBaseRate;
            var expense = new Expense
            {
                TripId = model.TripId,
                Title = model.Description,
                TotalAmount = convertedAmount,      
                Currency = tripBaseCurrency,
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
                        OwedAmount = Math.Round(split.OwedAmount * conversionFactor, 2),
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
        public async Task<IActionResult> Edit(int id)
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

            var currencyList = await GetCurrencyListAsync(); 

            foreach (var item in currencyList)
            {
                if (item.Value == entity.Currency)
                    item.Selected = true;
            }

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
                TripList = GetAllowedTripsForUser(),
                CategoryList = GetCategoryList(),
                CurrencyList = currencyList, 
                TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == entity.TripId)
                    .Select(t => new SelectListItem
                    {
                        Text = $"{t.DepartureLocation} - {t.ArrivalLocation}",
                        Value = t.Id.ToString()
                    }),
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
            var existingSplits = _unitOfWork.ExpenseSplit
       .GetAll(s => s.ExpenseId == id, includeProperties: "Debtor")
       .ToList();

            if (existingSplits.Any())
            {
                model.Splits = existingSplits.Select(s => new ExpenseSplitItemModel
                {
                    UserId = s.DebtorId,
                    UserName = s.Debtor?.UserName ?? s.DebtorId,
                    OwedAmount = s.OwedAmount
                }).ToList();
            }
            else
            {
                model.Splits = participants.Select(p => new ExpenseSplitItemModel
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? p.User.Email,
                    OwedAmount = 0
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
        {

            ModelState.Remove("Splits");

            for (int i = 0; i < model.Splits.Count; i++)
            {
                ModelState.Remove($"Splits[{i}].OwedAmount");
                ModelState.Remove($"Splits[{i}].UserName");

                var raw = Request.Form[$"Splits[{i}].OwedAmount"].ToString();
                if (!string.IsNullOrEmpty(raw))
                {
                    raw = raw.Replace(",", ".");
                    if (decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal parsed))
                    {
                        model.Splits[i].OwedAmount = parsed;
                    }
                }
            }
            ModelState.Remove("PayerList");
            ModelState.Remove("TripList");
            ModelState.Remove("CategoryList");
            ModelState.Remove("CurrencyList");
            ModelState.Remove("TransitList");
            ModelState.Remove("AccommodationList");
            ModelState.Remove("ActivityList");
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
                model.CurrencyList = await GetCurrencyListAsync();
                model.PayerList = participants.Select(p => new SelectListItem { Text = p.User.UserName ?? p.User.Email, Value = p.UserId });

                model.TransitList = _unitOfWork.Transit.GetAll(t => t.TripId == model.TripId).Select(t => new SelectListItem { Text = $"{t.DepartureLocation} - {t.ArrivalLocation}", Value = t.Id.ToString() });
                model.AccommodationList = _unitOfWork.Accommodation.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
                model.ActivityList = _unitOfWork.TripActivity.GetAll(a => a.TripId == model.TripId).Select(a => new SelectListItem { Text = a.Title, Value = a.Id.ToString() });

                return View(model);
            }

            var entity = _unitOfWork.Expense.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var trip = _unitOfWork.Trip.Get(t => t.Id == model.TripId);
            string tripBaseCurrency = trip?.BaseCurrency ?? "UAH";

            decimal expenseRate = 1.0m;
            decimal tripBaseRate = 1.0m;

            if (model.Currency != "UAH")
            {
                var rateInfo = await _exchangeRateService.GetRateAsync(model.Currency);
                if (rateInfo != null) expenseRate = (decimal)rateInfo.RateToUah;
            }
            if (tripBaseCurrency != "UAH")
            {
                var rateInfo = await _exchangeRateService.GetRateAsync(tripBaseCurrency);
                if (rateInfo != null) tripBaseRate = (decimal)rateInfo.RateToUah;
            }

            decimal conversionFactor = expenseRate / tripBaseRate;
            decimal convertedTotal = model.TotalAmount * conversionFactor;

            
            entity.TripId = model.TripId;
            entity.CategoryId = model.CategoryId;
            entity.TotalAmount = convertedTotal; 
            entity.Currency = tripBaseCurrency;
            entity.Title = model.Description;
            entity.Date = model.Date;
            entity.PayerId = model.PayerId;
            entity.ReceiptImageUrl = model.ReceiptImageUrl;

            entity.TransitId = model.TransitId;
            entity.AccommodationId = model.AccommodationId;
            entity.TripActivityId = model.TripActivityId;

            _unitOfWork.Expense.Update(entity);
            
            var oldSplits = _unitOfWork.ExpenseSplit.GetAll(s => s.ExpenseId == id).ToList();
            _unitOfWork.ExpenseSplit.RemoveRange(oldSplits);

            if (model.Splits != null)
            {
                foreach (var split in model.Splits)
                {
                    if (split.OwedAmount > 0)
                    {
                        _unitOfWork.ExpenseSplit.Add(new ExpenseSplit
                        {
                            ExpenseId = id,
                            DebtorId = split.UserId,
                            OwedAmount = Math.Round(split.OwedAmount * conversionFactor, 2), // Теж конвертуємо
                            IsSettled = (split.UserId == model.PayerId)
                        });
                    }
                }
            }
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

        private async Task<IEnumerable<SelectListItem>> GetCurrencyListAsync()
        {
            var baseCodes = new[] { "USD", "EUR", "PLN", "GBP" };

            var rates = await _exchangeRateService.GetRatesAsync(baseCodes);

            var items = new List<SelectListItem>
    {
        new SelectListItem { Text = "UAH — Гривня", Value = "UAH" }
    };

            foreach (var rate in rates)
            {
                items.Add(new SelectListItem
                {
                    Text = $"{rate.CurrencyCode} — 1 {rate.CurrencyCode} = {rate.RateToUah:N2} UAH",
                    Value = rate.CurrencyCode
                });
            }

            return items;
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