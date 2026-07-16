using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    public RegisterUserCommandHandler(IUserRepository userRepository , IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(
    RegisterUserCommand request,
    CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new Exception("El correo ya está registrado.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            null
        );

       

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        return user.Id;
    }
}