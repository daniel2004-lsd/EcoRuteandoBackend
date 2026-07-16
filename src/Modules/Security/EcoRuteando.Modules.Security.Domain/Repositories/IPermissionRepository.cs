using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task AddAsync(Permission permission, CancellationToken cancellationToken = default);

        Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);

        Task DeleteAsync(Permission permission, CancellationToken cancellationToken = default);
    }
}
