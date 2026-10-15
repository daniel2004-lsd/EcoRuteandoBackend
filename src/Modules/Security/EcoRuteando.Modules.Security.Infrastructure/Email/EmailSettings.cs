namespace EcoRuteando.Modules.Security.Infrastructure.Email;

public class EmailSettings
{
    public string Host { get; set; } = default!;

    public int Port { get; set; }

    public bool EnableSsl { get; set; }

    public string Username { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string FromName { get; set; } = default!;

    public string FromEmail { get; set; } = default!;

    /// <summary>
    /// API key de Resend (https://resend.com/api-keys).
    /// Render bloquea el SMTP outbound en planes gratis, así que el envío
    /// se hace por la API HTTPS de Resend (puerto 443).
    /// </summary>
    public string ApiKey { get; set; } = default!;
}
