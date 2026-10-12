using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.PurgeExpiredCache;

public record PurgeExpiredCacheCommand : IRequest<int>;
