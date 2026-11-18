using FitTrack.Api.Dtos;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IRepository<MembershipPlan> _plans;
    private readonly IRepository<Membership> _memberships;

    public PlansController(IRepository<MembershipPlan> plans, IRepository<Membership> memberships)
    {
        _plans = plans;
        _memberships = memberships;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PlanResponse>> GetAll()
    {

        var result = _plans.GetAll().Select(PlanResponse.From).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PlanResponse> GetById(int id)
    {
        var plan = _plans.GetById(id);

        if (plan is null)
        {
            return NotFound();
        }

        return Ok(PlanResponse.From(plan));
    }

    [HttpPost]
    [ProducesResponseType<PlanResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PlanResponse> Create(PlanRequest request)
    {

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Price = request.Price,
            DurationInDays = request.DurationInDays
        };

        _plans.Add(plan);

        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, PlanResponse.From(plan));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PlanResponse> Update(int id, PlanRequest request)
    {
        var plan = _plans.GetById(id);

        if (plan is null)
        {
            return NotFound();
        }

        plan.Name = request.Name;
        plan.Price = request.Price;
        plan.DurationInDays = request.DurationInDays;

        return Ok(PlanResponse.From(plan));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        var plan = _plans.GetById(id);

        if (plan is null)
        {
            return NotFound();
        }

        if (_memberships.Any(m => m.MembershipPlanId == id))
        {
            return Problem(
                detail: "The plan is used by existing memberships and cannot be deleted.",
                statusCode: StatusCodes.Status409Conflict);
        }

        _plans.Remove(plan);

        return NoContent();
    }
}