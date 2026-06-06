using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
namespace TravelManager.Infrastructure.Data
{
    public static class RuntimeSeeder
    {
        public static async Task SeedDemoDataAsync(this WebApplication app, string existingUserEmail)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (await db.Trips.AnyAsync()) return;

            var user = await userManager.FindByEmailAsync(existingUserEmail);
            if (user == null)
                throw new Exception($"Юзер '{existingUserEmail}' не знайдений.");

            var uid = user.Id;

            const int sPlanned = 1;
            const int sInProgress = 2;
            const int sCompleted = 3;
            const int sCancelled = 4;
            const int bConfirmed = 3;
            const int bPending = 2;
            const int bNone = 1;
            const int tFlight = 1;
            const int tTrain = 2;
            const int tBus = 3;
            const int tCar = 4;
            const int tFerry = 5;
            const int cAccom = 1;
            const int cTransport = 2;
            const int cFood = 3;
            const int cEntertain = 4;
            const int cShopping = 5;
            const int cOther = 6;
            const int rOrganizer = 1;

            // ══════════════════════════════════════════════════════════════════════
            // 1. Японія: Токіо → Кіото → Осака  (завершена)
            // ══════════════════════════════════════════════════════════════════════
            var t1 = new Trip
            {
                Title = "Японія: Токіо, Кіото, Осака",
                Description = "Сакура, храми, вагю та неон — мрія здійснилась.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2025, 4, 16),
                BaseCurrency = "JPY",
                CreatorId = uid,
                StatusId = sCompleted,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t1); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t1.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t1.Id, CityName = "Токіо", Country = "Japan", ArrivalDate = new DateTime(2025, 4, 2), DepartureDate = new DateTime(2025, 4, 8), Latitude = 35.6762, Longitude = 139.6503 },
                new TripDestination { TripId = t1.Id, CityName = "Кіото", Country = "Japan", ArrivalDate = new DateTime(2025, 4, 8), DepartureDate = new DateTime(2025, 4, 13), Latitude = 35.0116, Longitude = 135.7681 },
                new TripDestination { TripId = t1.Id, CityName = "Осака", Country = "Japan", ArrivalDate = new DateTime(2025, 4, 13), DepartureDate = new DateTime(2025, 4, 16), Latitude = 34.6937, Longitude = 135.5023 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t1.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Токіо (NRT)", DepartureTime = new DateTime(2025, 4, 1, 10, 0, 0), ArrivalTime = new DateTime(2025, 4, 2, 8, 30, 0), CarrierInfo = "Turkish Airlines TK198", BookingReference = "TK-881JPY" },
                new Transit { TripId = t1.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Токіо (Shinagawa)", ArrivalLocation = "Кіото", DepartureTime = new DateTime(2025, 4, 8, 9, 0, 0), ArrivalTime = new DateTime(2025, 4, 8, 11, 17, 0), CarrierInfo = "Shinkansen Nozomi (JR Pass)", BookingReference = "JR-PASS-001" },
                new Transit { TripId = t1.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Кіото", ArrivalLocation = "Осака (Shin-Osaka)", DepartureTime = new DateTime(2025, 4, 13, 10, 30, 0), ArrivalTime = new DateTime(2025, 4, 13, 10, 45, 0), CarrierInfo = "Shinkansen Hikari", BookingReference = "JR-PASS-002" },
                new Transit { TripId = t1.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Осака (KIX)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2025, 4, 16, 14, 0, 0), ArrivalTime = new DateTime(2025, 4, 17, 6, 0, 0), CarrierInfo = "Turkish Airlines TK199", BookingReference = "TK-882KBP" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t1.Id, BookingStatusId = bConfirmed, Name = "Park Hyatt Tokyo", Address = "3-7-1-2 Nishi-Shinjuku, Tokyo", CheckInTime = new DateTime(2025, 4, 2, 15, 0, 0), CheckOutTime = new DateTime(2025, 4, 8, 11, 0, 0), BookingReference = "PHT-2025", ContactPhone = "+81 3-5322-1234", WebsiteUrl = "https://www.hyatt.com", Latitude = 35.6862, Longitude = 139.6946 },
                new Accommodation { TripId = t1.Id, BookingStatusId = bConfirmed, Name = "Kyoto Machiya Gion", Address = "Gion, Higashiyama-ku, Kyoto", CheckInTime = new DateTime(2025, 4, 8, 16, 0, 0), CheckOutTime = new DateTime(2025, 4, 13, 10, 0, 0), BookingReference = "KMG-441", Latitude = 35.0037, Longitude = 135.7785 },
                new Accommodation { TripId = t1.Id, BookingStatusId = bConfirmed, Name = "Dormy Inn Osaka Shinsaibashi", Address = "1-5-21 Shinsaibashisuji, Osaka", CheckInTime = new DateTime(2025, 4, 13, 15, 0, 0), CheckOutTime = new DateTime(2025, 4, 16, 10, 0, 0), BookingReference = "DIO-772", Latitude = 34.6737, Longitude = 135.5003 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t1.Id, BookingStatusId = bConfirmed, Title = "Shibuya Crossing + Harajuku", Address = "Shibuya, Tokyo", StartTime = new DateTime(2025, 4, 3, 18, 0, 0), EndTime = new DateTime(2025, 4, 3, 21, 0, 0), Notes = "Вечірній час — найбільше людей і атмосфери" },
                new TripActivity { TripId = t1.Id, BookingStatusId = bConfirmed, Title = "Токійський Діснейленд", Address = "1-1 Maihama, Urayasu, Chiba", StartTime = new DateTime(2025, 4, 4, 9, 0, 0), EndTime = new DateTime(2025, 4, 4, 21, 0, 0), Notes = "Квитки ¥9400, купити онлайн заздалегідь" },
                new TripActivity { TripId = t1.Id, BookingStatusId = bConfirmed, Title = "Fushimi Inari-taisha на світанку", Address = "68 Fukakusa Yabunouchicho, Kyoto", StartTime = new DateTime(2025, 4, 9, 5, 30, 0), EndTime = new DateTime(2025, 4, 9, 9, 0, 0), Notes = "Вхід вільний. До 7:00 — без туристів, тисячі тории у тумані" },
                new TripActivity { TripId = t1.Id, BookingStatusId = bConfirmed, Title = "Традиційна чайна церемонія", Address = "Urasenke, Kamigyō-ku, Kyoto", StartTime = new DateTime(2025, 4, 10, 14, 0, 0), EndTime = new DateTime(2025, 4, 10, 16, 0, 0), Notes = "¥3500, резервація обов'язкова" },
                new TripActivity { TripId = t1.Id, BookingStatusId = bConfirmed, Title = "Доtonbori нічна прогулянка", Address = "Dotonbori, Namba, Osaka", StartTime = new DateTime(2025, 4, 13, 20, 0, 0), Notes = "Takoyaki + Okonomiyaki — must eat" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t1.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Токіо–Київ", TotalAmount = 42000, Currency = "JPY", Date = new DateTime(2025, 4, 1), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cTransport, Title = "JR Pass 14 днів", TotalAmount = 66000, Currency = "JPY", Date = new DateTime(2025, 4, 2), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cAccom, Title = "Park Hyatt Tokyo (6 ночей)", TotalAmount = 180000, Currency = "JPY", Date = new DateTime(2025, 4, 2), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cAccom, Title = "Kyoto Machiya (5 ночей)", TotalAmount = 65000, Currency = "JPY", Date = new DateTime(2025, 4, 8), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cEntertain, Title = "Токійський Діснейленд (x1)", TotalAmount = 9400, Currency = "JPY", Date = new DateTime(2025, 4, 4), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cFood, Title = "Wagyu Omakase Gion", TotalAmount = 28000, Currency = "JPY", Date = new DateTime(2025, 4, 11), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cFood, Title = "Ramen Ichiran Shibuya", TotalAmount = 1800, Currency = "JPY", Date = new DateTime(2025, 4, 3), PayerId = uid },
                new Expense { TripId = t1.Id, CategoryId = cShopping, Title = "Акіхабара — аніме та гаджети", TotalAmount = 35000, Currency = "JPY", Date = new DateTime(2025, 4, 5), PayerId = uid }
            );
            var cl1 = new Checklist { TripId = t1.Id, Title = "Японія — чеклист" };
            db.Checklists.Add(cl1); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl1.Id, Content = "Купити JR Pass онлайн (дешевше ніж на місці)", IsChecked = true },
                new ChecklistItem { ChecklistId = cl1.Id, Content = "Замовити Pocket WiFi", IsChecked = true },
                new ChecklistItem { ChecklistId = cl1.Id, Content = "Взяти готівку JPY (картки беруть не всюди)", IsChecked = true },
                new ChecklistItem { ChecklistId = cl1.Id, Content = "Вивчити базові слова японською", IsChecked = true },
                new ChecklistItem { ChecklistId = cl1.Id, Content = "Завантажити Google Maps офлайн", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 2. Італія: Рим → Флоренція → Венеція  (завершена)
            // ══════════════════════════════════════════════════════════════════════
            var t2 = new Trip
            {
                Title = "Велике Італійське турне",
                Description = "Мистецтво Відродження, паста та gondola.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2025, 6, 5),
                EndDate = new DateTime(2025, 6, 18),
                BaseCurrency = "EUR",
                CreatorId = uid,
                StatusId = sCompleted,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t2); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t2.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t2.Id, CityName = "Рим", Country = "Italy", ArrivalDate = new DateTime(2025, 6, 5), DepartureDate = new DateTime(2025, 6, 9), Latitude = 41.9028, Longitude = 12.4964 },
                new TripDestination { TripId = t2.Id, CityName = "Флоренція", Country = "Italy", ArrivalDate = new DateTime(2025, 6, 9), DepartureDate = new DateTime(2025, 6, 14), Latitude = 43.7696, Longitude = 11.2558 },
                new TripDestination { TripId = t2.Id, CityName = "Венеція", Country = "Italy", ArrivalDate = new DateTime(2025, 6, 14), DepartureDate = new DateTime(2025, 6, 18), Latitude = 45.4408, Longitude = 12.3155 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t2.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Рим (FCO)", DepartureTime = new DateTime(2025, 6, 5, 6, 0, 0), ArrivalTime = new DateTime(2025, 6, 5, 9, 0, 0), CarrierInfo = "Ryanair FR4821", BookingReference = "RYR-556IT" },
                new Transit { TripId = t2.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Roma Termini", ArrivalLocation = "Firenze S.M.N.", DepartureTime = new DateTime(2025, 6, 9, 10, 0, 0), ArrivalTime = new DateTime(2025, 6, 9, 11, 30, 0), CarrierInfo = "Trenitalia Frecciarossa", BookingReference = "TRN-221FI" },
                new Transit { TripId = t2.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Firenze S.M.N.", ArrivalLocation = "Venezia S. Lucia", DepartureTime = new DateTime(2025, 6, 14, 9, 30, 0), ArrivalTime = new DateTime(2025, 6, 14, 11, 45, 0), CarrierInfo = "Trenitalia Frecciargento", BookingReference = "TRN-334VE" },
                new Transit { TripId = t2.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Венеція (VCE)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2025, 6, 18, 17, 0, 0), ArrivalTime = new DateTime(2025, 6, 18, 21, 30, 0), CarrierInfo = "WizzAir W66701", BookingReference = "WZZ-881KBP" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t2.Id, BookingStatusId = bConfirmed, Name = "Hotel Artemide Roma", Address = "Via Nazionale 22, 00184 Roma", CheckInTime = new DateTime(2025, 6, 5, 14, 0, 0), CheckOutTime = new DateTime(2025, 6, 9, 11, 0, 0), BookingReference = "ART-101", ContactPhone = "+39 06 489911", Latitude = 41.9001, Longitude = 12.4988 },
                new Accommodation { TripId = t2.Id, BookingStatusId = bConfirmed, Name = "Hotel Davanzati Florence", Address = "Via Porta Rossa 5, 50123 Firenze", CheckInTime = new DateTime(2025, 6, 9, 15, 0, 0), CheckOutTime = new DateTime(2025, 6, 14, 11, 0, 0), BookingReference = "DAV-220FI", ContactPhone = "+39 055 286666", Latitude = 43.7696, Longitude = 11.2524 },
                new Accommodation { TripId = t2.Id, BookingStatusId = bConfirmed, Name = "Hotel Palazzo Stern Venice", Address = "Dorsoduro 2792, 30123 Venezia", CheckInTime = new DateTime(2025, 6, 14, 16, 0, 0), CheckOutTime = new DateTime(2025, 6, 18, 10, 0, 0), BookingReference = "STR-330VE", ContactPhone = "+39 041 2770869", Latitude = 45.4320, Longitude = 12.3254 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t2.Id, BookingStatusId = bConfirmed, Title = "Колізей + Римський форум", Address = "Piazza del Colosseo 1, Roma", StartTime = new DateTime(2025, 6, 6, 9, 0, 0), EndTime = new DateTime(2025, 6, 6, 13, 0, 0), Notes = "Квиток €16, бронювати онлайн — черги на 2 години" },
                new TripActivity { TripId = t2.Id, BookingStatusId = bConfirmed, Title = "Ватикан та Сикстинська капела", Address = "Piazza San Pietro, Roma", StartTime = new DateTime(2025, 6, 7, 8, 0, 0), EndTime = new DateTime(2025, 6, 7, 13, 0, 0), Notes = "Skip-the-line €35. Вхід у капелу тільки з турагентом" },
                new TripActivity { TripId = t2.Id, BookingStatusId = bConfirmed, Title = "Галерея Уффіці", Address = "Piazzale degli Uffizi 6, Firenze", StartTime = new DateTime(2025, 6, 10, 9, 0, 0), EndTime = new DateTime(2025, 6, 10, 13, 0, 0), Notes = "Боттічеллі, Леонардо, Мікеланджело — прийти до відкриття" },
                new TripActivity { TripId = t2.Id, BookingStatusId = bConfirmed, Title = "Подорож на гондолі", Address = "San Marco, Venezia", StartTime = new DateTime(2025, 6, 15, 18, 0, 0), EndTime = new DateTime(2025, 6, 15, 19, 0, 0), Notes = "€80 за гондолу (до 6 осіб). Торгуватись!" },
                new TripActivity { TripId = t2.Id, BookingStatusId = bConfirmed, Title = "Острів Мурано — виготовлення скла", Address = "Murano, Venezia", StartTime = new DateTime(2025, 6, 16, 10, 0, 0), EndTime = new DateTime(2025, 6, 16, 14, 0, 0), Notes = "Вапоретто лінія 4.1 від Fondamente Nove" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t2.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Рим", TotalAmount = 180, Currency = "EUR", Date = new DateTime(2025, 6, 5), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cTransport, Title = "Frecciarossa Рим–Флоренція", TotalAmount = 45, Currency = "EUR", Date = new DateTime(2025, 6, 9), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cTransport, Title = "Frecciargento Флоренція–Венеція", TotalAmount = 52, Currency = "EUR", Date = new DateTime(2025, 6, 14), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cAccom, Title = "Hotel Artemide (4 ночі)", TotalAmount = 520, Currency = "EUR", Date = new DateTime(2025, 6, 5), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cAccom, Title = "Hotel Davanzati (5 ночей)", TotalAmount = 450, Currency = "EUR", Date = new DateTime(2025, 6, 9), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cAccom, Title = "Palazzo Stern (4 ночі)", TotalAmount = 680, Currency = "EUR", Date = new DateTime(2025, 6, 14), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cEntertain, Title = "Ватикан skip-the-line", TotalAmount = 35, Currency = "EUR", Date = new DateTime(2025, 6, 7), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cEntertain, Title = "Гондола Венеція", TotalAmount = 80, Currency = "EUR", Date = new DateTime(2025, 6, 15), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cFood, Title = "Ristorante La Pergola Roma", TotalAmount = 140, Currency = "EUR", Date = new DateTime(2025, 6, 6), PayerId = uid },
                new Expense { TripId = t2.Id, CategoryId = cFood, Title = "Bistecca alla Fiorentina", TotalAmount = 95, Currency = "EUR", Date = new DateTime(2025, 6, 11), PayerId = uid }
            );
            var cl2 = new Checklist { TripId = t2.Id, Title = "Італія — документи" };
            db.Checklists.Add(cl2); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl2.Id, Content = "Паспорт та копія", IsChecked = true },
                new ChecklistItem { ChecklistId = cl2.Id, Content = "Страховка (Schengen)", IsChecked = true },
                new ChecklistItem { ChecklistId = cl2.Id, Content = "Бронювання Колізею онлайн", IsChecked = true },
                new ChecklistItem { ChecklistId = cl2.Id, Content = "Бронювання Ватикану онлайн", IsChecked = true },
                new ChecklistItem { ChecklistId = cl2.Id, Content = "Завантажити Roma Pass", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 3. США: Нью-Йорк → Лас-Вегас → Лос-Анджелес  (завершена)
            // ══════════════════════════════════════════════════════════════════════
            var t3 = new Trip
            {
                Title = "Американська мрія: NY → Vegas → LA",
                Description = "Хмарочоси, Гранд-Каньйон та Голлівуд.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2025, 9, 10),
                EndDate = new DateTime(2025, 9, 27),
                BaseCurrency = "USD",
                CreatorId = uid,
                StatusId = sCompleted,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t3); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t3.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t3.Id, CityName = "Нью-Йорк", Country = "USA", ArrivalDate = new DateTime(2025, 9, 10), DepartureDate = new DateTime(2025, 9, 16), Latitude = 40.7128, Longitude = -74.0060 },
                new TripDestination { TripId = t3.Id, CityName = "Лас-Вегас", Country = "USA", ArrivalDate = new DateTime(2025, 9, 16), DepartureDate = new DateTime(2025, 9, 21), Latitude = 36.1699, Longitude = -115.1398 },
                new TripDestination { TripId = t3.Id, CityName = "Лос-Анджелес", Country = "USA", ArrivalDate = new DateTime(2025, 9, 21), DepartureDate = new DateTime(2025, 9, 27), Latitude = 34.0522, Longitude = -118.2437 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t3.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Нью-Йорк (JFK)", DepartureTime = new DateTime(2025, 9, 10, 8, 0, 0), ArrivalTime = new DateTime(2025, 9, 10, 14, 30, 0), CarrierInfo = "Delta DL401", BookingReference = "DL-993JFK" },
                new Transit { TripId = t3.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Нью-Йорк (JFK)", ArrivalLocation = "Лас-Вегас (LAS)", DepartureTime = new DateTime(2025, 9, 16, 7, 30, 0), ArrivalTime = new DateTime(2025, 9, 16, 10, 45, 0), CarrierInfo = "Southwest WN2241", BookingReference = "SW-441LAS" },
                new Transit { TripId = t3.Id, TransitTypeId = tCar, BookingStatusId = bConfirmed, DepartureLocation = "Лас-Вегас", ArrivalLocation = "Лос-Анджелес", DepartureTime = new DateTime(2025, 9, 21, 9, 0, 0), ArrivalTime = new DateTime(2025, 9, 21, 13, 30, 0), CarrierInfo = "Enterprise Rent-A-Car", BookingReference = "ENT-554LA" },
                new Transit { TripId = t3.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Лос-Анджелес (LAX)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2025, 9, 27, 22, 0, 0), ArrivalTime = new DateTime(2025, 9, 29, 12, 0, 0), CarrierInfo = "Lufthansa LH456", BookingReference = "LH-771KBP" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t3.Id, BookingStatusId = bConfirmed, Name = "The Standard High Line NYC", Address = "848 Washington St, New York, NY 10014", CheckInTime = new DateTime(2025, 9, 10, 15, 0, 0), CheckOutTime = new DateTime(2025, 9, 16, 11, 0, 0), BookingReference = "STD-101NY", ContactPhone = "+1 212-645-4646", Latitude = 40.7435, Longitude = -74.0077 },
                new Accommodation { TripId = t3.Id, BookingStatusId = bConfirmed, Name = "The Cosmopolitan Las Vegas", Address = "3708 Las Vegas Blvd S, Las Vegas, NV 89109", CheckInTime = new DateTime(2025, 9, 16, 15, 0, 0), CheckOutTime = new DateTime(2025, 9, 21, 11, 0, 0), BookingReference = "COS-888LV", ContactPhone = "+1 702-698-7000", Latitude = 36.1100, Longitude = -115.1742 },
                new Accommodation { TripId = t3.Id, BookingStatusId = bConfirmed, Name = "Chateau Marmont Hollywood", Address = "8221 Sunset Blvd, Los Angeles, CA 90046", CheckInTime = new DateTime(2025, 9, 21, 16, 0, 0), CheckOutTime = new DateTime(2025, 9, 27, 11, 0, 0), BookingReference = "CHM-221LA", ContactPhone = "+1 323-656-1010", Latitude = 34.0987, Longitude = -118.3617 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t3.Id, BookingStatusId = bConfirmed, Title = "Статуя Свободи + Елліс-Айленд", Address = "Liberty Island, New York", StartTime = new DateTime(2025, 9, 11, 9, 0, 0), EndTime = new DateTime(2025, 9, 11, 14, 0, 0), Notes = "Квиток $24, ферон відправляється кожні 30 хв" },
                new TripActivity { TripId = t3.Id, BookingStatusId = bConfirmed, Title = "Шоу Cirque du Soleil — O", Address = "Bellagio, 3600 S Las Vegas Blvd", StartTime = new DateTime(2025, 9, 17, 19, 30, 0), EndTime = new DateTime(2025, 9, 17, 22, 0, 0), Notes = "VIP квитки $185. Одне з найкращих шоу у світі" },
                new TripActivity { TripId = t3.Id, BookingStatusId = bConfirmed, Title = "Гранд-Каньйон — одноденний тур", Address = "Grand Canyon National Park, AZ", StartTime = new DateTime(2025, 9, 18, 6, 0, 0), EndTime = new DateTime(2025, 9, 18, 22, 0, 0), Notes = "Виїзд о 6:00 з Vegas, вхід $35/авто" },
                new TripActivity { TripId = t3.Id, BookingStatusId = bConfirmed, Title = "Universal Studios Hollywood", Address = "100 Universal City Plaza, Universal City", StartTime = new DateTime(2025, 9, 22, 9, 0, 0), EndTime = new DateTime(2025, 9, 22, 20, 0, 0), Notes = "Express Pass $109, Harry Potter world — must!" },
                new TripActivity { TripId = t3.Id, BookingStatusId = bConfirmed, Title = "Venice Beach + Santa Monica Pier", Address = "Venice Beach, Los Angeles", StartTime = new DateTime(2025, 9, 24, 11, 0, 0), EndTime = new DateTime(2025, 9, 24, 18, 0, 0), Notes = "Роликові ковзани в оренду $10/год" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t3.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–NY–Kiev", TotalAmount = 1100, Currency = "USD", Date = new DateTime(2025, 9, 10), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cTransport, Title = "Перельот NY–Vegas", TotalAmount = 180, Currency = "USD", Date = new DateTime(2025, 9, 16), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cTransport, Title = "Оренда авто Vegas–LA (4 дні)", TotalAmount = 220, Currency = "USD", Date = new DateTime(2025, 9, 21), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cAccom, Title = "The Standard NYC (6 ночей)", TotalAmount = 1560, Currency = "USD", Date = new DateTime(2025, 9, 10), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cAccom, Title = "The Cosmopolitan LV (5 ночей)", TotalAmount = 950, Currency = "USD", Date = new DateTime(2025, 9, 16), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cAccom, Title = "Chateau Marmont LA (6 ночей)", TotalAmount = 2100, Currency = "USD", Date = new DateTime(2025, 9, 21), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cEntertain, Title = "Cirque du Soleil O — VIP", TotalAmount = 185, Currency = "USD", Date = new DateTime(2025, 9, 17), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cEntertain, Title = "Гранд-Каньйон тур", TotalAmount = 89, Currency = "USD", Date = new DateTime(2025, 9, 18), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cFood, Title = "Nobu Restaurant Malibu", TotalAmount = 280, Currency = "USD", Date = new DateTime(2025, 9, 23), PayerId = uid },
                new Expense { TripId = t3.Id, CategoryId = cFood, Title = "In-N-Out + Nathan's Hot Dog", TotalAmount = 35, Currency = "USD", Date = new DateTime(2025, 9, 11), PayerId = uid }
            );
            var cl3 = new Checklist { TripId = t3.Id, Title = "США — підготовка" };
            db.Checklists.Add(cl3); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl3.Id, Content = "ESTA авторизація ($21)", IsChecked = true },
                new ChecklistItem { ChecklistId = cl3.Id, Content = "Міжнародні права водія", IsChecked = true },
                new ChecklistItem { ChecklistId = cl3.Id, Content = "Роумінг або eSIM США", IsChecked = true },
                new ChecklistItem { ChecklistId = cl3.Id, Content = "Бронювання Cirque du Soleil", IsChecked = true },
                new ChecklistItem { ChecklistId = cl3.Id, Content = "Travel insurance", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 4. Таїланд: Бангкок → Чіанг-Май → Пхукет  (завершена)
            // ══════════════════════════════════════════════════════════════════════
            var t4 = new Trip
            {
                Title = "Таїланд: Бангкок, Чіанг-Май, Пхукет",
                Description = "Храми, слони та найкращий вуличний стріт-фуд у світі.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2025, 11, 15),
                EndDate = new DateTime(2025, 11, 30),
                BaseCurrency = "THB",
                CreatorId = uid,
                StatusId = sCompleted,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t4); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t4.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t4.Id, CityName = "Бангкок", Country = "Thailand", ArrivalDate = new DateTime(2025, 11, 15), DepartureDate = new DateTime(2025, 11, 20), Latitude = 13.7563, Longitude = 100.5018 },
                new TripDestination { TripId = t4.Id, CityName = "Чіанг-Май", Country = "Thailand", ArrivalDate = new DateTime(2025, 11, 20), DepartureDate = new DateTime(2025, 11, 24), Latitude = 18.7883, Longitude = 98.9853 },
                new TripDestination { TripId = t4.Id, CityName = "Пхукет", Country = "Thailand", ArrivalDate = new DateTime(2025, 11, 24), DepartureDate = new DateTime(2025, 11, 30), Latitude = 7.8804, Longitude = 98.3923 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t4.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Бангкок (BKK)", DepartureTime = new DateTime(2025, 11, 15, 2, 0, 0), ArrivalTime = new DateTime(2025, 11, 15, 18, 0, 0), CarrierInfo = "Thai Airways TG971", BookingReference = "TG-441BKK" },
                new Transit { TripId = t4.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Бангкок (DMK)", ArrivalLocation = "Чіанг-Май (CNX)", DepartureTime = new DateTime(2025, 11, 20, 8, 30, 0), ArrivalTime = new DateTime(2025, 11, 20, 9, 45, 0), CarrierInfo = "AirAsia FD3112", BookingReference = "AA-229CNX" },
                new Transit { TripId = t4.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Чіанг-Май (CNX)", ArrivalLocation = "Пхукет (HKT)", DepartureTime = new DateTime(2025, 11, 24, 11, 0, 0), ArrivalTime = new DateTime(2025, 11, 24, 12, 30, 0), CarrierInfo = "Nok Air DD9210", BookingReference = "NK-881HKT" },
                new Transit { TripId = t4.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Пхукет (HKT)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2025, 11, 30, 1, 0, 0), ArrivalTime = new DateTime(2025, 11, 30, 9, 0, 0), CarrierInfo = "Thai Airways TG972", BookingReference = "TG-442KBP" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t4.Id, BookingStatusId = bConfirmed, Name = "Mandarin Oriental Bangkok", Address = "48 Oriental Avenue, Bang Rak, Bangkok 10500", CheckInTime = new DateTime(2025, 11, 15, 14, 0, 0), CheckOutTime = new DateTime(2025, 11, 20, 11, 0, 0), BookingReference = "MO-BKK-881", ContactPhone = "+66 2 659 9000", Latitude = 13.7234, Longitude = 100.5157 },
                new Accommodation { TripId = t4.Id, BookingStatusId = bConfirmed, Name = "137 Pillars House Chiang Mai", Address = "2 Nawatgate Road, Watgate, Muang, Chiang Mai 50000", CheckInTime = new DateTime(2025, 11, 20, 15, 0, 0), CheckOutTime = new DateTime(2025, 11, 24, 11, 0, 0), BookingReference = "137P-441", ContactPhone = "+66 53 247 788", Latitude = 18.7953, Longitude = 99.0067 },
                new Accommodation { TripId = t4.Id, BookingStatusId = bConfirmed, Name = "Trisara Phuket", Address = "60/1 Moo 6, Srissonthorn Road, Phuket 83110", CheckInTime = new DateTime(2025, 11, 24, 14, 0, 0), CheckOutTime = new DateTime(2025, 11, 30, 11, 0, 0), BookingReference = "TRS-PKT-220", ContactPhone = "+66 76 310 100", Latitude = 7.9915, Longitude = 98.2769 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t4.Id, BookingStatusId = bConfirmed, Title = "Храм Ват Пхо + Смарагдовий Будда", Address = "2 Sanam Chai Road, Bangkok", StartTime = new DateTime(2025, 11, 16, 8, 0, 0), EndTime = new DateTime(2025, 11, 16, 12, 0, 0), Notes = "Вхід 200 THB. Коліна та плечі мають бути закриті" },
                new TripActivity { TripId = t4.Id, BookingStatusId = bConfirmed, Title = "Вечірній ринок Chatuchak", Address = "587/10 Kamphaeng Phet 2 Road, Bangkok", StartTime = new DateTime(2025, 11, 17, 17, 0, 0), EndTime = new DateTime(2025, 11, 17, 22, 0, 0), Notes = "15 000 крамниць — прийти з картою і без поспіху" },
                new TripActivity { TripId = t4.Id, BookingStatusId = bConfirmed, Title = "Слонячий заповідник Elephant Nature Park", Address = "Kuet Chang, Mae Taeng District, Chiang Mai", StartTime = new DateTime(2025, 11, 21, 8, 0, 0), EndTime = new DateTime(2025, 11, 21, 17, 0, 0), Notes = "Тільки гуманний парк без катання! $80, включає обід" },
                new TripActivity { TripId = t4.Id, BookingStatusId = bConfirmed, Title = "Куlinарні курси тайської кухні", Address = "Zabb E-Lee Thai Cooking School, Chiang Mai", StartTime = new DateTime(2025, 11, 22, 9, 0, 0), EndTime = new DateTime(2025, 11, 22, 14, 0, 0), Notes = "Ринок + готуємо 5 страв. 1200 THB" },
                new TripActivity { TripId = t4.Id, BookingStatusId = bConfirmed, Title = "Острів Phi Phi — одноденний тур", Address = "Phi Phi Islands, Krabi", StartTime = new DateTime(2025, 11, 26, 8, 0, 0), EndTime = new DateTime(2025, 11, 26, 20, 0, 0), Notes = "Спідбот тур 1800 THB, сонцезахисний крем обов'язково" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t4.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Бангкок–Київ", TotalAmount = 28000, Currency = "THB", Date = new DateTime(2025, 11, 15), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cTransport, Title = "Внутрішні перельоти (x3)", TotalAmount = 8500, Currency = "THB", Date = new DateTime(2025, 11, 20), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cAccom, Title = "Mandarin Oriental BKK (5 ночей)", TotalAmount = 75000, Currency = "THB", Date = new DateTime(2025, 11, 15), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cAccom, Title = "137 Pillars Chiang Mai (4 ночі)", TotalAmount = 32000, Currency = "THB", Date = new DateTime(2025, 11, 20), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cEntertain, Title = "Elephant Nature Park", TotalAmount = 2800, Currency = "THB", Date = new DateTime(2025, 11, 21), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cEntertain, Title = "Phi Phi спідбот тур", TotalAmount = 1800, Currency = "THB", Date = new DateTime(2025, 11, 26), PayerId = uid },
                new Expense { TripId = t4.Id, CategoryId = cFood, Title = "Вуличний стріт-фуд Бангкок", TotalAmount = 3200, Currency = "THB", Date = new DateTime(2025, 11, 17), PayerId = uid }
            );
            var cl4 = new Checklist { TripId = t4.Id, Title = "Таїланд — здоров'я та безпека" };
            db.Checklists.Add(cl4); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl4.Id, Content = "Вакцинація від гепатиту А та тифу", IsChecked = true },
                new ChecklistItem { ChecklistId = cl4.Id, Content = "Репелент від комарів (DEET 30%+)", IsChecked = true },
                new ChecklistItem { ChecklistId = cl4.Id, Content = "Сонцезахисний крем SPF 50+", IsChecked = true },
                new ChecklistItem { ChecklistId = cl4.Id, Content = "Таблетки від малярії", IsChecked = true },
                new ChecklistItem { ChecklistId = cl4.Id, Content = "Спортивний одяг що покриває плечі", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 5. Іспанія: Мадрид → Севілья → Барселона  (завершена)
            // ══════════════════════════════════════════════════════════════════════
            var t5 = new Trip
            {
                Title = "Іспанія: Мадрид, Севілья, Барселона",
                Description = "Фламенко, гауді, сангрія та Прадо.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2026, 2, 20),
                EndDate = new DateTime(2026, 3, 5),
                BaseCurrency = "EUR",
                CreatorId = uid,
                StatusId = sCompleted,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t5); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t5.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t5.Id, CityName = "Мадрид", Country = "Spain", ArrivalDate = new DateTime(2026, 2, 20), DepartureDate = new DateTime(2026, 2, 24), Latitude = 40.4168, Longitude = -3.7038 },
                new TripDestination { TripId = t5.Id, CityName = "Севілья", Country = "Spain", ArrivalDate = new DateTime(2026, 2, 24), DepartureDate = new DateTime(2026, 2, 28), Latitude = 37.3891, Longitude = -5.9845 },
                new TripDestination { TripId = t5.Id, CityName = "Барселона", Country = "Spain", ArrivalDate = new DateTime(2026, 2, 28), DepartureDate = new DateTime(2026, 3, 5), Latitude = 41.3851, Longitude = 2.1734 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t5.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Мадрид (MAD)", DepartureTime = new DateTime(2026, 2, 20, 7, 0, 0), ArrivalTime = new DateTime(2026, 2, 20, 10, 30, 0), CarrierInfo = "Iberia IB3163", BookingReference = "IB-554MAD" },
                new Transit { TripId = t5.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Madrid Atocha", ArrivalLocation = "Sevilla Santa Justa", DepartureTime = new DateTime(2026, 2, 24, 9, 0, 0), ArrivalTime = new DateTime(2026, 2, 24, 11, 30, 0), CarrierInfo = "Renfe AVE (300 km/h)", BookingReference = "AVE-221SEV" },
                new Transit { TripId = t5.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Севілья (SVQ)", ArrivalLocation = "Барселона (BCN)", DepartureTime = new DateTime(2026, 2, 28, 8, 0, 0), ArrivalTime = new DateTime(2026, 2, 28, 9, 30, 0), CarrierInfo = "Vueling VY2401", BookingReference = "VY-881BCN" },
                new Transit { TripId = t5.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Барселона (BCN)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2026, 3, 5, 16, 0, 0), ArrivalTime = new DateTime(2026, 3, 5, 21, 30, 0), CarrierInfo = "WizzAir W63302", BookingReference = "WZZ-554KBP" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t5.Id, BookingStatusId = bConfirmed, Name = "Hotel Villa Magna Madrid", Address = "Paseo de la Castellana 22, 28046 Madrid", CheckInTime = new DateTime(2026, 2, 20, 15, 0, 0), CheckOutTime = new DateTime(2026, 2, 24, 11, 0, 0), BookingReference = "VM-441MAD", Latitude = 40.4254, Longitude = -3.6869 },
                new Accommodation { TripId = t5.Id, BookingStatusId = bConfirmed, Name = "Hotel Alfonso XIII Seville", Address = "Calle San Fernando 2, 41004 Sevilla", CheckInTime = new DateTime(2026, 2, 24, 14, 0, 0), CheckOutTime = new DateTime(2026, 2, 28, 11, 0, 0), BookingReference = "A13-220SEV", Latitude = 37.3823, Longitude = -5.9938 },
                new Accommodation { TripId = t5.Id, BookingStatusId = bConfirmed, Name = "Hotel Arts Barcelona", Address = "Carrer de la Marina 19-21, 08005 Barcelona", CheckInTime = new DateTime(2026, 2, 28, 15, 0, 0), CheckOutTime = new DateTime(2026, 3, 5, 11, 0, 0), BookingReference = "ART-330BCN", Latitude = 41.3878, Longitude = 2.1970 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t5.Id, BookingStatusId = bConfirmed, Title = "Музей Прадо — шедеври Гойї", Address = "Paseo del Prado s/n, 28014 Madrid", StartTime = new DateTime(2026, 2, 21, 10, 0, 0), EndTime = new DateTime(2026, 2, 21, 14, 0, 0), Notes = "€15, безкоштовно 18:00-20:00 в будні" },
                new TripActivity { TripId = t5.Id, BookingStatusId = bConfirmed, Title = "Фламенко шоу Casa Patas", Address = "Cañizares 10, 28012 Madrid", StartTime = new DateTime(2026, 2, 22, 21, 0, 0), EndTime = new DateTime(2026, 2, 22, 23, 0, 0), Notes = "Найавтентичніше фламенко Мадриду, €35 + консумація" },
                new TripActivity { TripId = t5.Id, BookingStatusId = bConfirmed, Title = "Алькасар Севільї", Address = "Patio de Banderas s/n, 41004 Sevilla", StartTime = new DateTime(2026, 2, 25, 9, 30, 0), EndTime = new DateTime(2026, 2, 25, 12, 30, 0), Notes = "€13.50, бронювати онлайн за тиждень" },
                new TripActivity { TripId = t5.Id, BookingStatusId = bConfirmed, Title = "Саграда Фамілія + Башти", Address = "C/ de Mallorca 401, 08013 Barcelona", StartTime = new DateTime(2026, 3, 1, 9, 0, 0), EndTime = new DateTime(2026, 3, 1, 12, 0, 0), Notes = "€26 з башнями. Квитки за 2 місяці — продаються миттєво" },
                new TripActivity { TripId = t5.Id, BookingStatusId = bConfirmed, Title = "Парк Гуель + Каса Батльо", Address = "08024 Barcelona", StartTime = new DateTime(2026, 3, 2, 10, 0, 0), EndTime = new DateTime(2026, 3, 2, 15, 0, 0), Notes = "Парк Гуель €10, Каса Батльо €29" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t5.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Мадрид", TotalAmount = 160, Currency = "EUR", Date = new DateTime(2026, 2, 20), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cTransport, Title = "AVE Мадрид–Севілья", TotalAmount = 55, Currency = "EUR", Date = new DateTime(2026, 2, 24), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cAccom, Title = "Villa Magna Madrid (4 ночі)", TotalAmount = 960, Currency = "EUR", Date = new DateTime(2026, 2, 20), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cAccom, Title = "Alfonso XIII Seville (4 ночі)", TotalAmount = 1100, Currency = "EUR", Date = new DateTime(2026, 2, 24), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cEntertain, Title = "Фламенко шоу Madrid", TotalAmount = 35, Currency = "EUR", Date = new DateTime(2026, 2, 22), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cEntertain, Title = "Саграда Фамілія (x1)", TotalAmount = 26, Currency = "EUR", Date = new DateTime(2026, 3, 1), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cFood, Title = "Tapas El Lateral Madrid", TotalAmount = 65, Currency = "EUR", Date = new DateTime(2026, 2, 21), PayerId = uid },
                new Expense { TripId = t5.Id, CategoryId = cFood, Title = "Paella La Mar Salada BCN", TotalAmount = 78, Currency = "EUR", Date = new DateTime(2026, 3, 3), PayerId = uid }
            );
            var cl5 = new Checklist { TripId = t5.Id, Title = "Іспанія — must do" };
            db.Checklists.Add(cl5); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl5.Id, Content = "Саграда Фамілія — квитки за 2 місяці", IsChecked = true },
                new ChecklistItem { ChecklistId = cl5.Id, Content = "AVE потяг — бронювати за 60 днів", IsChecked = true },
                new ChecklistItem { ChecklistId = cl5.Id, Content = "Алькасар Севілья — онлайн бронь", IsChecked = true },
                new ChecklistItem { ChecklistId = cl5.Id, Content = "Завантажити Google Translate офлайн (іспанська)", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 6. Відень → Прага → Будапешт  (запланована, літо 2026)
            // ══════════════════════════════════════════════════════════════════════
            var t6 = new Trip
            {
                Title = "Центральна Європа: Відень, Прага, Будапешт",
                Description = "Дунайська класика — три столиці за 2 тижні.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2026, 7, 10),
                EndDate = new DateTime(2026, 7, 24),
                BaseCurrency = "EUR",
                CreatorId = uid,
                StatusId = sPlanned,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t6); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t6.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t6.Id, CityName = "Відень", Country = "Austria", ArrivalDate = new DateTime(2026, 7, 10), DepartureDate = new DateTime(2026, 7, 15), Latitude = 48.2082, Longitude = 16.3738 },
                new TripDestination { TripId = t6.Id, CityName = "Прага", Country = "Czech Republic", ArrivalDate = new DateTime(2026, 7, 15), DepartureDate = new DateTime(2026, 7, 20), Latitude = 50.0755, Longitude = 14.4378 },
                new TripDestination { TripId = t6.Id, CityName = "Будапешт", Country = "Hungary", ArrivalDate = new DateTime(2026, 7, 20), DepartureDate = new DateTime(2026, 7, 24), Latitude = 47.4979, Longitude = 19.0402 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t6.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Відень (VIE)", DepartureTime = new DateTime(2026, 7, 10, 7, 0, 0), ArrivalTime = new DateTime(2026, 7, 10, 9, 0, 0), CarrierInfo = "Austrian OS731", BookingReference = "AUA-710VIE" },
                new Transit { TripId = t6.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Wien Hauptbahnhof", ArrivalLocation = "Praha hl.n.", DepartureTime = new DateTime(2026, 7, 15, 8, 30, 0), ArrivalTime = new DateTime(2026, 7, 15, 12, 40, 0), CarrierInfo = "ÖBB Railjet RJ74", BookingReference = "RJ-441PRG" },
                new Transit { TripId = t6.Id, TransitTypeId = tTrain, BookingStatusId = bPending, DepartureLocation = "Praha hl.n.", ArrivalLocation = "Budapest-Keleti", DepartureTime = new DateTime(2026, 7, 20, 9, 0, 0), ArrivalTime = new DateTime(2026, 7, 20, 13, 30, 0), CarrierInfo = "RegioJet SC504" },
                new Transit { TripId = t6.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Будапешт (BUD)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2026, 7, 24, 18, 0, 0), ArrivalTime = new DateTime(2026, 7, 24, 21, 30, 0), CarrierInfo = "WizzAir W67701" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t6.Id, BookingStatusId = bConfirmed, Name = "Hotel Sacher Wien", Address = "Philharmoniker Str. 4, 1010 Wien", CheckInTime = new DateTime(2026, 7, 10, 15, 0, 0), CheckOutTime = new DateTime(2026, 7, 15, 11, 0, 0), BookingReference = "SCH-710VIE", Latitude = 48.2035, Longitude = 16.3690 },
                new Accommodation { TripId = t6.Id, BookingStatusId = bConfirmed, Name = "Mandarin Oriental Prague", Address = "Nebovidská 459/1, 118 00 Praha 1", CheckInTime = new DateTime(2026, 7, 15, 15, 0, 0), CheckOutTime = new DateTime(2026, 7, 20, 11, 0, 0), BookingReference = "MO-PRG-715", Latitude = 50.0845, Longitude = 14.4042 },
                new Accommodation { TripId = t6.Id, BookingStatusId = bPending, Name = "Four Seasons Budapest", Address = "Széchenyi István tér 5-6, 1051 Budapest", CheckInTime = new DateTime(2026, 7, 20, 15, 0, 0), CheckOutTime = new DateTime(2026, 7, 24, 11, 0, 0), Latitude = 47.4981, Longitude = 19.0514 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t6.Id, BookingStatusId = bConfirmed, Title = "Палац Шенбрунн + огляд Відня", Address = "Schönbrunner Schloßstraße 47, 1130 Wien", StartTime = new DateTime(2026, 7, 11, 10, 0, 0), EndTime = new DateTime(2026, 7, 11, 14, 0, 0), Notes = "Grand Tour €23. Садиба та оглядова вежа — must" },
                new TripActivity { TripId = t6.Id, BookingStatusId = bPending, Title = "Відень Філармоніка — вечірній концерт", Address = "Musikvereinsgebäude, Musikvereinsplatz 1, Wien", StartTime = new DateTime(2026, 7, 13, 19, 30, 0), Notes = "Квитки від €50. Дрес-код обов'язковий" },
                new TripActivity { TripId = t6.Id, BookingStatusId = bNone, Title = "Карлів міст + Страговський монастир", Address = "Karlův most, Praha 1", StartTime = new DateTime(2026, 7, 16, 7, 0, 0), EndTime = new DateTime(2026, 7, 16, 11, 0, 0), Notes = "До 8:00 — без туристів, найкраще освітлення" },
                new TripActivity { TripId = t6.Id, BookingStatusId = bNone, Title = "Купальні Сечені Будапешт", Address = "Állatkerti körút 9-11, 1146 Budapest", StartTime = new DateTime(2026, 7, 21, 10, 0, 0), EndTime = new DateTime(2026, 7, 21, 15, 0, 0), Notes = "Вхід 8200 HUF. Взяти шапочку для басейну" },
                new TripActivity { TripId = t6.Id, BookingStatusId = bNone, Title = "Прогулянка на кораблі по Дунаю", Address = "Vigadó tér pier, Budapest", StartTime = new DateTime(2026, 7, 22, 20, 0, 0), EndTime = new DateTime(2026, 7, 22, 22, 0, 0), Notes = "Вечірній круїз з вечерею €45, Парламент вночі — неймовірно" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t6.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Відень", TotalAmount = 120, Currency = "EUR", Date = new DateTime(2026, 7, 10), PayerId = uid },
                new Expense { TripId = t6.Id, CategoryId = cTransport, Title = "ÖBB Railjet Відень–Прага", TotalAmount = 49, Currency = "EUR", Date = new DateTime(2026, 7, 15), PayerId = uid },
                new Expense { TripId = t6.Id, CategoryId = cAccom, Title = "Hotel Sacher Wien (5 ночей)", TotalAmount = 1450, Currency = "EUR", Date = new DateTime(2026, 7, 10), PayerId = uid },
                new Expense { TripId = t6.Id, CategoryId = cAccom, Title = "Mandarin Oriental Prague (5 ночей)", TotalAmount = 1200, Currency = "EUR", Date = new DateTime(2026, 7, 15), PayerId = uid },
                new Expense { TripId = t6.Id, CategoryId = cEntertain, Title = "Шенбрунн Grand Tour", TotalAmount = 23, Currency = "EUR", Date = new DateTime(2026, 7, 11), PayerId = uid }
            );
            var cl6 = new Checklist { TripId = t6.Id, Title = "Центральна Європа — план" };
            db.Checklists.Add(cl6); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl6.Id, Content = "Квитки на Філармоніку Відень", IsChecked = false },
                new ChecklistItem { ChecklistId = cl6.Id, Content = "Зворотній рейс Будапешт–Київ", IsChecked = false },
                new ChecklistItem { ChecklistId = cl6.Id, Content = "Four Seasons Budapest — підтвердити", IsChecked = false },
                new ChecklistItem { ChecklistId = cl6.Id, Content = "RegioJet Прага–Будапешт — купити", IsChecked = false },
                new ChecklistItem { ChecklistId = cl6.Id, Content = "Страховка Schengen", IsChecked = true }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 7. Греція: Афіни → Санторіні → Крит  (запланована, серпень 2026)
            // ══════════════════════════════════════════════════════════════════════
            var t7 = new Trip
            {
                Title = "Греція: Афіни, Санторіні, Крит",
                Description = "Егейське море, білі будинки та оливковий закат.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2026, 8, 20),
                EndDate = new DateTime(2026, 9, 3),
                BaseCurrency = "EUR",
                CreatorId = uid,
                StatusId = sPlanned,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t7); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t7.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t7.Id, CityName = "Афіни", Country = "Greece", ArrivalDate = new DateTime(2026, 8, 20), DepartureDate = new DateTime(2026, 8, 24), Latitude = 37.9838, Longitude = 23.7275 },
                new TripDestination { TripId = t7.Id, CityName = "Санторіні", Country = "Greece", ArrivalDate = new DateTime(2026, 8, 24), DepartureDate = new DateTime(2026, 8, 29), Latitude = 36.3932, Longitude = 25.4615 },
                new TripDestination { TripId = t7.Id, CityName = "Іракліон (Крит)", Country = "Greece", ArrivalDate = new DateTime(2026, 8, 29), DepartureDate = new DateTime(2026, 9, 3), Latitude = 35.3387, Longitude = 25.1442 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t7.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Афіни (ATH)", DepartureTime = new DateTime(2026, 8, 20, 6, 0, 0), ArrivalTime = new DateTime(2026, 8, 20, 9, 30, 0), CarrierInfo = "Aegean Airlines A3601", BookingReference = "AEG-820ATH" },
                new Transit { TripId = t7.Id, TransitTypeId = tFerry, BookingStatusId = bConfirmed, DepartureLocation = "Афіни (Пірей)", ArrivalLocation = "Санторіні (Тіра)", DepartureTime = new DateTime(2026, 8, 24, 7, 30, 0), ArrivalTime = new DateTime(2026, 8, 24, 13, 30, 0), CarrierInfo = "SeaJets Paros Jet", BookingReference = "SJ-441SAN" },
                new Transit { TripId = t7.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Санторіні (JTR)", ArrivalLocation = "Іракліон (HER)", DepartureTime = new DateTime(2026, 8, 29, 11, 0, 0), ArrivalTime = new DateTime(2026, 8, 29, 11, 50, 0), CarrierInfo = "Sky Express GQ250", BookingReference = "GQ-829HER" },
                new Transit { TripId = t7.Id, TransitTypeId = tFlight, BookingStatusId = bPending, DepartureLocation = "Іракліон (HER)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2026, 9, 3, 16, 0, 0), ArrivalTime = new DateTime(2026, 9, 3, 20, 30, 0), CarrierInfo = "WizzAir W61703" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t7.Id, BookingStatusId = bConfirmed, Name = "Hotel Grande Bretagne Athens", Address = "1 Vasileos Georgiou A Str, 10564 Athens", CheckInTime = new DateTime(2026, 8, 20, 15, 0, 0), CheckOutTime = new DateTime(2026, 8, 24, 11, 0, 0), BookingReference = "GB-820ATH", Latitude = 37.9761, Longitude = 23.7350 },
                new Accommodation { TripId = t7.Id, BookingStatusId = bConfirmed, Name = "Canaves Oia Epitome", Address = "Oia, 847 02 Santorini", CheckInTime = new DateTime(2026, 8, 24, 15, 0, 0), CheckOutTime = new DateTime(2026, 8, 29, 11, 0, 0), BookingReference = "COE-824SAN", Latitude = 36.4618, Longitude = 25.3753 },
                new Accommodation { TripId = t7.Id, BookingStatusId = bPending, Name = "Domes Noruz Crete", Address = "Agia Pelagia, 715 00 Crete", CheckInTime = new DateTime(2026, 8, 29, 14, 0, 0), CheckOutTime = new DateTime(2026, 9, 3, 11, 0, 0), Latitude = 35.3804, Longitude = 24.9994 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t7.Id, BookingStatusId = bConfirmed, Title = "Акрополь та Парфенон", Address = "Dionysiou Areopagitou, Athens 117 42", StartTime = new DateTime(2026, 8, 21, 8, 0, 0), EndTime = new DateTime(2026, 8, 21, 11, 0, 0), Notes = "€20. Йти до 9:00 — до спеки та натовпу" },
                new TripActivity { TripId = t7.Id, BookingStatusId = bConfirmed, Title = "Захід сонця в Ойя (Санторіні)", Address = "Oia, Santorini", StartTime = new DateTime(2026, 8, 25, 19, 30, 0), EndTime = new DateTime(2026, 8, 25, 21, 0, 0), Notes = "Найвідоміший закат Греції — прийти за 1 год щоб зайняти місце" },
                new TripActivity { TripId = t7.Id, BookingStatusId = bNone, Title = "Вулканічний острів Неа Камені", Address = "Nea Kameni, Santorini", StartTime = new DateTime(2026, 8, 26, 9, 0, 0), EndTime = new DateTime(2026, 8, 26, 14, 0, 0), Notes = "Тур з катера €20, купання в гарячих джерелах" },
                new TripActivity { TripId = t7.Id, BookingStatusId = bNone, Title = "Кносський палац (Крит)", Address = "Knossou, 714 09 Heraklion", StartTime = new DateTime(2026, 8, 30, 9, 0, 0), EndTime = new DateTime(2026, 8, 30, 13, 0, 0), Notes = "Мінойська цивілізація, €15. Аудіогід рекомендується" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t7.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Афіни", TotalAmount = 140, Currency = "EUR", Date = new DateTime(2026, 8, 20), PayerId = uid },
                new Expense { TripId = t7.Id, CategoryId = cTransport, Title = "Паром Афіни–Санторіні", TotalAmount = 65, Currency = "EUR", Date = new DateTime(2026, 8, 24), PayerId = uid },
                new Expense { TripId = t7.Id, CategoryId = cAccom, Title = "Grande Bretagne Athens (4 ночі)", TotalAmount = 1200, Currency = "EUR", Date = new DateTime(2026, 8, 20), PayerId = uid },
                new Expense { TripId = t7.Id, CategoryId = cAccom, Title = "Canaves Oia (5 ночей)", TotalAmount = 3500, Currency = "EUR", Date = new DateTime(2026, 8, 24), PayerId = uid },
                new Expense { TripId = t7.Id, CategoryId = cEntertain, Title = "Акрополь — вхід", TotalAmount = 20, Currency = "EUR", Date = new DateTime(2026, 8, 21), PayerId = uid }
            );
            var cl7 = new Checklist { TripId = t7.Id, Title = "Греція — підготовка" };
            db.Checklists.Add(cl7); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl7.Id, Content = "Готель Крит — підтвердити бронь", IsChecked = false },
                new ChecklistItem { ChecklistId = cl7.Id, Content = "Зворотній рейс з Криту", IsChecked = false },
                new ChecklistItem { ChecklistId = cl7.Id, Content = "Сонцезахисний крем SPF 50+ (Санторіні = сильне сонце)", IsChecked = false },
                new ChecklistItem { ChecklistId = cl7.Id, Content = "Страховка Schengen", IsChecked = true },
                new ChecklistItem { ChecklistId = cl7.Id, Content = "Орендувати ATV на Криті", IsChecked = false }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 8. Марокко: Марракеш → Фес → Шефшауен  (запланована, жовтень 2026)
            // ══════════════════════════════════════════════════════════════════════
            var t8 = new Trip
            {
                Title = "Марокко: Марракеш, Фес, Шефшауен",
                Description = "Медини, спеції, синє місто та Сахара.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2026, 10, 5),
                EndDate = new DateTime(2026, 10, 17),
                BaseCurrency = "MAD",
                CreatorId = uid,
                StatusId = sPlanned,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t8); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t8.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t8.Id, CityName = "Марракеш", Country = "Morocco", ArrivalDate = new DateTime(2026, 10, 5), DepartureDate = new DateTime(2026, 10, 9), Latitude = 31.6295, Longitude = -7.9811 },
                new TripDestination { TripId = t8.Id, CityName = "Фес", Country = "Morocco", ArrivalDate = new DateTime(2026, 10, 9), DepartureDate = new DateTime(2026, 10, 13), Latitude = 34.0181, Longitude = -5.0078 },
                new TripDestination { TripId = t8.Id, CityName = "Шефшауен", Country = "Morocco", ArrivalDate = new DateTime(2026, 10, 13), DepartureDate = new DateTime(2026, 10, 17), Latitude = 35.1688, Longitude = -5.2636 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t8.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Марракеш (RAK)", DepartureTime = new DateTime(2026, 10, 5, 5, 0, 0), ArrivalTime = new DateTime(2026, 10, 5, 9, 30, 0), CarrierInfo = "Royal Air Maroc AT922", BookingReference = "RAM-1005MRK" },
                new Transit { TripId = t8.Id, TransitTypeId = tBus, BookingStatusId = bConfirmed, DepartureLocation = "Марракеш, Gare Routière", ArrivalLocation = "Фес, CTM Station", DepartureTime = new DateTime(2026, 10, 9, 7, 0, 0), ArrivalTime = new DateTime(2026, 10, 9, 14, 30, 0), CarrierInfo = "CTM Bus (кондиціонер)", BookingReference = "CTM-441FES" },
                new Transit { TripId = t8.Id, TransitTypeId = tBus, BookingStatusId = bNone, DepartureLocation = "Фес", ArrivalLocation = "Шефшауен", DepartureTime = new DateTime(2026, 10, 13, 8, 0, 0), ArrivalTime = new DateTime(2026, 10, 13, 11, 30, 0), CarrierInfo = "CTM або місцевий автобус" },
                new Transit { TripId = t8.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Танжер (TNG)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2026, 10, 17, 14, 0, 0), ArrivalTime = new DateTime(2026, 10, 17, 20, 30, 0), CarrierInfo = "Ryanair або Transavia" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t8.Id, BookingStatusId = bConfirmed, Name = "La Mamounia Marrakech", Address = "Avenue Bab Jdid, 40040 Marrakech", CheckInTime = new DateTime(2026, 10, 5, 15, 0, 0), CheckOutTime = new DateTime(2026, 10, 9, 11, 0, 0), BookingReference = "LM-1005MAR", Latitude = 31.6219, Longitude = -8.0076 },
                new Accommodation { TripId = t8.Id, BookingStatusId = bConfirmed, Name = "Riad Fes Maya", Address = "1 Derb Idrissy Zouak, Fès el-Bali, Fes", CheckInTime = new DateTime(2026, 10, 9, 14, 0, 0), CheckOutTime = new DateTime(2026, 10, 13, 10, 0, 0), BookingReference = "RFM-441FES", Latitude = 34.0649, Longitude = -4.9795 },
                new Accommodation { TripId = t8.Id, BookingStatusId = bNone, Name = "Lina Ryad & Spa Chefchaouen", Address = "Rue Tariq Ibn Ziad, Chefchaouen 91000", CheckInTime = new DateTime(2026, 10, 13, 13, 0, 0), CheckOutTime = new DateTime(2026, 10, 17, 10, 0, 0), Latitude = 35.1707, Longitude = -5.2686 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t8.Id, BookingStatusId = bConfirmed, Title = "Джемаа-ель-Фна — нічний ринок", Address = "Place Jemaa El Fna, Marrakech", StartTime = new DateTime(2026, 10, 6, 20, 0, 0), EndTime = new DateTime(2026, 10, 6, 23, 30, 0), Notes = "Казочки, факіри, їжа. Торгуватись на все!" },
                new TripActivity { TripId = t8.Id, BookingStatusId = bConfirmed, Title = "Садки Менара та Мечеть Кутубія", Address = "Avenue de la Menara, Marrakech", StartTime = new DateTime(2026, 10, 7, 9, 0, 0), EndTime = new DateTime(2026, 10, 7, 12, 0, 0), Notes = "Вхід у мечеть лише мусульманам, зовнішній огляд безкоштовний" },
                new TripActivity { TripId = t8.Id, BookingStatusId = bConfirmed, Title = "Мандрівка по медині Фесу з гідом", Address = "Fès el-Bali, Fes", StartTime = new DateTime(2026, 10, 10, 9, 0, 0), EndTime = new DateTime(2026, 10, 10, 14, 0, 0), Notes = "Без гіда — заблукаєш гарантовано. 200 MAD/год" },
                new TripActivity { TripId = t8.Id, BookingStatusId = bNone, Title = "Синє місто — фотопрогулянка", Address = "Chefchaouen Medina, Blue City", StartTime = new DateTime(2026, 10, 14, 7, 0, 0), EndTime = new DateTime(2026, 10, 14, 11, 0, 0), Notes = "Найкраще світло о 7-9 ранку. Без туристів!" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t8.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Марракеш", TotalAmount = 2800, Currency = "MAD", Date = new DateTime(2026, 10, 5), PayerId = uid },
                new Expense { TripId = t8.Id, CategoryId = cTransport, Title = "CTM автобус Марракеш–Фес", TotalAmount = 180, Currency = "MAD", Date = new DateTime(2026, 10, 9), PayerId = uid },
                new Expense { TripId = t8.Id, CategoryId = cAccom, Title = "La Mamounia (4 ночі)", TotalAmount = 12000, Currency = "MAD", Date = new DateTime(2026, 10, 5), PayerId = uid },
                new Expense { TripId = t8.Id, CategoryId = cAccom, Title = "Riad Fes Maya (4 ночі)", TotalAmount = 2400, Currency = "MAD", Date = new DateTime(2026, 10, 9), PayerId = uid },
                new Expense { TripId = t8.Id, CategoryId = cFood, Title = "Вечеря Dar Moha Marrakech", TotalAmount = 850, Currency = "MAD", Date = new DateTime(2026, 10, 7), PayerId = uid },
                new Expense { TripId = t8.Id, CategoryId = cShopping, Title = "Прянощі та килими у медині", TotalAmount = 1200, Currency = "MAD", Date = new DateTime(2026, 10, 6), PayerId = uid }
            );
            var cl8 = new Checklist { TripId = t8.Id, Title = "Марокко — важливо" };
            db.Checklists.Add(cl8); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl8.Id, Content = "Зворотній рейс з Танжера — купити", IsChecked = false },
                new ChecklistItem { ChecklistId = cl8.Id, Content = "Готель Шефшауен — забронювати", IsChecked = false },
                new ChecklistItem { ChecklistId = cl8.Id, Content = "Взяти готівку MAD (картки рідко беруть)", IsChecked = false },
                new ChecklistItem { ChecklistId = cl8.Id, Content = "Скромний одяг для медини (плечі+коліна)", IsChecked = false },
                new ChecklistItem { ChecklistId = cl8.Id, Content = "Завантажити Maps.me офлайн Марокко", IsChecked = false }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 9. Норвегія: Осло → Берген → Фіорди  (запланована, зима 2026)
            // ══════════════════════════════════════════════════════════════════════
            var t9 = new Trip
            {
                Title = "Норвегія: Полярне сяйво та фіорди",
                Description = "Осло, Bergen, Flåm — полярне сяйво та засніжені фіорди.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2026, 12, 26),
                EndDate = new DateTime(2027, 1, 4),
                BaseCurrency = "NOK",
                CreatorId = uid,
                StatusId = sPlanned,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t9); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t9.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t9.Id, CityName = "Осло", Country = "Norway", ArrivalDate = new DateTime(2026, 12, 26), DepartureDate = new DateTime(2026, 12, 29), Latitude = 59.9139, Longitude = 10.7522 },
                new TripDestination { TripId = t9.Id, CityName = "Берген", Country = "Norway", ArrivalDate = new DateTime(2026, 12, 29), DepartureDate = new DateTime(2027, 1, 2), Latitude = 60.3913, Longitude = 5.3221 },
                new TripDestination { TripId = t9.Id, CityName = "Флом (Согне-фіорд)", Country = "Norway", ArrivalDate = new DateTime(2027, 1, 2), DepartureDate = new DateTime(2027, 1, 4), Latitude = 60.8632, Longitude = 7.1151 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t9.Id, TransitTypeId = tFlight, BookingStatusId = bConfirmed, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Осло (OSL)", DepartureTime = new DateTime(2026, 12, 26, 8, 0, 0), ArrivalTime = new DateTime(2026, 12, 26, 11, 30, 0), CarrierInfo = "SAS SK4765", BookingReference = "SAS-1226OSL" },
                new Transit { TripId = t9.Id, TransitTypeId = tTrain, BookingStatusId = bConfirmed, DepartureLocation = "Oslo Sentralstasjon", ArrivalLocation = "Bergen", DepartureTime = new DateTime(2026, 12, 29, 8, 5, 0), ArrivalTime = new DateTime(2026, 12, 29, 14, 58, 0), CarrierInfo = "NSB Bergen Railway (найгарніший потяг Норвегії)", BookingReference = "NSB-BRG-441" },
                new Transit { TripId = t9.Id, TransitTypeId = tFerry, BookingStatusId = bPending, DepartureLocation = "Bergen", ArrivalLocation = "Флом", DepartureTime = new DateTime(2027, 1, 2, 10, 0, 0), ArrivalTime = new DateTime(2027, 1, 2, 14, 0, 0), CarrierInfo = "Norled Hardangerfjord" },
                new Transit { TripId = t9.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Берген (BGO)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2027, 1, 4, 16, 0, 0), ArrivalTime = new DateTime(2027, 1, 4, 21, 0, 0), CarrierInfo = "Norwegian DY4401" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t9.Id, BookingStatusId = bConfirmed, Name = "The Thief Oslo", Address = "Landgangen 1, 0252 Oslo", CheckInTime = new DateTime(2026, 12, 26, 15, 0, 0), CheckOutTime = new DateTime(2026, 12, 29, 11, 0, 0), BookingReference = "TT-1226OSL", Latitude = 59.9063, Longitude = 10.7205 },
                new Accommodation { TripId = t9.Id, BookingStatusId = bConfirmed, Name = "Opus XVI Bergen", Address = "C. Sundts Gate 18, 5004 Bergen", CheckInTime = new DateTime(2026, 12, 29, 15, 0, 0), CheckOutTime = new DateTime(2027, 1, 2, 11, 0, 0), BookingReference = "OPUS-1229BRG", Latitude = 60.3924, Longitude = 5.3245 },
                new Accommodation { TripId = t9.Id, BookingStatusId = bNone, Name = "Fretheim Hotel Flåm", Address = "Flåm, 5743 Norway", CheckInTime = new DateTime(2027, 1, 2, 16, 0, 0), CheckOutTime = new DateTime(2027, 1, 4, 10, 0, 0), Latitude = 60.8632, Longitude = 7.1151 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t9.Id, BookingStatusId = bConfirmed, Title = "Музей Вікінгів + фіорд-сафарі", Address = "Huk Aveny 35, 0287 Oslo", StartTime = new DateTime(2026, 12, 27, 10, 0, 0), EndTime = new DateTime(2026, 12, 27, 14, 0, 0), Notes = "€18, дивовижні кораблі вікінгів X ст." },
                new TripActivity { TripId = t9.Id, BookingStatusId = bNone, Title = "Полярне сяйво — тур за місто", Address = "Around Oslo/Bergen", StartTime = new DateTime(2026, 12, 28, 21, 0, 0), EndTime = new DateTime(2026, 12, 29, 1, 0, 0), Notes = "Залежить від погоди. Темне небо та мороз — необхідні умови. NOK 1500/особу" },
                new TripActivity { TripId = t9.Id, BookingStatusId = bConfirmed, Title = "Фунікулер Fløibanen + огляд Бергену", Address = "Vetrlidsallmenningen 23A, Bergen", StartTime = new DateTime(2026, 12, 30, 10, 0, 0), EndTime = new DateTime(2026, 12, 30, 13, 0, 0), Notes = "NOK 200 туди-назад. Вид на всі сім гір Бергену" },
                new TripActivity { TripId = t9.Id, BookingStatusId = bNone, Title = "Flåm Railway — найкрасивіша залізниця", Address = "Flåm Station, 5743 Flåm", StartTime = new DateTime(2027, 1, 3, 10, 0, 0), EndTime = new DateTime(2027, 1, 3, 13, 0, 0), Notes = "NOK 600 туди-назад. Водоспади, засніжені гори — неймовірно" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t9.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Осло", TotalAmount = 3200, Currency = "NOK", Date = new DateTime(2026, 12, 26), PayerId = uid },
                new Expense { TripId = t9.Id, CategoryId = cTransport, Title = "Bergen Railway Oslo–Bergen", TotalAmount = 1100, Currency = "NOK", Date = new DateTime(2026, 12, 29), PayerId = uid },
                new Expense { TripId = t9.Id, CategoryId = cAccom, Title = "The Thief Oslo (3 ночі)", TotalAmount = 8700, Currency = "NOK", Date = new DateTime(2026, 12, 26), PayerId = uid },
                new Expense { TripId = t9.Id, CategoryId = cAccom, Title = "Opus XVI Bergen (4 ночі)", TotalAmount = 9600, Currency = "NOK", Date = new DateTime(2026, 12, 29), PayerId = uid },
                new Expense { TripId = t9.Id, CategoryId = cEntertain, Title = "Музей Вікінгів Осло", TotalAmount = 200, Currency = "NOK", Date = new DateTime(2026, 12, 27), PayerId = uid },
                new Expense { TripId = t9.Id, CategoryId = cFood, Title = "Maaemo Oslo (3* Michelin)", TotalAmount = 6500, Currency = "NOK", Date = new DateTime(2026, 12, 27), PayerId = uid }
            );
            var cl9 = new Checklist { TripId = t9.Id, Title = "Норвегія зимова" };
            db.Checklists.Add(cl9); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Зворотній рейс Bergen–Kyiv", IsChecked = false },
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Готель Флом — забронювати", IsChecked = false },
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Термобілизна + термошкарпетки", IsChecked = false },
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Flåm Railway — квитки онлайн", IsChecked = false },
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Паром Берген–Флом — підтвердити", IsChecked = false },
                new ChecklistItem { ChecklistId = cl9.Id, Content = "Завантажити Aurora Forecast app", IsChecked = false }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════════
            // 10. Перу: Ліма → Куско → Мачу-Пікчу  (запланована, 2027)
            // ══════════════════════════════════════════════════════════════════════
            var t10 = new Trip
            {
                Title = "Перу: Ліма, Куско, Мачу-Пікчу",
                Description = "Інки, Анди та найтаємничіше місто світу.",
                DepartureLocation = "Київ",
                ReturnLocation = "Київ",
                StartDate = new DateTime(2027, 3, 15),
                EndDate = new DateTime(2027, 3, 30),
                BaseCurrency = "USD",
                CreatorId = uid,
                StatusId = sPlanned,
                CreatedAt = DateTime.UtcNow
            };
            db.Trips.Add(t10); await db.SaveChangesAsync();
            db.TripParticipants.Add(new TripParticipant { TripId = t10.Id, UserId = uid, RoleId = rOrganizer });
            db.TripDestinations.AddRange(
                new TripDestination { TripId = t10.Id, CityName = "Ліма", Country = "Peru", ArrivalDate = new DateTime(2027, 3, 15), DepartureDate = new DateTime(2027, 3, 18), Latitude = -12.0464, Longitude = -77.0428 },
                new TripDestination { TripId = t10.Id, CityName = "Куско", Country = "Peru", ArrivalDate = new DateTime(2027, 3, 18), DepartureDate = new DateTime(2027, 3, 25), Latitude = -13.5319, Longitude = -71.9675 },
                new TripDestination { TripId = t10.Id, CityName = "Мачу-Пікчу", Country = "Peru", ArrivalDate = new DateTime(2027, 3, 22), DepartureDate = new DateTime(2027, 3, 24), Latitude = -13.1631, Longitude = -72.5450 }
            );
            db.Transits.AddRange(
                new Transit { TripId = t10.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Київ (KBP)", ArrivalLocation = "Ліма (LIM)", DepartureTime = new DateTime(2027, 3, 15, 9, 0, 0), ArrivalTime = new DateTime(2027, 3, 16, 5, 0, 0), CarrierInfo = "LATAM Airlines via Madrid" },
                new Transit { TripId = t10.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Ліма (LIM)", ArrivalLocation = "Куско (CUZ)", DepartureTime = new DateTime(2027, 3, 18, 6, 0, 0), ArrivalTime = new DateTime(2027, 3, 18, 7, 15, 0), CarrierInfo = "LATAM PE221" },
                new Transit { TripId = t10.Id, TransitTypeId = tTrain, BookingStatusId = bNone, DepartureLocation = "Куско (Poroy)", ArrivalLocation = "Агуас-Кальєнтес (Мачу-Пікчу)", DepartureTime = new DateTime(2027, 3, 22, 6, 0, 0), ArrivalTime = new DateTime(2027, 3, 22, 9, 22, 0), CarrierInfo = "PeruRail Vistadome" },
                new Transit { TripId = t10.Id, TransitTypeId = tTrain, BookingStatusId = bNone, DepartureLocation = "Агуас-Кальєнтес", ArrivalLocation = "Куско", DepartureTime = new DateTime(2027, 3, 24, 15, 0, 0), ArrivalTime = new DateTime(2027, 3, 24, 19, 0, 0), CarrierInfo = "PeruRail Expedition" },
                new Transit { TripId = t10.Id, TransitTypeId = tFlight, BookingStatusId = bNone, DepartureLocation = "Ліма (LIM)", ArrivalLocation = "Київ (KBP)", DepartureTime = new DateTime(2027, 3, 30, 22, 0, 0), ArrivalTime = new DateTime(2027, 4, 1, 16, 0, 0), CarrierInfo = "LATAM + Iberia via Madrid" }
            );
            db.Accommodations.AddRange(
                new Accommodation { TripId = t10.Id, BookingStatusId = bNone, Name = "Belmond Miraflores Park Lima", Address = "Av. Malecón de la Reserva 1035, Lima 15074", CheckInTime = new DateTime(2027, 3, 16, 15, 0, 0), CheckOutTime = new DateTime(2027, 3, 18, 11, 0, 0), Latitude = -12.1291, Longitude = -77.0310 },
                new Accommodation { TripId = t10.Id, BookingStatusId = bNone, Name = "Belmond Palacio Nazarenas", Address = "Plazoleta Nazarenas 144, Cusco", CheckInTime = new DateTime(2027, 3, 18, 15, 0, 0), CheckOutTime = new DateTime(2027, 3, 22, 10, 0, 0), Latitude = -13.5153, Longitude = -71.9784 },
                new Accommodation { TripId = t10.Id, BookingStatusId = bNone, Name = "Belmond Sanctuary Lodge", Address = "Machu Picchu, Cusco 08680", CheckInTime = new DateTime(2027, 3, 22, 12, 0, 0), CheckOutTime = new DateTime(2027, 3, 24, 10, 0, 0), WebsiteUrl = "https://www.belmond.com/sanctuary-lodge-machu-picchu", Latitude = -13.1637, Longitude = -72.5447 },
                new Accommodation { TripId = t10.Id, BookingStatusId = bNone, Name = "Hotel B Lima", Address = "Sáenz Peña 204, Barranco, Lima", CheckInTime = new DateTime(2027, 3, 28, 14, 0, 0), CheckOutTime = new DateTime(2027, 3, 30, 11, 0, 0), Latitude = -12.1530, Longitude = -77.0223 }
            );
            db.TripActivities.AddRange(
                new TripActivity { TripId = t10.Id, BookingStatusId = bNone, Title = "Мачу-Пікчу — перший ранок", Address = "Machu Picchu Archaeological Site", StartTime = new DateTime(2027, 3, 22, 6, 0, 0), EndTime = new DateTime(2027, 3, 22, 12, 0, 0), Notes = "ОБОВ'ЯЗКОВО купити квиток за 3 місяці! Лише 2500 осіб/день. $52" },
                new TripActivity { TripId = t10.Id, BookingStatusId = bNone, Title = "Гора Huayna Picchu", Address = "Huayna Picchu, Machu Picchu", StartTime = new DateTime(2027, 3, 23, 7, 0, 0), EndTime = new DateTime(2027, 3, 23, 11, 0, 0), Notes = "Лише 400 осіб/день, +$10 до квитка Мачу-Пікчу. Дуже крутий підйом 1 год" },
                new TripActivity { TripId = t10.Id, BookingStatusId = bNone, Title = "Долина Священна Інків", Address = "Sacred Valley, Cusco Region", StartTime = new DateTime(2027, 3, 19, 8, 0, 0), EndTime = new DateTime(2027, 3, 19, 18, 0, 0), Notes = "Олантайтамбо, Пісак, ринок. Тур $60 з Куско" },
                new TripActivity { TripId = t10.Id, BookingStatusId = bNone, Title = "Ресторан Central Lima — #1 Латинська Америка", Address = "Av. Pedro de Osma 301, Barranco, Lima", StartTime = new DateTime(2027, 3, 17, 20, 0, 0), EndTime = new DateTime(2027, 3, 17, 23, 0, 0), Notes = "Резервація за 4+ місяці. Дегустаційне меню $200+. Must!" },
                new TripActivity { TripId = t10.Id, BookingStatusId = bNone, Title = "Акліматизація в Куско + Собор", Address = "Plaza de Armas, Cusco", StartTime = new DateTime(2027, 3, 18, 15, 0, 0), EndTime = new DateTime(2027, 3, 18, 18, 0, 0), Notes = "3400м — перший день повільно. Coca tea обов'язково!" }
            );
            db.Expenses.AddRange(
                new Expense { TripId = t10.Id, CategoryId = cTransport, Title = "Авіаквитки Київ–Ліма–Київ", TotalAmount = 1800, Currency = "USD", Date = new DateTime(2027, 3, 15), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cTransport, Title = "Внутрішні рейси Ліма–Куско (x2)", TotalAmount = 320, Currency = "USD", Date = new DateTime(2027, 3, 18), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cTransport, Title = "PeruRail Vistadome (x2)", TotalAmount = 180, Currency = "USD", Date = new DateTime(2027, 3, 22), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cAccom, Title = "Belmond Sanctuary Lodge (2 ночі)", TotalAmount = 1600, Currency = "USD", Date = new DateTime(2027, 3, 22), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cEntertain, Title = "Квиток Мачу-Пікчу + Huayna Picchu", TotalAmount = 62, Currency = "USD", Date = new DateTime(2027, 3, 22), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cEntertain, Title = "Sacred Valley тур", TotalAmount = 60, Currency = "USD", Date = new DateTime(2027, 3, 19), PayerId = uid },
                new Expense { TripId = t10.Id, CategoryId = cFood, Title = "Central Lima — дегустація", TotalAmount = 220, Currency = "USD", Date = new DateTime(2027, 3, 17), PayerId = uid }
            );
            var cl10 = new Checklist { TripId = t10.Id, Title = "Перу — критичне" };
            db.Checklists.Add(cl10); await db.SaveChangesAsync();
            db.ChecklistItems.AddRange(
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Квиток Мачу-Пікчу — ЗАРАЗ (лімітовано!)", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Huayna Picchu — окремий квиток", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Central Lima — резервація 4+ місяці", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Всі авіаквитки та поїзди", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Таблетки від висотної хвороби (Diamox)", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Вакцинація від жовтої лихоманки", IsChecked = false },
                new ChecklistItem { ChecklistId = cl10.Id, Content = "Travel insurance з евакуацією", IsChecked = false }
            );
            await db.SaveChangesAsync();
        }
    }
}
