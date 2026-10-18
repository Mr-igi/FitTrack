using FitTrack.ConsoleApp.Models;
using FitTrack.ConsoleApp.Repositories;

namespace FitTrack.ConsoleApp.Services;

public class StatisticsService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<MembershipPlan> _plans;
    private readonly IRepository<Membership> _memberships;
    private readonly IRepository<TrainingSession> _sessions;
    private readonly IRepository<Booking> _bookings;

    public StatisticsService(
        IRepository<User> users,
        IRepository<MembershipPlan> plans,
        IRepository<Membership> memberships,
        IRepository<TrainingSession> sessions,
        IRepository<Booking> bookings)
    {
        _users = users;
        _plans = plans;
        _memberships = memberships;
        _sessions = sessions;
        _bookings = bookings;
    }
    public int CountActiveMembers(DateOnly date)
    {
        return _memberships
            .Find(m => m.IsActiveOn(date))
            .Select(m => m.UserId)
            .Distinct()
            .Count();
    }
    public decimal GetTotalRevenue()
    {
        return _memberships.GetAll().Sum(m => m.PricePaid);
    }
    public List<(string PlanName, int Count, decimal Total)> GetRevenueByPlan()
    {
        return _memberships.GetAll()
            .GroupBy(m => m.MembershipPlanId)
            .Select(g => (
                PlanName: _plans.GetById(g.Key)?.Name ?? "Unknown",
                Count: g.Count(),
                Total: g.Sum(m => m.PricePaid)))
            .OrderByDescending(x => x.Total)
            .ToList();
    }
    public List<(string Title, int Booked, int Capacity)> GetSessionOccupancy()
    {
        return _sessions.GetAll()
            .Select(s => (
                Title: s.Title,
                Booked: _bookings.Find(b => b.TrainingSessionId == s.Id).Count(),
                Capacity: s.Capacity))
            .OrderByDescending(x => x.Booked)
            .ToList();
    }
    public List<(string FullName, DateOnly EndDate)> GetExpiringSoon(DateOnly today, int days)
    {
        var limit = today.AddDays(days);

        return _memberships.GetAll()
            .GroupBy(m => m.UserId)
            .Select(g => (UserId: g.Key, EndDate: g.Max(m => m.EndDate)))
            .Where(x => x.EndDate >= today && x.EndDate <= limit)
            .OrderBy(x => x.EndDate)
            .Select(x => (
                FullName: _users.GetById(x.UserId)?.FullName ?? "Unknown",
                EndDate: x.EndDate))
            .ToList();
    }
    public List<string> GetMembersWithoutBookings()
    {
        return _users
            .Find(u => u.Role == Role.Member)
            .Where(u => !_bookings.Any(b => b.UserId == u.Id))
            .Select(u => u.FullName)
            .ToList();
    }
}