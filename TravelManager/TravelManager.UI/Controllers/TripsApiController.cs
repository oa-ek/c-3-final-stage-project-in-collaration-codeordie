
using global::TravelManager.Domain.Entities;
using global::TravelManager.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace TravelManager.UI.Controllers
{        
    [ApiController]
    [Route("api/trips")]
    [Produces("application/json")]
    public class TripsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TripsApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TripApiDto>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var trips = _unitOfWork.Trip
                .GetAll(includeProperties: "Status")
                .Select(MapToDto)
                .ToList();

            return Ok(trips);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TripApiDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var trip = _unitOfWork.Trip.Get(t => t.Id == id, includeProperties: "Status");
            if (trip == null) return NotFound(new { message = $"Поїздку з ID={id} не знайдено." });

            return Ok(MapToDto(trip));
        }

        [HttpPost]
        [ProducesResponseType(typeof(TripApiDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] TripCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var firstUser = _unitOfWork.TripParticipant
                .GetAll()
                .Select(tp => tp.UserId)
                .FirstOrDefault();

            if (firstUser == null)
                return BadRequest(new { message = "В системі немає жодного користувача." });

            var trip = new Trip
            {
                Title = dto.Title,
                Description = dto.Description,
                DepartureLocation = dto.DepartureLocation,
                ReturnLocation = dto.ReturnLocation,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                BaseCurrency = dto.BaseCurrency,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatorId = firstUser
            };

            _unitOfWork.Trip.Add(trip);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetById), new { id = trip.Id }, MapToDto(trip));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TripApiDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] TripCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var trip = _unitOfWork.Trip.Get(t => t.Id == id);
            if (trip == null) return NotFound(new { message = $"Поїздку з ID={id} не знайдено." });

            trip.Title = dto.Title;
            trip.Description = dto.Description;
            trip.DepartureLocation = dto.DepartureLocation;
            trip.ReturnLocation = dto.ReturnLocation;
            trip.StartDate = dto.StartDate;
            trip.EndDate = dto.EndDate;
            trip.BaseCurrency = dto.BaseCurrency;

            _unitOfWork.Trip.Update(trip);
            await _unitOfWork.SaveAsync();

            return Ok(MapToDto(trip));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = _unitOfWork.Trip.Get(t => t.Id == id);
            if (trip == null) return NotFound(new { message = $"Поїздку з ID={id} не знайдено." });

            _unitOfWork.Trip.Remove(trip);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        private static TripApiDto MapToDto(Trip t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            DepartureLocation = t.DepartureLocation,
            ReturnLocation = t.ReturnLocation,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            BaseCurrency = t.BaseCurrency,
            Status = t.Status?.Name ?? "Unknown",
            CreatedAt = t.CreatedAt
        };
    }

    public class TripApiDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string DepartureLocation { get; set; } = string.Empty;

        public string? ReturnLocation { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BaseCurrency { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class TripCreateDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string DepartureLocation { get; set; } = string.Empty;

        public string? ReturnLocation { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string BaseCurrency { get; set; } = string.Empty;
    }

}
