using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(
    Guid UserId)
    : IRequest<CurrentUserResponse>;