namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface IAdminUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
