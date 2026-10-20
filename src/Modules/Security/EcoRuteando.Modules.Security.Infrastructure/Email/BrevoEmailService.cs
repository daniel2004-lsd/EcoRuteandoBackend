using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Email;

/// <summary>
/// Servicio de email basado en la API HTTPS de Brevo (https://brevo.com).
/// Usa el puerto 443, permitido por Render en planes gratuitos (a diferencia
/// del SMTP en puertos 25/465/587, que Render bloquea desde sept-2025).
/// Plan gratis: 300 emails/día. Permite verificar un remitente personal
/// (ej. Gmail) sin necesidad de dominio propio.
/// </summary>
public sealed class BrevoEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        IOptions<EmailSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<BrevoEmailService> logger)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            if (!string.IsNullOrWhiteSpace(_settings.Host) && !string.IsNullOrWhiteSpace(_settings.Username))
            {
                try
                {
                    using var smtp = new System.Net.Mail.SmtpClient(_settings.Host, _settings.Port)
                    {
                        Credentials = new System.Net.NetworkCredential(_settings.Username, _settings.Password),
                        EnableSsl = _settings.EnableSsl
                    };
                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(_settings.FromEmail, _settings.FromName),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };
                    mail.To.Add(to);
                    await smtp.SendMailAsync(mail);
                    _logger.LogInformation("Email enviado vía SMTP ({Host}) → Para: {To} | Asunto: {Subject}", _settings.Host, to, subject);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar email vía SMTP ({Host}) → Para: {To}", _settings.Host, to);
                }
            }

            _logger.LogWarning(
                "Email no configurado (falta Brevo ApiKey o credenciales SMTP válidas). Email NO enviado → Para: {To} | Asunto: {Subject}",
                to, subject);
            return;
        }

        var payload = new BrevoEmailRequest
        {
            Sender = new BrevoSender
            {
                Email = _settings.FromEmail,
                Name = _settings.FromName
            },
            To = new[] { new BrevoRecipient { Email = to } },
            Subject = subject,
            HtmlContent = htmlBody
        };

        var client = _httpClientFactory.CreateClient("brevo");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("api-key", _settings.ApiKey);

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Brevo devolvió {StatusCode} → Para: {To} | Asunto: {Subject} | Detalle: {Body}",
                (int)response.StatusCode, to, subject, body);
            return;
        }

        _logger.LogInformation(
            "Email enviado vía Brevo → Para: {To} | Asunto: {Subject}",
            to, subject);
    }

    private sealed class BrevoEmailRequest
    {
        [JsonPropertyName("sender")]
        public BrevoSender Sender { get; set; } = default!;

        [JsonPropertyName("to")]
        public BrevoRecipient[] To { get; set; } = default!;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = default!;

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; } = default!;
    }

    private sealed class BrevoSender
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }

    private sealed class BrevoRecipient
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;
    }
}
