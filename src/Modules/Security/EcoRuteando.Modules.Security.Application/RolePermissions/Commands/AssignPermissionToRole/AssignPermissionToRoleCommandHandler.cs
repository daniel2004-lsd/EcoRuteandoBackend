using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandHandler
    : IRequestHandler<AssignPermissionToRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public AssignPermissionToRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        ISecurityUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task Handle(
        AssignPermissionToRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new NotFoundException("El rol no existe.");
        }

        var permission = await _permissionRepository.GetByIdAsync(
            request.PermissionId,
            cancellationToken);

        if (permission is null)
        {
            throw new NotFoundException("El permiso no existe.");
        }

        var existing = await _rolePermissionRepository.GetAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        if (existing is not null)
        {
            throw new DomainException(
                "El rol ya tiene asignado este permiso.");
        }

        var rolePermission = new RolePermission(
            request.RoleId,
            request.PermissionId);

        await _rolePermissionRepository.AddAsync(
            rolePermission,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "permission.granted",
            entityName: "role_permissions",
            entityId: $"{request.RoleId}/{request.PermissionId}",
            afterData: $"{{\"role\":\"{role.Name}\",\"permission\":\"{permission.Name}\"}}",
            cancellationToken: cancellationToken);
    }
}