using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Commands.AssignRoleToUser;

public sealed record AssignRoleToUserCommand(
    Guid UserId,
    int RoleId)
    : IRequest;