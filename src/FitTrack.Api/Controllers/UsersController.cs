using FitTrack.Api.Dtos;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _users;
    private readonly TimeProvider _timeProvider;
    public UsersController(IRepository<User> users, TimeProvider timeProvider)
    {
        _users = users;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserResponse>> GetAll()
    {
        var result = _users.GetAll().Select(UserResponse.From).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> GetById(int id)
    {
        var user = _users.GetById(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(UserResponse.From(user));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<UserResponse> Create(CreateUserRequest request)
    {
        bool emailTaken = _users.Any(u =>
            string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));

        if (emailTaken)
        {
            return Problem(
                detail: "A user with this email already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = request.Role ?? Role.Member,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        _users.Add(user);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, UserResponse.From(user));
    }
}