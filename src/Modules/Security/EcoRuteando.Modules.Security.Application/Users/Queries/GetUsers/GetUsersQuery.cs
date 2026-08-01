using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery
    : IRequest<IReadOnlyList<UserResponse>>;