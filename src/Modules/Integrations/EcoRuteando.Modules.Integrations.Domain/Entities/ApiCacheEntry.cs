using System.Security.Cryptography;
using System.Text;

namespace EcoRuteando.Modules.Integrations.Domain.Entities;

public class ApiCacheEntry
{
    public long Id { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Endpoint { get; private set; } = string.Empty;
    public string Parameters { get; private set; } = "{}";
    public string ParametersHash { get; private set; } = string.Empty;
    public string Response { get; private set; } = "{}";
    public bool IsValid { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }

    private ApiCacheEntry() { }

    public static string ComputeParametersHash(string parameters)
    {
        var normalized = string.IsNullOrWhiteSpace(parameters) ? "{}" : parameters.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static ApiCacheEntry Create(
        string provider,
        string endpoint,
        string parameters,
        string response,
        int ttlSeconds)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider cannot be empty.", nameof(provider));
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty.", nameof(response));

        var normalizedParams = string.IsNullOrWhiteSpace(parameters) ? "{}" : parameters.Trim();
        var ttl = ttlSeconds > 0 ? ttlSeconds : 3600;

        return new ApiCacheEntry
        {
            Provider = provider.Trim(),
            Endpoint = endpoint.Trim(),
            Parameters = normalizedParams,
            ParametersHash = ComputeParametersHash(normalizedParams),
            Response = response.Trim(),
            IsValid = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ttl)
        };
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}
