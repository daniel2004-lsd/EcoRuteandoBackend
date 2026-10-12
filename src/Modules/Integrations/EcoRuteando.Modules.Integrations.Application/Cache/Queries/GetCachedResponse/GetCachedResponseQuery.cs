using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Queries.GetCachedResponse;

public record GetCachedResponseQuery(string Provider, string Endpoint, string Parameters) : IRequest<ApiCacheEntryDto?>;
