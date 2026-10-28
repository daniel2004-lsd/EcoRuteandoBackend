using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.DeactivatePoi;

public sealed record DeactivatePoiCommand(
    Guid Id,
    Guid? RequestedByUserId = null,
    bool IsAdmin = false) : IRequest;