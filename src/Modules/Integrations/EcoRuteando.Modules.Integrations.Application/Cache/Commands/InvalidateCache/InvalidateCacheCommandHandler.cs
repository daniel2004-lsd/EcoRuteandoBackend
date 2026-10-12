using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.InvalidateCache;

public class InvalidateCacheCommandHandler : IRequestHandler<InvalidateCacheCommand, bool>
{
    private readonly IApiCacheRepository _repository;

    public InvalidateCacheCommandHandler(IApiCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(InvalidateCacheCommand request, CancellationToken cancellationToken)
    {
        if (request.EntryId.HasValue)
        {
            await _repository.InvalidateAsync(request.EntryId.Value, cancellationToken);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            await _repository.InvalidateByProviderAsync(request.Provider, cancellationToken);
            return true;
        }

        return false;
    }
}
