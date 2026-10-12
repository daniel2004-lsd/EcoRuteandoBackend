using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Queries.GetCachedResponse;

public class GetCachedResponseQueryHandler : IRequestHandler<GetCachedResponseQuery, ApiCacheEntryDto?>
{
    private readonly IApiCacheRepository _repository;

    public GetCachedResponseQueryHandler(IApiCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiCacheEntryDto?> Handle(GetCachedResponseQuery request, CancellationToken cancellationToken)
    {
        var hash = ApiCacheEntry.ComputeParametersHash(request.Parameters);
        var entry = await _repository.GetValidEntryAsync(request.Provider, request.Endpoint, hash, cancellationToken);
        if (entry == null)
            return null;

        var now = DateTime.UtcNow;
        return new ApiCacheEntryDto(
            entry.Id,
            entry.Provider,
            entry.Endpoint,
            entry.Parameters,
            entry.ParametersHash,
            entry.Response,
            entry.IsValid,
            entry.CreatedAt,
            entry.ExpiresAt,
            entry.ExpiresAt <= now);
    }
}
