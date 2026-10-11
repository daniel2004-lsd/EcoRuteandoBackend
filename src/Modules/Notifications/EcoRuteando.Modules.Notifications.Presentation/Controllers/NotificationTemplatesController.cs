using EcoRuteando.Modules.Notifications.Application.Templates.Commands.CreateNotificationTemplate;
using EcoRuteando.Modules.Notifications.Application.Templates.Queries.GetNotificationTemplates;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Notifications.Presentation.Controllers;

[ApiController]
[Route("api/notification-templates")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class NotificationTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationTemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates([FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var templates = await _mediator.Send(new GetNotificationTemplatesQuery(activeOnly), cancellationToken);
        return Ok(templates);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateApiRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _mediator.Send(
            new CreateNotificationTemplateCommand(
                request.Name,
                request.NotificationType,
                request.Subject,
                request.HtmlBody,
                request.TextBody),
            cancellationToken);

        return CreatedAtAction(nameof(GetTemplates), new { id = template.Id }, template);
    }
}

public sealed class CreateTemplateApiRequest
{
    public string Name { get; init; } = string.Empty;
    public NotificationType NotificationType { get; init; } = NotificationType.System;
    public string? Subject { get; init; }
    public string? HtmlBody { get; init; }
    public string? TextBody { get; init; }
}
