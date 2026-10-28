namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface ITrustedDeviceService
{
    Task<string> GenerateTrustedDeviceTokenAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    Task<bool> ValidateTrustedDeviceTokenAsync(string? token, Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
