using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.UI.Models.ViewModels;

namespace TravelManager.UI.Controllers
{
    public class TripDestinationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TripDestinationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var destinations = _unitOfWork.TripDestination.GetAll(includeProperties: "Trip");

            var viewModels = destinations.Select(d => new TripDestinationListViewModel
            {
                Id = d.Id,
                TripName = d.Trip?.Title ?? string.Empty,
                CityName = d.CityName,
                Country = d.Country,
                ArrivalDate = d.ArrivalDate,
                DepartureDate = d.DepartureDate
            }).OrderBy(d => d.ArrivalDate).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new TripDestinationFormViewModel
            {
                TripList = GetTripList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripDestinationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                return View(model);
            }

            var entity = new TripDestination
            {
                TripId = model.TripId,
                CityName = model.CityName,
                Country = model.Country,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ArrivalDate = model.ArrivalDate,
                DepartureDate = model.DepartureDate
            };

            _unitOfWork.TripDestination.Add(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            var model = new TripDestinationFormViewModel
            {
                Id = entity.Id,
                TripId = entity.TripId,
                CityName = entity.CityName,
                Country = entity.Country,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ArrivalDate = entity.ArrivalDate,
                DepartureDate = entity.DepartureDate,
                TripList = GetTripList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripDestinationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TripList = GetTripList();
                return View(model);
            }

            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.TripId = model.TripId;
            entity.CityName = model.CityName;
            entity.Country = model.Country;
            entity.Latitude = model.Latitude;
            entity.Longitude = model.Longitude;
            entity.ArrivalDate = model.ArrivalDate;
            entity.DepartureDate = model.DepartureDate;

            _unitOfWork.TripDestination.Update(entity);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = _unitOfWork.TripDestination.Get(u => u.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            _unitOfWork.TripDestination.Remove(entity);
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
    }
}