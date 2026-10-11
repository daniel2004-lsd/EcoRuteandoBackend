using EcoRuteando.Modules.Notifications.Domain.Enums;

namespace EcoRuteando.Modules.Notifications.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? ActionUrl { get; private set; }
    public string? ExtraData { get; private set; } // JSONB stored as string
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsSent { get; private set; }
    public DateTime? SentAt { get; private set; }
    public Guid? TemplateId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public NotificationTemplate? Template { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType notificationType,
        string title,
        string message,
        string? actionUrl = null,
        string? extraData = null,
        NotificationChannel channel = NotificationChannel.InApp,
        Guid? templateId = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            ExtraData = extraData,
            IsRead = false,
            Channel = channel,
            IsSent = true,
            SentAt = DateTime.UtcNow,
            TemplateId = templateId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
