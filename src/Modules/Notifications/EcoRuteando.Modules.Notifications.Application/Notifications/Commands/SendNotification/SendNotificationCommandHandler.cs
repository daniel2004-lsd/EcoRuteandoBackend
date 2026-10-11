using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, SendNotificationResponse>
{
    private readonly INotificationRepository _repository;
    private readonly IConfiguration _configuration;

    public SendNotificationCommandHandler(INotificationRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<SendNotificationResponse> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendNotificationResponse(false, 0, "El título y mensaje son requeridos.");
        }

        // Si se especificó un usuario destinatario
        if (request.TargetUserId.HasValue)
        {
            var notification = Notification.Create(
                request.TargetUserId.Value,
                request.NotificationType,
                request.Title,
                request.Message,
                request.ActionUrl,
                request.ExtraData,
                request.Channel,
                request.TemplateId);

            await _repository.AddAsync(notification, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return new SendNotificationResponse(true, 1, "Notificación enviada al usuario.");
        }

        // Broadcast a todos los usuarios registrados activos
        var userIds = new List<Guid>();
        var connStr = _configuration.GetConnectionString("DefaultConnection");
        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand("SELECT id FROM security.users WHERE is_active = TRUE", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                userIds.Add(reader.GetGuid(0));
            }
        }

        if (userIds.Count == 0)
        {
            return new SendNotificationResponse(false, 0, "No se encontraron usuarios destinatarios activos.");
        }

        var notifications = userIds.Select(uid => Notification.Create(
            uid,
            request.NotificationType,
            request.Title,
            request.Message,
            request.ActionUrl,
            request.ExtraData,
            request.Channel,
            request.TemplateId)).ToList();

        await _repository.AddRangeAsync(notifications, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new SendNotificationResponse(true, notifications.Count, $"Notificación enviada a {notifications.Count} usuarios.");
    }
}
