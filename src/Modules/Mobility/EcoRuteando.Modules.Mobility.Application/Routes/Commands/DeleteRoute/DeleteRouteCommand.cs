using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.DeleteRoute;

public sealed record DeleteRouteCommand(
    Guid Id) : IRequest;
