using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EcoRuteando.Modules.Security.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailSettings> options,
        ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.Username))
        {
            _logger.LogWarning(
                "SMTP no configurado. Email NO enviado → Para: {To} | Asunto: {Subject} | Cuerpo: {Body}",
                to, subject, htmlBody);
            return;
        }

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        message.To.Add(MailboxAddress.Parse(to));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _settings.Username,
            _settings.Password);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}
