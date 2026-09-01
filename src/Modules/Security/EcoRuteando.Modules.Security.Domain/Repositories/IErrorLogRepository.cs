using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IErrorLogRepository
{
    Task AddAsync(ErrorLog errorLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ErrorLog>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ErrorLog>> GetByLevelAsync(
        ErrorLevel level,
        CancellationToken cancellationToken = default);
}
