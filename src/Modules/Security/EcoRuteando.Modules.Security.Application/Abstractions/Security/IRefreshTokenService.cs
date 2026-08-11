namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();

    string ComputeHash(string refreshToken);
}