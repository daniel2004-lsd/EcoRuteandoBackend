using System.Text.Json;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Commands.ShareRoute;

public sealed record ShareRouteCommand(
    Guid UsageId,
    Guid UserId,
    string? SocialNetwork = null,
    JsonDocument? SharedData = null)
    : IRequest<Guid>;