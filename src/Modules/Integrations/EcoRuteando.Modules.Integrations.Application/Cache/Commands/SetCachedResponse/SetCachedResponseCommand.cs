using EcoRuteando.Modules.Integrations.Application.Cache.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.SetCachedResponse;

public record SetCachedResponseCommand(
    string Provider,
    string Endpoint,
    string Parameters,
    string Response,
    int TtlSeconds = 3600) : IRequest<ApiCacheEntryDto>;
