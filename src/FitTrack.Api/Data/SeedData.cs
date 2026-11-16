using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.Api.Data;

public static class SeedData
{
    public static void Seed(IServiceProvider services)
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
}