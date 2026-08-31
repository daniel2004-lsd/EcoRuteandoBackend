using EcoRuteando.Shared.BaseClasses;
using NpgsqlTypes;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public enum ErrorLevel
{
    [PgName("INFO")]
    Info = 0,

    [PgName("WARNING")]
    Warning = 1,

    [PgName("ERROR")]
    Error = 2,

    [PgName("CRITICAL")]
    Critical = 3
}

public sealed class ErrorLog : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public ErrorLevel ErrorLevel { get; private set; }
    public string? Source { get; private set; }
    public string Message { get; private set; } = default!;
    public string? StackTrace { get; private set; }
    public string? ContextData { get; private set; }

    private ErrorLog() { }

    public ErrorLog(
        Guid? userId,
        ErrorLevel errorLevel,
        string? source,
        string message,
        string? stackTrace,
        string? contextData)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ErrorLevel = errorLevel;
        Source = source;
        Message = message;
        StackTrace = stackTrace;
        ContextData = contextData;
        CreatedAt = DateTime.UtcNow;
    }
}
