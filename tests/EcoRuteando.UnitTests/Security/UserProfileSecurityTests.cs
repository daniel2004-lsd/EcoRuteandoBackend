using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;
using EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public sealed class UserProfileSecurityTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ISecurityUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISender> _senderMock = new();

    private UpdateMyProfileCommandHandler CreateHandler()
    {
        return new UpdateMyProfileCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _senderMock.Object);
    }

    private static User CreateVerifiedUser(string email = "original@ecoruteando.com", string passwordHash = "hashed_pass")
    {
        var user = new User("Juan", "Perez", email, passwordHash, "+573001234567");
        user.VerifyEmail();
        return user;
    }

    [Fact]
    public void UserEntity_WhenEmailChanges_ResetsEmailVerifiedToFalse()
    {
        // Arrange
        var user = CreateVerifiedUser();
        Assert.True(user.EmailVerified);

        // Act
        user.UpdateProfile("Juan", "Perez", "newemail@ecoruteando.com", "+573001234567");

        // Assert
        Assert.Equal("newemail@ecoruteando.com", user.Email);
        Assert.False(user.EmailVerified, "EmailVerified must be false when email changes to prevent account takeover");
    }

    [Fact]
    public void UserEntity_WhenEmailDoesNotChange_PreservesEmailVerifiedTrue()
    {
        // Arrange
        var user = CreateVerifiedUser("test@ecoruteando.com");
        Assert.True(user.EmailVerified);

        // Act - Same email with whitespace and different casing
        user.UpdateProfile("Carlos", "Gomez", "  TEST@ecoruteando.com  ", "+573009998877");

        // Assert
        Assert.Equal("test@ecoruteando.com", user.Email);
        Assert.True(user.EmailVerified, "EmailVerified must remain true when email has not changed");
        Assert.Equal("Carlos", user.FirstName);
        Assert.Equal("Gomez", user.LastName);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenPasswordIsIncorrect_ThrowsDomainExceptionAndDoesNotUpdate()
    {
        // Arrange
        var user = CreateVerifiedUser();
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.Verify("WrongPassword!", user.PasswordHash!))
            .Returns(false);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            user.Id,
            "Juan",
            "Perez",
            "new@ecoruteando.com",
            null,
            CurrentPassword: "WrongPassword!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("La contraseña actual es incorrecta.", ex.Message);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenPasswordMissingForStandardUser_ThrowsDomainException()
    {
        // Arrange
        var user = CreateVerifiedUser();
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            user.Id,
            "Juan",
            "Perez",
            "new@ecoruteando.com",
            null,
            CurrentPassword: null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("La contraseña actual es incorrecta.", ex.Message);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenEmailChanges_DispatchesVerificationEmailAndResetsVerified()
    {
        // Arrange
        var user = CreateVerifiedUser("old@ecoruteando.com");
        var newEmail = "new@ecoruteando.com";

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.Verify("CorrectPass123!", user.PasswordHash!))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(newEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            user.Id,
            "Juan",
            "Perez",
            newEmail,
            "+573001234567",
            CurrentPassword: "CorrectPass123!");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newEmail, response.Email);
        Assert.False(user.EmailVerified);

        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(s => s.Send(
            It.Is<SendVerificationEmailCommand>(c => c.UserId == user.Id && c.Email == newEmail && !string.IsNullOrWhiteSpace(c.Code)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenEmailDoesNotChange_DoesNotDispatchVerificationEmail()
    {
        // Arrange
        var user = CreateVerifiedUser("same@ecoruteando.com");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.Verify("CorrectPass123!", user.PasswordHash!))
            .Returns(true);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            user.Id,
            "NewFirstName",
            "NewLastName",
            "SAME@ecoruteando.com",
            "+573001234567",
            CurrentPassword: "CorrectPass123!");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("same@ecoruteando.com", response.Email);
        Assert.Equal("NewFirstName", response.FirstName);
        Assert.True(user.EmailVerified, "EmailVerified must remain true when email has not changed");

        _senderMock.Verify(s => s.Send(
            It.IsAny<SendVerificationEmailCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenNewEmailAlreadyInUse_ThrowsConflictException()
    {
        // Arrange
        var user = CreateVerifiedUser("user@ecoruteando.com");
        var conflictingUser = CreateVerifiedUser("existing@ecoruteando.com");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.Verify("CorrectPass123!", user.PasswordHash!))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("existing@ecoruteando.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingUser);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            user.Id,
            "Juan",
            "Perez",
            "existing@ecoruteando.com",
            null,
            CurrentPassword: "CorrectPass123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("El correo no está disponible.", ex.Message);
        _senderMock.Verify(s => s.Send(It.IsAny<SendVerificationEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfile_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new UpdateMyProfileCommand(
            Guid.NewGuid(),
            "Juan",
            "Perez",
            "test@ecoruteando.com",
            null,
            CurrentPassword: "AnyPassword123!");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
