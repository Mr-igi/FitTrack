using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.Core.Services;

public class BookingService
{
    private const int CancellationDeadlineHours = 48;

    private readonly IRepository<User> _users;
    private readonly IRepository<Membership> _memberships;
    private readonly IRepository<TrainingSession> _sessions;
    private readonly IRepository<Booking> _bookings;

    public event Action<TrainingSession>? SessionFilled;

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
            ?? throw new BookingException($"User with id {userId} was not found.", ErrorKind.NotFound);

        if (user.Role != Role.Member)
        {
            throw new BookingException("Only members can book training sessions.", ErrorKind.Forbidden);
        }

        var session = _sessions.GetById(sessionId)
            ?? throw new BookingException($"Training session with id {sessionId} was not found.", ErrorKind.NotFound);

        if (session.StartTime <= now)
        {
            throw new BookingException("Cannot book a session that has already started.", ErrorKind.Conflict);
        }

        var sessionDate = DateOnly.FromDateTime(session.StartTime);
        bool hasActiveMembership = _memberships.Any(m =>
            m.UserId == userId && m.IsActiveOn(sessionDate));

        if (!hasActiveMembership)
        {
            throw new BookingException("You do not have an active membership on the session date.", ErrorKind.Conflict);
        }

        bool alreadyBooked = _bookings.Any(b =>
            b.UserId == userId && b.TrainingSessionId == sessionId);

        if (alreadyBooked)
        {
            throw new BookingException("You are already booked for this session.", ErrorKind.Conflict);
        }

        int bookedCount = _bookings.Find(b => b.TrainingSessionId == sessionId).Count();

        if (bookedCount >= session.Capacity)
        {
            throw new BookingException("This session is full.", ErrorKind.Conflict);
        }

        var booking = new Booking
        {
            UserId = userId,
            TrainingSessionId = sessionId,
            BookedAt = now
        };

        var savedBooking = _bookings.Add(booking);

        if (bookedCount + 1 == session.Capacity)
        {
            SessionFilled?.Invoke(session);
        }

        return savedBooking;
    }

    public void CancelBooking(int userId, int bookingId, DateTime now)
    {
        var booking = _bookings.GetById(bookingId)
            ?? throw new BookingException($"Booking with id {bookingId} was not found.", ErrorKind.NotFound);

        if (booking.UserId != userId)
        {
            throw new BookingException("You can only cancel your own bookings.", ErrorKind.Forbidden);
        }

        var session = _sessions.GetById(booking.TrainingSessionId)
            ?? throw new BookingException("The session for this booking was not found.", ErrorKind.NotFound);

        TimeSpan timeUntilSession = session.StartTime - now;

        if (timeUntilSession < TimeSpan.FromHours(CancellationDeadlineHours))
        {
            throw new BookingException(
                $"Bookings can only be cancelled at least {CancellationDeadlineHours} hours before the session.",
                ErrorKind.Conflict);
        }

        _bookings.Remove(booking);
    }
}