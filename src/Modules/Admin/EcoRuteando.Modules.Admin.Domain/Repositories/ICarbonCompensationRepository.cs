using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface ICarbonCompensationRepository
{
    Task<CarbonCompensation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CarbonCompensation?> GetByCertificateCodeAsync(string certificateCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CarbonCompensation>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task AddAsync(CarbonCompensation compensation, CancellationToken cancellationToken = default);
}
