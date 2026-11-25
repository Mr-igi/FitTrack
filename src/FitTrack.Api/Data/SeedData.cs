using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.Api.Data;

public static class SeedData
{
    public static void Seed(IServiceProvider services)
    {
        var plans = services.GetRequiredService<IRepository<MembershipPlan>>();
        var users = services.GetRequiredService<IRepository<User>>();
        var memberships = services.GetRequiredService<IRepository<Membership>>();
        var sessions = services.GetRequiredService<IRepository<TrainingSession>>();

        if (plans.Any(_ => true))
        {
            return;
        }

        var now = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        var monthly = plans.Add(new MembershipPlan { Name = "Monthly", Price = 3000m, DurationInDays = 30 });
        plans.Add(new MembershipPlan { Name = "Quarterly", Price = 8000m, DurationInDays = 90 });
        plans.Add(new MembershipPlan { Name = "Yearly", Price = 30000m, DurationInDays = 365 });

        var milos = users.Add(new User { FirstName = "Milos", LastName = "Petrovic", Email = "milos@fittrack.com", CreatedAt = now });
        var marko = users.Add(new User { FirstName = "Marko", LastName = "Jovanovic", Email = "marko@fittrack.com", Role = Role.Trainer, CreatedAt = now });
        users.Add(new User { FirstName = "Ana", LastName = "Nikolic", Email = "ana@fittrack.com", CreatedAt = now });
        var petar = users.Add(new User { FirstName = "Petar", LastName = "Ilic", Email = "petar@fittrack.com", CreatedAt = now });
        var jovana = users.Add(new User { FirstName = "Jovana", LastName = "Markovic", Email = "jovana@fittrack.com", CreatedAt = now });

        memberships.Add(Membership.Create(milos.Id, monthly, today));
        memberships.Add(Membership.Create(petar.Id, monthly, today));
        memberships.Add(Membership.Create(jovana.Id, monthly, today));

        sessions.Add(new TrainingSession
        {
            TrainerId = marko.Id,
            Title = "CrossFit",
            StartTime = today.AddDays(3).ToDateTime(new TimeOnly(18, 0)),
            DurationMinutes = 60,
            Location = "Room 2",
            Capacity = 2
        });

        sessions.Add(new TrainingSession
        {
            TrainerId = marko.Id,
            Title = "Boxing",
            StartTime = today.AddDays(1).ToDateTime(new TimeOnly(10, 0)),
            DurationMinutes = 60,
            Location = "Room 3",
            Capacity = 10
        });

        sessions.Add(new TrainingSession
        {
            TrainerId = marko.Id,
            Title = "Yoga",
            StartTime = today.AddDays(10).ToDateTime(new TimeOnly(9, 0)),
            DurationMinutes = 45,
            Location = "Room 1",
            Capacity = 15
        });
    }
}