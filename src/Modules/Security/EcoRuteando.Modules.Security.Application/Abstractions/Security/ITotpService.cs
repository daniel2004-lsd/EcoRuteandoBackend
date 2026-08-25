namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string secret, string email, string issuer);
    bool ValidateCode(string secret, string code);
    string[] GenerateRecoveryCodes();
}
