using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler
    : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AssignRoleToUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("El usuario no existe.");
        }

        var role = await _roleRepository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new NotFoundException("El rol no existe.");
        }

        var existing = await _userRoleRepository.GetAsync(
            request.UserId,
            request.RoleId,
            cancellationToken);

        if (existing is not null)
        {
            throw new DomainException("El usuario ya tiene asignado este rol.");
        }

        var userRole = new UserRole(
            request.UserId,
            request.RoleId);

        await _userRoleRepository.AddAsync(
            userRole,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}