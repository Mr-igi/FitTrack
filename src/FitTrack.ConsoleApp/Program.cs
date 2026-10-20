using FitTrack.ConsoleApp.Exceptions;
using FitTrack.ConsoleApp.Models;
using FitTrack.ConsoleApp.Repositories;
using FitTrack.ConsoleApp.Services;

var users = new InMemoryRepository<User>();
var plans = new InMemoryRepository<MembershipPlan>();
var memberships = new InMemoryRepository<Membership>();
var sessions = new InMemoryRepository<TrainingSession>();
var bookings = new InMemoryRepository<Booking>();

var milos = users.Add(new User { FirstName = "Milos", LastName = "Petrovic", Email = "milos@fittrack.com" });
var marko = users.Add(new User { FirstName = "Marko", LastName = "Jovanovic", Email = "marko@fittrack.com", Role = Role.Trainer });
var ana = users.Add(new User { FirstName = "Ana", LastName = "Nikolic", Email = "ana@fittrack.com" });
var petar = users.Add(new User { FirstName = "Petar", LastName = "Ilic", Email = "petar@fittrack.com" });
var jovana = users.Add(new User { FirstName = "Jovana", LastName = "Markovic", Email = "jovana@fittrack.com" });

var monthly = plans.Add(new MembershipPlan { Name = "Monthly", Price = 3000m, DurationInDays = 30 });
var yearly = plans.Add(new MembershipPlan { Name = "Yearly", Price = 30000m, DurationInDays = 365 });

var startDate = new DateOnly(2026, 10, 1);
memberships.Add(Membership.Create(milos.Id, monthly, startDate));
memberships.Add(Membership.Create(petar.Id, monthly, startDate));
memberships.Add(Membership.Create(jovana.Id, monthly, startDate));

var crossFit = sessions.Add(new TrainingSession
{
    TrainerId = marko.Id,
    Title = "CrossFit",
    StartTime = new DateTime(2026, 10, 20, 18, 0, 0),
    DurationMinutes = 60,
    Location = "Room 2",
    Capacity = 2
});

var yoga = sessions.Add(new TrainingSession
{
    TrainerId = marko.Id,
    Title = "Yoga",
    StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
    DurationMinutes = 45,
    Location = "Room 1",
    Capacity = 15
});

var boxing = sessions.Add(new TrainingSession
{
    TrainerId = marko.Id,
    Title = "Boxing",
    StartTime = new DateTime(2026, 10, 18, 10, 0, 0),
    DurationMinutes = 60,
    Location = "Room 3",
    Capacity = 10
});

var bookingService = new BookingService(users, memberships, sessions, bookings);
var membershipService = new MembershipService(users, plans, memberships);
var statisticsService = new StatisticsService(users, plans, memberships, sessions, bookings);

var notificationService = new NotificationService(users);

bookingService.SessionFilled += notificationService.OnSessionFilled;

membershipService.MembershipExpired += notificationService.OnMembershipExpired;

var now = new DateTime(2026, 10, 17, 12, 0, 0);
var today = DateOnly.FromDateTime(now);

Console.WriteLine("--- Booking ---");
TryBook(milos.Id, crossFit.Id, "Milos books CrossFit");
TryBook(milos.Id, crossFit.Id, "Milos books CrossFit again");
TryBook(ana.Id, crossFit.Id, "Ana books CrossFit");
TryBook(marko.Id, crossFit.Id, "Trainer Marko books CrossFit");
TryBook(milos.Id, yoga.Id, "Milos books past Yoga session");
TryBook(milos.Id, 99, "Milos books non-existing session");
TryBook(petar.Id, crossFit.Id, "Petar books CrossFit");
TryBook(jovana.Id, crossFit.Id, "Jovana books CrossFit");
TryBook(milos.Id, boxing.Id, "Milos books Boxing tomorrow");

Console.WriteLine();
Console.WriteLine("--- Cancelling ---");
TryCancel(milos.Id, 99, "Milos cancels non-existing booking");
TryCancel(milos.Id, 2, "Milos cancels Petar's booking");
TryCancel(milos.Id, 3, "Milos cancels Boxing (less than 48h left)");
TryCancel(milos.Id, 1, "Milos cancels CrossFit");

