using FitTrack.Core.Models;

namespace FitTrack.Api.Dtos;

public record PlanResponse(
    int Id,
    string Name,
    decimal Price,
    int DurationInDays,
    int DiscountPercent,
    decimal FinalPrice)
{
    public static PlanResponse From(MembershipPlan plan)
    {
        return new PlanResponse(
            plan.Id,
            plan.Name,
            plan.Price,
            plan.DurationInDays,
            plan.GetDiscountPercent(),
            plan.GetFinalPrice());
    }
}