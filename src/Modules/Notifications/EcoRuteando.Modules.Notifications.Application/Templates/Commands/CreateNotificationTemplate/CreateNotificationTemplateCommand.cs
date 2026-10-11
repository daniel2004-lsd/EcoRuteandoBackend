using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Templates.Commands.CreateNotificationTemplate;

public record CreateNotificationTemplateCommand(
    string Name,
    NotificationType NotificationType,
    string? Subject,
    string? HtmlBody,
    string? TextBody) : IRequest<NotificationTemplate>;
