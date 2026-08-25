using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Application.Abstractions.Logging;

public interface IErrorLogService
{
    Task LogInfoAsync(
        string message,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default);

    Task LogWarningAsync(
        string message,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default);

    Task LogErrorAsync(
        string message,
        Exception? exception = null,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default);

    Task LogCriticalAsync(
        string message,
        Exception? exception = null,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default);
}
