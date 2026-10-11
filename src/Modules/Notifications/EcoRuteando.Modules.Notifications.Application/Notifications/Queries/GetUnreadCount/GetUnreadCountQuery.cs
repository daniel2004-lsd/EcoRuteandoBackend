using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
