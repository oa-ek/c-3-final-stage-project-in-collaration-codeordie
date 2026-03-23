using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TransitsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransitsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var transits = new List<Transit>();

            var viewModels = transits.Select(t => new TransitListViewModel
            {
                Id = t.Id,
                TripTitle = t.Trip?.Title ?? "Невідомо",
                TransitTypeName = t.TransitType?.Name ?? GetTransitTypeName(t.TransitTypeId),
                DepartureLocation = t.DepartureLocation,
                ArrivalLocation = t.ArrivalLocation,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                BookingStatusName = t.BookingStatus?.Name ?? GetBookingStatusName(t.BookingStatusId)
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new TransitFormViewModel
            {
        
                TripList = GetTripList(),
                TransitTypeList = GetTransitTypeList(),
                BookingStatusList = GetBookingStatusList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransitFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                model.TransitTypeList = GetTransitTypeList();
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }


            /*
            var entity = new Transit
            {
                TripId = model.TripId,
                TransitTypeId = model.TransitTypeId,
                BookingStatusId = model.BookingStatusId,
                DepartureLocation = model.DepartureLocation,
                ArrivalLocation = model.ArrivalLocation,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                CarrierInfo = model.CarrierInfo,
                BookingReference = model.BookingReference
            };

            _unitOfWork.Transit.Add(entity);
            await _unitOfWork.SaveAsync();
            */

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
      
            return Content("Редагування тимчасово недоступне. Чекаємо оновлення бази від Дарини.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransitFormViewModel model)
        {
      
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
       
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

        private IEnumerable<SelectListItem> GetTransitTypeList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Літак (Flight)", Value = "1" },
                new SelectListItem { Text = "Поїзд (Train)", Value = "2" },
                new SelectListItem { Text = "Автобус (Bus)", Value = "3" },
                new SelectListItem { Text = "Оренда авто (Car Rental)", Value = "4" },
                new SelectListItem { Text = "Таксі/Трансфер (Taxi)", Value = "5" }
            };
        }

        private IEnumerable<SelectListItem> GetBookingStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Не заброньовано (Not Booked)", Value = "1" },
                new SelectListItem { Text = "Очікує підтвердження (Pending)", Value = "2" },
                new SelectListItem { Text = "Підтверджено (Confirmed)", Value = "3" },
                new SelectListItem { Text = "Скасовано (Cancelled)", Value = "4" }
            };
        }

        private string GetTransitTypeName(int id) => id switch
        {
            1 => "Літак",
            2 => "Поїзд",
            3 => "Автобус",
            4 => "Оренда авто",
            5 => "Таксі/Трансфер",
            _ => "Інше"
        };

        private string GetBookingStatusName(int id) => id switch
        {
            1 => "Не заброньовано",
            2 => "Очікує",
            3 => "Підтверджено",
            4 => "Скасовано",
            _ => "Невідомо"
        };
    }
}