using FitTrack.Core.Models;
using Xunit;

namespace FitTrack.Tests;

public class MembershipPlanTests
{
    [Theory]

    [InlineData(1, 0)]
    [InlineData(30, 0)]
    [InlineData(89, 0)]
    [InlineData(90, 10)]
    [InlineData(364, 10)]
    [InlineData(365, 20)]
    [InlineData(730, 20)]

    public void GetDiscountPercent_ReturnsExpectedPercent(int durationInDays, int expectedPercent)
    {
        var plan = new MembershipPlan { Name = "Test", Price = 1000m, DurationInDays = durationInDays };

        int percent = plan.GetDiscountPercent();

        Assert.Equal(expectedPercent, percent);
    }

    [Theory]
    [InlineData(3000, 30, 3000)]
    [InlineData(8000, 90, 7200)]
    [InlineData(30000, 365, 24000)]
    [InlineData(1000, 89, 1000)]
    [InlineData(1000, 365, 800)]
    public void GetFinalPrice_AppliesDiscount(int price, int durationInDays, int expectedFinalPrice)
    {
        var plan = new MembershipPlan { Name = "Test", Price = price, DurationInDays = durationInDays };

        decimal finalPrice = plan.GetFinalPrice();

        Assert.Equal((decimal)expectedFinalPrice, finalPrice);
    }
}