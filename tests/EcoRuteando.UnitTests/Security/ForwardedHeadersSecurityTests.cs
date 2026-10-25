using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class ForwardedHeadersSecurityTests
{
    private static ForwardedHeadersMiddleware CreateMiddleware(ForwardedHeadersOptions options)
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        return new ForwardedHeadersMiddleware(next, NullLoggerFactory.Instance, Options.Create(options));
    }

    [Fact]
    public async Task ForwardedHeaders_WhenUntrustedProxySendsXForwardedFor_ShouldIgnoreHeaderAndKeepRemoteIpAddress()
    {
        // Arrange: Solo se confía en el proxy interno 10.0.0.1
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        var attackerIp = IPAddress.Parse("198.51.100.5");
        context.Connection.RemoteIpAddress = attackerIp;
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.195"; // IP spoofing attempt
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        // Act
        await middleware.Invoke(context);

        // Assert: La cabecera falsa debe ser ignorada porque 198.51.100.5 no es un proxy confiable
        Assert.Equal(attackerIp, context.Connection.RemoteIpAddress);
        Assert.Equal("http", context.Request.Scheme);
    }

    [Fact]
    public async Task ForwardedHeaders_WhenTrustedProxySendsXForwardedFor_ShouldUpdateRemoteIpAddressAndScheme()
    {
        // Arrange: Se confía en el proxy 10.0.0.1
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.195";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        // Act
        await middleware.Invoke(context);

        // Assert: RemoteIpAddress debe resolverse a la IP real del cliente reportada por el proxy
        Assert.Equal(IPAddress.Parse("203.0.113.195"), context.Connection.RemoteIpAddress);
        Assert.Equal("https", context.Request.Scheme);
    }

    [Fact]
    public async Task ForwardedHeaders_WhenTrustedCidrNetworkSendsXForwardedFor_ShouldUpdateRemoteIpAddress()
    {
        // Arrange: Se confía en la subred privada Docker (172.16.0.0/12)
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        // IP dentro del rango 172.16.0.0/12 (ej. contenedor bridge 172.18.0.50)
        context.Connection.RemoteIpAddress = IPAddress.Parse("172.18.0.50");
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.88";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.Equal(IPAddress.Parse("203.0.113.88"), context.Connection.RemoteIpAddress);
        Assert.Equal("https", context.Request.Scheme);
    }

    [Fact]
    public async Task ForwardedHeaders_WhenUntrustedNetworkSendsXForwardedFor_ShouldIgnoreHeader()
    {
        // Arrange: Se confía únicamente en 172.16.0.0/12
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        // IP fuera del rango 172.16.0.0/12
        var externalIp = IPAddress.Parse("192.168.1.100");
        context.Connection.RemoteIpAddress = externalIp;
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.88";

        // Act
        await middleware.Invoke(context);

        // Assert: No debe ser modificada
        Assert.Equal(externalIp, context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task ForwardedHeaders_ForwardedLimit_ShouldEnforceHopLimitAndPreventSpoofedChain()
    {
        // Arrange: ForwardedLimit = 1 restringe el procesamiento a 1 único salto
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        // Cadena con múltiples IPs (el atacante envió 198.51.100.99, el proxy legítimo añadió 203.0.113.50)
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.99, 203.0.113.50";

        // Act
        await middleware.Invoke(context);

        // Assert: Solo el salto más inmediato al proxy legítimo (203.0.113.50) se procesa como IP remota
        Assert.Equal(IPAddress.Parse("203.0.113.50"), context.Connection.RemoteIpAddress);
        // La IP residual anterior permanece en la cabecera sin consumirse debido a ForwardedLimit = 1
        Assert.Equal("198.51.100.99", context.Request.Headers["X-Forwarded-For"].ToString());
    }

    [Fact]
    public async Task ForwardedHeaders_DefaultOptions_ShouldNotTrustExternalPublicIps()
    {
        // Arrange: Opciones estándar de ASP.NET Core (solo loopback 127.0.0.1/::1)
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };

        var middleware = CreateMiddleware(options);

        var context = new DefaultHttpContext();
        var publicClientIp = IPAddress.Parse("203.0.113.1");
        context.Connection.RemoteIpAddress = publicClientIp;
        context.Request.Headers["X-Forwarded-For"] = "8.8.8.8";

        // Act
        await middleware.Invoke(context);

        // Assert: RemoteIpAddress permanece intacto (el cliente directo no es un proxy loopback)
        Assert.Equal(publicClientIp, context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task RateLimiterPartitionKey_WithForwardedHeaders_UsesResolvedClientIpWhenBehindTrustedProxy()
    {
        // Arrange
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor,
            ForwardLimit = 1
        };
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));

        var middleware = CreateMiddleware(options);

        static string GetClientPartitionKey(HttpContext httpContext)
        {
            var ip = httpContext.Connection.RemoteIpAddress;
            return ip is not null ? ip.ToString() : "unknown_client_ip";
        }

        // Caso 1: Solicitud a través del proxy de confianza
        var contextTrusted = new DefaultHttpContext();
        contextTrusted.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        contextTrusted.Request.Headers["X-Forwarded-For"] = "203.0.113.25";

        await middleware.Invoke(contextTrusted);
        var partitionKeyTrusted = GetClientPartitionKey(contextTrusted);

        // Caso 2: Atacante directo enviando X-Forwarded-For falso para intentar saltarse el rate limit
        var contextAttacker = new DefaultHttpContext();
        contextAttacker.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.9");
        contextAttacker.Request.Headers["X-Forwarded-For"] = "203.0.113.25"; // Intento de usar la cuota de otra IP

        await middleware.Invoke(contextAttacker);
        var partitionKeyAttacker = GetClientPartitionKey(contextAttacker);

        // Assert:
        // El cliente legítimo obtiene su IP real
        Assert.Equal("203.0.113.25", partitionKeyTrusted);
        // El atacante directo NO puede usurpar la cuota de 203.0.113.25; su clave es su IP real de socket
        Assert.Equal("198.51.100.9", partitionKeyAttacker);
    }
}
