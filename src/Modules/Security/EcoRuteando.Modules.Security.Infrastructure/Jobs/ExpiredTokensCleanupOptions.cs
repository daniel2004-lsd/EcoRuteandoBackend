namespace EcoRuteando.Modules.Security.Infrastructure.Jobs;

public sealed class ExpiredTokensCleanupOptions
{
    public const string SectionName = "BackgroundJobs:ExpiredTokensCleanup";

    public int IntervalMinutes { get; set; } = 60;
}
