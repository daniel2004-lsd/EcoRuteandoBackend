namespace EcoRuteando.Modules.Notifications.Domain.Entities;

public class DeviceToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Platform { get; private set; } = string.Empty; // android, ios, web
    public string ExpoPushToken { get; private set; } = string.Empty;
    public string? DeviceId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DeviceToken() { }

    public static DeviceToken Create(Guid userId, string platform, string expoPushToken, string? deviceId)
    {
        return new DeviceToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Platform = platform.ToLowerInvariant(),
            ExpoPushToken = expoPushToken,
            DeviceId = deviceId,
            IsActive = true,
            LastUsedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateToken(string expoPushToken, string? deviceId)
    {
        ExpoPushToken = expoPushToken;
        DeviceId = deviceId;
        IsActive = true;
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
