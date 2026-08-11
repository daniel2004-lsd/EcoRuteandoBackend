using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Commands.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(
    Guid UserId,
    int RoleId)
    : IRequest;