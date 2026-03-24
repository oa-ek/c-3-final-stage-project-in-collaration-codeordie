using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class AccommodationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccommodationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var accommodations = _unitOfWork.Accommodation.GetAll(includeProperties: "Trip");

            var viewModels = accommodations.Select(a => new AccommodationListViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Address = a.Address,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var trips = _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });

            var model = new AccommodationFormViewModel
            {
                TripList = trips
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccommodationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
                {
                    Text = t.Title,
                    Value = t.Id.ToString()
                });
                return View(model);
            }

            var entity = new Accommodation
            {
                Name = model.Name,
                Address = model.Address,
                CheckInTime = model.CheckInTime,
                CheckOutTime = model.CheckOutTime,
                BookingReference = model.BookingReference,
                TripId = model.TripId,
                BookingStatusId = 1
            };

            _unitOfWork.Accommodation.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            var trips = _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });

            var model = new AccommodationFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                CheckInTime = entity.CheckInTime,
                CheckOutTime = entity.CheckOutTime,
                BookingReference = entity.BookingReference,
                TripId = entity.TripId,
                TripList = trips
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccommodationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = _unitOfWork.Trip.GetAll().Select(t => new SelectListItem
                {
                    Text = t.Title,
                    Value = t.Id.ToString()
                });
                return View(model);
            }

            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Address = model.Address;
            entity.CheckInTime = model.CheckInTime;
            entity.CheckOutTime = model.CheckOutTime;
            entity.BookingReference = model.BookingReference;
            entity.TripId = model.TripId;

            _unitOfWork.Accommodation.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.Accommodation.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            _unitOfWork.Accommodation.Remove(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}