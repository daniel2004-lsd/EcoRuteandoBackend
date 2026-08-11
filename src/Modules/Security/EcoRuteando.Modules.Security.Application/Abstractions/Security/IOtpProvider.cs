namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface IOtpProvider
{
    string GenerateCode();
}