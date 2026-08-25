using EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;
using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Commands.UpdateSecurityPolicy;

public sealed class UpdateSecurityPolicyCommandValidator
    : AbstractValidator<UpdateSecurityPolicyCommand>
{
    public UpdateSecurityPolicyCommandValidator()
    {
        RuleFor(x => x.MinPasswordLength)
            .InclusiveBetween(6, 128)
            .WithMessage("La longitud mínima de contraseña debe estar entre 6 y 128.");

        RuleFor(x => x.PasswordExpirationDays)
            .InclusiveBetween(0, 3650)
            .WithMessage("La expiración de contraseña debe estar entre 0 y 3650 días (0 = no expira).");

        RuleFor(x => x.MaxFailedAttempts)
            .InclusiveBetween(1, 100)
            .WithMessage("El máximo de intentos fallidos debe estar entre 1 y 100.");

        RuleFor(x => x.LockoutTimeMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("El tiempo de bloqueo debe estar entre 1 y 1440 minutos.");

        RuleFor(x => x.MaxActiveSessions)
            .InclusiveBetween(0, 50)
            .WithMessage("El máximo de sesiones activas debe estar entre 0 y 50 (0 = ilimitadas).");
    }
}
