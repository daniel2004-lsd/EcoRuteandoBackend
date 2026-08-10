using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;

public sealed class LogoutUserCommandValidator
    : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("El refresh token es obligatorio.");
    }
}