using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.CreateTransportFactor;

public record CreateTransportFactorCommand(
    TransportType TransportType,
    decimal Co2FactorKgKm,
    decimal? CalorieFactorKm,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Source) : IRequest<TransportFactorDto>;
