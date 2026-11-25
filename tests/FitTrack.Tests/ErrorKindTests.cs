using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class ErrorKindTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<MembershipPlan> _plans = new();
    private readonly InMemoryRepository<Membership> _memberships = new();
    private readonly InMemoryRepository<TrainingSession> _sessions = new();
    private readonly InMemoryRepository<Booking> _bookings = new();

    private readonly BookingService _bookingService;
    private readonly MembershipService _membershipService;

    private readonly DateTime _now = new(2026, 10, 17, 12, 0, 0);

    public ErrorKindTests()
    {
        _bookingService = new BookingService(_users, _memberships, _sessions, _bookings);
        _membershipService = new MembershipService(_users, _plans, _memberships);
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

    [Fact]
    public void BookSession_WhenUserNotFound_IsNotFound()
    {
        var ex = Assert.Throws<BookingException>(() => _bookingService.BookSession(99, 1, _now));

        Assert.Equal(ErrorKind.NotFound, ex.Kind);
    }

    [Fact]
    public void BookSession_WhenUserIsNotMember_IsForbidden()
    {
        var trainer = AddUser(Role.Trainer);

        var ex = Assert.Throws<BookingException>(() => _bookingService.BookSession(trainer.Id, 1, _now));

        Assert.Equal(ErrorKind.Forbidden, ex.Kind);    }

    [Fact]
    public void BookSession_WhenSessionAlreadyStarted_IsConflict()
    {
        var member = AddUser();
        var session = _sessions.Add(new TrainingSession
        {
            TrainerId = 1,
            Title = "CrossFit",
            StartTime = _now.AddHours(-1),
            DurationMinutes = 60,
            Location = "Room 2",
            Capacity = 10
        });

        var ex = Assert.Throws<BookingException>(() => _bookingService.BookSession(member.Id, session.Id, _now));

        Assert.Equal(ErrorKind.Conflict, ex.Kind);    }

    [Fact]
    public void CancelBooking_WhenBookingNotFound_IsNotFound()
    {
        var member = AddUser();

        var ex = Assert.Throws<BookingException>(() => _bookingService.CancelBooking(member.Id, 99, _now));

        Assert.Equal(ErrorKind.NotFound, ex.Kind);
    }

    [Fact]
    public void CancelBooking_WhenBookingBelongsToAnotherUser_IsForbidden()
    {
        var owner = AddUser();
        var other = AddUser();
        var booking = _bookings.Add(new Booking { UserId = owner.Id, TrainingSessionId = 1, BookedAt = _now });

        var ex = Assert.Throws<BookingException>(() => _bookingService.CancelBooking(other.Id, booking.Id, _now));

        Assert.Equal(ErrorKind.Forbidden, ex.Kind);
    }

    [Fact]
    public void Purchase_WhenStartDateIsInThePast_IsInvalid()
    {
        var member = AddUser();
        var plan = _plans.Add(new MembershipPlan { Name = "Monthly", Price = 3000m, DurationInDays = 30 });
        var today = DateOnly.FromDateTime(_now);

        var ex = Assert.Throws<MembershipException>(() =>
            _membershipService.Purchase(member.Id, plan.Id, today.AddDays(-1), today));

        Assert.Equal(ErrorKind.Invalid, ex.Kind);
    }
}