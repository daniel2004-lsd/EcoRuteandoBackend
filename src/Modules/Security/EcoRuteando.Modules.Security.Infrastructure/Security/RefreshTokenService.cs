using System.Security.Cryptography;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }

    public string ComputeHash(string refreshToken)
    {
        using var sha = SHA256.Create();

        var hash = sha.ComputeHash(
            System.Text.Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(hash);
    }
}