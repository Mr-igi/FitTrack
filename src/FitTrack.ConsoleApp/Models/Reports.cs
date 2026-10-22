namespace FitTrack.ConsoleApp.Models;

public record PlanRevenue(string PlanName, int Count, decimal Total);

public record SessionOccupancy(string Title, int Booked, int Capacity);

public record ExpiringMembership(string FullName, DateOnly EndDate);