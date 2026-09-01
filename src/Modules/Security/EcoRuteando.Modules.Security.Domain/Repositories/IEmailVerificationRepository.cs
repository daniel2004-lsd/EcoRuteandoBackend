using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IEmailVerificationRepository
{
    Task AddAsync(EmailVerification verification, CancellationToken cancellationToken = default);

    Task<EmailVerification?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<EmailVerification?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Update(EmailVerification verification);
}
