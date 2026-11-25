using System.ComponentModel.DataAnnotations;

namespace FitTrack.Api.Dtos;

public record CreateBookingRequest(
    [Range(1, int.MaxValue)] int UserId,
    [Range(1, int.MaxValue)] int TrainingSessionId);
