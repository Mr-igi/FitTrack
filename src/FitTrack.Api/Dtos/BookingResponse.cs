using FitTrack.Core.Models;

namespace FitTrack.Api.Dtos;

public record BookingResponse(int Id, int UserId, int TrainingSessionId, DateTime BookedAt)
{
    public static BookingResponse From(Booking booking)
    {
        return new BookingResponse(booking.Id, booking.UserId, booking.TrainingSessionId, booking.BookedAt);
    }
}