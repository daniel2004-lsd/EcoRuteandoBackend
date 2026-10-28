namespace EcoRuteando.Shared.Abstractions.Users;

public sealed record UserLookupDto(Guid Id, string Email, string Name);

public interface IUserLookupService
{
    Task<Dictionary<Guid, UserLookupDto>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);
}
