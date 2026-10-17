using FitTrack.ConsoleApp.Exceptions;
using FitTrack.ConsoleApp.Models;
using FitTrack.ConsoleApp.Services;

var users = new List<User>
{
    new() { Id = 1, FirstName = "Milos", LastName = "Petrovic", Email = "milos@fittrack.com" },
    new() { Id = 2, FirstName = "Marko", LastName = "Jovanovic", Email = "marko@fittrack.com", Role = Role.Trainer },
    new() { Id = 3, FirstName = "Ana", LastName = "Nikolic", Email = "ana@fittrack.com" },
    new() { Id = 4, FirstName = "Petar", LastName = "Ilic", Email = "petar@fittrack.com" },
    new() { Id = 5, FirstName = "Jovana", LastName = "Markovic", Email = "jovana@fittrack.com" }
};

var monthly = new MembershipPlan { Id = 1, Name = "Monthly", Price = 3000m, DurationInDays = 30 };

var startDate = new DateOnly(2026, 10, 1);
var memberships = new List<Membership>
{
    Membership.Create(1, monthly, startDate),
    Membership.Create(4, monthly, startDate),
    Membership.Create(5, monthly, startDate)
};

var sessions = new List<TrainingSession>
{
    new()
    {
        Id = 1,
        TrainerId = 2,
        Title = "CrossFit",
        StartTime = new DateTime(2026, 10, 20, 18, 0, 0),
        DurationMinutes = 60,
        Location = "Room 2",
        Capacity = 2
    },
    new()
    {
        Id = 2,
        TrainerId = 2,
        Title = "Yoga",
        StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
        DurationMinutes = 45,
        Location = "Room 1",
        Capacity = 15
    },
    new()
    {
        Id = 3,
        TrainerId = 2,
        Title = "Boxing",
        StartTime = new DateTime(2026, 10, 18, 10, 0, 0),
        DurationMinutes = 60,
        Location = "Room 3",
        Capacity = 10
    }
};

var bookings = new List<Booking>();

var bookingService = new BookingService(users, memberships, sessions, bookings);

var now = new DateTime(2026, 10, 17, 12, 0, 0);

Console.WriteLine("--- Booking ---");
TryBook(1, 1, "Milos books CrossFit");
TryBook(1, 1, "Milos books CrossFit again");
TryBook(3, 1, "Ana books CrossFit");
TryBook(2, 1, "Trainer Marko books CrossFit");
TryBook(1, 2, "Milos books past Yoga session");
TryBook(1, 99, "Milos books non-existing session");
TryBook(4, 1, "Petar books CrossFit");
TryBook(5, 1, "Jovana books CrossFit");
TryBook(1, 3, "Milos books Boxing tomorrow");

Console.WriteLine();
Console.WriteLine("--- Cancelling ---");
TryCancel(1, 99, "Milos cancels non-existing booking");
TryCancel(1, 2, "Milos cancels Petar's booking");
TryCancel(1, 3, "Milos cancels Boxing (less than 48h left)");
TryCancel(1, 1, "Milos cancels CrossFit");

Console.WriteLine();
Console.WriteLine("--- After cancelling ---");
TryBook(5, 1, "Jovana books CrossFit again");

Console.WriteLine();
Console.WriteLine($"Total bookings: {bookings.Count}");

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