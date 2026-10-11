using EcoRuteando.Modules.Notifications.Application.Notifications.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId, bool? UnreadOnly = null, int Limit = 50) : IRequest<IReadOnlyList<NotificationDto>>;
