namespace EcoRuteando.Modules.Security.Application.Abstractions.BackgroundJobs;

public static class BackgroundTaskQueueExtensions
{
    public static ValueTask EnqueueAsync<TService>(
        this IBackgroundTaskQueue queue,
        Func<TService, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
        where TService : notnull
    {
        return queue.QueueBackgroundWorkItemAsync(async (serviceProvider, token) =>
        {
            var service = (TService?)serviceProvider.GetService(typeof(TService))
                ?? throw new InvalidOperationException(
                    $"El servicio '{typeof(TService).Name}' no está registrado " +
                    "para ejecutarse como tarea en segundo plano.");

            await operation(service, token);
        }, cancellationToken);
    }

    public static ValueTask EnqueueAsync<TService>(
        this IBackgroundTaskQueue queue,
        Func<TService, Task> operation,
        CancellationToken cancellationToken = default)
        where TService : notnull
    {
        return queue.EnqueueAsync<TService>(
            (service, _) => operation(service),
            cancellationToken);
    }
}
