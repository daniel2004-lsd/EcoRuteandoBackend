using EcoRuteando.Modules.Admin.Application.System.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.UpdateTransportFactor;

public record UpdateTransportFactorCommand(
    Guid Id,
    decimal Co2FactorKgKm,
    decimal? CalorieFactorKm,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    string? Source) : IRequest<TransportFactorDto>;
