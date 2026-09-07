namespace EcoRuteando.Modules.Security.Infrastructure.Bootstrap;

public interface IAdminBootstrapService
{
    Task EnsureAdminAsync(CancellationToken cancellationToken = default);
}