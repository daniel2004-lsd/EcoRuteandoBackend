using EcoRuteando.Modules.Security.Application.Validation;
using EcoRuteando.Modules.Security.Domain.Entities;
using Xunit;

namespace EcoRuteando.UnitTests.Security.Validation;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrWhitespacePassword_ReturnsNull(string? password)
    {
        var result = PasswordPolicy.Validate(null, password);

        Assert.Null(result);
    }

    [Fact]
    public void Validate_WithDefaultPolicy_WhenShorterThan8Chars_ReturnsLengthError()
    {
        var result = PasswordPolicy.Validate(null, "Pass1!");

        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", result);
    }

    [Fact]
    public void Validate_WithDefaultPolicy_WhenMissingUppercase_ReturnsUppercaseError()
    {
        var result = PasswordPolicy.Validate(null, "password123!");

        Assert.Equal("La contraseña debe contener al menos una mayúscula.", result);
    }

    [Fact]
    public void Validate_WithDefaultPolicy_WhenMissingNumber_ReturnsNumberError()
    {
        var result = PasswordPolicy.Validate(null, "Password!");

        Assert.Equal("La contraseña debe contener al menos un número.", result);
    }

    [Fact]
    public void Validate_WithDefaultPolicy_WhenMissingSpecialChar_ReturnsSpecialCharError()
    {
        var result = PasswordPolicy.Validate(null, "Password123");

        Assert.Equal("La contraseña debe contener al menos un carácter especial.", result);
    }

    [Fact]
    public void Validate_WithDefaultPolicy_WhenMeetingAllCriteria_ReturnsNull()
    {
        var result = PasswordPolicy.Validate(null, "SecurePass123!");

        Assert.Null(result);
    }

    [Fact]
    public void Validate_WithCustomPolicy_AppliesCustomMinLength()
    {
        var customPolicy = new SecurityPolicy(
            minPasswordLength: 12,
            requireUppercase: false,
            requireNumbers: false,
            requireSpecialChars: false,
            passwordExpirationDays: 90,
            maxFailedAttempts: 5,
            lockoutTimeMinutes: 15,
            maxActiveSessions: 2);

        var shortResult = PasswordPolicy.Validate(customPolicy, "shortpass");
        var validResult = PasswordPolicy.Validate(customPolicy, "longenoughpassword");

        Assert.Equal("La contraseña debe tener al menos 12 caracteres.", shortResult);
        Assert.Null(validResult);
    }

    [Fact]
    public void Validate_WithCustomPolicy_WhenRulesDisabled_PassesWithoutUppercaseOrDigits()
    {
        var lenientPolicy = new SecurityPolicy(
            minPasswordLength: 6,
            requireUppercase: false,
            requireNumbers: false,
            requireSpecialChars: false,
            passwordExpirationDays: 0,
            maxFailedAttempts: 10,
            lockoutTimeMinutes: 5,
            maxActiveSessions: 5);

        var result = PasswordPolicy.Validate(lenientPolicy, "simple");

        Assert.Null(result);
    }
}
