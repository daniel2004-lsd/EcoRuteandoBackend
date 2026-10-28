using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Commands.UpsertDailyStatistic;

public sealed class UpsertDailyStatisticCommandValidator : AbstractValidator<UpsertDailyStatisticCommand>
{
    public UpsertDailyStatisticCommandValidator()
    {
        RuleFor(x => x.StatisticDate)
            .NotEmpty().WithMessage("La fecha de la estadística es obligatoria.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de la estadística no puede ser futura.");

        RuleFor(x => x.TotalUsers)
            .GreaterThanOrEqualTo(0).WithMessage("El total de usuarios no puede ser negativo.");

        RuleFor(x => x.NewUsers)
            .GreaterThanOrEqualTo(0).WithMessage("Los nuevos usuarios no pueden ser negativos.");

        RuleFor(x => x.ActiveUsers)
            .GreaterThanOrEqualTo(0).WithMessage("Los usuarios activos no pueden ser negativos.");

        RuleFor(x => x.TotalRoutes)
            .GreaterThanOrEqualTo(0).WithMessage("El total de rutas no puede ser negativo.");

        RuleFor(x => x.CompletedRoutes)
            .GreaterThanOrEqualTo(0).WithMessage("Las rutas completadas no pueden ser negativas.");

        RuleFor(x => x.TotalCommunities)
            .GreaterThanOrEqualTo(0).WithMessage("El total de comunidades no puede ser negativo.");

        RuleFor(x => x.TotalReports)
            .GreaterThanOrEqualTo(0).WithMessage("El total de reportes no puede ser negativo.");

        RuleFor(x => x.TotalCo2Saved)
            .GreaterThanOrEqualTo(0).WithMessage("El CO2 ahorrado no puede ser negativo.");

        RuleFor(x => x.TotalDistanceKm)
            .GreaterThanOrEqualTo(0).WithMessage("La distancia total no puede ser negativa.");
    }
}
