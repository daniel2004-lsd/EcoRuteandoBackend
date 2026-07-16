using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly SecurityDbContext _dbContext;

        public UserRepository(SecurityDbContext context)
        {
            _dbContext = context;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(
                    user => user.Email == email,
                    cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(
                    user => user.Id == id,
                    cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}