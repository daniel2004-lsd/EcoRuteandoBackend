using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.SetCachedResponse;

public class SetCachedResponseCommandHandler : IRequestHandler<SetCachedResponseCommand, ApiCacheEntryDto>
{
    private readonly IApiCacheRepository _repository;

    public SetCachedResponseCommandHandler(IApiCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiCacheEntryDto> Handle(SetCachedResponseCommand request, CancellationToken cancellationToken)
    {
        var entry = ApiCacheEntry.Create(
            request.Provider,
            request.Endpoint,
            request.Parameters,
            request.Response,
            request.TtlSeconds);

        await _repository.AddAsync(entry, cancellationToken);

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
