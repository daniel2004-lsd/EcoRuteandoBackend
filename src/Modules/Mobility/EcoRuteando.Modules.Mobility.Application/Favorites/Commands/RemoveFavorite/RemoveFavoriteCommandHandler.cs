using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Commands.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler
    : IRequestHandler<RemoveFavoriteCommand>
{
    private readonly IFavoriteRouteRepository _favoriteRouteRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public RemoveFavoriteCommandHandler(
        IFavoriteRouteRepository favoriteRouteRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _favoriteRouteRepository = favoriteRouteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveFavoriteCommand request,
        CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRouteRepository.GetAsync(
            request.UserId,
            request.RouteId,
            cancellationToken);

        if (favorite is null)
        {
            throw new NotFoundException("La ruta no está en favoritos.");
        }

        _favoriteRouteRepository.Delete(favorite);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
