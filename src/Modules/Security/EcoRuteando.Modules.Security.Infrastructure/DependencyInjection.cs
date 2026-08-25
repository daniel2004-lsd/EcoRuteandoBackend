using EcoRuteando.Modules.Security.Application.Abstractions.BackgroundJobs;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Authorization;
using EcoRuteando.Modules.Security.Infrastructure.Email;
using EcoRuteando.Modules.Security.Infrastructure.Logging;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using EcoRuteando.Shared.Abstractions;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace EcoRuteando.Modules.Security.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'.");


        services.Configure<JwtOptions>(
            configuration.GetSection("Jwt"));


        services.AddDbContext<SecurityDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    o.MigrationsAssembly("EcoRuteando.Modules.Security.Infrastructure");
                    o.MapEnum<TwoFactorMethod>("two_factor_method", "security");
                    o.MapEnum<OAuthProvider>("oauth_provider", "security");
                    o.MapEnum<ErrorLevel>("error_level", "security");
                });
        });

        services.AddScoped<IUnitOfWork>(sp =>
        sp.GetRequiredService<SecurityDbContext>());

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordRecoveryRepository, PasswordRecoveryRepository>();
        services.AddScoped<ISecurityPolicyRepository, SecurityPolicyRepository>();
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
        services.AddScoped<IOAuthAccountRepository, OAuthAccountRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

        // Servicios
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<SmtpEmailService>();
        services.AddScoped<QueuedEmailService>();
        services.AddScoped<IEmailService>(sp =>
            sp.GetRequiredService<QueuedEmailService>());
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IOtpProvider, OtpProvider>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Logging
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();

        // Background jobs
        services.Configure<Jobs.ExpiredTokensCleanupOptions>(
            configuration.GetSection(Jobs.ExpiredTokensCleanupOptions.SectionName));
        services.AddHostedService<Jobs.ExpiredTokensCleanupJob>();

        services.Configure<Jobs.BackgroundJobQueueOptions>(
            configuration.GetSection(Jobs.BackgroundJobQueueOptions.SectionName));
        services.AddSingleton<IBackgroundTaskQueue, Jobs.BackgroundTaskQueue>();
        services.AddHostedService<Jobs.QueuedHostedService>();

        // OAuth providers
        services.Configure<GoogleOptions>(configuration.GetSection("Google"));
        services.Configure<FacebookOptions>(configuration.GetSection("Facebook"));
        services.AddHttpClient<IOAuthProvider, GoogleOAuthProvider>();
        services.AddHttpClient<IOAuthProvider, FacebookOAuthProvider>();

        return services;
    }
}