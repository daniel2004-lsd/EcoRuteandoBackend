using EcoRuteando.Modules.Security.Application.Abstractions.BackgroundJobs;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;

namespace EcoRuteando.Modules.Security.Infrastructure.Email;

public sealed class QueuedEmailService : IEmailService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<QueuedEmailService> _logger;

    public QueuedEmailService(
        IBackgroundTaskQueue queue,
        ILogger<QueuedEmailService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        await _queue.EnqueueAsync<SmtpEmailService>(async (smtpEmailService, _) =>
        {
            try
            {
                await smtpEmailService.SendAsync(to, subject, htmlBody);

                _logger.LogInformation(
                    "Email enviado en segundo plano → Para: {To} | Asunto: {Subject}",
                    to, subject);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar email en segundo plano → Para: {To} | Asunto: {Subject}",
                    to, subject);
            }
        });
    }
}
