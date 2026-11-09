using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class StatisticsServiceTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<MembershipPlan> _plans = new();
    private readonly InMemoryRepository<Membership> _memberships = new();
    private readonly InMemoryRepository<TrainingSession> _sessions = new();
    private readonly InMemoryRepository<Booking> _bookings = new();

    private readonly StatisticsService _service;

    private readonly DateOnly _today = new(2026, 10, 17);

    public StatisticsServiceTests()
    {
        _service = new StatisticsService(_users, _plans, _memberships, _sessions, _bookings);
    }


    private User AddUser(string firstName = "Test", Role role = Role.Member)
    {
        return _users.Add(new User
        {
            FirstName = firstName,
            LastName = "Test",
            Email = $"{firstName.ToLower()}@fittrack.com",
            CreatedAt = new DateTime(2026, 10, 1),
            Role = role
        });
    }

    private MembershipPlan AddPlan(string name, decimal price = 3000m, int days = 30)
    {
        return _plans.Add(new MembershipPlan { Name = name, Price = price, DurationInDays = days });
    }

    private void AddMembership(int userId, DateOnly start, DateOnly end, decimal pricePaid = 3000m, int planId = 1)
    {
        _memberships.Add(new Membership
        {
            UserId = userId,
            MembershipPlanId = planId,
            PricePaid = pricePaid,
            StartDate = start,
            EndDate = end
        });
    }

    private TrainingSession AddSession(string title, int capacity)
    {
        return _sessions.Add(new TrainingSession
        {
            TrainerId = 1,
            Title = title,
            StartTime = new DateTime(2026, 10, 20, 18, 0, 0),
            DurationMinutes = 60,
            Location = "Room 1",
            Capacity = capacity
        });
    }

    private void AddBooking(int userId, int sessionId)
    {
        _bookings.Add(new Booking { UserId = userId, TrainingSessionId = sessionId, BookedAt = new DateTime(2026, 10, 17, 12, 0, 0) });
    }

    [Fact]
    public void CountActiveMembers_CountsOnlyUsersWithActiveMembership()
    {
        var ana = AddUser("Ana");
        var bora = AddUser("Bora");
        var ceca = AddUser("Ceca");
        AddMembership(ana.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));
        AddMembership(bora.Id, new DateOnly(2026, 10, 10), new DateOnly(2026, 11, 9));
        AddMembership(ceca.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        int count = _service.CountActiveMembers(_today);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CountActiveMembers_CountsUserOnlyOnce()
    {
        var ana = AddUser("Ana");
        AddMembership(ana.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));
        AddMembership(ana.Id, new DateOnly(2026, 10, 15), new DateOnly(2026, 11, 14));

        int count = _service.CountActiveMembers(_today);

        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData(9, 30, 0)]
    [InlineData(10, 1, 1)]
    [InlineData(10, 30, 1)]
    [InlineData(10, 31, 0)]
    public void CountActiveMembers_OnBoundaryDates(int month, int day, int expectedCount)
    {
        var ana = AddUser("Ana");
        AddMembership(ana.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));

        int count = _service.CountActiveMembers(new DateOnly(2026, month, day));

        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void GetTotalRevenue_SumsAmountsPaid()
    {
        AddMembership(1, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), pricePaid: 3000m);
        AddMembership(2, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), pricePaid: 3000m);
        AddMembership(3, new DateOnly(2026, 10, 1), new DateOnly(2027, 9, 30), pricePaid: 24000m);

        decimal total = _service.GetTotalRevenue();

        Assert.Equal(30000m, total);
    }

    [Fact]
    public void GetTotalRevenue_WhenNoMemberships_ReturnsZero()
    {
        decimal total = _service.GetTotalRevenue();

        Assert.Equal(0m, total);
    }


    [Fact]
    public void GetRevenueByPlan_GroupsByPlanAndSortsByTotalDescending()
    {
        var monthly = AddPlan("Monthly");
        var yearly = AddPlan("Yearly", 30000m, 365);
        AddMembership(1, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), 3000m, monthly.Id);
        AddMembership(2, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), 3000m, monthly.Id);
        AddMembership(3, new DateOnly(2026, 10, 1), new DateOnly(2027, 9, 30), 24000m, yearly.Id);

        var result = _service.GetRevenueByPlan();

        Assert.Equal(2, result.Count);
        Assert.Equal(new PlanRevenue("Yearly", 1, 24000m), result[0]);
        Assert.Equal(new PlanRevenue("Monthly", 2, 6000m), result[1]);
    }

    [Fact]
    public void GetRevenueByPlan_WhenPlanIsMissing_UsesUnknownName()
    {
        AddMembership(1, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), 3000m, planId: 99);

        var result = _service.GetRevenueByPlan();

        var row = Assert.Single(result);
        Assert.Equal(new PlanRevenue("Unknown", 1, 3000m), row);
    }

    [Fact]
    public void GetSessionOccupancy_CountsBookingsPerSessionAndSortsByMostBooked()
    {
        var yoga = AddSession("Yoga", 15);
        var crossFit = AddSession("CrossFit", 2);
        var boxing = AddSession("Boxing", 10);
        AddBooking(1, crossFit.Id);
        AddBooking(2, crossFit.Id);
        AddBooking(1, boxing.Id);

        var result = _service.GetSessionOccupancy();

        Assert.Equal(3, result.Count);
        Assert.Equal(new SessionOccupancy("CrossFit", 2, 2), result[0]);
        Assert.Equal(new SessionOccupancy("Boxing", 1, 10), result[1]);
        Assert.Equal(new SessionOccupancy("Yoga", 0, 15), result[2]);
    }

    [Fact]
    public void GetExpiringSoon_ReturnsOnlyMembershipsEndingInWindowSortedByEndDate()
    {
        var ana = AddUser("Ana");
        var bora = AddUser("Bora");
        var ceca = AddUser("Ceca");
        var dara = AddUser("Dara");
        AddMembership(ana.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));
        AddMembership(bora.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 20));
        AddMembership(ceca.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 15));
        AddMembership(dara.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 10));

        var result = _service.GetExpiringSoon(_today, 14);

        Assert.Equal(2, result.Count);
        Assert.Equal(new ExpiringMembership("Bora Test", new DateOnly(2026, 10, 20)), result[0]);
        Assert.Equal(new ExpiringMembership("Ana Test", new DateOnly(2026, 10, 30)), result[1]);
    }

    [Fact]
    public void GetExpiringSoon_UsesLatestMembershipOfUser()
    {
        var ana = AddUser("Ana");
        AddMembership(ana.Id, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30));
        AddMembership(ana.Id, new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 29));

        var result = _service.GetExpiringSoon(_today, 14);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 1)]
    [InlineData(14, 1)]
    [InlineData(15, 0)]
    public void GetExpiringSoon_OnWindowBoundaries(int endsInDays, int expectedCount)
    {
        var ana = AddUser("Ana");
        AddMembership(ana.Id, new DateOnly(2026, 9, 1), _today.AddDays(endsInDays));

        var result = _service.GetExpiringSoon(_today, 14);

        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetMembersWithoutBookings_ReturnsOnlyMembersWithNoBookings()
    {
        AddUser("Ana");
        AddUser("Marko", Role.Trainer);
        var petar = AddUser("Petar");
        var session = AddSession("CrossFit", 10);
        AddBooking(petar.Id, session.Id);

        var result = _service.GetMembersWithoutBookings();

        var name = Assert.Single(result);
        Assert.Equal("Ana Test", name);
    }
}