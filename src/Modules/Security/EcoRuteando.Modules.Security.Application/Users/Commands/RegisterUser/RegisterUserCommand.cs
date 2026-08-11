    using MediatR;

    namespace EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;

    public sealed record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password
    ) : IRequest<Guid>;