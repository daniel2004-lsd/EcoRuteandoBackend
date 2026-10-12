using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Queries.GetRecentCacheEntries;

public record GetRecentCacheEntriesQuery(string? Provider = null, int Limit = 20) : IRequest<IReadOnlyList<ApiCacheEntryDto>>;
