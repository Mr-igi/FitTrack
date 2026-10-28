using FitTrack.Core.Exceptions;
using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.Core.Services;

public class MembershipService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<MembershipPlan> _plans;
    private readonly IRepository<Membership> _memberships;

    public event Action<Membership>? MembershipExpired;

    public MembershipService(
        IRepository<User> users,
        IRepository<MembershipPlan> plans,
        IRepository<Membership> memberships)
    {
        _users = users;
        _plans = plans;
        _memberships = memberships;
    }

    public Membership Purchase(int userId, int planId, DateOnly startDate, DateOnly today)
    {
        var user = _users.GetById(userId)
            ?? throw new MembershipException($"User with id {userId} was not found.");

        if (user.Role != Role.Member)
        {
            throw new MembershipException("Only members can purchase a membership.");
        }

        var plan = _plans.GetById(planId)
            ?? throw new MembershipException($"Membership plan with id {planId} was not found.");

        if (startDate < today)
        {
            throw new MembershipException("Start date cannot be in the past.");
        }

        var existing = _memberships
            .Find(m => m.UserId == userId && m.EndDate >= startDate)
            .ToList();

        if (existing.Any())
        {
            startDate = existing.Max(m => m.EndDate).AddDays(1);
        }

        var membership = Membership.Create(userId, plan, startDate);
        return _memberships.Add(membership);
    }

    public int MarkExpiredMemberships(DateOnly today)
    {
        var expired = _memberships
            .Find(m => m.EndDate < today && !m.IsMarkedExpired)
            .ToList();

        foreach (var membership in expired)
        {
            membership.IsMarkedExpired = true;
            bool hasLaterMembership = _memberships.Any(other =>
                other.UserId == membership.UserId && other.EndDate > membership.EndDate);

            if (!hasLaterMembership)
            {
                MembershipExpired?.Invoke(membership);
            }
        }

        return expired.Count;
    }
}