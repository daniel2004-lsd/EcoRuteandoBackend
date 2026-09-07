using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Validation;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Bootstrap;

public sealed class AdminBootstrapService : IAdminBootstrapService
{
    private const string AdminRoleName = "Admin";

    private readonly AdminBootstrapOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly ILogger<AdminBootstrapService> _logger;

    public AdminBootstrapService(
        IOptions<AdminBootstrapOptions> options,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        ISecurityPolicyRepository securityPolicyRepository,
        IPasswordHasher passwordHasher,
        ISecurityUnitOfWork unitOfWork,
        ILogger<AdminBootstrapService> logger)
    {
        _options = options.Value;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _securityPolicyRepository = securityPolicyRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task EnsureAdminAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Email)
            || string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogInformation(
                "Bootstrap de administrador omitido (Admin__Email/Admin__Password no configurados).");
            return;
        }

        var email = _options.Email.Trim().ToLowerInvariant();

        var policy = await _securityPolicyRepository.GetAsync(cancellationToken);

        var passwordError = PasswordPolicy.Validate(policy, _options.Password);

        if (passwordError is not null)
        {
            throw new InvalidOperationException(
                $"El administrador inicial no se pudo crear: {passwordError}");
        }

        var adminRole = await _roleRepository.GetByNameAsync(
            AdminRoleName,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"El rol '{AdminRoleName}' no existe. Ejecuta Liquibase antes de arrancar la API.");

        var existingUser = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (existingUser is null)
        {
            var passwordHash = _passwordHasher.Hash(_options.Password);

            var user = new User(
                string.IsNullOrWhiteSpace(_options.FirstName)
                    ? "Administrador"
                    : _options.FirstName.Trim(),
                _options.LastName?.Trim(),
                email,
                passwordHash,
                null);

            user.AcceptTerms();
            user.VerifyEmail();
            user.AssignPrimaryRole(adminRole);

            await _userRepository.AddAsync(user, cancellationToken);

            user.UserRoles.Add(new UserRole(user.Id, adminRole.Id));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Administrador inicial creado: {Email} con rol '{Role}'.",
                email,
                AdminRoleName);

            return;
        }

        var changed = false;

        if (!existingUser.EmailVerified)
        {
            existingUser.VerifyEmail();
            changed = true;
        }

        if (existingUser.PrimaryRoleId != adminRole.Id)
        {
            existingUser.AssignPrimaryRole(adminRole);
            changed = true;
        }

        var membership = await _userRoleRepository.GetAsync(
            existingUser.Id,
            adminRole.Id,
            cancellationToken);

        if (membership is null)
        {
            existingUser.UserRoles.Add(new UserRole(existingUser.Id, adminRole.Id));
            changed = true;
        }

        if (!changed)
        {
            _logger.LogInformation(
                "El administrador {Email} ya está configurado con el rol '{Role}'.",
                email,
                AdminRoleName);
            return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Rol '{Role}' asegurado para {Email}.",
            AdminRoleName,
            email);
    }
}