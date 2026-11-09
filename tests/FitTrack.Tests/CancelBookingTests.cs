using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class CancelBookingTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<Membership> _memberships = new();
    private readonly InMemoryRepository<TrainingSession> _sessions = new();
    private readonly InMemoryRepository<Booking> _bookings = new();

    private readonly BookingService _service;

    private readonly DateTime _now = new(2026, 10, 17, 12, 0, 0);

    public CancelBookingTests()
    {
        _service = new BookingService(_users, _memberships, _sessions, _bookings);
    }

    private User AddMemberWithMembership()
    {
        var user = _users.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@fittrack.com",
            CreatedAt = new DateTime(2026, 10, 1)
        });

        _memberships.Add(new Membership
        {
            UserId = user.Id,
            MembershipPlanId = 1,
            PricePaid = 3000m,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2026, 10, 30)
        });

        return user;
    }

    private TrainingSession AddSession(int capacity = 10)
    {
        return _sessions.Add(new TrainingSession
        {
            TrainerId = 1,
            Title = "CrossFit",
            StartTime = new DateTime(2026, 10, 20, 18, 0, 0),
            DurationMinutes = 60,
            Location = "Room 2",
            Capacity = capacity
        });
    }

    private Booking Book(User member, TrainingSession session)
    {
        return _service.BookSession(member.Id, session.Id, _now);
    }


    [Fact]
    public void CancelBooking_WhenBookingNotFound_Throws()
    {
        var member = AddMemberWithMembership();

        var ex = Assert.Throws<BookingException>(() => _service.CancelBooking(member.Id, 99, _now));

        Assert.Equal("Booking with id 99 was not found.", ex.Message);
    }

    [Fact]
    public void CancelBooking_WhenBookingBelongsToAnotherUser_Throws()
    {
        var session = AddSession();
        var owner = AddMemberWithMembership();
        var other = AddMemberWithMembership();
        var booking = Book(owner, session);

        var ex = Assert.Throws<BookingException>(() => _service.CancelBooking(other.Id, booking.Id, _now));

        Assert.Equal("You can only cancel your own bookings.", ex.Message);

        Assert.NotNull(_bookings.GetById(booking.Id));
    }

    [Fact]
    public void CancelBooking_WhenSessionNotFound_Throws()
    {
        var member = AddMemberWithMembership();
        var booking = _bookings.Add(new Booking
        {
            UserId = member.Id,
            TrainingSessionId = 99,
            BookedAt = _now
        });

        var ex = Assert.Throws<BookingException>(() => _service.CancelBooking(member.Id, booking.Id, _now));

        Assert.Equal("The session for this booking was not found.", ex.Message);
    }

    [Fact]
    public void CancelBooking_WhenLessThan48HoursBeforeSession_Throws()
    {
        var session = AddSession();
        var member = AddMemberWithMembership();
        var booking = Book(member, session);
        var cancelTime = session.StartTime.AddHours(-47);

        var ex = Assert.Throws<BookingException>(() => _service.CancelBooking(member.Id, booking.Id, cancelTime));

        Assert.Equal("Bookings can only be cancelled at least 48 hours before the session.", ex.Message);

        Assert.NotNull(_bookings.GetById(booking.Id));
    }


    [Fact]
    public void CancelBooking_WhenExactly48HoursBeforeSession_Succeeds()
    {
        var session = AddSession();
        var member = AddMemberWithMembership();
        var booking = Book(member, session);
        var cancelTime = session.StartTime.AddHours(-48);

        _service.CancelBooking(member.Id, booking.Id, cancelTime);

        Assert.Null(_bookings.GetById(booking.Id));
    }

    [Fact]
    public void CancelBooking_WhenMoreThan48HoursBeforeSession_RemovesBooking()
    {
        var session = AddSession();
        var member = AddMemberWithMembership();
        var booking = Book(member, session);

        _service.CancelBooking(member.Id, booking.Id, _now);

        Assert.Null(_bookings.GetById(booking.Id));
    }

    [Fact]
    public void CancelBooking_FreesSpotForAnotherMember()
    {
        var session = AddSession(capacity: 1);
        var first = AddMemberWithMembership();
        var second = AddMemberWithMembership();
        var booking = Book(first, session);

        _service.CancelBooking(first.Id, booking.Id, _now);
        var secondBooking = Book(second, session);

        Assert.Equal(second.Id, secondBooking.UserId);
    }
}