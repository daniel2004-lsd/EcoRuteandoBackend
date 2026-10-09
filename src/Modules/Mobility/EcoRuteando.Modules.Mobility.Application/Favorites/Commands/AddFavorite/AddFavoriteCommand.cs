using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Commands.AddFavorite;

public sealed record AddFavoriteCommand(
    Guid RouteId,
    string? Label,
    Guid UserId)
    : IRequest;
