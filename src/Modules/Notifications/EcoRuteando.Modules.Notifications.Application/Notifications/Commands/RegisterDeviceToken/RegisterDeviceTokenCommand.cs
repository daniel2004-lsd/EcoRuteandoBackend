using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.RegisterDeviceToken;

public record RegisterDeviceTokenCommand(
    Guid UserId,
    string Platform,
    string ExpoPushToken,
    string? DeviceId = null) : IRequest<bool>;
