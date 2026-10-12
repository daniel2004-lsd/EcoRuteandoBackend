using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly AdminDbContext _dbContext;

    public SystemConfigurationRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SystemConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = await _dbContext.SystemConfigurations
            .FirstOrDefaultAsync(c => c.Id == 1, cancellationToken);

        if (config == null)
        {
            config = SystemConfiguration.CreateDefault();
            _dbContext.SystemConfigurations.Add(config);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return config;
    }

    public async Task UpdateConfigurationAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _dbContext.SystemConfigurations.Update(configuration);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
