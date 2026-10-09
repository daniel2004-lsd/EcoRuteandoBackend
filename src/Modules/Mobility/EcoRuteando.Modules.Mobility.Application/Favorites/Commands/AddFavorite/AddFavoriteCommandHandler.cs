using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandler
    : IRequestHandler<AddFavoriteCommand>
{
    private readonly IFavoriteRouteRepository _favoriteRouteRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public AddFavoriteCommandHandler(
        IFavoriteRouteRepository favoriteRouteRepository,
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _favoriteRouteRepository = favoriteRouteRepository;
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AddFavoriteCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _favoriteRouteRepository.GetAsync(
            request.UserId,
            request.RouteId,
            cancellationToken);

        // Idempotencia (CU11): una misma ruta no debe duplicarse en favoritos.
        if (existing is not null)
        {
            return;
        }

        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        var favorite = new FavoriteRoute(
            request.UserId,
            request.RouteId,
            request.Label);

        await _favoriteRouteRepository.AddAsync(favorite, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
