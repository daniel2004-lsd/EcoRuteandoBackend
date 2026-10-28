using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Shared.Exceptions;
using Xunit;

namespace EcoRuteando.UnitTests.Security.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithEmptyFirstName_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new User("", "Pérez", "juan@example.com", "hash", "3001234567"));

        Assert.Equal("El nombre es obligatorio", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new User("Juan", "Pérez", "   ", "hash", "3001234567"));

        Assert.Equal("El correo electrónico es obligatorio.", ex.Message);
    }

    [Fact]
    public void Constructor_WithValidData_NormalizesEmailToLowerCaseAndSetsDefaults()
    {
        var user = new User("  Juan  ", "  Pérez  ", "Juan.Perez@Example.COM  ", "hash123", "  3001234567  ");

        Assert.Equal("Juan", user.FirstName);
        Assert.Equal("Pérez", user.LastName);
        Assert.Equal("juan.perez@example.com", user.Email);
        Assert.Equal("3001234567", user.PhoneNumber);
        Assert.Equal("active", user.Status);
        Assert.False(user.AcceptedTerms);
        Assert.Null(user.TermsAcceptedAt);
        Assert.False(user.EmailVerified);
        Assert.False(user.IsGuest);
        Assert.Equal("es", user.PreferredLanguage);
        Assert.Equal("light", user.UiTheme);
        Assert.Equal(0, user.FailedAttempts);
        Assert.Null(user.LockedUntil);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void AcceptTerms_SetsAcceptedTermsToTrueAndRecordsTimestamp()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        user.AcceptTerms();

        Assert.True(user.AcceptedTerms);
        Assert.NotNull(user.TermsAcceptedAt);
        Assert.True(user.TermsAcceptedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void VerifyEmail_SetsEmailVerifiedToTrue()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        user.VerifyEmail();

        Assert.True(user.EmailVerified);
    }

    [Fact]
    public void AssignPrimaryRole_WithNullRole_ThrowsDomainException()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        var ex = Assert.Throws<DomainException>(() => user.AssignPrimaryRole(null!));

        Assert.Equal("El rol es obligatorio.", ex.Message);
    }

    [Fact]
    public void AssignPrimaryRole_WithValidRole_SetsPrimaryRoleAndId()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);
        var role = new Role("Citizen", "Usuario regular");

        user.AssignPrimaryRole(role);

        Assert.Same(role, user.PrimaryRole);
        Assert.Equal(role.Id, user.PrimaryRoleId);
    }

    [Theory]
    [InlineData("invalid-hex")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("123456")]
    public void Update_WithInvalidPrimaryColor_ThrowsDomainException(string invalidColor)
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        var ex = Assert.Throws<DomainException>(() =>
            user.Update("Ana", "Gómez", "3001234567", invalidColor));

        Assert.Equal("El color primario debe ser un hexadecimal válido (ej. #1ABC9C).", ex.Message);
    }

    [Fact]
    public void Update_WithValidColorAndData_UpdatesProperties()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        user.Update("Ana María", "Gómez R.", "3109876543", "#1ABC9C");

        Assert.Equal("Ana María", user.FirstName);
        Assert.Equal("Gómez R.", user.LastName);
        Assert.Equal("3109876543", user.PhoneNumber);
        Assert.Equal("#1ABC9C", user.PrimaryColor);
    }

    [Fact]
    public void UpdateProfile_WithEmptyFirstNameOrEmail_ThrowsDomainException()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        var ex1 = Assert.Throws<DomainException>(() =>
            user.UpdateProfile("", "Gómez", "ana@example.com", null));
        Assert.Equal("El nombre es obligatorio.", ex1.Message);

        var ex2 = Assert.Throws<DomainException>(() =>
            user.UpdateProfile("Ana", "Gómez", "", null));
        Assert.Equal("El correo electrónico es obligatorio.", ex2.Message);
    }

    [Fact]
    public void FailedAttempts_BelowThreshold_DoesNotLockAccount()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        user.IncrementFailedAttempts(maxAttempts: 5, ignoredLockoutMinutes: 0);
        user.IncrementFailedAttempts(maxAttempts: 5, ignoredLockoutMinutes: 0);

        Assert.Equal(2, user.FailedAttempts);
        Assert.False(user.IsLocked);
        Assert.Null(user.LockedUntil);
        Assert.Equal(3, user.GetAttemptsRemaining(5));
    }

    [Fact]
    public void FailedAttempts_ReachingThreshold_LocksAccountWithEscalationTier()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        // Threshold = 3
        user.IncrementFailedAttempts(3, 0); // 1
        user.IncrementFailedAttempts(3, 0); // 2
        user.IncrementFailedAttempts(3, 0); // 3 -> Lockout Level 0 (30 seconds)

        Assert.Equal(3, user.FailedAttempts);
        Assert.True(user.IsLocked);
        Assert.NotNull(user.LockedUntil);
        Assert.True(user.GetLockedSecondsRemaining() > 0);
    }

    [Fact]
    public void ResetFailedAttempts_ClearsCountAndUnlockAccount()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);
        user.IncrementFailedAttempts(1, 0);
        Assert.True(user.IsLocked);

        user.ResetFailedAttempts();

        Assert.Equal(0, user.FailedAttempts);
        Assert.Null(user.LockedUntil);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_ThrowsDomainException()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        var ex = Assert.Throws<DomainException>(() => user.ChangePassword(""));

        Assert.Equal("La contraseña es obligatoria.", ex.Message);
    }

    [Fact]
    public void ChangePassword_WithValidHash_UpdatesPasswordAndResetsLockout()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);
        user.IncrementFailedAttempts(1, 0);

        user.ChangePassword("new_hash_456");

        Assert.Equal("new_hash_456", user.PasswordHash);
        Assert.Equal(0, user.FailedAttempts);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void RequestDeletion_SetsStatusAndDeletionTimestamp()
    {
        var user = new User("Ana", "Gómez", "ana@example.com", "hash", null);

        user.RequestDeletion();

        Assert.Equal("deleted", user.Status);
        Assert.NotNull(user.DeletionRequestedAt);
    }
}
