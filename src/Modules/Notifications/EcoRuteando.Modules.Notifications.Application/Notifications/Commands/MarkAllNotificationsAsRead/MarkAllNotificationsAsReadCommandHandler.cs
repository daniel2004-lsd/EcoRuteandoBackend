using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly INotificationRepository _repository;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _repository.GetByUserIdAsync(request.UserId, unreadOnly: true, limit: 500, cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }
}
