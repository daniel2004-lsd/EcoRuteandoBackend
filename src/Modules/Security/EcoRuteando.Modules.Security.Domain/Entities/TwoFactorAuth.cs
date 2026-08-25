using EcoRuteando.Shared.BaseClasses;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public enum TwoFactorMethod
{
    TOTP = 0,
    SMS = 1,
    Email = 2
}

public sealed class TwoFactorAuth : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public TwoFactorMethod Method { get; private set; }
    public byte[]? EncryptedSecret { get; private set; }
    public bool IsActive { get; private set; }

    public User User { get; private set; } = default!;

    private TwoFactorAuth() { }

    public TwoFactorAuth(Guid userId, TwoFactorMethod method, byte[]? encryptedSecret)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Method = method;
        EncryptedSecret = encryptedSecret;
        IsActive = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsActive = false;
        EncryptedSecret = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSecret(byte[] encryptedSecret)
    {
        EncryptedSecret = encryptedSecret;
        UpdatedAt = DateTime.UtcNow;
    }
}
