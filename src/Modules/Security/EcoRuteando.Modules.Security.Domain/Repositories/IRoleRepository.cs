using EcoRuteando.Modules.Security.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Domain.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task AddAsync(Role role, CancellationToken cancellationToken = default);

        Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

        Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
    }
}
