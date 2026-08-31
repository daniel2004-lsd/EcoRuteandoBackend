using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.CreatePoi;

public sealed record CreatePoiCommand(
    string Name,
    string PoiType,
    double Lat,
    double Lng,
    string? Description = null,
    string? Address = null,
    string? IconUrl = null,
    string? Source = null,
    Guid? CreatedBy = null)
    : IRequest<Guid>;