using FitTrack.Core.Models;
using FitTrack.Core.Repositories;

namespace FitTrack.ConsoleApp.Services;

public class NotificationService
{
    private readonly IRepository<User> _users;
    public NotificationService(IRepository<User> users)
    {
        _users = users;
    }

    public void OnMembershipExpired(Membership membership)
    {

        var user = _users.GetById(membership.UserId);
        string name = user?.FullName ?? "Unknown";

        Console.WriteLine($"[NOTIFY] {name}: your membership expired on {membership.EndDate:yyyy-MM-dd}.");
    }

    public void OnSessionFilled(TrainingSession session)
    {

        var trainer = _users.GetById(session.TrainerId);
        string name = trainer?.FullName ?? "Unknown";

        Console.WriteLine($"[NOTIFY] Trainer {name}: \"{session.Title}\" on {session.StartTime:yyyy-MM-dd HH:mm} is now full.");
    }
}