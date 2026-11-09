using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class MembershipServiceTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<MembershipPlan> _plans = new();
    private readonly InMemoryRepository<Membership> _memberships = new();

    private readonly MembershipService _service;

    private readonly DateOnly _today = new(2026, 10, 17);

    public MembershipServiceTests()
    {
        _service = new MembershipService(_users, _plans, _memberships);
    }

    private User AddUser(Role role = Role.Member)
    {
        return _users.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@fittrack.com",
            Role = role,
            CreatedAt = new DateTime(2026, 10, 1)
        });
    }

    private MembershipPlan AddPlan(decimal price = 3000m, int days = 30)
    {
        return _plans.Add(new MembershipPlan
        {
            Name = "Test plan",
            Price = price,
            DurationInDays = days
        });
    }

    private void AddExistingMembership(int userId, DateOnly start, DateOnly end)
    {
        _memberships.Add(new Membership
        {
            UserId = userId,
            MembershipPlanId = 1,
            PricePaid = 3000m,
            StartDate = start,
            EndDate = end
        });
    }


    [Fact]
    public void Purchase_WhenUserNotFound_Throws()
    {
        var plan = AddPlan();

        var ex = Assert.Throws<MembershipException>(() => _service.Purchase(99, plan.Id, _today, _today));

        Assert.Equal("User with id 99 was not found.", ex.Message);
    }

    [Fact]
    public void Purchase_WhenUserIsNotMember_Throws()
    {
        var trainer = AddUser(Role.Trainer);
        var plan = AddPlan();

        var ex = Assert.Throws<MembershipException>(() => _service.Purchase(trainer.Id, plan.Id, _today, _today));

        Assert.Equal("Only members can purchase a membership.", ex.Message);
    }

    [Fact]
    public void Purchase_WhenPlanNotFound_Throws()
    {
        var member = AddUser();

        var ex = Assert.Throws<MembershipException>(() => _service.Purchase(member.Id, 99, _today, _today));

        Assert.Equal("Membership plan with id 99 was not found.", ex.Message);
    }

    [Fact]
    public void Purchase_WhenStartDateIsInThePast_Throws()
    {
        var member = AddUser();
        var plan = AddPlan();

        var ex = Assert.Throws<MembershipException>(() =>
            _service.Purchase(member.Id, plan.Id, _today.AddDays(-1), _today));

        Assert.Equal("Start date cannot be in the past.", ex.Message);

        Assert.Empty(_memberships.GetAll());
    }


    [Fact]
    public void Purchase_FirstMembership_StartsOnRequestedDate()
    {
        var member = AddUser();
        var plan = AddPlan();
        var start = new DateOnly(2026, 10, 20);

        var membership = _service.Purchase(member.Id, plan.Id, start, _today);

        Assert.Equal(start, membership.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 18), membership.EndDate);

        Assert.Equal(member.Id, membership.UserId);
        Assert.Equal(plan.Id, membership.MembershipPlanId);
        Assert.Equal(3000m, membership.PricePaid);

        Assert.NotNull(_memberships.GetById(membership.Id));
    }

    [Fact]
    public void Purchase_StartingToday_IsAllowed()
    {
        var member = AddUser();
        var plan = AddPlan();

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(_today, membership.StartDate);
    }

    [Fact]
    public void Purchase_YearlyPlan_StoresPriceWithDiscount()
    {
        var member = AddUser();
        var plan = AddPlan(price: 30000m, days: 365);

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(24000m, membership.PricePaid);
    }


    [Fact]
    public void Purchase_WhenActiveMembershipExists_StartsAfterIt()
    {
        var member = AddUser();
        var plan = AddPlan();
        AddExistingMembership(member.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(new DateOnly(2026, 10, 31), membership.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 29), membership.EndDate);
    }

    [Fact]
    public void Purchase_WithSeveralExistingMemberships_StartsAfterTheLatestOne()
    {
        var member = AddUser();
        var plan = AddPlan();
        AddExistingMembership(member.Id, new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 29));
        AddExistingMembership(member.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(new DateOnly(2026, 11, 30), membership.StartDate);
        Assert.Equal(new DateOnly(2026, 12, 29), membership.EndDate);
    }

    [Fact]
    public void Purchase_WhenExistingEndsOnStartDate_StartsNextDay()
    {
        var member = AddUser();
        var plan = AddPlan();
        AddExistingMembership(member.Id, new DateOnly(2026, 10, 1), _today);

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(new DateOnly(2026, 10, 18), membership.StartDate);
    }

    [Fact]
    public void Purchase_WhenExistingEndedBeforeStartDate_StartsOnRequestedDate()
    {
        var member = AddUser();
        var plan = AddPlan();
        AddExistingMembership(member.Id, new DateOnly(2026, 10, 1), _today.AddDays(-1));

        var membership = _service.Purchase(member.Id, plan.Id, _today, _today);

        Assert.Equal(_today, membership.StartDate);
    }

    [Fact]
    public void Purchase_WhenOtherUserHasMembership_IgnoresIt()
    {
        var buyer = AddUser();
        var other = AddUser();
        var plan = AddPlan();
        AddExistingMembership(other.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));

        var membership = _service.Purchase(buyer.Id, plan.Id, _today, _today);

        Assert.Equal(_today, membership.StartDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Purchase_WhenPlanDurationIsNotPositive_Throws(int durationInDays)
    {
        var member = AddUser();
        var plan = AddPlan(days: durationInDays);

        var ex = Assert.Throws<MembershipException>(() => _service.Purchase(member.Id, plan.Id, _today, _today));

        Assert.Equal("Membership plan duration must be at least one day.", ex.Message);

        Assert.Empty(_memberships.GetAll());
    }
}