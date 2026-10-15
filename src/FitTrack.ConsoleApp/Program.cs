using FitTrack.ConsoleApp.Models;

var milos = new User
{
    Id = 1,
    FirstName = "Milos",
    LastName = "Petrovic",
    Email = "milos@fittrack.com"
};

var trainer = new User
{
    Id = 2,
    FirstName = "Marko",
    LastName = "Jovanovic",
    Email = "marko@fittrack.com",
    Role = Role.Trainer
};

var monthly = new MembershipPlan { Id = 1, Name = "Monthly", Price = 3000m, DurationInDays = 30 };
var yearly = new MembershipPlan { Id = 2, Name = "Yearly", Price = 30000m, DurationInDays = 365 };

var startDate = new DateOnly(2026, 10, 1);
var membership = Membership.Create(milos.Id, monthly, startDate);

Console.WriteLine($"{milos.FullName} ({milos.Role}) purchased: {monthly.Name}");
Console.WriteLine($"Valid from {membership.StartDate} to {membership.EndDate}, paid {membership.PricePaid} RSD");

var today = DateOnly.FromDateTime(DateTime.Today);
Console.WriteLine($"Active on 2026-10-15: {membership.IsActiveOn(new DateOnly(2026, 10, 15))}");
Console.WriteLine($"Active today ({today}): {membership.IsActiveOn(today)}");

var session = new TrainingSession
{
    Id = 1,
    TrainerId = trainer.Id,
    Title = "CrossFit",
    StartTime = new DateTime(2026, 10, 20, 18, 0, 0),
    DurationMinutes = 60,
    Location = "Room 2",
    Capacity = 10
};

Console.WriteLine();
Console.WriteLine($"Session: {session.Title}, trainer {trainer.FullName}");
Console.WriteLine($"{session.StartTime:dd.MM.yyyy HH:mm} - {session.EndTime:HH:mm}, {session.Location}, max {session.Capacity} people");

var booking = new Booking { Id = 1, UserId = milos.Id, TrainingSessionId = session.Id };
Console.WriteLine($"{milos.FullName} booked session #{booking.TrainingSessionId} at {booking.BookedAt:HH:mm}");