using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<int>;
