using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class HealthController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Healthy", service = "FitTrack API" });

    }
}