using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Jobs;

public sealed class ExpiredTokensCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredTokensCleanupJob> _logger;
    private readonly ExpiredTokensCleanupOptions _options;

    public ExpiredTokensCleanupJob(
        IServiceScopeFactory scopeFactory,
        IOptions<ExpiredTokensCleanupOptions> options,
        ILogger<ExpiredTokensCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Job de limpieza de tokens expirados iniciado. Intervalo: {IntervalMinutes} minutos.",
            _options.IntervalMinutes);

        using var timer = new PeriodicTimer(
            TimeSpan.FromMinutes(_options.IntervalMinutes));

        try
        {
            do
            {
                await CleanupAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job de limpieza de tokens expirados detenido.");
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();

            var now = DateTime.UtcNow;

            var emailVerifications = await dbContext.EmailVerifications
                .Where(v => v.ExpiresAt < now)
                .ExecuteDeleteAsync(cancellationToken);

            var passwordRecoveries = await dbContext.PasswordRecoveries
                .Where(r => r.ExpiresAt < now)
                .ExecuteDeleteAsync(cancellationToken);

            var refreshTokens = await dbContext.RefreshTokens
                .Where(t => t.ExpiresAt < now)
                .ExecuteDeleteAsync(cancellationToken);

            var sessions = await dbContext.Sessions
                .Where(s => s.ExpiresAt < now)
                .ExecuteDeleteAsync(cancellationToken);

            if (emailVerifications + passwordRecoveries + refreshTokens + sessions > 0)
            {
                _logger.LogInformation(
                    "Limpieza de tokens expirados completada. " +
                    "Verificaciones: {EmailVerifications}, " +
                    "Recuperaciones: {PasswordRecoveries}, " +
                    "RefreshTokens: {RefreshTokens}, " +
                    "Sesiones: {Sessions}.",
                    emailVerifications,
                    passwordRecoveries,
                    refreshTokens,
                    sessions);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Error al ejecutar la limpieza de tokens expirados.");
        }
    }
}
