using System.ComponentModel.DataAnnotations;
using FitTrack.Api.Dtos;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;
    private readonly IRepository<Booking> _bookings;
    private readonly TimeProvider _timeProvider;
    public BookingsController(
        BookingService bookingService,
        IRepository<Booking> bookings,
        TimeProvider timeProvider)
    {
        _bookingService = bookingService;
        _bookings = bookings;
        _timeProvider = timeProvider;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingResponse> GetById(int id)
    {
        var booking = _bookings.GetById(id);

        if (booking is null)
        {
            return NotFound();
        }

        return Ok(BookingResponse.From(booking));
    }

    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<BookingResponse> Create(CreateBookingRequest request)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var booking = _bookingService.BookSession(request.UserId, request.TrainingSessionId, now);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, BookingResponse.From(booking));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Cancel(int id, [FromQuery, Range(1, int.MaxValue)] int userId)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        _bookingService.CancelBooking(userId, id, now);

        return NoContent();
    }
}