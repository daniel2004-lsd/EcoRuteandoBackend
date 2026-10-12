namespace EcoRuteando.Modules.Integrations.Application.Cache.Dtos;

public record ApiCacheEntryDto(
    long Id,
    string Provider,
    string Endpoint,
    string Parameters,
    string ParametersHash,
    string Response,
    bool IsValid,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsExpired);
