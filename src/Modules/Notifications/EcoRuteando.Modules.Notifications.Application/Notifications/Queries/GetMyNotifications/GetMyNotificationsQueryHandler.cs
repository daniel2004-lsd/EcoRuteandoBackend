using EcoRuteando.Modules.Notifications.Application.Notifications.Dtos;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetMyNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetByUserIdAsync(
            request.UserId,
            request.UnreadOnly,
            request.Limit,
            cancellationToken);

        return notifications.Select(NotificationDto.FromEntity).ToList();
    }
}
