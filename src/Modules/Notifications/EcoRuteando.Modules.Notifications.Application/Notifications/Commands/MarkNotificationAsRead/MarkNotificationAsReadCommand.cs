using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
