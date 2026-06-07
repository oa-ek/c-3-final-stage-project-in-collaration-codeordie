// TravelManager.Infrastructure/Data/DatabaseSeeder.cs
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(this ModelBuilder builder) { 
            SeedLookupData(builder);
            //SeedTestData(modelBuilder);
        }

        private static void SeedLookupData(ModelBuilder builder)
        {
            {
                builder.Entity<IdentityRole>().HasData(
                    new IdentityRole { Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                    new IdentityRole { Id = "role-user", Name = "User", NormalizedName = "USER", ConcurrencyStamp = "2" }
                );

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

                builder.Entity<ChecklistTemplate>().HasData(
        new ChecklistTemplate
        {
            Id = 1,
            OwnerId = null,
            Title = "Базовий пакувальний список",
            Description = "Документи, одяг, гігієна, електроніка",
            IconClass = "bi-backpack4-fill"
        },
        new ChecklistTemplate
        {
            Id = 2,
            OwnerId = null,
            Title = "Пляжна відпустка",
            Description = "Все для відпочинку біля моря",
            IconClass = "bi-umbrella-fill"
        },
        new ChecklistTemplate
        {
            Id = 3,
            OwnerId = null,
            Title = "Бізнес-поїздка",
            Description = "Документи, ноутбук, ділові матеріали",
            IconClass = "bi-briefcase-fill"
        },
        new ChecklistTemplate
        {
            Id = 4,
            OwnerId = null,
            Title = "Гірський похід",
            Description = "Спорядження, медикаменти, їжа",
            IconClass = "bi-tree-fill"
        }
    );

                builder.Entity<ChecklistTemplateItem>().HasData(
                    // ── Базовий пакувальний список (Id=1) ──
                    new ChecklistTemplateItem { Id = 1, ChecklistTemplateId = 1, SortOrder = 1, Content = "Паспорт / ID-картка" },
                    new ChecklistTemplateItem { Id = 2, ChecklistTemplateId = 1, SortOrder = 2, Content = "Квитки (роздруківка або PDF)" },
                    new ChecklistTemplateItem { Id = 3, ChecklistTemplateId = 1, SortOrder = 3, Content = "Страховий поліс" },
                    new ChecklistTemplateItem { Id = 4, ChecklistTemplateId = 1, SortOrder = 4, Content = "Готівка / банківська картка" },
                    new ChecklistTemplateItem { Id = 5, ChecklistTemplateId = 1, SortOrder = 5, Content = "Зарядний пристрій та кабелі" },
                    new ChecklistTemplateItem { Id = 6, ChecklistTemplateId = 1, SortOrder = 6, Content = "Павербанк" },
                    new ChecklistTemplateItem { Id = 7, ChecklistTemplateId = 1, SortOrder = 7, Content = "Зубна щітка та паста" },
                    new ChecklistTemplateItem { Id = 8, ChecklistTemplateId = 1, SortOrder = 8, Content = "Шампунь / гель для душу" },
                    new ChecklistTemplateItem { Id = 9, ChecklistTemplateId = 1, SortOrder = 9, Content = "Рушник" },
                    new ChecklistTemplateItem { Id = 10, ChecklistTemplateId = 1, SortOrder = 10, Content = "Нижня білизна (кількість днів + 1)" },
                    new ChecklistTemplateItem { Id = 11, ChecklistTemplateId = 1, SortOrder = 11, Content = "Шкарпетки" },
                    new ChecklistTemplateItem { Id = 12, ChecklistTemplateId = 1, SortOrder = 12, Content = "Футболки / блузки" },
                    new ChecklistTemplateItem { Id = 13, ChecklistTemplateId = 1, SortOrder = 13, Content = "Штани / спідниця" },
                    new ChecklistTemplateItem { Id = 14, ChecklistTemplateId = 1, SortOrder = 14, Content = "Куртка / кофта" },
                    new ChecklistTemplateItem { Id = 15, ChecklistTemplateId = 1, SortOrder = 15, Content = "Зручне взуття" },
                    new ChecklistTemplateItem { Id = 16, ChecklistTemplateId = 1, SortOrder = 16, Content = "Ліки першої необхідності" },
                    new ChecklistTemplateItem { Id = 17, ChecklistTemplateId = 1, SortOrder = 17, Content = "Навушники" },
                    new ChecklistTemplateItem { Id = 18, ChecklistTemplateId = 1, SortOrder = 18, Content = "Книга / планшет для дороги" },

                    // ── Пляжна відпустка (Id=2) ──
                    new ChecklistTemplateItem { Id = 19, ChecklistTemplateId = 2, SortOrder = 1, Content = "Сонцезахисний крем SPF 50+" },
                    new ChecklistTemplateItem { Id = 20, ChecklistTemplateId = 2, SortOrder = 2, Content = "Купальник / плавки" },
                    new ChecklistTemplateItem { Id = 21, ChecklistTemplateId = 2, SortOrder = 3, Content = "Пляжний рушник" },
                    new ChecklistTemplateItem { Id = 22, ChecklistTemplateId = 2, SortOrder = 4, Content = "Сонцезахисні окуляри" },
                    new ChecklistTemplateItem { Id = 23, ChecklistTemplateId = 2, SortOrder = 5, Content = "Шляпа / кепка" },
                    new ChecklistTemplateItem { Id = 24, ChecklistTemplateId = 2, SortOrder = 6, Content = "Пляжні сандалі / в'єтнамки" },
                    new ChecklistTemplateItem { Id = 25, ChecklistTemplateId = 2, SortOrder = 7, Content = "Засіб від комарів" },
                    new ChecklistTemplateItem { Id = 26, ChecklistTemplateId = 2, SortOrder = 8, Content = "Водонепроникна сумка / чохол для телефону" },
                    new ChecklistTemplateItem { Id = 27, ChecklistTemplateId = 2, SortOrder = 9, Content = "Книга або електронна читалка" },
                    new ChecklistTemplateItem { Id = 28, ChecklistTemplateId = 2, SortOrder = 10, Content = "Пляжна парасолька / шезлонг" },

                    // ── Бізнес-поїздка (Id=3) ──
                    new ChecklistTemplateItem { Id = 29, ChecklistTemplateId = 3, SortOrder = 1, Content = "Ноутбук та зарядник" },
                    new ChecklistTemplateItem { Id = 30, ChecklistTemplateId = 3, SortOrder = 2, Content = "Візитки" },
                    new ChecklistTemplateItem { Id = 31, ChecklistTemplateId = 3, SortOrder = 3, Content = "Ділові документи / презентації" },
                    new ChecklistTemplateItem { Id = 32, ChecklistTemplateId = 3, SortOrder = 4, Content = "Ручка та блокнот" },
                    new ChecklistTemplateItem { Id = 33, ChecklistTemplateId = 3, SortOrder = 5, Content = "Ділове вбрання (костюм / плаття)" },
                    new ChecklistTemplateItem { Id = 34, ChecklistTemplateId = 3, SortOrder = 6, Content = "Дорожній адаптер" },
                    new ChecklistTemplateItem { Id = 35, ChecklistTemplateId = 3, SortOrder = 7, Content = "USB-хаб" },
                    new ChecklistTemplateItem { Id = 36, ChecklistTemplateId = 3, SortOrder = 8, Content = "Маска для сну в літаку" },

                    // ── Гірський похід (Id=4) ──
                    new ChecklistTemplateItem { Id = 37, ChecklistTemplateId = 4, SortOrder = 1, Content = "Трекінгові черевики" },
                    new ChecklistTemplateItem { Id = 38, ChecklistTemplateId = 4, SortOrder = 2, Content = "Трекінгові палиці" },
                    new ChecklistTemplateItem { Id = 39, ChecklistTemplateId = 4, SortOrder = 3, Content = "Рюкзак (30-50 л)" },
                    new ChecklistTemplateItem { Id = 40, ChecklistTemplateId = 4, SortOrder = 4, Content = "Термобілизна" },
                    new ChecklistTemplateItem { Id = 41, ChecklistTemplateId = 4, SortOrder = 5, Content = "Дощовик / мембранна куртка" },
                    new ChecklistTemplateItem { Id = 42, ChecklistTemplateId = 4, SortOrder = 6, Content = "Аптечка першої допомоги" },
                    new ChecklistTemplateItem { Id = 43, ChecklistTemplateId = 4, SortOrder = 7, Content = "Компас / GPS-навігатор" },
                    new ChecklistTemplateItem { Id = 44, ChecklistTemplateId = 4, SortOrder = 8, Content = "Ліхтарик з запасними батарейками" },
                    new ChecklistTemplateItem { Id = 45, ChecklistTemplateId = 4, SortOrder = 9, Content = "Запас їжі та води на маршрут" },
                    new ChecklistTemplateItem { Id = 46, ChecklistTemplateId = 4, SortOrder = 10, Content = "Спальник відповідний до температури" }
                );

                //private static void SeedTestData(ModelBuilder builder)
                //{
                //    var faker = new Faker("uk");

                //    // Користувачі
                //    var users = new List<User>();
                //    for (int i = 1; i <= 30; i++)
                //    {
                //        users.Add(new User
                //        {
                //            Id = Guid.NewGuid().ToString(),
                //            UserName = $"user{i}@travel.com",
                //            Email = $"user{i}@travel.com",
                //            EmailConfirmed = true,
                //            FirstName = faker.Name.FirstName(),
                //            LastName = faker.Name.LastName(),
                //            CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 200))
                //        });
                //    }
                //    builder.Entity<User>().HasData(users);

                //    var trips = new List<Trip>();
                //    var destinations = new List<TripDestination>();
                //    var accommodations = new List<Accommodation>();
                //    var transits = new List<Transit>();
                //    var activities = new List<TripActivity>();
                //    var expenses = new List<Expense>();
                //    var expenseSplits = new List<ExpenseSplit>();
                //    var checklists = new List<Checklist>();
                //    var checklistItems = new List<ChecklistItem>();

                //    for (int t = 1; t <= 45; t++)
                //    {
                //        var creator = users[faker.Random.Int(0, users.Count - 1)];
                //        var startDate = DateTime.UtcNow.AddDays(faker.Random.Int(-90, 60));

                //        var trip = new Trip
                //        {
                //            Id = t,
                //            Title = faker.Lorem.Sentence(faker.Random.Int(2, 5)),
                //            Description = faker.Lorem.Paragraph(2),
                //            DepartureLocation = faker.Address.City(),
                //            ReturnLocation = faker.Address.City(),
                //            StartDate = startDate,
                //            EndDate = startDate.AddDays(faker.Random.Int(5, 15)),
                //            BaseCurrency = faker.PickRandom(new[] { "UAH", "USD", "EUR" }),
                //            CreatorId = creator.Id,
                //            StatusId = faker.Random.Int(1, 4),
                //            CreatedAt = DateTime.UtcNow
                //        };
                //        trips.Add(trip);

                //        // Destinations
                //        for (int d = 1; d <= faker.Random.Int(2, 5); d++)
                //        {
                //            destinations.Add(new TripDestination
                //            {
                //                Id = t * 100 + d,
                //                TripId = trip.Id,
                //                CityName = faker.Address.City(),
                //                Country = faker.Address.Country(),
                //                Latitude = faker.Address.Latitude(),
                //                Longitude = faker.Address.Longitude(),
                //                ArrivalDate = startDate.AddDays(d - 1),
                //                DepartureDate = startDate.AddDays(d)
                //            });
                //        }

                //        // Accommodation
                //        for (int a = 1; a <= faker.Random.Int(1, 3); a++)
                //        {
                //            accommodations.Add(new Accommodation
                //            {
                //                Id = t * 1000 + a,
                //                TripId = trip.Id,
                //                BookingStatusId = faker.Random.Int(1, 4),
                //                Name = faker.Company.CompanyName() + " Hotel",
                //                Address = faker.Address.FullAddress(),
                //                CheckInTime = startDate.AddDays(faker.Random.Int(0, 5)),
                //                CheckOutTime = startDate.AddDays(faker.Random.Int(6, 12)),
                //                BookingReference = "BK" + faker.Random.Number(10000, 99999)
                //            });
                //        }

                //        // Expenses
                //        for (int e = 1; e <= faker.Random.Int(6, 15); e++)
                //        {
                //            var payer = users[faker.Random.Int(0, users.Count - 1)];
                //            var amount = faker.Random.Decimal(150, 4500);

                //            var expense = new Expense
                //            {
                //                Id = t * 1000 + e,
                //                TripId = trip.Id,
                //                CategoryId = faker.Random.Int(1, 6),
                //                Title = faker.Commerce.ProductName(),
                //                TotalAmount = amount,
                //                Currency = trip.BaseCurrency,
                //                Date = startDate.AddDays(faker.Random.Int(0, (int)(trip.EndDate - trip.StartDate).TotalDays)),
                //                PayerId = payer.Id
                //            };
                //            expenses.Add(expense);

                //            // Splits
                //            var splitCount = faker.Random.Int(2, 4);
                //            for (int s = 0; s < splitCount; s++)
                //            {
                //                var debtor = users[faker.Random.Int(0, users.Count - 1)];
                //                expenseSplits.Add(new ExpenseSplit
                //                {
                //                    Id = expense.Id * 10 + s,
                //                    ExpenseId = expense.Id,
                //                    DebtorId = debtor.Id,
                //                    OwedAmount = Math.Round(amount / splitCount, 2),
                //                    IsSettled = faker.Random.Bool(0.65f)
                //                });
                //            }
                //        }

                //        // Checklists
                //        for (int c = 1; c <= faker.Random.Int(1, 3); c++)
                //        {
                //            var checklist = new Checklist
                //            {
                //                Id = t * 100 + c * 10,
                //                TripId = trip.Id,
                //                Title = faker.Random.ArrayElement(new[] { "Речі в валізу", "Документи", "Підготовка", "Важливі завдання" })
                //            };
                //            checklists.Add(checklist);

                //            for (int ci = 1; ci <= faker.Random.Int(5, 12); ci++)
                //            {
                //                checklistItems.Add(new ChecklistItem
                //                {
                //                    Id = checklist.Id * 10 + ci,
                //                    ChecklistId = checklist.Id,
                //                    Content = faker.Lorem.Sentence(faker.Random.Int(3, 8)),
                //                    IsChecked = faker.Random.Bool(0.4f)
                //                });
                //            }
                //        }
                //    }

                //    builder.Entity<Trip>().HasData(trips);
                //    builder.Entity<TripDestination>().HasData(destinations);
                //    builder.Entity<Accommodation>().HasData(accommodations);
                //    builder.Entity<Expense>().HasData(expenses);
                //    builder.Entity<ExpenseSplit>().HasData(expenseSplits);
                //    builder.Entity<Checklist>().HasData(checklists);
                //    builder.Entity<ChecklistItem>().HasData(checklistItems);
                //}
            }
        }

        private static void SeedTestData(ModelBuilder builder)
        {
            var faker = new Faker("uk");

            // Користувачі
            var users = new List<User>();
            for (int i = 1; i <= 30; i++)
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = $"user{i}@travel.com",
                    Email = $"user{i}@travel.com",
                    EmailConfirmed = true,
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 200))
                });
            }
            builder.Entity<User>().HasData(users);

            var trips = new List<Trip>();
            var destinations = new List<TripDestination>();
            var accommodations = new List<Accommodation>();
            var transits = new List<Transit>();
            var activities = new List<TripActivity>();
            var expenses = new List<Expense>();
            var expenseSplits = new List<ExpenseSplit>();
            var checklists = new List<Checklist>();
            var checklistItems = new List<ChecklistItem>();

            for (int t = 1; t <= 45; t++)
            {
                var creator = users[faker.Random.Int(0, users.Count - 1)];
                var startDate = DateTime.UtcNow.AddDays(faker.Random.Int(-90, 60));

                var trip = new Trip
                {
                    Id = t,
                    Title = faker.Lorem.Sentence(faker.Random.Int(2, 5)),
                    Description = faker.Lorem.Paragraph(2),
                    DepartureLocation = faker.Address.City(),
                    ReturnLocation = faker.Address.City(),
                    StartDate = startDate,
                    EndDate = startDate.AddDays(faker.Random.Int(5, 15)),
                    BaseCurrency = faker.PickRandom(new[] { "UAH", "USD", "EUR" }),
                    CreatorId = creator.Id,
                    StatusId = faker.Random.Int(1, 4),
                    CreatedAt = DateTime.UtcNow
                };
                trips.Add(trip);

                // Destinations
                for (int d = 1; d <= faker.Random.Int(2, 5); d++)
                {
                    destinations.Add(new TripDestination
                    {
                        Id = t * 100 + d,
                        TripId = trip.Id,
                        CityName = faker.Address.City(),
                        Country = faker.Address.Country(),
                        Latitude = faker.Address.Latitude(),
                        Longitude = faker.Address.Longitude(),
                        ArrivalDate = startDate.AddDays(d - 1),
                        DepartureDate = startDate.AddDays(d)
                    });
                }

                // Accommodation
                for (int a = 1; a <= faker.Random.Int(1, 3); a++)
                {
                    accommodations.Add(new Accommodation
                    {
                        Id = t * 1000 + a,
                        TripId = trip.Id,
                        BookingStatusId = faker.Random.Int(1, 4),
                        Name = faker.Company.CompanyName() + " Hotel",
                        Address = faker.Address.FullAddress(),
                        CheckInTime = startDate.AddDays(faker.Random.Int(0, 5)),
                        CheckOutTime = startDate.AddDays(faker.Random.Int(6, 12)),
                        BookingReference = "BK" + faker.Random.Number(10000, 99999)
                    });
                }

                // Expenses
                for (int e = 1; e <= faker.Random.Int(6, 15); e++)
                {
                    var payer = users[faker.Random.Int(0, users.Count - 1)];
                    var amount = faker.Random.Decimal(150, 4500);

                    var expense = new Expense
                    {
                        Id = t * 1000 + e,
                        TripId = trip.Id,
                        CategoryId = faker.Random.Int(1, 6),
                        Title = faker.Commerce.ProductName(),
                        TotalAmount = amount,
                        Currency = trip.BaseCurrency,
                        Date = startDate.AddDays(faker.Random.Int(0, (int)(trip.EndDate - trip.StartDate).TotalDays)),
                        PayerId = payer.Id
                    };
                    expenses.Add(expense);

                    // Splits
                    var splitCount = faker.Random.Int(2, 4);
                    for (int s = 0; s < splitCount; s++)
                    {
                        var debtor = users[faker.Random.Int(0, users.Count - 1)];
                        expenseSplits.Add(new ExpenseSplit
                        {
                            Id = expense.Id * 10 + s,
                            ExpenseId = expense.Id,
                            DebtorId = debtor.Id,
                            OwedAmount = Math.Round(amount / splitCount, 2),
                            IsSettled = faker.Random.Bool(0.65f)
                        });
                    }
                }

                // Checklists
                for (int c = 1; c <= faker.Random.Int(1, 3); c++)
                {
                    var checklist = new Checklist
                    {
                        Id = t * 100 + c * 10,
                        TripId = trip.Id,
                        Title = faker.Random.ArrayElement(new[] { "Речі в валізу", "Документи", "Підготовка", "Важливі завдання" })
                    };
                    checklists.Add(checklist);

                    for (int ci = 1; ci <= faker.Random.Int(5, 12); ci++)
                    {
                        checklistItems.Add(new ChecklistItem
                        {
                            Id = checklist.Id * 10 + ci,
                            ChecklistId = checklist.Id,
                            Content = faker.Lorem.Sentence(faker.Random.Int(3, 8)),
                            IsChecked = faker.Random.Bool(0.4f)
                        });
                    }
                }
            }

            builder.Entity<Trip>().HasData(trips);
            builder.Entity<TripDestination>().HasData(destinations);
            builder.Entity<Accommodation>().HasData(accommodations);
            builder.Entity<Expense>().HasData(expenses);
            builder.Entity<ExpenseSplit>().HasData(expenseSplits);
            builder.Entity<Checklist>().HasData(checklists);
            builder.Entity<ChecklistItem>().HasData(checklistItems);
        }
    }
}