using EcoRuteando.Modules.Notifications.Domain.Entities;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Dtos;

public record NotificationDto(
    Guid Id,
    string NotificationType,
    string Title,
    string Message,
    string? ActionUrl,
    string? ExtraData,
    bool IsRead,
    DateTime? ReadAt,
    string Channel,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(Notification entity) =>
        new(
            entity.Id,
            entity.NotificationType.ToString().ToLowerInvariant(),
            entity.Title,
            entity.Message,
            entity.ActionUrl,
            entity.ExtraData,
            entity.IsRead,
            entity.ReadAt,
            entity.Channel.ToString().ToLowerInvariant(),
            entity.CreatedAt);
}
