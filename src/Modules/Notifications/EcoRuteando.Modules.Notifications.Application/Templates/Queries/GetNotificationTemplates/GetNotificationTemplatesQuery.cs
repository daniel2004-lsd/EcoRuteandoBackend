using EcoRuteando.Modules.Notifications.Domain.Entities;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Templates.Queries.GetNotificationTemplates;

public record GetNotificationTemplatesQuery(bool ActiveOnly = false) : IRequest<IReadOnlyList<NotificationTemplate>>;
