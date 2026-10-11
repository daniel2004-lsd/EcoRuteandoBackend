using System.Security.Claims;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.MarkNotificationAsRead;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.RegisterDeviceToken;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.SendNotification;
using EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetMyNotifications;
using EcoRuteando.Modules.Notifications.Application.Notifications.Queries.GetUnreadCount;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Notifications.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta las notificaciones del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetRequiredUserId();
        var result = await _mediator.Send(new GetMyNotificationsQuery(userId, unreadOnly, limit), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el conteo de notificaciones no leídas para la campanita del header.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetRequiredUserId();
        var count = await _mediator.Send(new GetUnreadCountQuery(userId), cancellationToken);
        return Ok(new { unreadCount = count });
    }

    /// <summary>
    /// Marca una notificación específica como leída.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetRequiredUserId();
        var success = await _mediator.Send(new MarkNotificationAsReadCommand(id, userId), cancellationToken);
        if (!success)
        {
            return NotFound(new { message = "Notificación no encontrada o no pertenece al usuario." });
        }
        return NoContent();
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas.
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetRequiredUserId();
        var count = await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId), cancellationToken);
        return Ok(new { markedCount = count });
    }

    /// <summary>
    /// Registra o actualiza el push token del dispositivo del usuario (FCM/Expo).
    /// </summary>
    [HttpPost("device-token")]
    public async Task<IActionResult> RegisterDeviceToken(
        [FromBody] RegisterDeviceTokenApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetRequiredUserId();
        await _mediator.Send(
            new RegisterDeviceTokenCommand(userId, request.Platform, request.ExpoPushToken, request.DeviceId),
            cancellationToken);

        return Ok(new { message = "Token de dispositivo registrado exitosamente." });
    }

    /// <summary>
    /// CU10: Enviar notificación individual o masiva (Administrador).
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new SendNotificationCommand(
                request.Title,
                request.Message,
                request.NotificationType,
                request.Channel,
                request.TargetUserId,
                request.ActionUrl,
                request.ExtraData,
                request.TemplateId),
            cancellationToken);

        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(response);
    }

    private Guid GetRequiredUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado.");
        }
        return userId;
    }
}

public sealed class RegisterDeviceTokenApiRequest
{
    public string Platform { get; init; } = "web";
    public string ExpoPushToken { get; init; } = string.Empty;
    public string? DeviceId { get; init; }
}

public sealed class SendNotificationApiRequest
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationType NotificationType { get; init; } = NotificationType.System;
    public NotificationChannel Channel { get; init; } = NotificationChannel.InApp;
    public Guid? TargetUserId { get; init; }
    public string? ActionUrl { get; init; }
    public string? ExtraData { get; init; }
    public Guid? TemplateId { get; init; }
}
