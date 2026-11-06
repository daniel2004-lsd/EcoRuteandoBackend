using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly AdminDbContext _dbContext;

    public CompanyRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .Include(c => c.FleetTypes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByNitAsync(string nit, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .Include(c => c.FleetTypes)
            .FirstOrDefaultAsync(c => c.Nit == nit.Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .Include(c => c.FleetTypes)
            .Where(c => c.IsActive)
            .OrderBy(c => c.BusinessName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(company, cancellationToken);
    }

    public void Update(Company company)
    {
        _dbContext.Companies.Update(company);
    }
}
