using FitTrack.ConsoleApp.Exceptions;
using FitTrack.ConsoleApp.Models;
using FitTrack.ConsoleApp.Repositories;

namespace FitTrack.ConsoleApp.Services;

public class BookingService
{
    private const int CancellationDeadlineHours = 48;

    private readonly IRepository<User> _users;
    private readonly IRepository<Membership> _memberships;
    private readonly IRepository<TrainingSession> _sessions;
    private readonly IRepository<Booking> _bookings;

    public BookingService(
        IRepository<User> users,
        IRepository<Membership> memberships,
        IRepository<TrainingSession> sessions,
        IRepository<Booking> bookings)
    {
        _users = users;
        _memberships = memberships;
        _sessions = sessions;
        _bookings = bookings;
    }

    public Booking BookSession(int userId, int sessionId, DateTime now)
    {
        var user = _users.GetById(userId)
            ?? throw new BookingException($"User with id {userId} was not found.");

        if (user.Role != Role.Member)
        {
            throw new BookingException("Only members can book training sessions.");
        }

        var session = _sessions.GetById(sessionId)
            ?? throw new BookingException($"Training session with id {sessionId} was not found.");

        if (session.StartTime <= now)
        {
            throw new BookingException("Cannot book a session that has already started.");
        }

        var sessionDate = DateOnly.FromDateTime(session.StartTime);
        bool hasActiveMembership = _memberships.Any(m =>
            m.UserId == userId && m.IsActiveOn(sessionDate));

        if (!hasActiveMembership)
        {
            throw new BookingException("You do not have an active membership on the session date.");
        }

        bool alreadyBooked = _bookings.Any(b =>
            b.UserId == userId && b.TrainingSessionId == sessionId);

        if (alreadyBooked)
        {
            throw new BookingException("You are already booked for this session.");
        }

        int bookedCount = _bookings.Find(b => b.TrainingSessionId == sessionId).Count();

        if (bookedCount >= session.Capacity)
        {
            throw new BookingException("This session is full.");
        }

        var booking = new Booking
        {
            UserId = userId,
            TrainingSessionId = sessionId,
            BookedAt = now
        };

        return _bookings.Add(booking);
    }

    public void CancelBooking(int userId, int bookingId, DateTime now)
    {
        var booking = _bookings.GetById(bookingId)
            ?? throw new BookingException($"Booking with id {bookingId} was not found.");

        if (booking.UserId != userId)
        {
            throw new BookingException("You can only cancel your own bookings.");
        }

        var session = _sessions.GetById(booking.TrainingSessionId)
            ?? throw new BookingException("The session for this booking was not found.");

        TimeSpan timeUntilSession = session.StartTime - now;

        if (timeUntilSession < TimeSpan.FromHours(CancellationDeadlineHours))
        {
            throw new BookingException(
                $"Bookings can only be cancelled at least {CancellationDeadlineHours} hours before the session.");
        }

        _bookings.Remove(booking);
    }
}