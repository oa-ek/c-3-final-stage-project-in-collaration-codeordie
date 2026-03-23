using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var transits = _unitOfWork.Transit.GetAll(includeProperties: "Trip,TransitType,BookingStatus");
            var viewModels = transits.Select(t => new TransitListViewModel
            {
                Id = t.Id,
                TripTitle = t.Trip?.Title ?? string.Empty,
                TransitTypeName = t.TransitType?.Name ?? string.Empty,
                DepartureLocation = t.DepartureLocation,
                ArrivalLocation = t.ArrivalLocation,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                BookingStatusName = t.BookingStatus?.Name ?? string.Empty
            }).ToList();
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create(int? tripId)
        {
            var model = new TransitFormViewModel
            {
                TripId = tripId ?? 0,
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

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            var model = new TransitFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                TransitTypeId = entity.TransitTypeId,
                BookingStatusId = entity.BookingStatusId,
                DepartureLocation = entity.DepartureLocation,
                ArrivalLocation = entity.ArrivalLocation,
                DepartureTime = entity.DepartureTime,
                ArrivalTime = entity.ArrivalTime,
                CarrierInfo = entity.CarrierInfo,
                BookingReference = entity.BookingReference,
                TripList = GetTripList(),
                TransitTypeList = GetTransitTypeList(),
                BookingStatusList = GetBookingStatusList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransitFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                model.TransitTypeList = GetTransitTypeList();
                model.BookingStatusList = GetBookingStatusList();
                return View(model);
            }

            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            entity.TripId = model.TripId;
            entity.TransitTypeId = model.TransitTypeId;
            entity.BookingStatusId = model.BookingStatusId;
            entity.DepartureLocation = model.DepartureLocation;
            entity.ArrivalLocation = model.ArrivalLocation;
            entity.DepartureTime = model.DepartureTime;
            entity.ArrivalTime = model.ArrivalTime;
            entity.CarrierInfo = model.CarrierInfo;
            entity.BookingReference = model.BookingReference;

            _unitOfWork.Transit.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = model.TripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Transit.Get(u => u.Id == id);
            if (entity == null) return NotFound();

            int tripId = entity.TripId;
            _unitOfWork.Transit.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        private IEnumerable<SelectListItem> GetTripList()
        {
            return _unitOfWork.Trip.GetAll().Select(t => new SelectListItem { Text = t.Title, Value = t.Id.ToString() });
        }

        private IEnumerable<SelectListItem> GetTransitTypeList()
        {
            return _unitOfWork.TransitType.GetAll().Select(t => new SelectListItem { Text = t.Name, Value = t.Id.ToString() });
        }

        private IEnumerable<SelectListItem> GetBookingStatusList()
        {
            return _unitOfWork.BookingStatus.GetAll().Select(b => new SelectListItem { Text = b.Name, Value = b.Id.ToString() });
        }
    }
}