using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Queries.GetRecentCacheEntries;

public class GetRecentCacheEntriesQueryHandler : IRequestHandler<GetRecentCacheEntriesQuery, IReadOnlyList<ApiCacheEntryDto>>
{
    private readonly IApiCacheRepository _repository;

    public GetRecentCacheEntriesQueryHandler(IApiCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ApiCacheEntryDto>> Handle(GetRecentCacheEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetRecentEntriesAsync(request.Provider, request.Limit, cancellationToken);
        var now = DateTime.UtcNow;

        return entries.Select(e => new ApiCacheEntryDto(
            e.Id,
            e.Provider,
            e.Endpoint,
            e.Parameters,
            e.ParametersHash,
            e.Response,
            e.IsValid,
            e.CreatedAt,
            e.ExpiresAt,
            e.ExpiresAt <= now
        )).ToList();
    }
}
