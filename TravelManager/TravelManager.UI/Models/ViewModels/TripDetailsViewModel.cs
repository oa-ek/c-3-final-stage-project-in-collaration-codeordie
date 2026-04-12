using System.Collections.Generic;
using TravelManager.Domain.Entities;

namespace TravelManager.UI.Models.ViewModels
{
    public class TripDetailsViewModel
    {
        public Trip Trip { get; set; } = null!;
        public IEnumerable<TripDestination> Destinations { get; set; } = new List<TripDestination>();
        public IEnumerable<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public IEnumerable<Transit> Transits { get; set; } = new List<Transit>();
        public IEnumerable<Checklist> Checklists { get; set; } = new List<Checklist>();
        public IEnumerable<Expense> Expenses { get; set; } = new List<Expense>();

        // Дані з proj2 — учасники та роль поточного юзера
        public IEnumerable<TripParticipantViewModel> Participants { get; set; } = new List<TripParticipantViewModel>();
        public string CurrentUserRole { get; set; } = "None";
    }
}