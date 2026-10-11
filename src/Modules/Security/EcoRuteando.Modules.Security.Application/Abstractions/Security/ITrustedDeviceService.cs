namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface ITrustedDeviceService
{
    string GenerateTrustedDeviceToken(Guid userId, int days = 30);
    bool ValidateTrustedDeviceToken(string? token, Guid userId);
}
