using Microsoft.EntityFrameworkCore;
using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(this ModelBuilder builder)
        {
            builder.Entity<TripStatus>().HasData(
                new TripStatus { Id = 1, Name = "Planned" },
                new TripStatus { Id = 2, Name = "In Progress" },
                new TripStatus { Id = 3, Name = "Completed" },
                new TripStatus { Id = 4, Name = "Cancelled" }
            );

            builder.Entity<BookingStatus>().HasData(
                new BookingStatus { Id = 1, Name = "Not Booked" },
                new BookingStatus { Id = 2, Name = "Pending" },
                new BookingStatus { Id = 3, Name = "Confirmed" },
                new BookingStatus { Id = 4, Name = "Cancelled" }
            );

            builder.Entity<ExpenseCategory>().HasData(
                new ExpenseCategory { Id = 1, Name = "Accommodation" },
                new ExpenseCategory { Id = 2, Name = "Transport" },
                new ExpenseCategory { Id = 3, Name = "Food" },
                new ExpenseCategory { Id = 4, Name = "Entertainment" },
                new ExpenseCategory { Id = 5, Name = "Shopping" },
                new ExpenseCategory { Id = 6, Name = "Other" }
            );

            builder.Entity<TransitType>().HasData(
                new TransitType { Id = 1, Name = "Flight" },
                new TransitType { Id = 2, Name = "Train" },
                new TransitType { Id = 3, Name = "Bus" },
                new TransitType { Id = 4, Name = "Car" },
                new TransitType { Id = 5, Name = "Ferry" },
                new TransitType { Id = 6, Name = "Other" }
            );

            builder.Entity<TripRole>().HasData(
                new TripRole { Id = 1, Name = "Organizer" },
                new TripRole { Id = 2, Name = "Participant" },
                new TripRole { Id = 3, Name = "Viewer" }
            );
        }
    }
}