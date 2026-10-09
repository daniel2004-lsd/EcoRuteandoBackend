using EcoRuteando.Modules.Security.Application.Validation;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordValidator
    : AbstractValidator<ChangeMyPasswordCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private SecurityPolicy? _policy;
    private bool _policyLoaded;

    public ChangeMyPasswordValidator(
        ISecurityPolicyRepository securityPolicyRepository)
    {
        _securityPolicyRepository = securityPolicyRepository;

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contraseña actual es obligatoria.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contraseña es obligatoria.")
            .CustomAsync(ValidateNewPasswordAgainstPolicyAsync);
    }

    private async Task ValidateNewPasswordAgainstPolicyAsync(
        string password,
        ValidationContext<ChangeMyPasswordCommand> context,
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
