using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmtpClient = global::System.Net.Mail.SmtpClient;
using MailMessage = global::System.Net.Mail.MailMessage;
using MailAddress = global::System.Net.Mail.MailAddress;
using NetworkCredential = global::System.Net.NetworkCredential;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;

public class AddTicketMessageCommandHandler : IRequestHandler<AddTicketMessageCommand, SupportMessageDto?>
{
    private readonly ISupportTicketRepository _repository;
    private readonly IUserLookupService _userLookupService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddTicketMessageCommandHandler> _logger;

    public AddTicketMessageCommandHandler(
        ISupportTicketRepository repository,
        IUserLookupService userLookupService,
        IConfiguration configuration,
        ILogger<AddTicketMessageCommandHandler> logger)
    {
        _repository = repository;
        _userLookupService = userLookupService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SupportMessageDto?> Handle(AddTicketMessageCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, includeMessages: false, cancellationToken);
        if (ticket == null)
        {
            return null;
        }

        // Anti-IDOR: Si no es administrador, solo puede enviar mensajes a sus propios tickets
        if (!request.IsAdmin && (!ticket.UserId.HasValue || ticket.UserId.Value != request.SenderId))
        {
            return null;
        }

        if (request.IsAdmin)
        {
            // Al responder el administrador, cambiar estado a Resuelto para que pase a respondidos
            ticket.UpdateStatus(SupportTicketStatus.Resolved);
        }

        var message = SupportMessage.Create(
            ticket.Id,
            request.SenderId,
            request.IsAdmin,
            request.Message,
            request.AttachmentUrl);

        await _repository.AddMessageAsync(message, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Si fue una respuesta del administrador, notificar al usuario por correo electrónico
        if (request.IsAdmin && ticket.UserId.HasValue)
        {
            try
            {
                var usersMap = await _userLookupService.GetUsersByIdsAsync(new[] { ticket.UserId.Value }, cancellationToken);
                if (usersMap.TryGetValue(ticket.UserId.Value, out var u) && !string.IsNullOrWhiteSpace(u.Email))
                {
                    await SendTicketNotificationEmailAsync(u.Email, u.Name, ticket.Subject, request.Message, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de notificación de soporte para el ticket {TicketId}", ticket.Id);
            }
        }

        return SupportMessageDto.FromEntity(message);
    }

    private static string HtmlEncode(string? text) => global::System.Net.WebUtility.HtmlEncode(text ?? "");

    private async Task SendTicketNotificationEmailAsync(
        string toEmail,
        string userName,
        string subject,
        string replyMessage,
        CancellationToken cancellationToken)
    {
        var host = _configuration["EmailSettings:Host"] ?? _configuration["EmailSettings__Host"] ?? "smtp.gmail.com";
        var portStr = _configuration["EmailSettings:Port"] ?? _configuration["EmailSettings__Port"] ?? "587";
        int.TryParse(portStr, out var port);
        if (port <= 0) port = 587;
        var user = _configuration["EmailSettings:Username"] ?? _configuration["EmailSettings__Username"];
        var pass = _configuration["EmailSettings:Password"] ?? _configuration["EmailSettings__Password"];
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? _configuration["EmailSettings__FromEmail"] ?? "ecoruteando.app@gmail.com";
        var fromName = _configuration["EmailSettings:FromName"] ?? _configuration["EmailSettings__FromName"] ?? "EcoRuteando Soporte";

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            _logger.LogWarning("Credenciales de email no configuradas. No se pudo enviar correo a {Email}", toEmail);
            return;
        }

        var encName = HtmlEncode(userName);
        var encSubject = HtmlEncode(subject);
        var encReply = HtmlEncode(replyMessage);

        var html = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 12px rgba(0,0,0,0.05);"">
  <div style=""background: linear-gradient(135deg, #059669 0%, #10b981 100%); padding: 28px 24px; text-align: center; color: white;"">
    <h1 style=""margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">🌿 EcoRuteando</h1>
    <p style=""margin: 6px 0 0; font-size: 14px; opacity: 0.9;"">Centro de Soporte y Atención Ciudadana</p>
  </div>
  <div style=""padding: 32px 24px;"">
    <h2 style=""color: #1e293b; font-size: 18px; margin-top: 0;"">¡Hola, {encName}!</h2>
    <p style=""color: #475569; font-size: 14px; line-height: 1.6;"">
      Hemos recibido tu solicitud y nuestro equipo de soporte ha registrado la siguiente respuesta para tu ticket:
    </p>
    <div style=""background-color: #f8fafc; border-left: 4px solid #10b981; border-radius: 8px; padding: 16px; margin: 20px 0;"">
      <p style=""margin: 0 0 8px; font-size: 12px; font-weight: 700; color: #64748b; text-transform: uppercase;"">Asunto de tu consulta:</p>
      <p style=""margin: 0 0 16px; font-size: 15px; font-weight: 600; color: #1e293b;"">{encSubject}</p>
      <p style=""margin: 0 0 8px; font-size: 12px; font-weight: 700; color: #059669; text-transform: uppercase;"">Respuesta Oficial de Soporte:</p>
      <p style=""margin: 0; font-size: 14px; color: #334155; line-height: 1.6; white-space: pre-wrap;"">{encReply}</p>
    </div>
    <div style=""margin: 24px 0; padding: 12px 16px; background-color: #ecfdf5; border-radius: 8px; border: 1px solid #a7f3d0;"">
      <span style=""font-size: 13px; font-weight: 600; color: #065f46;"">✓ Estado del ticket: Resuelto / Respondido</span>
    </div>
    <p style=""color: #64748b; font-size: 13px; line-height: 1.5;"">
      Puedes ingresar a tu cuenta en EcoRuteando en cualquier momento para revisar el historial completo de mensajes o formular nuevas preguntas.
    </p>
  </div>
  <div style=""background-color: #f1f5f9; padding: 16px 24px; text-align: center; border-top: 1px solid #e2e8f0;"">
    <p style=""margin: 0; font-size: 12px; color: #94a3b8;"">EcoRuteando — Movilidad sostenible y consciente para todos.</p>
  </div>
</div>";

        using var smtp = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };
        var mail = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"[EcoRuteando Soporte] Respuesta a tu consulta: {subject}",
            Body = html,
            IsBodyHtml = true
        };
        mail.To.Add(toEmail);
        await smtp.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("Notificación de soporte enviada con éxito a {ToEmail}", toEmail);
    }
}
