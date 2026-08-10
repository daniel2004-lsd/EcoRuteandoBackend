namespace EcoRuteando.Modules.Security.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody);
}