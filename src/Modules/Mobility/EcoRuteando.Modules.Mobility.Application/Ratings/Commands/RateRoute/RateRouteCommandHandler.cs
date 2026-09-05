using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Commands.RateRoute;

/// <summary>
/// Handler del CU09 (HU-11 "Calificar rutas").
/// Guarda o actualiza la calificación del usuario para una ruta.
/// Precondición: el usuario debe haber finalizado un trayecto de la ruta.
/// </summary>
public sealed class RateRouteCommandHandler
    : IRequestHandler<RateRouteCommand>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public RateRouteCommandHandler(
        IRatingRepository ratingRepository,
        IRouteRepository routeRepository,
        IRouteUsageRepository routeUsageRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _ratingRepository = ratingRepository;
        _routeRepository = routeRepository;
        _routeUsageRepository = routeUsageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RateRouteCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        // Precondición CU09: solo se puede calificar una ruta completada.
        var hasCompletedUsage = await _routeUsageRepository.HasCompletedUsageAsync(
            request.UserId,
            request.RouteId,
            cancellationToken);

        if (!hasCompletedUsage)
        {
            throw new DomainException(
                "Debes completar la ruta antes de poder calificarla.");
        }

        var existing = await _ratingRepository.GetByUserAndRouteAsync(
            request.UserId,
            request.RouteId,
            cancellationToken);

        if (existing is null)
        {
            var rating = new Rating(
                request.UserId,
                request.RouteId,
                request.RatingValue,
                request.Comment,
                request.UsageId);

            await _ratingRepository.AddAsync(rating, cancellationToken);
        }
        else
        {
            existing.Update(
                request.RatingValue,
                request.Comment);

            _ratingRepository.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}