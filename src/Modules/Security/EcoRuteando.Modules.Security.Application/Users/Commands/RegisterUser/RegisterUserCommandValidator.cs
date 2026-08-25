using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator
    : AbstractValidator<RegisterUserCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private SecurityPolicy? _policy;
    private bool _policyLoaded;

    public RegisterUserCommandValidator(
        ISecurityPolicyRepository securityPolicyRepository)
    {
        _securityPolicyRepository = securityPolicyRepository;

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("El apellido no puede exceder 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo electrónico no es válido.")
            .MaximumLength(150)
            .WithMessage("El correo electrónico no puede exceder 150 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .CustomAsync(ValidatePasswordAgainstPolicyAsync);
    }

    private async Task ValidatePasswordAgainstPolicyAsync(
        string password,
        ValidationContext<RegisterUserCommand> context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var policy = await GetPolicyAsync(cancellationToken);

        var error = Validation.PasswordPolicy.Validate(policy, password);

        if (error is not null)
        {
            context.AddFailure(error);
        }
    }

    private async Task<SecurityPolicy?> GetPolicyAsync(
        CancellationToken cancellationToken)
    {
        if (!_policyLoaded)
        {
            _policy = await _securityPolicyRepository.GetAsync(cancellationToken);
            _policyLoaded = true;
        }

        return _policy;
    }
}
