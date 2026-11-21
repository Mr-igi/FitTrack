using FitTrack.Core.Models;

namespace FitTrack.Api.Dtos;

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    Role Role,
    DateTime CreatedAt)
{
    public static UserResponse From(User user)
    {
        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt);
    }
}