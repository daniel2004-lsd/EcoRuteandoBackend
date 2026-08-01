
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string? LastName,
    string? PhoneNumber
) : IRequest;

