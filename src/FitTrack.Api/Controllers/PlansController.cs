using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IRepository<MembershipPlan> _plans;
    public PlansController(IRepository<MembershipPlan> plans)
    {
        _plans = plans;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_plans.GetAll());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var plan = _plans.GetById(id);

        if (plan is null)
        {
            return NotFound();
        }
        return Ok(plan);
    }
}