using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.PurgeExpiredCache;

public class PurgeExpiredCacheCommandHandler : IRequestHandler<PurgeExpiredCacheCommand, int>
{
    private readonly IApiCacheRepository _repository;

    public PurgeExpiredCacheCommandHandler(IApiCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(PurgeExpiredCacheCommand request, CancellationToken cancellationToken)
    {
        return await _repository.PurgeExpiredAsync(cancellationToken);
    }
}
