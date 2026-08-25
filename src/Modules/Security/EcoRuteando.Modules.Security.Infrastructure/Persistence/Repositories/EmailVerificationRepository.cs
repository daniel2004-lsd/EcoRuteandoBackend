using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly SecurityDbContext _context;

    public EmailVerificationRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        EmailVerification verification,
        CancellationToken cancellationToken = default)
    {
        await _context.EmailVerifications.AddAsync(verification, cancellationToken);
    }

    public async Task<EmailVerification?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerifications
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<EmailVerification?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerifications
            .Where(e => e.UserId == userId && !e.IsVerified && e.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Update(EmailVerification verification)
    {
        _context.EmailVerifications.Update(verification);
    }
}
