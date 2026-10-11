using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _repository;

    public GetUnreadCountQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}
