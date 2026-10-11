using EcoRuteando.Modules.Notifications.Domain.Enums;

namespace EcoRuteando.Modules.Notifications.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public NotificationType NotificationType { get; private set; }
    public string? Subject { get; private set; }
    public string? HtmlBody { get; private set; }
    public string? TextBody { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        string name,
        NotificationType notificationType,
        string? subject,
        string? htmlBody,
        string? textBody)
    {
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            NotificationType = notificationType,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? subject, string? htmlBody, string? textBody, bool isActive)
    {
        Subject = subject;
        HtmlBody = htmlBody;
        TextBody = textBody;
        IsActive = isActive;
    }
}
