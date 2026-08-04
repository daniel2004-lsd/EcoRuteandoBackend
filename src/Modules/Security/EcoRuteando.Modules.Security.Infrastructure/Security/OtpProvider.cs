using System.Security.Cryptography;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class OtpProvider : IOtpProvider
{
    public string GenerateCode()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }
}