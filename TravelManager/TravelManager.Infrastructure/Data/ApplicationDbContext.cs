using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TravelManager.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<TripRole> TripRoles => Set<TripRole>();
        public DbSet<TripParticipant> TripParticipants => Set<TripParticipant>();
        public DbSet<TripStatus> TripStatuses => Set<TripStatus>();
        public DbSet<BookingStatus> BookingStatuses => Set<BookingStatus>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<TripDestination> TripDestinations => Set<TripDestination>();
        public DbSet<Checklist> Checklists => Set<Checklist>();
        public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
        public DbSet<TransitType> TransitTypes => Set<TransitType>();
        public DbSet<Transit> Transits => Set<Transit>();
        public DbSet<Accommodation> Accommodations => Set<Accommodation>();
        public DbSet<TripActivity> TripActivities => Set<TripActivity>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
        public DbSet<TripDocument> TripDocuments => Set<TripDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TripParticipant>()
                .HasKey(tp => new { tp.TripId, tp.UserId });

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Payer)
                .WithMany(u => u.PaidExpenses)
                .HasForeignKey(e => e.PayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExpenseSplit>()
                .HasOne(es => es.Debtor)
                .WithMany(u => u.Debts)
                .HasForeignKey(es => es.DebtorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>().HasOne(e => e.Transit).WithMany().HasForeignKey(e => e.TransitId).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<Expense>().HasOne(e => e.Accommodation).WithMany().HasForeignKey(e => e.AccommodationId).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<Expense>().HasOne(e => e.TripActivity).WithMany().HasForeignKey(e => e.TripActivityId).OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<TripDocument>().HasOne(d => d.Transit).WithMany().HasForeignKey(d => d.TransitId).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<TripDocument>().HasOne(d => d.Accommodation).WithMany().HasForeignKey(d => d.AccommodationId).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<TripDocument>().HasOne(d => d.TripActivity).WithMany().HasForeignKey(d => d.TripActivityId).OnDelete(DeleteBehavior.ClientSetNull);

        }
    }
}
