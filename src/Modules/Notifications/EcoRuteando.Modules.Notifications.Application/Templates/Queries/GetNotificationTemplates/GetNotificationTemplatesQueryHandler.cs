using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Templates.Queries.GetNotificationTemplates;

public class GetNotificationTemplatesQueryHandler : IRequestHandler<GetNotificationTemplatesQuery, IReadOnlyList<NotificationTemplate>>
{
    private readonly INotificationTemplateRepository _repository;

    public GetNotificationTemplatesQueryHandler(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationTemplate>> Handle(GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.ActiveOnly, cancellationToken);
    }
}
