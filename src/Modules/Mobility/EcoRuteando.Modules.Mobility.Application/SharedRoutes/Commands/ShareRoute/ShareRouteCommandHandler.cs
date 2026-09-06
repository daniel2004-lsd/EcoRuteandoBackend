using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Commands.ShareRoute;

/// <summary>
/// Handler del CU20 (HU-14 "Compartir recorridos").
/// Guarda el registro del recorrido compartido con los datos que el usuario
/// autorizó hacer públicos (RF32.5: se omiten los datos privados).
/// Precondición: sesión activa y un recorrido finalizado (existente del usuario).
/// </summary>
public sealed class ShareRouteCommandHandler
    : IRequestHandler<ShareRouteCommand, Guid>
{
    private readonly ISharedRouteRepository _sharedRouteRepository;
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public ShareRouteCommandHandler(
        ISharedRouteRepository sharedRouteRepository,
        IRouteUsageRepository routeUsageRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _sharedRouteRepository = sharedRouteRepository;
        _routeUsageRepository = routeUsageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        ShareRouteCommand request,
        CancellationToken cancellationToken)
    {
        var usage = await _routeUsageRepository.GetByIdAsync(
            request.UsageId,
            request.UserId,
            cancellationToken);

        if (usage is null)
        {
            throw new NotFoundException("El recorrido no existe.");
        }

        var shared = new SharedRoute(
            request.UserId,
            request.UsageId,
            request.SocialNetwork,
            request.SharedData);

        await _sharedRouteRepository.AddAsync(shared, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shared.Id;
    }
}