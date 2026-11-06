using FitTrack.Core.Models;
using Xunit;

namespace FitTrack.Tests;
public class MembershipTests
{
    [Fact]
    public void Create_ThirtyDayPlan_EndsOnThirtiethDay()
    {

        var plan = new MembershipPlan { Id = 1, Name = "Monthly", Price = 3000m, DurationInDays = 30 };

        var start = new DateOnly(2026, 10, 1);

        var membership = Membership.Create(1, plan, start);
        Assert.Equal(new DateOnly(2026, 10, 30), membership.EndDate);    }

    [Fact]
    public void Create_YearlyPlan_StoresPriceWithDiscount()
    {
        var plan = new MembershipPlan { Id = 2, Name = "Yearly", Price = 30000m, DurationInDays = 365 };

        var start = new DateOnly(2026, 10, 1);

        var membership = Membership.Create(1, plan, start);

        Assert.Equal(24000m, membership.PricePaid);
    }

    [Fact]
    public void IsActiveOn_LastDayOfMembership_ReturnsTrue()
    {
        var plan = new MembershipPlan { Id = 1, Name = "Monthly", Price = 3000m, DurationInDays = 30 };
        var membership = Membership.Create(1, plan, new DateOnly(2026, 10, 1));

        bool isActive = membership.IsActiveOn(new DateOnly(2026, 10, 30));

        Assert.True(isActive);
    }
}