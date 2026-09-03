using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Queries.GetFavorites;

public sealed record GetFavoritesQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<GetFavoritesResponse>>;
