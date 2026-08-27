using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandHandler
    : IRequestHandler<RemovePermissionFromRoleCommand>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public RemovePermissionFromRoleCommandHandler(
        IRolePermissionRepository rolePermissionRepository,
        ISecurityUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
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

        await _auditLogService.LogAsync(
            null,
            "permission.revoked",
            entityName: "role_permissions",
            entityId: $"{request.RoleId}/{request.PermissionId}",
            cancellationToken: cancellationToken);
    }
}