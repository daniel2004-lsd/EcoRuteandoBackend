using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Application.System.Dtos;

public record TransportFactorDto(
    Guid Id,
    TransportType TransportType,
    string TransportTypeName,
    decimal Co2FactorKgKm,
    decimal? CalorieFactorKm,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    string? Source,
    DateTime CreatedAt,
    bool IsActive);
