using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using Xunit;

namespace EcoRuteando.UnitTests.Notifications.Entities;

public sealed class NotificationTests
{
    [Fact]
    public void Create_InitializesNotificationWithSentStateAndUnread()
    {
        var userId = Guid.NewGuid();

        var notification = Notification.Create(
            userId: userId,
            notificationType: NotificationType.Alert,
            title: "Alerta de Ruta",
            message: "Se ha detectado un obstáculo en tu ruta habitual.",
            actionUrl: "/routes/123",
            extraData: "{\"routeId\":\"123\"}",
            channel: NotificationChannel.Push);

        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(userId, notification.UserId);
        Assert.Equal(NotificationType.Alert, notification.NotificationType);
        Assert.Equal("Alerta de Ruta", notification.Title);
        Assert.Equal("Se ha detectado un obstáculo en tu ruta habitual.", notification.Message);
        Assert.Equal("/routes/123", notification.ActionUrl);
        Assert.Equal("{\"routeId\":\"123\"}", notification.ExtraData);
        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);
        Assert.Equal(NotificationChannel.Push, notification.Channel);
        Assert.True(notification.IsSent);
        Assert.NotNull(notification.SentAt);
    }

    [Fact]
    public void MarkAsRead_WhenUnread_SetsIsReadAndReadAtTimestamp()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.System,
            "Logro desbloqueado",
            "Has ahorrado tus primeros 10 kg de CO₂!");

        Assert.False(notification.IsRead);

        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_DoesNotChangeReadAtTimestamp()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.System,
            "Logro desbloqueado",
            "Mensaje");

        notification.MarkAsRead();
        var initialReadAt = notification.ReadAt;

        notification.MarkAsRead();

        Assert.Equal(initialReadAt, notification.ReadAt);
    }
}
