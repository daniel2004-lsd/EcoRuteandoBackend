using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;

public sealed record RevokeTrustedDeviceCommand(
    Guid UserId,
    Guid DeviceId) : IRequest<Unit>;
