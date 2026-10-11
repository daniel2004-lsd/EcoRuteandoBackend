namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class DailyStatistic
{
    public Guid Id { get; private set; }
    public DateOnly StatisticDate { get; private set; }
    public int TotalUsers { get; private set; }
    public int NewUsers { get; private set; }
    public int ActiveUsers { get; private set; }
    public int TotalRoutes { get; private set; }
    public int CompletedRoutes { get; private set; }
    public int TotalCommunities { get; private set; }
    public int TotalReports { get; private set; }
    public decimal TotalCo2Saved { get; private set; }
    public decimal TotalDistanceKm { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private DailyStatistic() { }

    public static DailyStatistic Create(
        DateOnly statisticDate,
        int totalUsers,
        int newUsers,
        int activeUsers,
        int totalRoutes,
        int completedRoutes,
        int totalCommunities,
        int totalReports,
        decimal totalCo2Saved,
        decimal totalDistanceKm)
    {
        return new DailyStatistic
        {
            Id = Guid.NewGuid(),
            StatisticDate = statisticDate,
            TotalUsers = Math.Max(0, totalUsers),
            NewUsers = Math.Max(0, newUsers),
            ActiveUsers = Math.Max(0, activeUsers),
            TotalRoutes = Math.Max(0, totalRoutes),
            CompletedRoutes = Math.Max(0, completedRoutes),
            TotalCommunities = Math.Max(0, totalCommunities),
            TotalReports = Math.Max(0, totalReports),
            TotalCo2Saved = Math.Max(0, totalCo2Saved),
            TotalDistanceKm = Math.Max(0, totalDistanceKm),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateMetrics(
        int totalUsers,
        int newUsers,
        int activeUsers,
        int totalRoutes,
        int completedRoutes,
        int totalCommunities,
        int totalReports,
        decimal totalCo2Saved,
        decimal totalDistanceKm)
    {
        TotalUsers = Math.Max(0, totalUsers);
        NewUsers = Math.Max(0, newUsers);
        ActiveUsers = Math.Max(0, activeUsers);
        TotalRoutes = Math.Max(0, totalRoutes);
        CompletedRoutes = Math.Max(0, completedRoutes);
        TotalCommunities = Math.Max(0, totalCommunities);
        TotalReports = Math.Max(0, totalReports);
        TotalCo2Saved = Math.Max(0, totalCo2Saved);
        TotalDistanceKm = Math.Max(0, totalDistanceKm);
        UpdatedAt = DateTime.UtcNow;
    }
}