Console.WriteLine();
Console.WriteLine("--- After cancelling ---");
TryBook(jovana.Id, crossFit.Id, "Jovana books CrossFit again");

Console.WriteLine();
Console.WriteLine("--- Purchasing memberships ---");
TryPurchase(ana.Id, monthly.Id, today, "Ana buys Monthly (no existing membership)");
TryPurchase(milos.Id, monthly.Id, today, "Milos buys Monthly (has one until 2026-10-30)");
TryPurchase(milos.Id, yearly.Id, today, "Milos buys Yearly (stacks after the new one)");
TryPurchase(marko.Id, monthly.Id, today, "Trainer Marko buys Monthly");
TryPurchase(99, monthly.Id, today, "Non-existing user buys Monthly");
TryPurchase(petar.Id, 99, today, "Petar buys non-existing plan");
TryPurchase(petar.Id, monthly.Id, new DateOnly(2026, 10, 1), "Petar buys Monthly starting in the past");

Console.WriteLine();
Console.WriteLine("--- Statistics ---");
Console.WriteLine($"Active members on {today:yyyy-MM-dd}: {statisticsService.CountActiveMembers(today)}");
Console.WriteLine($"Total revenue: {statisticsService.GetTotalRevenue()} RSD");

Console.WriteLine();
Console.WriteLine("Revenue by plan:");
foreach (var row in statisticsService.GetRevenueByPlan())
{
    Console.WriteLine($"  {row.PlanName}: {row.Count} sold, {row.Total} RSD");
}

Console.WriteLine();
Console.WriteLine("Session occupancy:");
foreach (var row in statisticsService.GetSessionOccupancy())
{
    Console.WriteLine($"  {row.Title}: {row.Booked}/{row.Capacity}");
}

Console.WriteLine();
Console.WriteLine("Memberships expiring in the next 14 days:");
foreach (var row in statisticsService.GetExpiringSoon(today, 14))
{
    Console.WriteLine($"  {row.FullName} - {row.EndDate:yyyy-MM-dd}");
}

Console.WriteLine();
Console.WriteLine("Members without bookings:");
foreach (var name in statisticsService.GetMembersWithoutBookings())
{
    Console.WriteLine($"  {name}");
}

Console.WriteLine();
Console.WriteLine($"Total bookings: {bookings.GetAll().Count()}");
Console.WriteLine($"Total memberships: {memberships.GetAll().Count()}");

Console.WriteLine();
Console.WriteLine("--- Expiration check ---");

Console.WriteLine("Checking on 2026-10-17...");
int markedToday = membershipService.MarkExpiredMemberships(today);
Console.WriteLine($"Marked {markedToday} membership(s) as expired.");

var later = new DateOnly(2026, 11, 5);

Console.WriteLine($"Checking on {later:yyyy-MM-dd}...");
int markedLater = membershipService.MarkExpiredMemberships(later);
Console.WriteLine($"Marked {markedLater} membership(s) as expired.");

Console.WriteLine($"Checking on {later:yyyy-MM-dd} again...");
int markedAgain = membershipService.MarkExpiredMemberships(later);
Console.WriteLine($"Marked {markedAgain} membership(s) as expired.");

void TryBook(int userId, int sessionId, string description)
{
    try
    {
        var booking = bookingService.BookSession(userId, sessionId, now);
        Console.WriteLine($"[OK]   {description} -> booking #{booking.Id}");
    }
    catch (BookingException ex)
    {
        Console.WriteLine($"[FAIL] {description} -> {ex.Message}");
    }
}

void TryCancel(int userId, int bookingId, string description)
{
    try
    {
        bookingService.CancelBooking(userId, bookingId, now);
        Console.WriteLine($"[OK]   {description}");
    }
    catch (BookingException ex)
    {
        Console.WriteLine($"[FAIL] {description} -> {ex.Message}");
    }
}

void TryPurchase(int userId, int planId, DateOnly start, string description)
{
    try
    {
        var m = membershipService.Purchase(userId, planId, start, today);
        Console.WriteLine($"[OK]   {description} -> {m.StartDate:yyyy-MM-dd} to {m.EndDate:yyyy-MM-dd}, paid {m.PricePaid} RSD");
    }
    catch (MembershipException ex)
    {
        Console.WriteLine($"[FAIL] {description} -> {ex.Message}");
    }
}