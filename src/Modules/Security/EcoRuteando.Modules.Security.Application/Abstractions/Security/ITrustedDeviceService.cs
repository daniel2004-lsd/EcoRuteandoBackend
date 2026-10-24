namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface ITrustedDeviceService
{
    string GenerateTrustedDeviceToken(Guid userId, int days = 30);
    Task<string> GenerateTrustedDeviceTokenAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    bool ValidateTrustedDeviceToken(string? token, Guid userId);
    Task<bool> ValidateTrustedDeviceTokenAsync(string? token, Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
