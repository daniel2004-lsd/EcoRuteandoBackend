namespace EcoRuteando.Modules.Security.Infrastructure.Jobs;

public sealed class BackgroundJobQueueOptions
{
    public const string SectionName = "BackgroundJobs:Queue";

    public int Capacity { get; set; } = 100;
}
