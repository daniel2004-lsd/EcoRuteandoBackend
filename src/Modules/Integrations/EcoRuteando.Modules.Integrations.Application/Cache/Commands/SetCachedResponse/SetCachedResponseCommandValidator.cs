using FluentValidation;

namespace EcoRuteando.Modules.Integrations.Application.Cache.Commands.SetCachedResponse;

public sealed class SetCachedResponseCommandValidator : AbstractValidator<SetCachedResponseCommand>
{
    public SetCachedResponseCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("El proveedor es obligatorio.")
            .MaximumLength(100).WithMessage("El proveedor no puede exceder 100 caracteres.");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("El endpoint es obligatorio.")
            .MaximumLength(500).WithMessage("El endpoint no puede exceder 500 caracteres.");

        RuleFor(x => x.Response)
            .NotEmpty().WithMessage("La respuesta no puede estar vacía.");

        RuleFor(x => x.TtlSeconds)
            .GreaterThan(0).WithMessage("El TTL debe ser mayor a 0 segundos.")
            .LessThanOrEqualTo(86400).WithMessage("El TTL no puede exceder 86400 segundos (24 horas).");
    }
}
