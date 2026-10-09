using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.DeleteRoute;

public sealed class DeleteRouteCommandHandler
    : IRequestHandler<DeleteRouteCommand>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public DeleteRouteCommandHandler(
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteRouteCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.Id,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        // Regla de negocio CU03: un usuario solo puede eliminar rutas que él creó.
        // El administrador puede eliminar cualquier ruta.
        if (!request.IsAdmin
            && (route.CreatedBy is null
                || route.CreatedBy != request.RequestedByUserId))
        {
            throw new ForbiddenException(
                "Solo puedes eliminar las rutas que tú mismo creaste.");
        }

        _routeRepository.Delete(route);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
