using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Commands.ShareRoute;

public sealed class ShareRouteCommandValidator
    : AbstractValidator<ShareRouteCommand>
{
    public ShareRouteCommandValidator()
    {
        RuleFor(x => x.UsageId)
            .NotEmpty()
            .WithMessage("El recorrido es obligatorio.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.SocialNetwork)
            .MaximumLength(50)
            .WithMessage("La red social no puede superar 50 caracteres.");
    }
}