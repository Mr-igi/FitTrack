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

var now = new DateTime(2026, 10, 17, 12, 0, 0);

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
Console.WriteLine($"Total bookings: {bookings.GetAll().Count()}");

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