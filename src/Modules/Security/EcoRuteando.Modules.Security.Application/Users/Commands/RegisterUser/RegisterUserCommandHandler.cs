using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    public RegisterUserCommandHandler(IUserRepository userRepository , IPasswordHasher passwordHasher, IRoleRepository roleRepository , IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
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
        var role = await _roleRepository.GetByNameAsync(
            "User",
            cancellationToken);

        if (role is null)
        {
            throw new Exception("El rol 'User' no existe.");
        }
        user.AssignPrimaryRole(role);




        await _userRepository.AddAsync(
            user,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }


        return user.Id;
    }
}