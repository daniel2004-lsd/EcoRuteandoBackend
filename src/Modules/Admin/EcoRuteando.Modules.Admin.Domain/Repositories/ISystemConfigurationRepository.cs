using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface ISystemConfigurationRepository
{
    Task<SystemConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task UpdateConfigurationAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default);
}
