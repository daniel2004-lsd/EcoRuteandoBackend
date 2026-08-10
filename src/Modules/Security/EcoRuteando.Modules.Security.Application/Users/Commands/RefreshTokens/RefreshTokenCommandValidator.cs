using FluentValidation;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("El refresh token es obligatorio.");
    }
}