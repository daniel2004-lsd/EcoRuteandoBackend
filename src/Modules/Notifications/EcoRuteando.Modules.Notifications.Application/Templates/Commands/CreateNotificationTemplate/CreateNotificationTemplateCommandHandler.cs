using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Templates.Commands.CreateNotificationTemplate;

public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplate>
{
    private readonly INotificationTemplateRepository _repository;

    public CreateNotificationTemplateCommandHandler(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationTemplate> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = NotificationTemplate.Create(
            request.Name,
            request.NotificationType,
            request.Subject,
            request.HtmlBody,
            request.TextBody);

        await _repository.AddAsync(template, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return template;
    }
}
