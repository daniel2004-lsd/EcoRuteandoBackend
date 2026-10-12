using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.InvalidateCache;

public record InvalidateCacheCommand(long? EntryId = null, string? Provider = null) : IRequest<bool>;
