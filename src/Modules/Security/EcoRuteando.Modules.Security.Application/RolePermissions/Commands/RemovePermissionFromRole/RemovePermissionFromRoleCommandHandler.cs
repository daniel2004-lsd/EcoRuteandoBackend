using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandHandler
    : IRequestHandler<RemovePermissionFromRoleCommand>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePermissionFromRoleCommandHandler(
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemovePermissionFromRoleCommand request,
        CancellationToken cancellationToken)
    {
        var rolePermission = await _rolePermissionRepository.GetAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        if (rolePermission is null)
        {
            throw new NotFoundException(
                "El rol no tiene asignado ese permiso.");
        }

        await _rolePermissionRepository.DeleteAsync(
            rolePermission,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}