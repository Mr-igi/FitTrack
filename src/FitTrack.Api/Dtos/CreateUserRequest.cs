using System.ComponentModel.DataAnnotations;
using FitTrack.Core.Models;

namespace FitTrack.Api.Dtos;

public record CreateUserRequest(
    [Required, StringLength(50, MinimumLength = 1)] string FirstName,
    [Required, StringLength(50, MinimumLength = 1)] string LastName,
    [Required, EmailAddress, StringLength(100)] string Email,
    [EnumDataType(typeof(Role))] Role? Role = null);
