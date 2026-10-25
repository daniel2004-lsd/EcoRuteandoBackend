using EcoRuteando.Api;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRuteando.IntegrationTests.Infrastructure;

public sealed class DummyEmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string body)
    {
        return Task.CompletedTask;
    }
}

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? "Host=localhost;Port=5450;Database=ecoruteando_test_e2e;Username=postgres;Password=postgres";
    public const string AesKeyBase64 = "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="; // 32 bytes AES-256 key

    static TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "a_very_secret_signing_key_for_testing_purposes_only_12345");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "EcoRuteando");
        Environment.SetEnvironmentVariable("Jwt__Audience", "EcoRuteandoUsers");
        Environment.SetEnvironmentVariable("Admin__Email", "admin_test@ecoruteando.com");
        Environment.SetEnvironmentVariable("Admin__Password", "Admin1234!");
        Environment.SetEnvironmentVariable("Encryption__TotpSecretKey", AesKeyBase64);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:SecretKey"] = "a_very_secret_signing_key_for_testing_purposes_only_12345",
                ["Jwt:Issuer"] = "EcoRuteando",
                ["Jwt:Audience"] = "EcoRuteandoUsers",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Admin:Email"] = "admin_test@ecoruteando.com",
                ["Admin:Password"] = "Admin1234!",
                ["Admin:FirstName"] = "Admin",
                ["Admin:LastName"] = "Test",
                ["Encryption:TotpSecretKey"] = AesKeyBase64
            };

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Reemplazar IEmailService por Dummy para pruebas HTTP E2E
            var emailServiceDescriptors = services.Where(d => d.ServiceType == typeof(IEmailService)).ToList();
            foreach (var descriptor in emailServiceDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<IEmailService, DummyEmailService>();

            // Reemplazar la configuración de Rate Limiter para pruebas automáticas
            var rateLimiterConfigs = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>))
                .ToList();
            foreach (var d in rateLimiterConfigs)
            {
                services.Remove(d);
            }

            services.AddRateLimiter(options =>
            {
                options.AddPolicy("auth", _ => System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("sensitive", _ => System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("api", _ => System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("test"));
            });
        });
    }
}
