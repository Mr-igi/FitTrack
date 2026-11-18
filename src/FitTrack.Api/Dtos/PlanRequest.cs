using System.ComponentModel.DataAnnotations;

namespace FitTrack.Api.Dtos;

public record PlanRequest(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Range(0.01, 1000000)] decimal Price,
    [Range(1, 3650)] int DurationInDays);
