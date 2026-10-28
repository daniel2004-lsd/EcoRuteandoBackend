using System.Reflection;
using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class RegistrationSecurityTests
{
    [Fact]
    public void RegisterUserResponse_MustNotExposeVerificationCodeOrSecretProperties()
    {
        var responseType = typeof(RegisterUserResponse);
        var properties = responseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(properties, p => p.Name.Equals("VerificationCode", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Name.Equals("Code", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Name.Equals("Token", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Name.Equals("Secret", StringComparison.OrdinalIgnoreCase));

        // Must still include UserId
        Assert.Contains(properties, p => p.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase));
    }
}
