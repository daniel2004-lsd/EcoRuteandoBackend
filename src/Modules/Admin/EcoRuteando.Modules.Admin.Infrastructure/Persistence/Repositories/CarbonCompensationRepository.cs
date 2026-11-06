using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public sealed class CarbonCompensationRepository : ICarbonCompensationRepository
{
    private readonly AdminDbContext _dbContext;

    public CarbonCompensationRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CarbonCompensation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CarbonCompensations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CarbonCompensation?> GetByCertificateCodeAsync(string certificateCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CarbonCompensations
            .FirstOrDefaultAsync(c => c.CertificateCode == certificateCode.Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<CarbonCompensation>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CarbonCompensations
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.CertifiedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CarbonCompensation compensation, CancellationToken cancellationToken = default)
    {
        await _dbContext.CarbonCompensations.AddAsync(compensation, cancellationToken);
    }
}
