using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.Api.Data;

public static class SeedData
{
    public static void Seed(IServiceProvider services)
    {
        SeedPlans(services);
        SeedUsers(services);
    }

    private static void SeedPlans(IServiceProvider services)
    {
        var plans = services.GetRequiredService<IRepository<MembershipPlan>>();

        if (plans.Any(_ => true))
        {
            return;
        }

        plans.Add(new MembershipPlan { Name = "Monthly", Price = 3000m, DurationInDays = 30 });
        plans.Add(new MembershipPlan { Name = "Quarterly", Price = 8000m, DurationInDays = 90 });
        plans.Add(new MembershipPlan { Name = "Yearly", Price = 30000m, DurationInDays = 365 });
    }

    private static void SeedUsers(IServiceProvider services)
    {

        var users = services.GetRequiredService<IRepository<User>>();

        if (users.Any(_ => true))
        {
            return;
        }

        var createdAt = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

        users.Add(new User { FirstName = "Milos", LastName = "Petrovic", Email = "milos@fittrack.com", CreatedAt = createdAt });
        users.Add(new User { FirstName = "Marko", LastName = "Jovanovic", Email = "marko@fittrack.com", Role = Role.Trainer, CreatedAt = createdAt });
        users.Add(new User { FirstName = "Ana", LastName = "Nikolic", Email = "ana@fittrack.com", CreatedAt = createdAt });
        users.Add(new User { FirstName = "Petar", LastName = "Ilic", Email = "petar@fittrack.com", CreatedAt = createdAt });
        users.Add(new User { FirstName = "Jovana", LastName = "Markovic", Email = "jovana@fittrack.com", CreatedAt = createdAt });
    }
}