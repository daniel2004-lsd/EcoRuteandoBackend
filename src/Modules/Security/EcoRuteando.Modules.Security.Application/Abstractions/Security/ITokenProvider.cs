namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface ITokenProvider
{
    string GenerateSecureToken();
}