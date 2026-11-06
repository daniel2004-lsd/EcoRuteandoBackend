using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetByNitAsync(string nit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    void Update(Company company);
}
