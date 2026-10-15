using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Email;

/// <summary>
/// Servicio de email basado en la API HTTPS de Resend (https://resend.com).
/// Usa el puerto 443, permitido por Render en planes gratuitos (a diferencia
/// del SMTP en puertos 25/465/587, que Render bloquea desde sept-2025).
/// </summary>
public sealed class ResendEmailService : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly EmailSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IOptions<EmailSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<ResendEmailService> logger)
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
            _logger.LogWarning(
                "Resend no configurado (falta EmailSettings__ApiKey). Email NO enviado → Para: {To} | Asunto: {Subject}",
                to, subject);
            return;
        }

        var payload = new ResendEmailRequest
        {
            From = $"{_settings.FromName} <{_settings.FromEmail}>",
            To = new[] { to },
            Subject = subject,
            Html = htmlBody
        };

        var client = _httpClientFactory.CreateClient("resend");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Resend devolvió {StatusCode} → Para: {To} | Asunto: {Subject} | Detalle: {Body}",
                (int)response.StatusCode, to, subject, body);
            return;
        }

        _logger.LogInformation(
            "Email enviado vía Resend → Para: {To} | Asunto: {Subject}",
            to, subject);
    }

    private sealed class ResendEmailRequest
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = default!;

        [JsonPropertyName("to")]
        public string[] To { get; set; } = default!;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = default!;

        [JsonPropertyName("html")]
        public string Html { get; set; } = default!;
    }
}
