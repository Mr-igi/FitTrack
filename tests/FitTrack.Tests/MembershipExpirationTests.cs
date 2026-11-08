using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class MembershipExpirationTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<MembershipPlan> _plans = new();
    private readonly InMemoryRepository<Membership> _memberships = new();

    private readonly MembershipService _service;

    private readonly DateOnly _today = new(2026, 11, 5);

    public MembershipExpirationTests()
    {
        _service = new MembershipService(_users, _plans, _memberships);
    }

    private User AddUser()
    {
        return _users.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@fittrack.com"
        });
    }

    private Membership AddMembership(int userId, DateOnly endDate, DateOnly? startDate = null)
    {
        return _memberships.Add(new Membership
        {
            UserId = userId,
            MembershipPlanId = 1,
            PricePaid = 3000m,
            StartDate = startDate ?? new DateOnly(2026, 10, 1),
            EndDate = endDate
        });
    }

    [Fact]
    public void MarkExpiredMemberships_WhenNothingHasEnded_ReturnsZero()
    {
        var member = AddUser();
        var membership = AddMembership(member.Id, new DateOnly(2026, 11, 30));

        int raisedCount = 0;
        _service.MembershipExpired += _ => raisedCount++;

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(0, marked);
        Assert.False(membership.IsMarkedExpired);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenMembershipEnded_MarksItAsExpired()
    {
        var member = AddUser();
        var membership = AddMembership(member.Id, new DateOnly(2026, 10, 30));

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(1, marked);
        Assert.True(membership.IsMarkedExpired);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenMembershipEndsToday_IsNotExpiredYet()
    {
        var member = AddUser();
        var membership = AddMembership(member.Id, _today);

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(0, marked);
        Assert.False(membership.IsMarkedExpired);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenMembershipEndedYesterday_IsExpired()
    {
        var member = AddUser();
        var membership = AddMembership(member.Id, _today.AddDays(-1));

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(1, marked);
        Assert.True(membership.IsMarkedExpired);
    }

    [Fact]
    public void MarkExpiredMemberships_CalledTwiceOnSameDate_ProcessesOnlyOnce()
    {
        var member = AddUser();
        AddMembership(member.Id, new DateOnly(2026, 10, 30));

        int raisedCount = 0;
        _service.MembershipExpired += _ => raisedCount++;

        int first = _service.MarkExpiredMemberships(_today);
        int second = _service.MarkExpiredMemberships(_today);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenMembershipEnded_RaisesMembershipExpired()
    {
        var member = AddUser();
        var membership = AddMembership(member.Id, new DateOnly(2026, 10, 30));

        Membership? expired = null;
        _service.MembershipExpired += m => expired = m;

        _service.MarkExpiredMemberships(_today);

        Assert.Same(membership, expired);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenUserHasLaterMembership_MarksItButRaisesNoEvent()
    {
        var member = AddUser();
        var first = AddMembership(member.Id, new DateOnly(2026, 10, 30));
        AddMembership(member.Id, new DateOnly(2026, 11, 29), startDate: new DateOnly(2026, 10, 31));

        int raisedCount = 0;
        _service.MembershipExpired += _ => raisedCount++;

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(1, marked);
        Assert.True(first.IsMarkedExpired);

        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void MarkExpiredMemberships_WhenLastMembershipExpires_RaisesEventForIt()
    {
        var member = AddUser();
        AddMembership(member.Id, new DateOnly(2026, 10, 30));
        var second = AddMembership(member.Id, new DateOnly(2026, 11, 29), startDate: new DateOnly(2026, 10, 31));

        var expiredList = new List<Membership>();
        _service.MembershipExpired += m => expiredList.Add(m);

        _service.MarkExpiredMemberships(_today);
        _service.MarkExpiredMemberships(new DateOnly(2026, 12, 5));

        Assert.Single(expiredList);
        Assert.Same(second, expiredList[0]);
    }

    [Fact]
    public void MarkExpiredMemberships_WithSeveralUsers_RaisesEventForEachOfThem()
    {
        var first = AddUser();
        var second = AddUser();
        AddMembership(first.Id, new DateOnly(2026, 10, 30));
        AddMembership(second.Id, new DateOnly(2026, 10, 30));

        var expiredUserIds = new List<int>();
        _service.MembershipExpired += m => expiredUserIds.Add(m.UserId);

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(2, marked);
        Assert.Equal(2, expiredUserIds.Count);
        Assert.Contains(first.Id, expiredUserIds);
        Assert.Contains(second.Id, expiredUserIds);
    }

    [Fact]
    public void MarkExpiredMemberships_WithoutSubscribers_DoesNotThrow()
    {
        var member = AddUser();
        AddMembership(member.Id, new DateOnly(2026, 10, 30));

        int marked = _service.MarkExpiredMemberships(_today);

        Assert.Equal(1, marked);
    }
}