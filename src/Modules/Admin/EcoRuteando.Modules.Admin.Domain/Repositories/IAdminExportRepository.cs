using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface IAdminExportRepository
{
    Task<AdminExport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminExport>> GetRecentAsync(Guid? userId = null, int limit = 20, CancellationToken cancellationToken = default);
    Task AddAsync(AdminExport export, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdminExport export, CancellationToken cancellationToken = default);
}
