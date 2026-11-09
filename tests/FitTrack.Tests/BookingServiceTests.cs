using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Xunit;

namespace FitTrack.Tests;

public class BookingServiceTests
{
    private readonly InMemoryRepository<User> _users = new();
    private readonly InMemoryRepository<Membership> _memberships = new();
    private readonly InMemoryRepository<TrainingSession> _sessions = new();
    private readonly InMemoryRepository<Booking> _bookings = new();

    private readonly BookingService _service;

    private readonly DateTime _now = new(2026, 10, 17, 12, 0, 0);

    public BookingServiceTests()
    {
        _service = new BookingService(_users, _memberships, _sessions, _bookings);
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

    private void AddMembership(int userId, DateOnly? endDate = null)
    {
        _memberships.Add(new Membership
        {
            UserId = userId,
            MembershipPlanId = 1,
            PricePaid = 3000m,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = endDate ?? new DateOnly(2026, 10, 30)
        });
    }

    private User AddMemberWithMembership()
    {
        var user = AddUser();
        AddMembership(user.Id);
        return user;
    }

    private TrainingSession AddSession(int capacity = 10, DateTime? startTime = null)
    {
        return _sessions.Add(new TrainingSession
        {
            TrainerId = 1,
            Title = "CrossFit",
            StartTime = startTime ?? new DateTime(2026, 10, 20, 18, 0, 0),
            DurationMinutes = 60,
            Location = "Room 2",
            Capacity = capacity
        });
    }

    [Fact]
    public void BookSession_WhenUserNotFound_Throws()
    {
        var session = AddSession();

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(99, session.Id, _now));

        Assert.Equal("User with id 99 was not found.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenUserIsNotMember_Throws()
    {
        var trainer = AddUser(Role.Trainer);
        AddMembership(trainer.Id);
        var session = AddSession();

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(trainer.Id, session.Id, _now));

        Assert.Equal("Only members can book training sessions.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenSessionNotFound_Throws()
    {
        var member = AddMemberWithMembership();

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, 99, _now));

        Assert.Equal("Training session with id 99 was not found.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenSessionAlreadyStarted_Throws()
    {
        var member = AddMemberWithMembership();
        var session = AddSession(startTime: _now.AddHours(-1));

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, session.Id, _now));

        Assert.Equal("Cannot book a session that has already started.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenSessionStartsExactlyNow_Throws()
    {
        var member = AddMemberWithMembership();
        var session = AddSession(startTime: _now);

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, session.Id, _now));

        Assert.Equal("Cannot book a session that has already started.", ex.Message);
    }

    [Fact]
    public void BookSession_WithoutActiveMembership_Throws()
    {
        var member = AddUser();
        var session = AddSession();

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, session.Id, _now));

        Assert.Equal("You do not have an active membership on the session date.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenMembershipEndsBeforeSessionDate_Throws()
    {
        var member = AddUser();
        AddMembership(member.Id, endDate: new DateOnly(2026, 10, 19));
        var session = AddSession();

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, session.Id, _now));

        Assert.Equal("You do not have an active membership on the session date.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenAlreadyBooked_Throws()
    {
        var member = AddMemberWithMembership();
        var session = AddSession();
        _service.BookSession(member.Id, session.Id, _now);

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(member.Id, session.Id, _now));

        Assert.Equal("You are already booked for this session.", ex.Message);
    }

    [Fact]
    public void BookSession_WhenSessionIsFull_Throws()
    {
        var session = AddSession(capacity: 1);
        var first = AddMemberWithMembership();
        var second = AddMemberWithMembership();
        _service.BookSession(first.Id, session.Id, _now);

        var ex = Assert.Throws<BookingException>(() => _service.BookSession(second.Id, session.Id, _now));

        Assert.Equal("This session is full.", ex.Message);
    }

    [Fact]
    public void BookSession_WithValidData_ReturnsSavedBooking()
    {
        var member = AddMemberWithMembership();
        var session = AddSession();

        var booking = _service.BookSession(member.Id, session.Id, _now);

        Assert.Equal(member.Id, booking.UserId);
        Assert.Equal(session.Id, booking.TrainingSessionId);

        Assert.Equal(_now, booking.BookedAt);

        Assert.NotNull(_bookings.GetById(booking.Id));
    }

    [Fact]
    public void BookSession_WhenLastSpotTaken_RaisesSessionFilled()
    {
        var member = AddMemberWithMembership();
        var session = AddSession(capacity: 1);

        TrainingSession? filledSession = null;
        _service.SessionFilled += s => filledSession = s;

        _service.BookSession(member.Id, session.Id, _now);

        Assert.Same(session, filledSession);
    }

    [Fact]
    public void BookSession_WhenSpotsRemain_DoesNotRaiseSessionFilled()
    {
        var member = AddMemberWithMembership();
        var session = AddSession(capacity: 2);

        int raisedCount = 0;
        _service.SessionFilled += _ => raisedCount++;

        _service.BookSession(member.Id, session.Id, _now);

        Assert.Equal(0, raisedCount);
    }
}