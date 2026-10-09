using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface ITwoFactorAuthRepository
{
    Task<TwoFactorAuth?> GetByUserIdAndMethodAsync(
        Guid userId,
        TwoFactorMethod method,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TwoFactorAuth>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TwoFactorAuth twoFactorAuth, CancellationToken cancellationToken = default);

    void Update(TwoFactorAuth twoFactorAuth);
}
