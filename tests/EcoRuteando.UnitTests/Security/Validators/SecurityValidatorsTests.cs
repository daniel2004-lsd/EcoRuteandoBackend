using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;
using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Security.Validators;

public sealed class SecurityValidatorsTests
{
    private readonly LoginUserCommandValidator _loginValidator = new();
    private readonly RefreshTokenCommandValidator _refreshTokenValidator = new();
    private readonly Mock<ISecurityPolicyRepository> _policyRepoMock = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Login_WhenEmailIsEmpty_ShouldHaveValidationError(string emptyEmail)
    {
        var command = new LoginUserCommand(emptyEmail, "Password123!");

        var result = _loginValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico es obligatorio.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("not-an-email@")]
    [InlineData("@nodomain.com")]
    public void Login_WhenEmailIsInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        var command = new LoginUserCommand(invalidEmail, "Password123!");

        var result = _loginValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("El correo electrónico no es válido.");
    }

    [Fact]
    public void Login_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        var command = new LoginUserCommand("test@example.com", "");

        var result = _loginValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("La contraseña es obligatoria.");
    }

    [Fact]
    public void Login_WithValidData_ShouldPassValidation()
    {
        var command = new LoginUserCommand("valid@example.com", "Secret123!");

        var result = _loginValidator.TestValidate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RefreshToken_WhenTokenIsEmpty_ShouldHaveValidationError(string emptyToken)
    {
        var command = new RefreshTokenCommand(emptyToken);

        var result = _refreshTokenValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
              .WithErrorMessage("El refresh token es obligatorio.");
    }

    [Fact]
    public void RefreshToken_WithValidToken_ShouldPassValidation()
    {
        var command = new RefreshTokenCommand("valid-refresh-token-xyz");

        var result = _refreshTokenValidator.TestValidate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Register_WhenFirstNameIsEmpty_ShouldHaveValidationError()
    {
        var validator = new RegisterUserCommandValidator(_policyRepoMock.Object);
        var command = new RegisterUserCommand("", "Pérez", "juan@example.com", "Password123!");

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("El nombre es obligatorio.");
    }

    [Fact]
    public async Task Register_WhenFirstNameExceeds100Chars_ShouldHaveValidationError()
    {
        var validator = new RegisterUserCommandValidator(_policyRepoMock.Object);
        var command = new RegisterUserCommand(new string('a', 101), "Pérez", "juan@example.com", "Password123!");

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("El nombre no puede exceder 100 caracteres.");
    }

    [Fact]
    public async Task Register_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        var validator = new RegisterUserCommandValidator(_policyRepoMock.Object);
        var command = new RegisterUserCommand("Juan", "Pérez", "bad-email", "Password123!");

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Register_WhenPasswordFailsPolicy_ShouldHaveValidationError()
    {
        _policyRepoMock.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SecurityPolicy(8, true, true, true, 90, 5, 15, 2));

        var validator = new RegisterUserCommandValidator(_policyRepoMock.Object);
        var command = new RegisterUserCommand("Juan", "Pérez", "juan@example.com", "weak");

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldPassValidation()
    {
        _policyRepoMock.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SecurityPolicy(8, true, true, true, 90, 5, 15, 2));

        var validator = new RegisterUserCommandValidator(_policyRepoMock.Object);
        var command = new RegisterUserCommand("Juan", "Pérez", "juan@example.com", "ValidPass123!");

        var result = await validator.TestValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
