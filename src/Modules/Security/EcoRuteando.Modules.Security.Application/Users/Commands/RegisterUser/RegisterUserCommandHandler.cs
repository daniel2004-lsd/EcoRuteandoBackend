using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly IAuditLogService _auditLogService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRoleRepository roleRepository,
        ISecurityUnitOfWork unitOfWork,
        ISender sender,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _auditLogService = auditLogService;
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
            throw new ConflictException("El correo ya está registrado.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            null
        );

        user.AcceptTerms();

        var role = await _roleRepository.GetByNameAsync(
            "User",
            cancellationToken);

        if (role is null)
        {
            throw new NotFoundException("El rol 'User' no existe.");
        }

        user.AssignPrimaryRole(role);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "user.registered",
            entityName: "users",
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        await _sender.Send(
            new SendVerificationEmailCommand(user.Id, null, null),
            cancellationToken);

        return user.Id;
    }
}
