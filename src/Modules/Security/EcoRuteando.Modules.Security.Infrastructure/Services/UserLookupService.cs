using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Shared.Abstractions.Users;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Services;

public sealed class UserLookupService : IUserLookupService
{
    private readonly SecurityDbContext _dbContext;

    public UserLookupService(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<Guid, UserLookupDto>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = userIds.Distinct().ToArray();
        if (distinctIds.Length == 0)
        {
            return new Dictionary<Guid, UserLookupDto>();
        }

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => distinctIds.Contains(u.Id))
            .Select(u => new UserLookupDto(
                u.Id,
                u.Email,
                $"{u.FirstName} {u.LastName}".Trim()))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return users;
    }
}
