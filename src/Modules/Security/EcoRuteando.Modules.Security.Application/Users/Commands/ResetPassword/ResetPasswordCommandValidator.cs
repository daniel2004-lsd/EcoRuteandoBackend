using EcoRuteando.Modules.Security.Application.Validation;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private SecurityPolicy? _policy;
    private bool _policyLoaded;

    public ResetPasswordCommandValidator(
        ISecurityPolicyRepository securityPolicyRepository)
    {
        _securityPolicyRepository = securityPolicyRepository;

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("El token es obligatorio.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contraseña es obligatoria.")
            .CustomAsync(ValidateNewPasswordAgainstPolicyAsync);
    }

    private async Task ValidateNewPasswordAgainstPolicyAsync(
        string password,
        ValidationContext<ResetPasswordCommand> context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (!_policyLoaded)
        {
            _policy = await _securityPolicyRepository.GetAsync(cancellationToken);
            _policyLoaded = true;
        }

        var error = PasswordPolicy.Validate(_policy, password);

        if (error is not null)
        {
            context.AddFailure(error);
        }
    }
}
