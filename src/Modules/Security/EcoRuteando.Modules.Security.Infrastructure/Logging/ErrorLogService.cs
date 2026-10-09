using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;

namespace EcoRuteando.Modules.Security.Infrastructure.Logging;

public sealed class ErrorLogService : IErrorLogService
{
    private readonly IErrorLogRepository _errorLogRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public ErrorLogService(
        IErrorLogRepository errorLogRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _errorLogRepository = errorLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task LogInfoAsync(
        string message,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(ErrorLevel.Info, message, null, userId, source, contextData, cancellationToken);
    }

    public async Task LogWarningAsync(
        string message,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(ErrorLevel.Warning, message, null, userId, source, contextData, cancellationToken);
    }

    public async Task LogErrorAsync(
        string message,
        Exception? exception = null,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(ErrorLevel.Error, message, exception, userId, source, contextData, cancellationToken);
    }

    public async Task LogCriticalAsync(
        string message,
        Exception? exception = null,
        Guid? userId = null,
        string? source = null,
        string? contextData = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(ErrorLevel.Critical, message, exception, userId, source, contextData, cancellationToken);
    }

    private async Task LogAsync(
        ErrorLevel level,
        string message,
        Exception? exception,
        Guid? userId,
        string? source,
        string? contextData,
        CancellationToken cancellationToken)
    {
        var errorLog = new ErrorLog(
            userId,
            level,
            source,
            message,
            exception?.StackTrace,
            contextData);

        await _errorLogRepository.AddAsync(errorLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
