using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(
    Guid RouteId,
    Guid UserId)
    : IRequest;
