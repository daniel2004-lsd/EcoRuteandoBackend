using EcoRuteando.Modules.Notifications.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.SendNotification;

public record SendNotificationCommand(
    string Title,
    string Message,
    NotificationType NotificationType = NotificationType.System,
    NotificationChannel Channel = NotificationChannel.InApp,
    Guid? TargetUserId = null,
    string? ActionUrl = null,
    string? ExtraData = null,
    Guid? TemplateId = null) : IRequest<SendNotificationResponse>;

public record SendNotificationResponse(bool Success, int SentCount, string Message);
