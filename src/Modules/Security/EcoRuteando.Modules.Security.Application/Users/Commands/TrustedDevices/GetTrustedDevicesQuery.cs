using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;

public sealed record GetTrustedDevicesQuery(
    Guid UserId) : IRequest<IReadOnlyList<TrustedDeviceResponse>>;

public sealed record TrustedDeviceResponse(
    Guid Id,
    string? DeviceName,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsRevoked,
    bool IsActive);
